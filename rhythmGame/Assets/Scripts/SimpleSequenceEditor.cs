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

        if (audioSource != null)
        {
            audioSource.clip = sequenceData.audioClip;
        }
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

        // 색상 그리기
        EditorGUI.DrawRect(rect, color);

        // 마우스가 색상 영역 위에 있을 때 툴팁 설정
        if (rect.Contains(Event.current.mousePosition))
        {
            GUI.tooltip = noteValue.ToString();  // enum 값 이름을 툴팁으로 설정
            Repaint();  // 화면 갱신
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

        GUILayout.Space(10);
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

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();

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
