#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RhythmGame;

public class SimpleSequenceEditor : EditorWindow
{
    private SequenceData sequenceData;
    private Vector2 scrollPos;
    private float beatHedight = 20;
    private float trackWidth = 50;
    private int totalBeats;
    private bool isPlaying = false;
    private int currentBeatTime = 0;
    private int playFromBeat = 0;
    private float startTime = 0;
    private AudioSource audioSource;

    [MenuItem("Tool/Simple Sequence Editor")]
    private static void ShowWindow()
    {
        var window = GetWindow<SimpleSequenceEditor>();
        window.titleContent = new GUIContent("Simple Sequencer");
        window.Show();
    }

    private void OnEnable()
    {
        EditorApplication.update += Update;
        CreateAudioSource();
    }

    private void OnDisable()
    {
        EditorApplication.update -= Update;
        if (audioSource != null)
        {
            DestroyImmediate(audioSource.gameObject);
            audioSource = null;
        }
    }

    private void CreateAudioSource()
    {
        var audioSourceGameObject = new GameObject("EditorAudioSource");
        audioSourceGameObject.hideFlags = HideFlags.HideAndDontSave;
        audioSource = audioSourceGameObject.AddComponent<AudioSource>();
    }

    private void InitializeTracks()
    {
        if (sequenceData == null) return;

        if (sequenceData.trackNotes == null)
        {
            sequenceData.trackNotes = new List<List<int>>();
        }

        while (sequenceData.trackNotes.Count < sequenceData.numberOfTracks)
        {
            sequenceData.trackNotes.Add(new List<int>());
        }

        foreach (var track in sequenceData.trackNotes)
        {
            while (track.Count < totalBeats)
            {
                track.Add((int)Enums.NoteType.None);
            }
        }

        // 이펙트 트랙 초기화
        if (sequenceData.effectTrack == null)
        {
            sequenceData.effectTrack = new List<int>();
        }

        while (sequenceData.effectTrack.Count < totalBeats)
        {
            sequenceData.effectTrack.Add(0);  // 0은 이펙트 없음을 의미
        }

        if (audioSource != null)
        {
            audioSource.clip = sequenceData.audioClip;
        }
    }

    private void DrawEffectTrack()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Label("이펙트 트랙", GUILayout.Width(trackWidth));

