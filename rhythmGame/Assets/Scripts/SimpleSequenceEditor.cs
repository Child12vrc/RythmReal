using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SimpleSequenceEditor : EditorWindow
{
    private SequenceData sequenceData;      //시퀀스 데이터 저장 해제
    private Vector2 scrollPos;              //스크롤 뷰 위치
    private float beatHedight = 20;         //각 비트의 높이
    private float trackWidth = 50;          //각 트랙의 너비
    private int totalBeats;                 //총 비트 수
    private bool isPlaying = false;         //시은ㄷ, 제셍 야브
    private int currentBeatTime = 0;        //현재 재생 중인 비트 시간
    private int playFromBeat = 0;           //재생을 시작할 비트
    private float startTime = 0;            //재생 시작 시간
    private AudioSource audioSource;        //오디오 재생을 위한 오디오 소스

    [MenuItem("Tool/Simple Sequence Editor")]

    private static void ShowWindow()
    {
        var window = GetWindow<SimpleSequenceEditor>();
        window.titleContent = new GUIContent("Simple Sequencer");
        window.Show();
    }

    private void OnEnable()                     // 윈도우 에디터 창을 열었을 때
    {
        EditorApplication.update += Update;     //업데이트 함수를 이벤트로 등록
        CreateAudioSource();                    //오디오를 사용할 것이기 때문에 오디오 등록을 해준다.
    }

    private void OnDisable()                    //윈도우 에디터 창을 닫았을 때
    {
        EditorApplication.update -= Update;     //업데이트 함수를 이벤트 해제
        if (audioSource != null)                         //윈도우 에디터 창을 닫았을 때 오디오 소스가 살아 있을 경우
        {
            DestroyImmediate(audioSource.gameObject);   //등록된 오디오 소스 오브젝트를 파괴 한다.
            audioSource = null;
        }
    }

    private void CreateAudioSource()            //유니티 Scene에서 처럼 오디오 소스를 사용 하기 위해서 보이지 않게 등록
    {
        var audioSourceGameObject = new GameObject("EditorAudioSource");
        audioSourceGameObject.hideFlags = HideFlags.HideAndDontSave;
        audioSource = audioSourceGameObject.AddComponent<AudioSource>();
    }

    private void InitializeTracks()             //시퀀스 데이터가 들어왔을 때 데이터를 셋팅해주는 함수
    {
        if (sequenceData == null) return;       //데이터가 없을 경우 그냥 리턴

        if (sequenceData.trackNotes == null)     //데이터가 있는데 트렉 노트가 없을 경우 자료형을 생성 시켜준다.
        {
            sequenceData.trackNotes = new List<List<int>>();
        }

        while (sequenceData.trackNotes.Count < sequenceData.numberOfTracks)      //트랙 수 만큼 List<int>()를 넣어준다.
        {
            sequenceData.trackNotes.Add(new List<int>());
        }

        foreach (var track in sequenceData.trackNotes)           //각 트랙 노트의 비트 수만큼 0 값으로 데이터를 넣어준다.
        {
            while (track.Count < totalBeats)
            {
                track.Add(0);
            }
        }

        if (audioSource != null)                                //가져온 시퀀스 데이터의 오디오 클립이 있을 경우 오디오 데이터를 할당한다.
        {
            audioSource.clip = sequenceData.audioClip;
        }
    }

    private void Update()
    {
        if (this == null)            //이 스크립트가 파괴 되었는지 확인
        {
            EditorApplication.update -= Update;     //이벤트 할당을 해제해준다.
            return;
        }

        if (isPlaying && audioSource != null && audioSource.isPlaying)
        {
            float elapseTime = audioSource.time;                                        //오디오 시간을 가져온다.
            currentBeatTime = Mathf.FloorToInt(elapseTime * sequenceData.bpm / 60f);    //BPM분의 60에 흐른 시간을 곱하면 현재 비트 이다.

            if (currentBeatTime >= totalBeats)
            {
                StopPlayBack();
            }
            Repaint();                          //업데이트가 끝난 후에 리페인트 ( 화면 갱신을 해주지 않는 업데이트이기 때문에)
        }
    }

    private void StartPlayBack(int fromBeat)                //오디오를 특정 비트부터 플레이 해주는 함수 
    {
        if (sequenceData == null || sequenceData.audioClip == null || audioSource == null) return;   //세 가지중 하나라도 없으면 리턴

        isPlaying = true;
        currentBeatTime = fromBeat;
        playFromBeat = fromBeat;

        if (audioSource.clip != sequenceData.audioClip)          //지금 오브젝트 클립이 받아온 시퀀스 데이터 오디오 클립과 다르다면
        {
            audioSource.clip = sequenceData.audioClip;          //지금 시퀀스 데이터 클립으로 변경 시켜준다.
        }

        float startTime = fromBeat * 60f / sequenceData.bpm;    //시작 시간을 비트와 BPM으로 부터 계산하여 추정한다.
        audioSource.time = startTime;                           //오디오 시간을 추정한 시간으로 변경
        audioSource.Play();

        this.startTime = (float)EditorApplication.timeSinceStartup - startTime;     //에디터에 시간을 반영한다.
        EditorApplication.update += Update;                                         //업데이트 이벤트를 등록
    }

    private void PausePlayBack()
    {
        isPlaying = false;
        if (audioSource != null) audioSource.Pause();
    }
    private void StopPlayBack()
    {
        isPlaying = false;
        if (audioSource != null) audioSource.Stop();
        EditorApplication.update -= Update;                     //업데이트 이벤트 해제
    }

    private void DrawBeat(int trackIndex, int beatIndex)
    {
        if (sequenceData == null || sequenceData.trackNotes == null || trackIndex >= sequenceData.trackNotes.Count) return;

        Rect rect = GUILayoutUtility.GetRect(trackWidth, beatHedight);
        bool isCurrentBeat = currentBeatTime == beatIndex;  //currentBeatTime 과 beatIndex가 동일하면 true 아니면 false
        //현재 트랙의 현재 비트 인덱스에 해당하는 노트 값을 가져오면서 검사해서 유효한 인덱스가 아닐 경우 0으로 설정 된다.
        int noteValue = (sequenceData.trackNotes[trackIndex].Count > beatIndex) ? sequenceData.trackNotes[trackIndex][beatIndex] : 0;

        Color color = Color.gray;
        if (isCurrentBeat) color = Color.cyan;
        else
        {
            switch (noteValue)
            {
                case 1: color = Color.green; break;
                case 2: color = Color.yellow; break;
                case 3: color = Color.red; break;
                case 4: color = Color.blue; break;
            }
        }

        EditorGUI.DrawRect(rect, color);

        //에디터 윈도우에서는 마우스 클릭 이벤트를 아래와 같은 형태로 받아 온다.
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            if (Event.current.button == 0)           //마우스 왼쪽 클릭
            {
                noteValue = (noteValue + 1) % 5;
                while (sequenceData.trackNotes[trackIndex].Count <= beatIndex)
                {
                    sequenceData.trackNotes[trackIndex].Add(0);
                }
                sequenceData.trackNotes[trackIndex][beatIndex] = noteValue;
            }
            else if (Event.current.button == 1)     //마우스 오른쪽 클릭
            {
                if (sequenceData.trackNotes[trackIndex].Count > beatIndex)
                {
                    sequenceData.trackNotes[trackIndex][beatIndex] = 0;
                }
            }
            Event.current.Use();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(200));                        //GUI 레이아웃 시작 설정
        EditorGUILayout.LabelField("시퀀스 데이터 설정", EditorStyles.boldLabel);   //폰트

        sequenceData = (SequenceData)EditorGUILayout.ObjectField("시퀀스 데이터", sequenceData, typeof(SequenceData), false);

        if (sequenceData == null)
        {
            EditorGUILayout.EndVertical();                                          //GUI 끝
            return;
        }

        InitializeTracks();

        EditorGUILayout.LabelField("BPM", sequenceData.bpm.ToString());
        EditorGUILayout.LabelField("오디오 클립", sequenceData.audioClip != null ? sequenceData.name : "Note");

        EditorGUILayout.LabelField("트랙 수 설정", EditorStyles.boldLabel);
        int newNumberOfTracks = EditorGUILayout.IntField("트랙 수", sequenceData.numberOfTracks);
        if (newNumberOfTracks != sequenceData.numberOfTracks)        //트랙 수가 다를 경우
        {
            sequenceData.numberOfTracks = newNumberOfTracks;        //트랙 수를 가져와서 갱신
            InitializeTracks();
        }

        if (sequenceData.numberOfTracks < 1) sequenceData.numberOfTracks = 1;       //트랙이 1 이하로 내려가면 최소 트랙은 1

        if (sequenceData.audioClip != null)
        {
            totalBeats = Mathf.FloorToInt((sequenceData.audioClip.length / 60f) * sequenceData.bpm);    //비트 수를 계산한다.
        }
        else
        {
            totalBeats = 0;
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(isPlaying ? "일시 정지" : "재생"))
        {
            if (isPlaying) PausePlayBack();
            else StartPlayBack(currentBeatTime);
        }
        if (GUILayout.Button("처음 부터 재생"))
        {
            StartPlayBack(0);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("특정 비트부터 재생", EditorStyles.boldLabel);
        playFromBeat = EditorGUILayout.IntSlider("비트 인덱스", playFromBeat, 0, totalBeats - 1);
        if (GUILayout.Button("해당 비트부터 재생"))
        {
            StartPlayBack(playFromBeat);
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);    //UI 사이 공백 10
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 210));
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(100));
        GUILayout.Space(beatHedight);

        for (int j = 0; j < totalBeats; j++)
        {
            float beatTime = j * 60 / sequenceData.bpm;
            int minutes = Mathf.FloorToInt(beatTime / 60f);
            int seconds = Mathf.FloorToInt(beatTime % 60f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{minutes:00}:{seconds:00}", GUILayout.Width(50));
            EditorGUILayout.LabelField($"{j}", GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(beatHedight - EditorGUIUtility.singleLineHeight);
        }

        EditorGUILayout.EndVertical();

        for (int i = 0; i < sequenceData.numberOfTracks; i++)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label($"트랙 {i + 1}", GUILayout.Width(trackWidth));
            for (int j = 0; j < totalBeats; j++)
            {
                DrawBeat(i, j);
            }
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("데이터 저장"))
        {
            sequenceData.SaveToJson();
        }

        if (GUILayout.Button("데이터 불러오기"))
        {
            sequenceData.LoadFromJson();
            InitializeTracks();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }
}
