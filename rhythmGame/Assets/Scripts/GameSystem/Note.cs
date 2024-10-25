using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Note
{
    public int trackIndex;          // ��Ʈ�� ���� �ε���
    public float startTime;         // ��Ʈ ���� �ð�
    public float duration;          // ��Ʈ ���� �ð�
    public int noteValue;
    public Vector3 startPosition;   // ��Ʈ�� ���� ��ġ
    public Vector3 endPosition;     // ��Ʈ�� ���� ��ġ

    public Note(int trackIndex, float startTime, float duration, int noteValue, Vector3 startPosition, Vector3 endPosition)
    {
        this.trackIndex = trackIndex;
        this.startTime = startTime;
        this.duration = duration;
        this.noteValue = noteValue;
        this.startPosition = startPosition;
        this.endPosition = endPosition;
    }
}
