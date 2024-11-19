using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Note
{
    public int trackIndex;   // ��Ʈ�� ���� Ʈ���� �ε���
    public float startTime;  // ��Ʈ ���� �ð�(�� ����)
    public float duration;   // ��Ʈ ���� �ð�(�� ����)
    public int noteValue;    // ��Ʈ�� Ÿ�� (2: �ճ�Ʈ ����, 3: �ճ�Ʈ ����, ��Ÿ ��: �Ϲ� ��Ʈ)

    // �⺻ ������
    public Note(int trackIndex, float startTime, float duration, int noteValue)
    {
        this.trackIndex = trackIndex;
        this.startTime = startTime;
        this.duration = duration;
        this.noteValue = noteValue;
    }

    // �Ϲ� ��Ʈ�� ���� ������ (noteValue �⺻�� ����)
    public Note(int trackIndex, float startTime, float duration) : this(trackIndex, startTime, duration, 0)
    {
    }
}
