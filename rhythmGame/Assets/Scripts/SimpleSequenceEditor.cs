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

        // ����Ʈ Ʈ�� �ʱ�ȭ
        if (sequenceData.effectTrack == null)
        {
            sequenceData.effectTrack = new List<int>();
        }

        while (sequenceData.effectTrack.Count < totalBeats)
        {
            sequenceData.effectTrack.Add(0);  // 0�� ����Ʈ ������ �ǹ�
        }

        if (audioSource != null)
        {
            audioSource.clip = sequenceData.audioClip;
        }
    }

    private void DrawEffectTrack()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Label("����Ʈ Ʈ��", GUILayout.Width(trackWidth));

        for (int j = 0; j < totalBeats; j++)
        {
            Rect rect = GUILayoutUtility.GetRect(trackWidth, beatHedight);
            bool isCurrentBeat = currentBeatTime == j;
            int effectValue = (sequenceData.effectTrack.Count > j) ? sequenceData.effectTrack[j] : 0;

            // ����Ʈ ���� ���� ���� ����
            Color color = Color.gray;
            if (isCurrentBeat)
            {
                color = Color.cyan;
            }
            else if (effectValue > 0)
            {
                // ����Ʈ ���� ������ �⺻ �ϴû� ��濡 ���� �Ķ��� �ؽ�Ʈ
                color = new Color(0.8f, 0.9f, 1f); // ���� �ϴû�
            }

            // ��� �׸���
            EditorGUI.DrawRect(rect, color);

            // ����Ʈ ��ȣ ǥ�� (0 ����)
            if (effectValue > 0)
            {
                GUIStyle numberStyle = new GUIStyle(EditorStyles.boldLabel);
                numberStyle.alignment = TextAnchor.MiddleCenter;
                numberStyle.normal.textColor = new Color(0.2f, 0.2f, 1f); // ���� �Ķ���
                EditorGUI.LabelField(rect, effectValue.ToString(), numberStyle);
            }

            // ���콺 �̺�Ʈ ó��
            if (rect.Contains(Event.current.mousePosition))
            {
                // ���� ǥ��
                GUI.tooltip = $"Effect: {effectValue}";

                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)  // ��Ŭ��
                    {
                        // ���� ����Ʈ ������ ��ȯ (0->1->2->...->10->0)
                        int nextEffect = (effectValue + 1) % 11;  // 0���� 10����

                        while (sequenceData.effectTrack.Count <= j)
                        {
                            sequenceData.effectTrack.Add(0);
                        }
                        sequenceData.effectTrack[j] = nextEffect;

                        // ���� ����Ǿ����� Unity�� �˸�
                        EditorUtility.SetDirty(sequenceData);
                    }
                    else if (Event.current.button == 1)  // ��Ŭ��
                    {
                        if (sequenceData.effectTrack.Count > j)
                        {
                            sequenceData.effectTrack[j] = 0;  // ����Ʈ ����
                            EditorUtility.SetDirty(sequenceData);
                        }
                    }
                    Event.current.Use();
                    Repaint(); // UI ��� ������Ʈ
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
        EditorGUILayout.LabelField("������ ������ ����", EditorStyles.boldLabel);

        sequenceData = (SequenceData)EditorGUILayout.ObjectField("������ ������", sequenceData, typeof(SequenceData), false);

        if (sequenceData == null)
        {
            EditorGUILayout.EndVertical();
            return;
        }

        InitializeTracks();

        EditorGUILayout.LabelField("BPM", sequenceData.bpm.ToString());
        EditorGUILayout.LabelField("����� Ŭ��", sequenceData.audioClip != null ? sequenceData.name : "Note");

        EditorGUILayout.LabelField("Ʈ�� �� ����", EditorStyles.boldLabel);
        int newNumberOfTracks = EditorGUILayout.IntField("Ʈ�� ��", sequenceData.numberOfTracks);
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
        if (GUILayout.Button(isPlaying ? "�Ͻ� ����" : "���"))
        {
            if (isPlaying) PausePlayBack();
            else StartPlayBack(currentBeatTime);
        }
        if (GUILayout.Button("ó�� ���� ���"))
        {
            StartPlayBack(0);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Ư�� ��Ʈ���� ���", EditorStyles.boldLabel);
        playFromBeat = EditorGUILayout.IntSlider("��Ʈ �ε���", playFromBeat, 0, totalBeats - 1);
        if (GUILayout.Button("�ش� ��Ʈ���� ���"))
        {
            StartPlayBack(playFromBeat);
        }
        EditorGUILayout.EndVertical();

        // ������ ����
        GUILayout.Space(10);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height - 210));
        EditorGUILayout.BeginHorizontal();

        // Ÿ�Ӷ��� ǥ��
        EditorGUILayout.BeginVertical(GUILayout.Width(100));
        GUILayout.Space(beatHedight);

        for (int j = 0; j < totalBeats; j++)
        {
            float beatTime = j * 60 / sequenceData.bpm;
            int minutes = Mathf.FloorToInt(beatTime / 60f);
            int seconds = Mathf.FloorToInt(beatTime % 60f);
            EditorGUILayout.BeginHorizontal();

            // 4���ڸ��� �ٸ� �������� ǥ��
            if (j % 4 == 0)
            {
                GUI.color = Color.yellow;  // ���� ǥ��
            }
            else
            {
                GUI.color = Color.white;   // �Ϲ� ����
            }

            EditorGUILayout.LabelField($"{minutes:00}:{seconds:00}", GUILayout.Width(50));
            EditorGUILayout.LabelField($"{j}", GUILayout.Width(30));
            GUI.color = Color.white;  // ���� �ʱ�ȭ

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(beatHedight - EditorGUIUtility.singleLineHeight);
        }

        EditorGUILayout.EndVertical();

        // ��Ʈ Ʈ�� ǥ��
        for (int i = 0; i < sequenceData.numberOfTracks; i++)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label($"Ʈ�� {i + 1}", GUILayout.Width(trackWidth));
            for (int j = 0; j < totalBeats; j++)
            {
                DrawBeat(i, j);
            }
            EditorGUILayout.EndVertical();
        }

        // ����Ʈ Ʈ�� ǥ��
        DrawEffectTrack();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();

        // ���� ǥ��
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