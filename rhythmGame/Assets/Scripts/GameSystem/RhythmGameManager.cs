using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    public SequenceData sequenceData;                       //시퀸스 데이터
    public NoteManager noteManager;                         //노트 매니저
    public float playbackSpeed = 1.0f;                      //재생 속도
    private bool notesGenerated = false;                    //노트 생성 완료 플레그 


    // Start is called before the first frame update
    void Start()
    {
        if (sequenceData == null)
        {
            Debug.LogError("시퀀스 데이터 없음");
            return;
        }

        sequenceData.LoadFromJson();

        if (sequenceData.trackNotes == null || sequenceData.trackNotes.Count == 0)
        {
            InitializeTrackNotes();
        }
        //매니저에 시퀀스 데이터를 가져와서 맵핑 시킨다.
        noteManager.audioClip = sequenceData.audioClip;
        noteManager.bpm = sequenceData.bpm;
        noteManager.SetSpeed(playbackSpeed);

        GenerateNotes();
        noteManager.Initialize();
    }


    //트렉 노트 초기화 
    private void InitializeTrackNotes()
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
        if (notesGenerated) return;                     //이미 노트가 생성되었다면 중복 생성 방지

        noteManager.notes.Clear();                      //노트 매니저에 접근하여 노트 초기화

        for (int trackIndex = 0; trackIndex < sequenceData.trackNotes.Count; trackIndex++)      //노트 트랙 수
        {
            for (int beatIndex = 0; beatIndex < sequenceData.trackNotes[trackIndex].Count; beatIndex++) //해당 트랙의 노트 
            {
                int noteValue = sequenceData.trackNotes[trackIndex][beatIndex];
                if (noteValue != 0)
                {
                    float startTime = beatIndex * 60f / sequenceData.bpm;
                    float durtaion = noteValue * 60f / sequenceData.bpm;
                    Note note = new Note(trackIndex, startTime, durtaion);
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
        noteManager.SetSpeed(speed);                //스피드를 받아서 노트 매니저에 전달 
    }

    //JSON 데이터에서 시퀸스 데이터 로드 

    public void LoadSequenceDataFromJson()
    {
        sequenceData.LoadFromJson();

        if (sequenceData.trackNotes == null || sequenceData.trackNotes.Count == 0)
        {
            InitializeTrackNotes();
        }
        //매니저에 시퀀스 데이터를 가져와서 맵핑 시킨다.
        noteManager.audioClip = sequenceData.audioClip;
        noteManager.bpm = sequenceData.bpm;
        noteManager.SetSpeed(playbackSpeed);

        notesGenerated = false;                 //새로운 데이터를 로드 했으므로 노트 재생성 허용
        GenerateNotes();
        noteManager.Initialize();
    }

}