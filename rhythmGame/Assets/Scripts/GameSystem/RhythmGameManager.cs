using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    public SequenceData sequenceData;
    public NoteManager noteManager;
    public float playbackSpeed = 1.0f;
    private bool notesGenerated = false;

    public AudioSource gameAudioSource;
    private bool isPlaying = false;
    private bool isInitialized = false;

    public List<SequenceData> availableTracks;
    private int currentTrackIndex = 0;

    private RhythmGameController gameController;

    void Start()
    {
      
        gameController = GetComponent<RhythmGameController>();

        // 명시적으로 gameController 할당
        if (gameController == null)
        {
            gameController = FindObjectOfType<RhythmGameController>();
            if (gameController == null)
            {
                Debug.LogError("RhythmGameController not found!");
            }
        }
    }

    public void Initialize()
    {
        if (isInitialized) return;

        // AudioSource 처리 수정
        if (gameAudioSource == null)
        {
            gameAudioSource = GetComponent<AudioSource>();
            if (gameAudioSource == null)
            {
                gameAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            // 기존 AudioSource가 있다면 초기화만 수행
            gameAudioSource.Stop();
            gameAudioSource.time = 0;
        }

        isInitialized = true;
        LoadCurrentTrack();
    }

    // StopMusic 함수 추가
    public void StopMusic()
    {
        if (gameAudioSource != null)
        {
            gameAudioSource.Stop();
        }
        isPlaying = false;
    }

    void Update()
    {
        if (isPlaying && !gameAudioSource.isPlaying && gameAudioSource.time > 0)
        {
            Debug.Log("Song Complete! Controller: " + (gameController != null));
            isPlaying = false;
            if (gameController != null)
            {
                gameController.OnSongComplete();
                Debug.Log("OnSongComplete called");
            }
            EndGame();
        }
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
        isInitialized = false;  // 초기화 상태 리셋
        notesGenerated = false; // 노트 생성 상태 리셋

        if (gameAudioSource != null)
        {
            gameAudioSource.time = 0;
        }

        Initialize(); // 트랙 다시 초기화
    }

    private void StopTrack()
    {
        if (gameAudioSource != null)
        {
            gameAudioSource.Stop();
        }
        Time.timeScale = 1;
        isPlaying = false;

        if (noteManager != null)
        {
            noteManager.ClearAll();
        }
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
        Debug.Log($"[RGM] Changing track to index: {trackIndex}");
        if (currentTrackIndex == trackIndex && isInitialized)
        {
            Debug.Log("[RGM] Track is already loaded and initialized");
            return;
        }

        StopTrack();
        isInitialized = false;
        notesGenerated = false;
        currentTrackIndex = trackIndex;
        Initialize();  // 트랙 변경 후 바로 초기화까지 수행
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
        //Debug.Log("[RGM] Starting note generation");

        if (notesGenerated)
        {
            //Debug.Log("[RGM] Notes already generated, returning");
            return;
        }

        if (noteManager == null)
        {
            //Debug.LogError("[RGM] NoteManager is null!");
            return;
        }

        //Debug.Log($"[RGM] Clearing existing notes. Current note count: {noteManager.notes.Count}");
        noteManager.notes.Clear();

        if (sequenceData == null || sequenceData.trackNotes == null)
        {
            //Debug.LogError("[RGM] SequenceData or trackNotes is null!");
            return;
        }

        //Debug.Log($"[RGM] Track count: {sequenceData.trackNotes.Count}");

        for (int trackIndex = 0; trackIndex < sequenceData.trackNotes.Count; trackIndex++)
        {
            if (sequenceData.trackNotes[trackIndex] == null)
            {
                Debug.LogError($"[RGM] Track {trackIndex} is null!");
                continue;
            }

            //Debug.Log($"[RGM] Generating notes for track {trackIndex}. Note count: {sequenceData.trackNotes[trackIndex].Count}");

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
                    //Debug.Log($"[RGM] Added note: Track={trackIndex}, Time={startTime:F2}, Value={noteValue}");
                }
            }
        }

        notesGenerated = true;
        //Debug.Log($"[RGM] Note generation complete. Total notes: {noteManager.notes.Count}");
    }

    private void LoadCurrentTrack()
    {
        Debug.Log($"[RGM] Loading track {currentTrackIndex}");
        if (availableTracks == null || availableTracks.Count == 0)
        {
            Debug.LogError("[RGM] No tracks available!");
            return;
        }

        sequenceData = availableTracks[currentTrackIndex];
        sequenceData.LoadFromJson();

        Debug.Log($"[RGM] Track loaded. Has notes: {sequenceData.trackNotes != null}, Has effect track: {sequenceData.effectTrack != null}");

        if (sequenceData.trackNotes == null || sequenceData.trackNotes.Count == 0)
        {
            Debug.Log("[RGM] Initializing track notes");
            InitializeTrackNotes();
        }
        if (sequenceData.effectTrack == null)
        {
            Debug.Log("[RGM] Initializing effect track");
            InitializeEffectTrack();
        }

        SetupTrack();
    }

    private void SetupTrack()
    {
        if (noteManager != null)
        {
            noteManager.audioClip = sequenceData.audioClip;
            noteManager.bpm = sequenceData.bpm;
            noteManager.SetSpeed(playbackSpeed);
            noteManager.SetSequenceData(sequenceData);

            // AudioSource 설정도 여기서 수행
            if (gameAudioSource != null)
            {
                gameAudioSource.clip = sequenceData.audioClip;
                gameAudioSource.playOnAwake = false;
            }

            notesGenerated = false;
            GenerateNotes();
            noteManager.Initialize();
        }

        isPlaying = true;
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
        if (gameAudioSource != null)
        {
            gameAudioSource.Stop();
        }

        StopAllCoroutines();
        isInitialized = false;
        isPlaying = false;
        notesGenerated = false;

        // 노트 매니저 정리
        if (noteManager != null)
        {
            noteManager.ClearAll();  // 노트 매니저에 추가할 함수
            noteManager.enabled = false;
        }
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