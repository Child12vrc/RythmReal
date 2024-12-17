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
    public float audioLatency = 0.1f;
    public float noteSpeed = 10;
    private AudioSource audioSource;
    private float startTime;
    private List<Note> activeNotes = new List<Note>();
    private float[] spawnOffsets;
    public bool debugMode = false;
    public float initialDelay = 3f;
    private float audioStartTime;

    public Transform[] spawnPoints;
    public Transform[] hitPoints;

    private bool isInitialized = false;
    private float songPosition = 0f;
    private NotePool notePool;

    private AudioVisualizer audioVisualizer;
    public GameObject[] visualizerObjects;

    private SequenceData sequenceData;
    private int currentEffectBeat = -1;
    public EffectManager effectManager;

    private void TriggerEffect(int effectValue)
    {
        Debug.Log($"[NoteManager] Triggering effect: {effectValue}");
        if (effectManager != null)
        {
            effectManager.ResetEffects();
            if (effectValue > 0)
            {
                effectManager.SetEffect(effectValue, true);
                if (debugMode)
                {
                    Debug.Log($"[NoteManager] Effect {effectValue} activated");
                }
            }
        }
        else
        {
            Debug.LogWarning("[NoteManager] EffectManager is null");
        }
    }

    public void Initialize()
    {
        Debug.Log("[NoteManager] Initialize start");

        if (notePrefabs == null)
        {
            Debug.LogError("[NoteManager] NotePrefabs is not assigned!");
            return;
        }

        // NotePool 초기화
        GameObject poolObject = new GameObject("NotePool");
        poolObject.transform.SetParent(transform);
        notePool = poolObject.AddComponent<NotePool>();
        Debug.Log("[NoteManager] NotePool created");
        notePool.Initialize(notePrefabs);
        Debug.Log("[NoteManager] NotePool initialized");

        // AudioVisualizer 초기화
        if (audioVisualizer == null)
        {
            audioVisualizer = gameObject.GetComponent<AudioVisualizer>();
            if (audioVisualizer == null)
            {
                audioVisualizer = gameObject.AddComponent<AudioVisualizer>();
                audioVisualizer.visualizerObjects = visualizerObjects;
            }
            Debug.Log("[NoteManager] AudioVisualizer setup complete");
        }

        // AudioSource 초기화
        if (audioSource == null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.clip = audioClip;
        audioSource.playOnAwake = false;
        Debug.Log($"[NoteManager] AudioSource setup complete. Clip assigned: {audioClip != null}");

        // 시간 설정
        startTime = Time.time;
        audioStartTime = startTime + initialDelay;

        // 노트 초기화
        Debug.Log($"[NoteManager] Current notes count: {notes.Count}");
        activeNotes.Clear();
        activeNotes.AddRange(notes);
        activeNotes.Sort((a, b) => a.startTime.CompareTo(b.startTime));
        Debug.Log($"[NoteManager] Active notes initialized: {activeNotes.Count}");

        // 오프셋 계산
        CalculateSpawnOffsets();

    
        StartCoroutine(StartAudioWithDelay());
        Debug.Log("[NoteManager] Started audio delay coroutine");

        currentEffectBeat = -1;
        isInitialized = true;
        Debug.Log("[NoteManager] Initialization complete");
    }

    public void SetSequenceData(SequenceData data)
    {
        Debug.Log($"[NoteManager] Setting sequence data: {data != null}");
        sequenceData = data;
    }

    private IEnumerator StartAudioWithDelay()
    {
        Debug.Log("[NoteManager] Starting audio with delay");
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        yield return new WaitForSeconds(initialDelay - audioLatency);
        audioSource.Play();
        Debug.Log("[NoteManager] Audio started");
    }

    private void CalculateSpawnOffsets()
    {
        Debug.Log("[NoteManager] Calculating spawn offsets");
        if (spawnPoints == null || hitPoints == null || spawnPoints.Length != hitPoints.Length)
        {
            Debug.LogError("[NoteManager] Spawn points or hit points setup is invalid");
            return;
        }

        spawnOffsets = new float[spawnPoints.Length];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            float distance = Vector3.Distance(spawnPoints[i].position, hitPoints[i].position);
            float beatDuration = 60f / bpm;
            spawnOffsets[i] = distance / noteSpeed;
            Debug.Log($"[NoteManager] Track {i} - Distance: {distance:F3}, Offset: {spawnOffsets[i]:F3}");
        }
    }

    public void ClearAll()
    {
        Debug.Log("[NoteManager] ClearAll called");
        notes.Clear();
        activeNotes.Clear();
        StopAllCoroutines();

        if (notePool != null)
        {
            Debug.Log("[NoteManager] Clearing note pool");
            notePool.ClearPool();
            Destroy(notePool.gameObject);
            notePool = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0;
        }

        isInitialized = false;
        Debug.Log("[NoteManager] ClearAll complete");
    }

    void Update()
    {
        if (!isInitialized || spawnPoints == null || hitPoints == null || spawnPoints.Length == 0)
        {
            if (debugMode) Debug.Log("[NoteManager] Skip update - not initialized or invalid setup");
            return;
        }

        songPosition = (Time.time - audioStartTime) + audioLatency;

        UpdateEffects();

        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note note = activeNotes[i];
            int trackIndex = Mathf.Clamp(note.trackIndex, 0, spawnPoints.Length - 1);
            float spawnOffset = spawnOffsets[trackIndex];
            float spawnTime = note.startTime - spawnOffset;

            if (songPosition < spawnTime) continue;

            if (songPosition >= spawnTime)
            {
                if (debugMode) Debug.Log($"[NoteManager] Spawning note for track {trackIndex} at time {songPosition:F2}");
                SpawnNoteObject(note);
                activeNotes.RemoveAt(i);
            }
        }
    }

    private NoteObject SpawnNoteObject(Note note)
    {
        Debug.Log($"[NoteManager] Attempting to spawn note for track {note.trackIndex}");

        if (notePool == null)
        {
            Debug.LogError("[NoteManager] NotePool is null!");
            return null;
        }

        int trackIndex = Mathf.Clamp(note.trackIndex, 0, spawnPoints.Length - 1);
        NoteObject noteComponent = notePool.GetNote();

        if (noteComponent == null)
        {
            Debug.LogError("[NoteManager] Failed to get note from pool");
            return null;
        }

        noteComponent.transform.position = spawnPoints[trackIndex].position;
        noteComponent.transform.rotation = spawnPoints[trackIndex].rotation;

        float beatDuration = 60f / bpm;
        noteComponent.Initialize(note, noteSpeed, spawnPoints[trackIndex],
            hitPoints[trackIndex], audioStartTime, notePool, beatDuration);

        Debug.Log($"[NoteManager] Successfully spawned note for track {trackIndex}");
        return noteComponent;
    }

    public void AddNote(Note note)
    {
        notes.Add(note);
        Debug.Log($"[NoteManager] Added note - Track: {note.trackIndex}, Time: {note.startTime}");
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        CalculateSpawnOffsets();
        Debug.Log($"[NoteManager] Speed updated to {newSpeed}");
    }

    private void UpdateEffects()
    {
        if (sequenceData?.effectTrack == null || effectManager == null)
        {
            if (debugMode) Debug.Log("[NoteManager] Skip effects update - missing dependencies");
            return;
        }

        float beatDuration = 60f / bpm;
        int currentBeat = Mathf.FloorToInt(songPosition / beatDuration);

        if (currentBeat >= 0 && currentBeat < sequenceData.effectTrack.Count && currentBeat != currentEffectBeat)
        {
            currentEffectBeat = currentBeat;
            int effectValue = sequenceData.effectTrack[currentBeat];

            if (effectValue > 0)
            {
                if (debugMode)
                {
                    Debug.Log($"[NoteManager] Trigger effect {effectValue} at beat {currentBeat}");
                }
                TriggerEffect(effectValue);
            }
        }
    }

    private void OnDisable()
    {
        Debug.Log("[NoteManager] OnDisable called");
        if (effectManager != null)
        {
            effectManager.ResetEffects();
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public float GetCurrentSongTime()
    {
        return songPosition;
    }

    private void OnDestroy()
    {
        Debug.Log("[NoteManager] OnDestroy called");
        ClearAll();
    }
}