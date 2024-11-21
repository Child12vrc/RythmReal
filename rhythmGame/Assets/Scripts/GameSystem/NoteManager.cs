using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoteManager : MonoBehaviour
{
    public AudioClip audioClip;
    public List<Note> notes = new List<Note>();
    public float bpm = 120f;
    public float speed = 1f;
    public GameObject notePrefabs;
    public float audioLatency = 0.1f;  // 오디오 레이턴시
    public float noteSpeed = 10;
    private AudioSource audioSource;
    private float startTime;
    private List<Note> activeNotes = new List<Note>();
    private float[] spawnOffsets;
    public bool debugMode = false;
    public float initialDelay = 3f;
    private float audioStartTime;  // 실제 음악이 시작되는 시간

    public Transform[] spawnPoints;
    public Transform[] hitPoints;

    private bool isInitialized = false;
    private float songPosition = 0f;  // 현재 곡의 진행 시간
    private NotePool notePool;

    private AudioVisualizer audioVisualizer;
    public GameObject[] visualizerObjects; // Inspector에서 설정할 시각화 오브젝트 배열

    private SequenceData sequenceData;  // 추가: 시퀀스 데이터 참조  
    private int currentEffectBeat = -1;  // 현재 이펙트 비트 추적용
    public EffectManager effectManager; // 이펙트 매니저 참조

    private void TriggerEffect(int effectValue)
    {
        // 이전 이펙트들을 모두 끔
        effectManager.ResetEffects();

        // 새로운 이펙트 활성화 (effectValue가 0이 아닌 경우에만)
        if (effectValue > 0)
        {
            effectManager.SetEffect(effectValue, true);

            if (debugMode)
            {
                Debug.Log($"Effect {effectValue} activated");
            }
        }
    }

    public void Initialize()
    {
        // 노트 풀 초기화
        GameObject poolObject = new GameObject("NotePool");
        poolObject.transform.SetParent(transform);
        notePool = poolObject.AddComponent<NotePool>();
        notePool.Initialize(notePrefabs);

        // 오디오 시각화 설정 - 기존 것이 있으면 유지
        if (audioVisualizer == null)
        {
            audioVisualizer = gameObject.GetComponent<AudioVisualizer>();
            if (audioVisualizer == null)
            {
                audioVisualizer = gameObject.AddComponent<AudioVisualizer>();
                audioVisualizer.visualizerObjects = visualizerObjects;
            }
        }

        // 이전 AudioSource 제거 후 새로 생성
        AudioSource existingAudioSource = gameObject.GetComponent<AudioSource>();
        if (existingAudioSource != null)
        {
            Destroy(existingAudioSource);
        }
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.playOnAwake = false;

        startTime = Time.time;
        audioStartTime = startTime + initialDelay;

        activeNotes.Clear();
        activeNotes.AddRange(notes);
        activeNotes.Sort((a, b) => a.startTime.CompareTo(b.startTime));

        CalculateSpawnOffsets();

        if (debugMode)
        {
            CreateDebugLines();
        }

        StartCoroutine(StartAudioWithDelay());

        currentEffectBeat = -1;
        isInitialized = true;
    }



    public void SetSequenceData(SequenceData data)  // 추가: 시퀀스 데이터 설정
    {
        sequenceData = data;
    }


    private IEnumerator StartAudioWithDelay()
    {
        // 이전에 재생 중이던 오디오가 있다면 정지
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        yield return new WaitForSeconds(initialDelay - audioLatency);
        audioSource.Play();
    }

    private void CalculateSpawnOffsets()
    {
        if (spawnPoints == null || hitPoints == null || spawnPoints.Length != hitPoints.Length)
        {
            Debug.LogError("Spawn points and hit points must be set and have the same length!");
            return;
        }

        spawnOffsets = new float[spawnPoints.Length];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            float distance = Vector3.Distance(spawnPoints[i].position, hitPoints[i].position);
            float beatDuration = 60f / (bpm * 4); // 1비트의 시간
            spawnOffsets[i] = distance / noteSpeed;

            if (debugMode)
            {
                Debug.Log($"Track {i} - Distance: {distance:F3}, Spawn Offset: {spawnOffsets[i]:F3}, Beat Duration: {beatDuration:F3}");
            }
        }
    }

    void Update()
    {
        if (!isInitialized || spawnPoints == null || hitPoints == null || spawnPoints.Length == 0) return;

        songPosition = (Time.time - audioStartTime) + audioLatency;

        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note note = activeNotes[i];
            int trackIndex = Mathf.Clamp(note.trackIndex, 0, spawnPoints.Length - 1);

            float spawnOffset = spawnOffsets[trackIndex];
            float spawnTime = note.startTime - spawnOffset;

            if (songPosition < spawnTime) continue;

            if (songPosition >= spawnTime && songPosition < note.startTime + note.duration)
            {
                // 롱노트 생성
                SpawnNoteObject(note);
                activeNotes.RemoveAt(i);
            }
        }

        // 입력 처리 (롱노트 유지)
        HandleInput();
    }

    private void HandleInput()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (Input.GetKey(KeyCode.Space)) // 예: 스페이스바로 판정
            {
                // 활성화된 롱노트를 누르고 있는지 확인
                foreach (var activeNote in activeNotes)
                {
                    if (activeNote.duration > 0) // 롱노트
                    {
                        // 누르고 있는 동안 판정
                        Debug.Log($"Holding Long Note on Track {activeNote.trackIndex}");
                    }
                }
            }
        }
    }


    void OnGUI()
    {
        if (debugMode && isInitialized)
        {
            float beatDuration = 60f / bpm;
            float currentBeat = songPosition / beatDuration;

            GUI.Label(new Rect(10, 10, 200, 20), $"Song Time: {songPosition:F3}");
            GUI.Label(new Rect(10, 30, 200, 20), $"Audio Time: {audioSource.time:F3}");
            GUI.Label(new Rect(10, 50, 200, 20), $"Current Beat: {currentBeat:F2}");
            GUI.Label(new Rect(10, 70, 200, 20), $"Beat Duration: {beatDuration:F3}");
            GUI.Label(new Rect(10, 90, 200, 20), $"Spawn Offset: {(spawnOffsets.Length > 0 ? spawnOffsets[0].ToString("F3") : "N/A")}");
        }
    }

    private NoteObject SpawnNoteObject(Note note)
    {
        int trackIndex = Mathf.Clamp(note.trackIndex, 0, spawnPoints.Length - 1);
        NoteObject noteComponent = notePool.GetNote();

        if (noteComponent == null)
        {
            Debug.LogWarning("NotePool has no available NoteObject to spawn.");
            return null;
        }

        Transform endNotePos = null;
        float endNoteTime = 0f;
        if (note.noteValue == 2)  // 롱노트 시작
        {
            float minTimeDiff = float.MaxValue;
            Note endNote = null;
            foreach (Note otherNote in notes)
            {
                if (otherNote.trackIndex == note.trackIndex &&
                    otherNote.noteValue == 3 &&
                    otherNote.startTime > note.startTime)
                {
                    float timeDiff = otherNote.startTime - note.startTime;
                    if (timeDiff < minTimeDiff)
                    {
                        minTimeDiff = timeDiff;
                        endNoteTime = otherNote.startTime;
                        endNote = otherNote;
                    }
                }
            }

            if (endNote != null)
            {
                // 끝노트의 위치를 계산
                GameObject tempEndPos = new GameObject("TempEndPos");
                float spawnOffset = spawnOffsets[trackIndex];
                float endSpawnTime = endNote.startTime;
                float moveDistance = noteSpeed * (endSpawnTime - note.startTime);
                Vector3 direction = (hitPoints[trackIndex].position - spawnPoints[trackIndex].position).normalized;
                tempEndPos.transform.position = spawnPoints[trackIndex].position + direction * moveDistance;
                endNotePos = tempEndPos.transform;
                noteComponent.temporaryEndPosObject = tempEndPos;
            }
        }

        if (endNoteTime > 0)
        {
            note.duration = endNoteTime - note.startTime;
        }

        noteComponent.transform.position = spawnPoints[trackIndex].position;
        noteComponent.transform.rotation = spawnPoints[trackIndex].rotation;

        float beatDuration = 60f / bpm;
        noteComponent.Initialize(note, noteSpeed, spawnPoints[trackIndex],
            hitPoints[trackIndex], endNotePos, audioStartTime, notePool,
            beatDuration, endNoteTime);

        return noteComponent;
    }

    public void AddNote(Note note)
    {
        // duration이 있는 경우 롱노트로 설정
        if (note.duration > 0 && note.noteValue == 0)
        {
            note = new Note(
                trackIndex: note.trackIndex,
                startTime: note.startTime,
                duration: note.duration,
                noteValue: 2  // 롱노트 시작
            );
        }
        notes.Add(note);

        if (debugMode)
        {
            Debug.Log($"Added Note - Track: {note.trackIndex}, StartTime: {note.startTime}, Duration: {note.duration}, NoteValue: {note.noteValue}");
        }
    }

    private void CreateDebugLines()
    {
        if (spawnPoints == null || hitPoints == null) return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // 시작점과 도착점 사이에 선 그리기
            GameObject lineObject = new GameObject($"DebugLine_{i}");
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, spawnPoints[i].position);
            line.SetPosition(1, hitPoints[i].position);
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;

            // 판정 위치에 큐브 표시
            GameObject hitMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hitMarker.transform.position = hitPoints[i].position;
            hitMarker.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            hitMarker.name = $"HitMarker_{i}";
        }
    }

    public float GetCurrentSongTime()
    {
        return songPosition;
    }


    void OnDrawGizmos()
    {
        if (spawnPoints == null || hitPoints == null) return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null && hitPoints[i] != null)
            {
                // 시작점과 도착점을 선으로 연결
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(spawnPoints[i].position, hitPoints[i].position);

                // 시작점과 도착점을 구체로 표시
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(spawnPoints[i].position, 0.3f);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hitPoints[i].position, 0.3f);
            }
        }
    }


    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        CalculateSpawnOffsets();
    }

    public void AdjustAudioLatency(float latency)
    {
        audioLatency = latency;
    }

    private void UpdateEffects()
    {
        if (sequenceData?.effectTrack == null || effectManager == null)
        {
            if (debugMode) Debug.LogWarning("Missing effect dependencies");
            return;
        }

        float beatDuration = 60f / (bpm * 4); // 1비트의 시간
        int currentBeat = Mathf.FloorToInt(songPosition / beatDuration);

        if (currentBeat >= 0 && currentBeat < sequenceData.effectTrack.Count &&
            currentBeat != currentEffectBeat)
        {
            currentEffectBeat = currentBeat;
            int effectValue = sequenceData.effectTrack[currentBeat];

            if (effectValue > 0)
            {
                if (debugMode)
                {
                    Debug.Log($"Triggering effect {effectValue} at beat {currentBeat}, time: {songPosition:F2}");
                }
                TriggerEffect(effectValue);
            }
        }
    }

    private void OnDisable()
    {
        if (effectManager != null)
        {
            effectManager.ResetEffects();
        }

        // 오디오 정지
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}