using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    public SequenceData sequenceData;
    public NoteManager noteManager;
    public float playbackSpeed = 1.0f;

    public bool IsDebug = false;                //디버그 모드 활성화

    private bool notesGenerated = false;

    void Start()
    {
        sequenceData = GameManager.Instance.LevelData.GetLevelObject().noteData;

        if (sequenceData == null)
        {
            Debug.LogError("sequenceData null");
            return;
        }

        sequenceData.LoadFromJson();

        if (sequenceData.trackNotes == null || sequenceData.trackNotes.Count == 0)
        {
            initializeTrackNotes();
        }
        //매니저에 시퀀스 데이터를 가져와서 맵핑 시킨다.
        noteManager.audioClip = sequenceData.audioClip;
        noteManager.bpm = sequenceData.bpm;
        noteManager.SetSpeed(playbackSpeed);

        GenerateNotes();
        noteManager.Initialized();
    }

    private void initializeTrackNotes()
    {
        sequenceData.trackNotes = new List<List<int>>();
        for (int i = 0; i < sequenceData.numberOfTracks; i++)
        {
            sequenceData.trackNotes.Add(new List<int>());
        }
    }

    //노트 생성
    private void GenerateNotes()
    {
        if (notesGenerated) return;         //이미 노트가 생성되었다면 중복 생성 방지

        noteManager.notes.Clear();          //노트 매니저에 접근하여 노트 초기화

        for(int trackIndex = 0;  trackIndex < sequenceData.trackNotes.Count; trackIndex++)      //노트 트랙 수
        {
            for(int beatIndex = 0; beatIndex < sequenceData.trackNotes[trackIndex].Count; beatIndex++)
            {
                int noteVaule = sequenceData.trackNotes[trackIndex][beatIndex];

                if(noteVaule != 0)
                {
                    float startTime = beatIndex * 60f / sequenceData.bpm;
                    float durtaion = noteVaule * 60f  / sequenceData.bpm;
                    Note note = new Note(trackIndex, startTime, durtaion, noteVaule, Vector3.zero, Vector3.zero);
                    noteManager.AddNote(note);
                }
            }
        }
        notesGenerated = true;
    }

    //재생 속도 설정
    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
        noteManager.SetSpeed(speed);        //스피드를 받아서 노트 매니저에 전달
    }

    //JSON 데이터에서 시퀀스 데이터 로드
    public void LoadSequenceDataFromJson()
    {
        sequenceData.LoadFromJson();

        if(sequenceData.trackNotes == null || sequenceData.trackNotes.Count == 0)
        {
            initializeTrackNotes();
        }

        //매니저에 시퀀스 데이터를 가져와서 맵핑 시킨다.
        noteManager.audioClip = sequenceData.audioClip;
        noteManager.bpm = sequenceData.bpm;
        noteManager.SetSpeed(playbackSpeed);

        notesGenerated = false;                 //새로운 데이터를 로드 했으므로 노트 재생성 허용
        GenerateNotes();
        noteManager.Initialized();
    }
}
