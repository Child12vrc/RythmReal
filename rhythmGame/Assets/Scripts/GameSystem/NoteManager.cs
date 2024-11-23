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
        if (effectManager != null)
        {
            effectManager.ResetEffects();
            if (effectValue > 0)
            {
                effectManager.SetEffect(effectValue, true);
                if (debugMode)
                {
                    Debug.Log($"Effect {effectValue} activated");
                }
            }
        }
    }

    public void Initialize()
    {
        GameObject poolObject = new GameObject("NotePool");
        poolObject.transform.SetParent(transform);
        notePool = poolObject.AddComponent<NotePool>();
        notePool.Initialize(notePrefabs);

        if (audioVisualizer == null)
        {
            audioVisualizer = gameObject.GetComponent<AudioVisualizer>();
            if (audioVisualizer == null)
            {
                audioVisualizer = gameObject.AddComponent<AudioVisualizer>();
                audioVisualizer.visualizerObjects = visualizerObjects;
            }
        }

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

    public void SetSequenceData(SequenceData data)
    {
        sequenceData = data;
    }

    private IEnumerator StartAudioWithDelay()
    {
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
            float beatDuration = 60f / bpm;
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

        // ÀÌÆåÆ® Ã³¸®
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
                SpawnNoteObject(note);
                activeNotes.RemoveAt(i);
            }
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

        noteComponent.transform.position = spawnPoints[trackIndex].position;
        noteComponent.transform.rotation = spawnPoints[trackIndex].rotation;

        float beatDuration = 60f / bpm;
        noteComponent.Initialize(note, noteSpeed, spawnPoints[trackIndex],
            hitPoints[trackIndex], audioStartTime, notePool, beatDuration);

        return noteComponent;
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

    private void CreateDebugLines()
    {
        if (spawnPoints == null || hitPoints == null) return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject lineObject = new GameObject($"DebugLine_{i}");
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, spawnPoints[i].position);
            line.SetPosition(1, hitPoints[i].position);
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;

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
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(spawnPoints[i].position, hitPoints[i].position);

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
        if (debugMode)
        {
            Debug.Log($"Added Note - Track: {note.trackIndex}, StartTime: {note.startTime}");
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

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}