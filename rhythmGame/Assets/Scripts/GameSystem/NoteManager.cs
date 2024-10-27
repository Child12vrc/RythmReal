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

    public void Initialize()
    {
        // 노트 풀 초기화
        GameObject poolObject = new GameObject("NotePool");
        poolObject.transform.SetParent(transform);
        notePool = poolObject.AddComponent<NotePool>();
        notePool.Initialize(notePrefabs);

        // 오디오 시각화 설정
        audioVisualizer = gameObject.AddComponent<AudioVisualizer>();
        audioVisualizer.visualizerObjects = visualizerObjects;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;

        startTime = Time.time;
        audioStartTime = startTime + initialDelay;

        activeNotes.Clear();
        activeNotes.AddRange(notes);
        activeNotes.Sort((a, b) => a.startTime.CompareTo(b.startTime));

        CalculateSpawnOffsets();



           // 오디오 분석을 위한 설정
        audioSource.playOnAwake = false;

        if (debugMode)
        {
            CreateDebugLines();
        }

        StartCoroutine(StartAudioWithDelay());
        isInitialized = true;
    }




    private IEnumerator StartAudioWithDelay()
    {
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
            // BPM을 고려한 노트 이동 시간 계산
            float beatDuration = 60f / bpm;  // 한 비트 당 시간(초)
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

        // 현재 곡의 진행 시간 계산 (비트 타이밍 고려)
        songPosition = (Time.time - audioStartTime) + audioLatency;

        // 노트 생성 처리
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note note = activeNotes[i];
            int trackIndex = Mathf.Clamp(note.trackIndex, 0, spawnPoints.Length - 1);
            float spawnOffset = spawnOffsets[trackIndex];
            float beatDuration = 60f / bpm;  // 한 비트 당 시간(초)

            // 노트 생성 시점 계산 (비트 타이밍 고려)
            float spawnTime = note.startTime - spawnOffset;

            // songPosition이 0보다 작으면 아직 initialDelay 시간
            if (songPosition < 0) continue;

            if (songPosition >= spawnTime && songPosition < note.startTime + note.duration)
            {
                if (debugMode)
                {
                    Debug.Log($"Before spawn - Song Pos: {songPosition:F3}, Spawn Time: {spawnTime:F3}, Note Start: {note.startTime:F3}");
                }

                SpawnNoteObject(note);
                activeNotes.RemoveAt(i);

                if (debugMode)
                {
                    Debug.Log($"Spawned note at {songPosition:F3}, Should hit at {note.startTime:F3}");
                }
            }
            else if (songPosition >= note.startTime + note.duration)
            {
                activeNotes.RemoveAt(i);
            }
        }

        if (debugMode && Time.frameCount % 60 == 0)
        {
            float beatDuration = 60f / bpm;
            float currentBeat = songPosition / beatDuration;
            Debug.Log($"Song Position: {songPosition:F3}, Beat: {currentBeat:F2}, Audio Time: {audioSource.time:F3}");
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

    private void SpawnNoteObject(Note note)
    {
        int trackIndex = Mathf.Clamp(note.trackIndex, 0, spawnPoints.Length - 1);

        // 풀에서 노트 가져오기
        NoteObject noteComponent = notePool.GetNote();

        // null 체크 추가
        if (noteComponent == null)
        {
            Debug.LogWarning("NotePool has no available NoteObject to spawn.");
            return;
        }

        noteComponent.transform.position = spawnPoints[trackIndex].position;
        noteComponent.transform.rotation = spawnPoints[trackIndex].rotation;

        // BPM 정보도 전달
        float beatDuration = 60f / bpm;
        noteComponent.Initialize(note, noteSpeed, spawnPoints[trackIndex], hitPoints[trackIndex], audioStartTime, notePool, beatDuration);
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

    public void AddNote(Note note)
    {
        notes.Add(note);
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
}