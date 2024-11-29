using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrackMenuItem : MonoBehaviour
{
    public TextMeshPro trackText;
    public MeshRenderer albumArtRenderer;
    public AudioSource previewAudioSource;
    private float originalZ;
    private SequenceData currentTrack;

    public void SetTrackInfo(SequenceData track)
    {
        currentTrack = track;
        trackText.text = $"{track.name} - {track.bpm}BPM";

        // �ٹ� ���� ����
        if (track.albumArt != null && albumArtRenderer != null)
        {
            albumArtRenderer.material.mainTexture = track.albumArt;
        }

        // �̸���� ����� ����
        if (previewAudioSource != null)
        {
            previewAudioSource.clip = track.audioClip;
        }
    }

    public void Select(float zOffset)
    {
        originalZ = transform.localPosition.z;
        Vector3 pos = transform.localPosition;
        pos.z += zOffset;
        transform.localPosition = pos;
    }

    public void Deselect()
    {
        Vector3 pos = transform.localPosition;
        pos.z = originalZ;
        transform.localPosition = pos;
        StopPreview();
    }

    public void PlayPreview()
    {
        if (previewAudioSource != null && currentTrack != null)
        {
            previewAudioSource.time = 60f; // 1�� ���� ���
            previewAudioSource.Play();
        }
    }

    public void StopPreview()
    {
        if (previewAudioSource != null)
        {
            previewAudioSource.Stop();
        }
    }
}