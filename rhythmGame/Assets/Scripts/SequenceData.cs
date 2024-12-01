using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
[CreateAssetMenu(fileName = "NewSequence", menuName = "Sequencer/Sequence")]
public class SequenceData : ScriptableObject
{
    public int bpm;
    public int numberOfTracks;
    public string trackName;
    public AudioClip audioClip;
    public Texture2D albumArt;  // 앨범 아트 추가
    public List<List<int>> trackNotes = new List<List<int>>();
    public List<int> effectTrack = new List<int>();
    public TextAsset trackJsonFile;

    [System.Serializable]
    private class SerializedData
    {
        public int bpm;
        public int numberOfTracks;
        public string audioClipPath;
        public string albumArtPath;  // 앨범 아트 경로 추가
        public List<List<int>> trackNotes;
        public List<int> effectTrack;
    }

#if UNITY_EDITOR
    public void SaveToJson()
    {
        if (trackJsonFile == null)
        {
            Debug.LogError("Track JSON 파일이 없습니다.");
            return;
        }

        var data = new SerializedData
        {
            bpm = this.bpm,
            numberOfTracks = this.numberOfTracks,
            audioClipPath = UnityEditor.AssetDatabase.GetAssetPath(audioClip),
            albumArtPath = UnityEditor.AssetDatabase.GetAssetPath(albumArt),  // 앨범 아트 경로 저장
            trackNotes = this.trackNotes,
            effectTrack = this.effectTrack
        };

        string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
        System.IO.File.WriteAllText(UnityEditor.AssetDatabase.GetAssetPath(trackJsonFile), jsonData);
        UnityEditor.AssetDatabase.Refresh();
    }
#endif

    public void LoadFromJson()
    {
        if (trackJsonFile == null)
        {
            Debug.LogError("Track JSON 파일이 없습니다.");
            return;
        }

        try
        {
            var data = JsonConvert.DeserializeObject<SerializedData>(trackJsonFile.text);

            if (data != null)
            {
                bpm = data.bpm;
                numberOfTracks = data.numberOfTracks;
                trackNotes = data.trackNotes ?? new List<List<int>>();
                effectTrack = data.effectTrack ?? new List<int>();

#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(data.audioClipPath))
                {
                    audioClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(data.audioClipPath);
                }
                if (!string.IsNullOrEmpty(data.albumArtPath))  // 앨범 아트 로드
                {
                    albumArt = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(data.albumArtPath);
                }
#endif
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON 파일을 로드하는 중 오류가 발생했습니다: {e.Message}");
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(SequenceData))]
public class SequenceDataEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        var sequenceData = (SequenceData)target;
        DrawDefaultInspector();

        if (sequenceData != null)
        {
            UnityEditor.EditorGUILayout.Space();

            // 앨범 아트 프리뷰 추가
            if (sequenceData.albumArt != null)
            {
                float previewSize = 100f;
                Rect previewRect = UnityEditor.EditorGUILayout.GetControlRect(false, previewSize);
                UnityEditor.EditorGUI.DrawPreviewTexture(previewRect, sequenceData.albumArt);
            }

            UnityEditor.EditorGUILayout.LabelField("Track Notes", UnityEditor.EditorStyles.boldLabel);

            if (sequenceData.trackNotes != null)
            {
                for (int i = 0; i < sequenceData.trackNotes.Count; i++)
                {
                    if (sequenceData.trackNotes[i] != null)
                    {
                        UnityEditor.EditorGUILayout.LabelField(
                            $"Track {i + 1}: [{string.Join(", ", sequenceData.trackNotes[i])}]"
                        );
                    }
                }
            }

            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.LabelField("Effect Track", UnityEditor.EditorStyles.boldLabel);
            if (sequenceData.effectTrack != null)
            {
                UnityEditor.EditorGUILayout.LabelField(
                    $"Effects: [{string.Join(", ", sequenceData.effectTrack)}]"
                );
            }
        }

        UnityEditor.EditorGUILayout.Space();

        if (GUILayout.Button("Load from JSON"))
        {
            sequenceData.LoadFromJson();
            UnityEditor.EditorUtility.SetDirty(sequenceData);
        }

        if (GUILayout.Button("Save to JSON"))
        {
            sequenceData.SaveToJson();
        }

        if (GUI.changed)
        {
            UnityEditor.EditorUtility.SetDirty(sequenceData);
        }
    }
}
#endif