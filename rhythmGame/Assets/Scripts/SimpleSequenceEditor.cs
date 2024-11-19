#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RhythmGame;

public class SimpleSequenceEditor : EditorWindow
{
    private SequenceData sequenceData;
    private Vector2 scrollPos;
    private float beatHeight = 20;       // 비트 높이
    private float trackWidth = 50;       // 트랙 너비
    private int beatsPerBar = 4;         // 1박자당 4비트
    private int totalBeats;
    private bool isPlaying = false;
    private int currentBeatTime = 0;
    private int playFromBeat = 0;
    private AudioSource audioSource;

    private bool isDragging = false;     // 드래그 상태
    private int dragStartTrack = -1;     // 드래그 시작 트랙
    private int dragStartBeat = -1;      // 드래그 시작 비트
    private int dragEndBeat = -1;        // 드래그 종료 비트

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

    private void DrawBeat(int trackIndex, int beatIndex)
    {
        if (sequenceData == null || sequenceData.trackNotes == null || trackIndex >= sequenceData.trackNotes.Count) return;

        Rect rect = GUILayoutUtility.GetRect(trackWidth, beatHeight);
        bool isCurrentBeat = currentBeatTime == beatIndex;
        Enums.NoteType noteValue = (Enums.NoteType)sequenceData.trackNotes[trackIndex][beatIndex];

        // 비트 색상 설정
        Color color = GetNoteColor(noteValue);

        // 현재 비트 색상 표시
        EditorGUI.DrawRect(rect, color);

        // 1박자 구분 선 표시
        if (beatIndex % beatsPerBar == 0)
        {
            Handles.color = Color.black;
            Handles.DrawLine(new Vector3(rect.x, rect.y, 0), new Vector3(rect.xMax, rect.y, 0));
        }

        // 롱노트 드래그 영역 표시
        if (isDragging && dragStartTrack == trackIndex && beatIndex >= dragStartBeat && beatIndex <= dragEndBeat)
        {
            EditorGUI.DrawRect(rect, new Color(1f, 1f, 0f, 0.5f));
        }

        // 마우스 이벤트 처리
        if (rect.Contains(Event.current.mousePosition))
        {
            GUI.tooltip = noteValue.ToString();

            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0) // 좌클릭
                {
                    StartLongNoteDrag(trackIndex, beatIndex);
                }
                else if (Event.current.button == 1) // 우클릭
                {
                    RemoveNoteAt(trackIndex, beatIndex);
                }

                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                EndLongNoteDrag(trackIndex, beatIndex);
                Event.current.Use();
            }
        }
    }

    private void StartLongNoteDrag(int trackIndex, int beatIndex)
    {
        if (sequenceData.trackNotes[trackIndex][beatIndex] == (int)Enums.NoteType.None)
        {
            isDragging = true;
            dragStartTrack = trackIndex;
            dragStartBeat = beatIndex;
            dragEndBeat = beatIndex;

            sequenceData.trackNotes[trackIndex][beatIndex] = (int)Enums.NoteType.LongNoteStart;
            EditorUtility.SetDirty(sequenceData);
        }
    }

    private void EndLongNoteDrag(int trackIndex, int beatIndex)
    {
        if (isDragging && dragStartTrack == trackIndex)
        {
            isDragging = false;
            dragEndBeat = Mathf.Max(dragStartBeat, beatIndex);

            for (int i = dragStartBeat + 1; i <= dragEndBeat; i++)
            {
                sequenceData.trackNotes[trackIndex][i] = (int)Enums.NoteType.LongNoteEnd;
            }

            EditorUtility.SetDirty(sequenceData);
        }
    }

    private void RemoveNoteAt(int trackIndex, int beatIndex)
    {
        if (sequenceData.trackNotes[trackIndex][beatIndex] == (int)Enums.NoteType.LongNoteStart)
        {
            int endBeat = FindLongNoteEnd(trackIndex, beatIndex);
            for (int i = beatIndex; i <= endBeat; i++)
            {
                sequenceData.trackNotes[trackIndex][i] = (int)Enums.NoteType.None;
            }
        }
        else if (sequenceData.trackNotes[trackIndex][beatIndex] == (int)Enums.NoteType.SingleNote)
        {
            sequenceData.trackNotes[trackIndex][beatIndex] = (int)Enums.NoteType.None;
        }

        EditorUtility.SetDirty(sequenceData);
    }

    private int FindLongNoteEnd(int trackIndex, int startBeat)
    {
        for (int i = startBeat + 1; i < sequenceData.trackNotes[trackIndex].Count; i++)
        {
            if ((Enums.NoteType)sequenceData.trackNotes[trackIndex][i] == Enums.NoteType.LongNoteEnd)
            {
                return i;
            }
        }
        return startBeat;
    }

    private Color GetNoteColor(Enums.NoteType noteValue)
    {
        switch (noteValue)
        {
            case Enums.NoteType.SingleNote: return Color.green;
            case Enums.NoteType.LongNoteStart: return Color.yellow;
            case Enums.NoteType.LongNoteEnd: return Color.red;
            default: return Color.gray;
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

        if (sequenceData.audioClip != null)
        {
            totalBeats = Mathf.FloorToInt((sequenceData.audioClip.length / 60f) * sequenceData.bpm) * beatsPerBar;
        }
        else
        {
            totalBeats = 0;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 210));
        EditorGUILayout.BeginHorizontal();

        // 트랙별 비트 그리기
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
    }
}
#endif
