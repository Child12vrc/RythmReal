using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    public SequenceData sequenceData;
    public NoteManager noteManager;
    public float playbackSpeed = 1.0f;
    private bool notesGenerated = false;

    private AudioSource gameAudioSource;
    private bool isPlaying = false;
    private bool isInitialized = false;

    public List<SequenceData> availableTracks;
    private int currentTrackIndex = 0;

    private RhythmGameController gameController;

    void Start()
    {
        gameAudioSource = GetComponent<AudioSource>();
        if (gameAudioSource == null)
        {
            gameAudioSource = gameObject.AddComponent<AudioSource>();
        }

        gameController = GetComponent<RhythmGameController>();
    }

    public void Initialize()
    {
        if (isInitialized) return;

        isInitialized = true;
        LoadCurrentTrack();
    }

    private void LoadCurrentTrack()
    {
        if (availableTracks == null || availableTracks.Count == 0)
        {
            Debug.LogError("No tracks available!");
            return;
        }

        sequenceData = availableTracks[currentTrackIndex];
        sequenceData.LoadFromJson();

        if (sequenceData.trackNotes == null || sequenceData.trackNotes.Count == 0)
        {
            InitializeTrackNotes();
        }
        if (sequenceData.effectTrack == null)
        {
            InitializeEffectTrack();
        }

        SetupTrack();
    }

    void Update()
    {
        if (isPlaying && !gameAudioSource.isPlaying && gameAudioSource.time > 0)
        {
            // ¿Ωæ«¿Ã ≥°≥µ¿ª ∂ß
            isPlaying = false;
            if (gameController != null)
            {
                gameController.OnSongComplete();
            }
            EndGame();
        }
    }


    private void SetupTrack()
    {
        noteManager.audioClip = sequenceData.audioClip;
        noteManager.bpm = sequenceData.bpm;
        noteManager.SetSpeed(playbackSpeed);
        noteManager.SetSequenceData(sequenceData);

        notesGenerated = false;
        GenerateNotes();
        noteManager.Initialize();

        isPlaying = true;
    }

    public void PauseGame()
    {
        if (isPlaying)
        {
            Time.timeScale = 0;
            if (gameAudioSource != null)
            {
                gameAudioSource.Pause();
            }
            isPlaying = false;
        }
    }

    public void ResumeGame()
    {
        if (!isPlaying)
        {
            Time.timeScale = 1;
            if (gameAudioSource != null)
            {
                gameAudioSource.UnPause();
            }
            isPlaying = true;
        }
    }

    public void RestartTrack()
    {
        StopTrack();
        SetupTrack();
    }

    private void StopTrack()
    {
        if (gameAudioSource != null)
        {
            gameAudioSource.Stop();
        }
        Time.timeScale = 1;
        isPlaying = false;
        noteManager.notes.Clear();
    }

    public void NextTrack()
    {
        StopTrack();
        currentTrackIndex = (currentTrackIndex + 1) % availableTracks.Count;
        LoadCurrentTrack();
    }

    public void PreviousTrack()
    {
        StopTrack();
        currentTrackIndex--;
        if (currentTrackIndex < 0) currentTrackIndex = availableTracks.Count - 1;
        LoadCurrentTrack();
    }

    public void ChangeTrack(int trackIndex)
    {
        if (trackIndex >= 0 && trackIndex < availableTracks.Count)
        {
            StopTrack();
            currentTrackIndex = trackIndex;
            LoadCurrentTrack();
        }
    }

    private void InitializeEffectTrack()
    {
        sequenceData.effectTrack = new List<int>();
        if (sequenceData.audioClip != null)
        {
            int totalBeats = Mathf.FloorToInt((sequenceData.audioClip.length / 60f) * sequenceData.bpm);
            for (int i = 0; i < totalBeats; i++)
            {
                sequenceData.effectTrack.Add(0);
            }
        }
    }

    private void InitializeTrackNotes()
    {
        sequenceData.trackNotes = new List<List<int>>();
        for (int i = 0; i < sequenceData.numberOfTracks; i++)
        {
            sequenceData.trackNotes.Add(new List<int>());
        }
    }

    private void GenerateNotes()
    {
        if (notesGenerated) return;
        noteManager.notes.Clear();

        for (int trackIndex = 0; trackIndex < sequenceData.trackNotes.Count; trackIndex++)
        {
            for (int beatIndex = 0; beatIndex < sequenceData.trackNotes[trackIndex].Count; beatIndex++)
            {
                int noteValue = sequenceData.trackNotes[trackIndex][beatIndex];
                if (noteValue != 0)
                {
                    float startTime = beatIndex * 60f / sequenceData.bpm;
                    float duration = 0f;

                    if (noteValue == 2)
                    {
                        float noteLength = 0;
                        for (int i = beatIndex + 1; i < sequenceData.trackNotes[trackIndex].Count; i++)
                        {
                            noteLength++;
                            if (sequenceData.trackNotes[trackIndex][i] == 3)
                            {
                                break;
                            }
                        }
                        duration = noteLength * 60f / sequenceData.bpm;
                    }

                    Note note = new Note(trackIndex, startTime, duration, noteValue);
                    noteManager.AddNote(note);
                }
            }
        }
        notesGenerated = true;
    }

    public int GetCurrentEffect()
    {
        if (sequenceData?.effectTrack == null) return 0;

        float currentTime = noteManager.GetCurrentSongTime();
        int currentBeat = Mathf.FloorToInt((currentTime * sequenceData.bpm) / 60f);

        if (currentBeat >= 0 && currentBeat < sequenceData.effectTrack.Count)
        {
            return sequenceData.effectTrack[currentBeat];
        }
        return 0;
    }

    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
        noteManager.SetSpeed(speed);
    }

    public string GetCurrentTrackInfo()
    {
        if (sequenceData != null)
        {
            return $"Track {currentTrackIndex + 1}/{availableTracks.Count}";
        }
        return "No track loaded";
    }

    public bool IsInitialized()
    {
        return isInitialized;
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }

    private void OnDisable()
    {
        StopTrack();
        isInitialized = false;
    }

    public void EndGame()
    {
        StopTrack();
        isInitialized = false;
        isPlaying = false;
        notesGenerated = false;
        noteManager.notes.Clear();
    }

    public SequenceData GetCurrentTrackData()
    {
        return sequenceData;
    }

    public int GetCurrentTrackIndex()
    {
        return currentTrackIndex;
    }

    public int GetTotalTracks()
    {
        return availableTracks?.Count ?? 0;
    }
}