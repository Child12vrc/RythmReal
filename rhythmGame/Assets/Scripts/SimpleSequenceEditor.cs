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
    private float trackWidth = 100;
    private int totalBeats;
    private bool isPlaying = false;
    private int currentBeatTime = 0;
    private int playFromBeat = 0;
    private float startTime = 0;
    private AudioSource audioSource;
    private Rect gridRect;

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
            sequenceData.trackNotes = new List<List<int>>();

        while (sequenceData.trackNotes.Count < sequenceData.numberOfTracks)
            sequenceData.trackNotes.Add(new List<int>());

        foreach (var track in sequenceData.trackNotes)
            while (track.Count < totalBeats)
                track.Add((int)Enums.NoteType.None);

        if (sequenceData.effectTrack == null)
            sequenceData.effectTrack = new List<int>();

        while (sequenceData.effectTrack.Count < totalBeats)
            sequenceData.effectTrack.Add(0);

        if (audioSource != null)
            audioSource.clip = sequenceData.audioClip;
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
            currentBeatTime = Mathf.FloorToInt(audioSource.time * sequenceData.bpm / 60f);
            if (currentBeatTime >= totalBeats)
                StopPlayBack();
            Repaint();
        }
    }

    private void OnGUI()
    {
        DrawSettingsPanel();
        if (sequenceData == null) return;

        DrawSequencer();
        HandleTooltip();
    }

    private void DrawSettingsPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        EditorGUILayout.LabelField("시퀀스 데이터 설정", EditorStyles.boldLabel);

        sequenceData = (SequenceData)EditorGUILayout.ObjectField(
            "시퀀스 데이터", sequenceData, typeof(SequenceData), false);

        if (sequenceData != null)
        {
            InitializeTracks();
            DrawSequenceSettings();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawSequenceSettings()
    {
        EditorGUILayout.LabelField("BPM", sequenceData.bpm.ToString());
        EditorGUILayout.LabelField("오디오 클립",
            sequenceData.audioClip != null ? sequenceData.name : "Note");

        EditorGUILayout.LabelField("트랙 수 설정", EditorStyles.boldLabel);
        int newNumberOfTracks = EditorGUILayout.IntField("트랙 수", sequenceData.numberOfTracks);

        if (newNumberOfTracks != sequenceData.numberOfTracks)
        {
            sequenceData.numberOfTracks = Mathf.Max(1, newNumberOfTracks);
            InitializeTracks();
        }

        totalBeats = sequenceData.audioClip != null ?
            Mathf.FloorToInt((sequenceData.audioClip.length / 60f) * sequenceData.bpm) : 0;

        DrawPlaybackControls();
    }

    private void DrawPlaybackControls()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(isPlaying ? "일시 정지" : "재생"))
        {
            if (isPlaying) PausePlayBack();
            else StartPlayBack(currentBeatTime);
        }
        if (GUILayout.Button("처음부터 재생"))
            StartPlayBack(0);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("특정 비트부터 재생", EditorStyles.boldLabel);
        playFromBeat = EditorGUILayout.IntSlider("비트 인덱스", playFromBeat, 0, totalBeats - 1);
        if (GUILayout.Button("해당 비트부터 재생"))
            StartPlayBack(playFromBeat);
    }

    private void DrawSequencer()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        float totalWidth = (sequenceData.numberOfTracks + 2) * trackWidth;
        float totalHeight = (totalBeats + 1) * beatHedight;
        gridRect = GUILayoutUtility.GetRect(totalWidth, totalHeight);

        DrawTimelineVertical();
        DrawTracksVertical();
        DrawEffectTrackVertical();

        EditorGUILayout.EndScrollView();
    }

    private void DrawTimelineVertical()
    {
        for (int j = 0; j < totalBeats; j++)
        {
            float yPos = (j + 1) * beatHedight;
            Rect timeRect = new Rect(0, gridRect.y + yPos, trackWidth, beatHedight);

            if (j % 4 == 0)
                EditorGUI.DrawRect(timeRect, new Color(1f, 1f, 0f, 0.2f));

            float beatTime = j * 60 / sequenceData.bpm;
            int minutes = Mathf.FloorToInt(beatTime / 60f);
            int seconds = Mathf.FloorToInt(beatTime % 60f);
            EditorGUI.LabelField(timeRect, $"{minutes:00}:{seconds:00}");
        }
    }

    private void DrawTracksVertical()
    {
        for (int i = 0; i < sequenceData.numberOfTracks; i++)
        {
            float xPos = (i + 1) * trackWidth;
            EditorGUI.LabelField(new Rect(xPos, gridRect.y, trackWidth, beatHedight), $"Track {i + 1}");

            for (int j = 0; j < totalBeats; j++)
                DrawBeatVertical(i, j);
        }
    }

    private void DrawBeatVertical(int trackIndex, int beatIndex)
    {
        float xPos = (trackIndex + 1) * trackWidth;
        float yPos = gridRect.y + ((beatIndex + 1) * beatHedight);
        Rect rect = new Rect(xPos, yPos, trackWidth, beatHedight);

        bool isCurrentBeat = currentBeatTime == beatIndex;
        Enums.NoteType noteValue = (sequenceData.trackNotes[trackIndex].Count > beatIndex) ?
            (Enums.NoteType)sequenceData.trackNotes[trackIndex][beatIndex] : Enums.NoteType.None;

        Color color = GetNoteColor(isCurrentBeat, noteValue);
        EditorGUI.DrawRect(rect, color);
        HandleBeatInput(rect, trackIndex, beatIndex, noteValue);
    }

    private Color GetNoteColor(bool isCurrentBeat, Enums.NoteType noteValue)
    {
        if (isCurrentBeat) return Color.cyan;

        switch (noteValue)
        {
            case Enums.NoteType.SingleNote: return Color.green;
            case Enums.NoteType.LongNoteStart: return Color.yellow;
            case Enums.NoteType.LongNoteEnd: return Color.red;
            case Enums.NoteType.Effect1: return Color.blue;
            case Enums.NoteType.Effect2: return Color.magenta;
            case Enums.NoteType.CameraEffect3: return Color.cyan;
            default: return Color.gray;
        }
    }

    private void HandleBeatInput(Rect rect, int trackIndex, int beatIndex, Enums.NoteType noteValue)
    {
        if (!rect.Contains(Event.current.mousePosition)) return;

        GUI.tooltip = noteValue.ToString();

        if (Event.current.type != EventType.MouseDown) return;

        if (Event.current.button == 0)
        {
            int maxEnumValue = (trackIndex == 4) ?
                Enum.GetValues(typeof(Enums.NoteType)).Length : 4;

            noteValue = (Enums.NoteType)(((int)noteValue + 1) % maxEnumValue);

            while (sequenceData.trackNotes[trackIndex].Count <= beatIndex)
                sequenceData.trackNotes[trackIndex].Add((int)Enums.NoteType.None);

            sequenceData.trackNotes[trackIndex][beatIndex] = (int)noteValue;
            EditorUtility.SetDirty(sequenceData);
        }
        else if (Event.current.button == 1 && sequenceData.trackNotes[trackIndex].Count > beatIndex)
        {
            sequenceData.trackNotes[trackIndex][beatIndex] = (int)Enums.NoteType.None;
            EditorUtility.SetDirty(sequenceData);
        }

        Event.current.Use();
    }

    private void DrawEffectTrackVertical()
    {
        float xPos = (sequenceData.numberOfTracks + 1) * trackWidth;
        EditorGUI.LabelField(new Rect(xPos, gridRect.y, trackWidth, beatHedight), "Effect");

        for (int j = 0; j < totalBeats; j++)
        {
            float yPos = gridRect.y + ((j + 1) * beatHedight);
            Rect rect = new Rect(xPos, yPos, trackWidth, beatHedight);

            bool isCurrentBeat = currentBeatTime == j;
            int effectValue = (sequenceData.effectTrack.Count > j) ? sequenceData.effectTrack[j] : 0;

            Color color = isCurrentBeat ? Color.cyan :
                         effectValue > 0 ? new Color(0.8f, 0.9f, 1f) : Color.gray;

            EditorGUI.DrawRect(rect, color);

            if (effectValue > 0)
            {
                var numberStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                numberStyle.normal.textColor = new Color(0.2f, 0.2f, 1f);
                EditorGUI.LabelField(rect, effectValue.ToString(), numberStyle);
            }

            HandleEffectInput(rect, j, effectValue);
        }
    }

    private void HandleEffectInput(Rect rect, int beatIndex, int effectValue)
    {
        if (!rect.Contains(Event.current.mousePosition)) return;

        GUI.tooltip = $"Effect: {effectValue}";

        if (Event.current.type == EventType.MouseDown)
        {
            if (Event.current.button == 0)
            {
                int nextEffect = (effectValue + 1) % 11;
                while (sequenceData.effectTrack.Count <= beatIndex)
                    sequenceData.effectTrack.Add(0);

                sequenceData.effectTrack[beatIndex] = nextEffect;
                EditorUtility.SetDirty(sequenceData);
            }
            else if (Event.current.button == 1 && sequenceData.effectTrack.Count > beatIndex)
            {
                sequenceData.effectTrack[beatIndex] = 0;
                EditorUtility.SetDirty(sequenceData);
            }

            Event.current.Use();
            Repaint();
        }
    }

    private void HandleTooltip()
    {
        if (string.IsNullOrEmpty(GUI.tooltip)) return;

        Vector2 mousePosition = Event.current.mousePosition;
        GUIStyle style = new GUIStyle
        {
            normal = { textColor = Color.white },
            fontSize = 12
        };

        GUI.Label(new Rect(mousePosition.x + 15, mousePosition.y, 200, 20),
            GUI.tooltip, style);
    }

    private void StartPlayBack(int fromBeat)
    {
        if (sequenceData == null || sequenceData.audioClip == null ||
            audioSource == null) return;

        isPlaying = true;
        currentBeatTime = fromBeat;
        playFromBeat = fromBeat;

        if (audioSource.clip != sequenceData.audioClip)
            audioSource.clip = sequenceData.audioClip;

        float startTime = fromBeat * 60f / sequenceData.bpm;
        audioSource.time = startTime;
        audioSource.Play();

        this.startTime = (float)EditorApplication.timeSinceStartup - startTime;
    }

    private void PausePlayBack()
    {
        isPlaying = false;
        audioSource?.Pause();
    }

    private void StopPlayBack()
    {
        isPlaying = false;
        audioSource?.Stop();
    }
}
#endif