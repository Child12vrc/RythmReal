using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    public SequenceData sequenceData;                       // ������ ������
    public NoteManager noteManager;                         // ��Ʈ �Ŵ���
    public float playbackSpeed = 1.0f;                      // ��� �ӵ�
    private bool notesGenerated = false;                    // ��Ʈ ���� �Ϸ� �÷���

    private AudioSource gameAudioSource;                    // ���� ����� �ҽ�
    private bool isPlaying = false;                         // ���� ���� ����

    // Ʈ�� ���� ������
    public List<SequenceData> availableTracks;             // ��� ������ Ʈ�� ����Ʈ
    private int currentTrackIndex = 0;                      // ���� Ʈ�� �ε���

    void Start()
    {
        gameAudioSource = GetComponent<AudioSource>();
        if (gameAudioSource == null)
        {
            gameAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // ù ��° Ʈ�� �ε�
        LoadCurrentTrack();
    }

    private void LoadCurrentTrack()
    {
        if (availableTracks == null || availableTracks.Count == 0)
        {
            Debug.LogError("No tracks available!");
            return;
        }

        // ���� Ʈ�� ������ ����
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

    private void SetupTrack()
    {
        // ��Ʈ �Ŵ��� ����
        noteManager.audioClip = sequenceData.audioClip;
        noteManager.bpm = sequenceData.bpm;
        noteManager.SetSpeed(playbackSpeed);
        noteManager.SetSequenceData(sequenceData);

        notesGenerated = false;
        GenerateNotes();
        noteManager.Initialize();

        isPlaying = true;
    }

    // ���� �Ͻ�����
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

    // ���� �簳
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

    // ���� Ʈ�� �����
    public void RestartTrack()
    {
        StopTrack();
        SetupTrack();
    }

    // Ʈ�� ���� �� �ʱ�ȭ
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

    // ���� Ʈ������ ��ȯ
    public void NextTrack()
    {
        StopTrack();
        currentTrackIndex = (currentTrackIndex + 1) % availableTracks.Count;
        LoadCurrentTrack();
    }

    // ���� Ʈ������ ��ȯ
    public void PreviousTrack()
    {
        StopTrack();
        currentTrackIndex--;
        if (currentTrackIndex < 0) currentTrackIndex = availableTracks.Count - 1;
        LoadCurrentTrack();
    }

    // Ư�� Ʈ������ ���� ��ȯ
    public void ChangeTrack(int trackIndex)
    {
        if (trackIndex >= 0 && trackIndex < availableTracks.Count)
        {
            StopTrack();
            currentTrackIndex = trackIndex;
            LoadCurrentTrack();
        }
    }

    // ����Ʈ Ʈ�� �ʱ�ȭ
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

    // Ʈ�� ��Ʈ �ʱ�ȭ 
    private void InitializeTrackNotes()
    {
        sequenceData.trackNotes = new List<List<int>>();
        for (int i = 0; i < sequenceData.numberOfTracks; i++)
        {
            sequenceData.trackNotes.Add(new List<int>());
        }
    }

    // ��Ʈ ���� 
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

                    // �ճ�Ʈ ó��
                    if (noteValue == 2) // �ճ�Ʈ ����
                    {
                        // ���� ��Ʈ(�ճ�Ʈ ��)������ ���� ���
                        float noteLength = 0;
                        for (int i = beatIndex + 1; i < sequenceData.trackNotes[trackIndex].Count; i++)
                        {
                            noteLength++;
                            if (sequenceData.trackNotes[trackIndex][i] == 3) // �ճ�Ʈ �� ã��
                            {
                                break;
                            }
                        }
                        duration = noteLength * 60f / sequenceData.bpm;
                    }

                    Note note = new Note(trackIndex, startTime, duration, noteValue);
                    noteManager.AddNote(note);

                    //if (debugMode)
                    //{
                    //    Debug.Log($"Generated Note - Track: {trackIndex}, Beat: {beatIndex}, NoteValue: {noteValue}, Duration: {duration}");
                    //}
                }
            }
        }
        notesGenerated = true;
    }

    // ���� ��Ʈ�� ����Ʈ ��ȣ ��������
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

    // ��� �ӵ� ����
    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
        noteManager.SetSpeed(speed);
    }

    // ���� Ʈ�� ���� ��������
    public string GetCurrentTrackInfo()
    {
        if (sequenceData != null)
        {
            return $"Track {currentTrackIndex + 1}/{availableTracks.Count}";
        }
        return "No track loaded";
    }
}