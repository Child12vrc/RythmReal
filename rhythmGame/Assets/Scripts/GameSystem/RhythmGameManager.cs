using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    public SequenceData sequenceData;                       // 시퀸스 데이터
    public NoteManager noteManager;                         // 노트 매니저
    public float playbackSpeed = 1.0f;                      // 재생 속도
    private bool notesGenerated = false;                    // 노트 생성 완료 플래그

    private AudioSource gameAudioSource;                    // 게임 오디오 소스
    private bool isPlaying = false;                         // 게임 진행 상태

    // 트랙 관련 변수들
    public List<SequenceData> availableTracks;             // 사용 가능한 트랙 리스트
    private int currentTrackIndex = 0;                      // 현재 트랙 인덱스

    void Start()
    {
        gameAudioSource = GetComponent<AudioSource>();
        if (gameAudioSource == null)
        {
            gameAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // 첫 번째 트랙 로드
        LoadCurrentTrack();
    }

    private void LoadCurrentTrack()
    {
        if (availableTracks == null || availableTracks.Count == 0)
        {
            Debug.LogError("No tracks available!");
            return;
        }

        // 현재 트랙 데이터 설정
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
        // 노트 매니저 설정
        noteManager.audioClip = sequenceData.audioClip;
        noteManager.bpm = sequenceData.bpm;
        noteManager.SetSpeed(playbackSpeed);
        noteManager.SetSequenceData(sequenceData);

        notesGenerated = false;
        GenerateNotes();
        noteManager.Initialize();

        isPlaying = true;
    }

    // 게임 일시정지
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

    // 게임 재개
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

    // 현재 트랙 재시작
    public void RestartTrack()
    {
        StopTrack();
        SetupTrack();
    }

    // 트랙 정지 및 초기화
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

    // 다음 트랙으로 전환
    public void NextTrack()
    {
        StopTrack();
        currentTrackIndex = (currentTrackIndex + 1) % availableTracks.Count;
        LoadCurrentTrack();
    }

    // 이전 트랙으로 전환
    public void PreviousTrack()
    {
        StopTrack();
        currentTrackIndex--;
        if (currentTrackIndex < 0) currentTrackIndex = availableTracks.Count - 1;
        LoadCurrentTrack();
    }

    // 특정 트랙으로 직접 전환
    public void ChangeTrack(int trackIndex)
    {
        if (trackIndex >= 0 && trackIndex < availableTracks.Count)
        {
            StopTrack();
            currentTrackIndex = trackIndex;
            LoadCurrentTrack();
        }
    }

    // 이펙트 트랙 초기화
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

    // 트랙 노트 초기화 
    private void InitializeTrackNotes()
    {
        sequenceData.trackNotes = new List<List<int>>();
        for (int i = 0; i < sequenceData.numberOfTracks; i++)
        {
            sequenceData.trackNotes.Add(new List<int>());
        }
    }

    // 노트 생성 
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

                    // 롱노트 처리
                    if (noteValue == 2) // 롱노트 시작
                    {
                        // 다음 노트(롱노트 끝)까지의 길이 계산
                        float noteLength = 0;
                        for (int i = beatIndex + 1; i < sequenceData.trackNotes[trackIndex].Count; i++)
                        {
                            noteLength++;
                            if (sequenceData.trackNotes[trackIndex][i] == 3) // 롱노트 끝 찾기
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

    // 현재 비트의 이펙트 번호 가져오기
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

    // 재생 속도 설정
    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
        noteManager.SetSpeed(speed);
    }

    // 현재 트랙 정보 가져오기
    public string GetCurrentTrackInfo()
    {
        if (sequenceData != null)
        {
            return $"Track {currentTrackIndex + 1}/{availableTracks.Count}";
        }
        return "No track loaded";
    }
}