        for (int j = 0; j < totalBeats; j++)
        {
            Rect rect = GUILayoutUtility.GetRect(trackWidth, beatHedight);
            bool isCurrentBeat = currentBeatTime == j;
            int effectValue = (sequenceData.effectTrack.Count > j) ? sequenceData.effectTrack[j] : 0;

            // 이펙트 값에 따른 색상 설정
            Color color = Color.gray;
            if (isCurrentBeat)
            {
                color = Color.cyan;
            }
            else if (effectValue > 0)
            {
                // 이펙트 값이 있으면 기본 하늘색 배경에 진한 파란색 텍스트
                color = new Color(0.8f, 0.9f, 1f); // 연한 하늘색
            }

            // 배경 그리기
            EditorGUI.DrawRect(rect, color);

            // 이펙트 번호 표시 (0 제외)
            if (effectValue > 0)
            {
                GUIStyle numberStyle = new GUIStyle(EditorStyles.boldLabel);
                numberStyle.alignment = TextAnchor.MiddleCenter;
                numberStyle.normal.textColor = new Color(0.2f, 0.2f, 1f); // 진한 파란색
                EditorGUI.LabelField(rect, effectValue.ToString(), numberStyle);
            }

            // 마우스 이벤트 처리
            if (rect.Contains(Event.current.mousePosition))
            {
                // 툴팁 표시
                GUI.tooltip = $"Effect: {effectValue}";

                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)  // 좌클릭
                    {
                        // 다음 이펙트 값으로 순환 (0->1->2->...->10->0)
                        int nextEffect = (effectValue + 1) % 11;  // 0부터 10까지

                        while (sequenceData.effectTrack.Count <= j)
                        {
                            sequenceData.effectTrack.Add(0);
                        }
                        sequenceData.effectTrack[j] = nextEffect;

                        // 값이 변경되었음을 Unity에 알림
                        EditorUtility.SetDirty(sequenceData);
                    }
                    else if (Event.current.button == 1)  // 우클릭
                    {
                        if (sequenceData.effectTrack.Count > j)
                        {
                            sequenceData.effectTrack[j] = 0;  // 이펙트 제거
                            EditorUtility.SetDirty(sequenceData);
                        }
                    }
                    Event.current.Use();
                    Repaint(); // UI 즉시 업데이트
                }
            }
        }

        EditorGUILayout.EndVertical();
    }


    private void Update()
    {
        if (this == null)
        {
            EditorApplication.update -= Update;
            return;
        }

        if (isPlaying && audioSource != null && audioSource.isPlaying)
        {
            float elapseTime = audioSource.time;
            currentBeatTime = Mathf.FloorToInt(elapseTime * sequenceData.bpm / 60f);

            if (currentBeatTime >= totalBeats)
            {
                StopPlayBack();
            }
            Repaint();
        }
    }

    private void StartPlayBack(int fromBeat)
    {
        if (sequenceData == null || sequenceData.audioClip == null || audioSource == null) return;

        isPlaying = true;
        currentBeatTime = fromBeat;
        playFromBeat = fromBeat;

        if (audioSource.clip != sequenceData.audioClip)
        {
            audioSource.clip = sequenceData.audioClip;
        }

        float startTime = fromBeat * 60f / sequenceData.bpm;
        audioSource.time = startTime;
        audioSource.Play();

        this.startTime = (float)EditorApplication.timeSinceStartup - startTime;
        EditorApplication.update += Update;
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
        EditorApplication.update -= Update;
    }

    private void DrawBeat(int trackIndex, int beatIndex)
    {
        if (sequenceData == null || sequenceData.trackNotes == null || trackIndex >= sequenceData.trackNotes.Count) return;

        Rect rect = GUILayoutUtility.GetRect(trackWidth, beatHedight);
        bool isCurrentBeat = currentBeatTime == beatIndex;
        Enums.NoteType noteValue = (sequenceData.trackNotes[trackIndex].Count > beatIndex) ? (Enums.NoteType)sequenceData.trackNotes[trackIndex][beatIndex] : Enums.NoteType.None;

        Color color = Color.gray;
        if (isCurrentBeat) color = Color.cyan;
        else
        {
            switch (noteValue)
            {
                case Enums.NoteType.SingleNote: color = Color.green; break;
                case Enums.NoteType.LongNoteStart: color = Color.yellow; break;
                case Enums.NoteType.LongNoteEnd: color = Color.red; break;
                case Enums.NoteType.Effect1: color = Color.blue; break;
                case Enums.NoteType.Effect2: color = Color.magenta; break;
                case Enums.NoteType.CameraEffect3: color = Color.cyan; break;
            }
        }

        EditorGUI.DrawRect(rect, color);

        if (rect.Contains(Event.current.mousePosition))
        {
            GUI.tooltip = noteValue.ToString();
            Repaint();
        }

        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            if (Event.current.button == 0)
            {
                int maxEnumValue = (trackIndex == 4) ? Enum.GetValues(typeof(Enums.NoteType)).Length : 4;

                noteValue = (Enums.NoteType)(((int)noteValue + 1) % maxEnumValue);

                while (sequenceData.trackNotes[trackIndex].Count <= beatIndex)
                {
                    sequenceData.trackNotes[trackIndex].Add((int)Enums.NoteType.None);
                }
                sequenceData.trackNotes[trackIndex][beatIndex] = (int)noteValue;
            }
            else if (Event.current.button == 1)
            {
                if (sequenceData.trackNotes[trackIndex].Count > beatIndex)
                {
                    sequenceData.trackNotes[trackIndex][beatIndex] = (int)Enums.NoteType.None;
                }
            }
            Event.current.Use();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        EditorGUILayout.LabelField("시퀀스 데이터 설정", EditorStyles.boldLabel);

        sequenceData = (SequenceData)EditorGUILayout.ObjectField("시퀀스 데이터", sequenceData, typeof(SequenceData), false);

        if (sequenceData == null)
        {
            EditorGUILayout.EndVertical();
            return;
        }

        InitializeTracks();

        EditorGUILayout.LabelField("BPM", sequenceData.bpm.ToString());
        EditorGUILayout.LabelField("오디오 클립", sequenceData.audioClip != null ? sequenceData.name : "Note");

        EditorGUILayout.LabelField("트랙 수 설정", EditorStyles.boldLabel);
        int newNumberOfTracks = EditorGUILayout.IntField("트랙 수", sequenceData.numberOfTracks);
        if (newNumberOfTracks != sequenceData.numberOfTracks)
        {
            sequenceData.numberOfTracks = newNumberOfTracks;
            InitializeTracks();
        }

        if (sequenceData.numberOfTracks < 1) sequenceData.numberOfTracks = 1;

        if (sequenceData.audioClip != null)
        {
            totalBeats = Mathf.FloorToInt((sequenceData.audioClip.length / 60f) * sequenceData.bpm);
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

        // 시퀀서 영역
        GUILayout.Space(10);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 210));
        EditorGUILayout.BeginHorizontal();

        // 타임라인 표시
        EditorGUILayout.BeginVertical(GUILayout.Width(100));
        GUILayout.Space(beatHedight);

        for (int j = 0; j < totalBeats; j++)
        {
            float beatTime = j * 60 / sequenceData.bpm;
            int minutes = Mathf.FloorToInt(beatTime / 60f);
            int seconds = Mathf.FloorToInt(beatTime % 60f);
            EditorGUILayout.BeginHorizontal();

            // 4박자마다 다른 색상으로 표시
            if (j % 4 == 0)
            {
                GUI.color = Color.yellow;  // 정박 표시
            }
            else
            {
                GUI.color = Color.white;   // 일반 박자
            }

            EditorGUILayout.LabelField($"{minutes:00}:{seconds:00}", GUILayout.Width(50));
            EditorGUILayout.LabelField($"{j}", GUILayout.Width(30));
            GUI.color = Color.white;  // 색상 초기화

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(beatHedight - EditorGUIUtility.singleLineHeight);
        }

        EditorGUILayout.EndVertical();

        // 노트 트랙 표시
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

        // 이펙트 트랙 표시
        DrawEffectTrack();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();

        // 툴팁 표시
        if (!string.IsNullOrEmpty(GUI.tooltip))
        {
            Vector2 mousePosition = Event.current.mousePosition;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;

            GUI.Label(new Rect(mousePosition.x + 15, mousePosition.y, 200, 20), GUI.tooltip, style);
        }
    }

}
#endif