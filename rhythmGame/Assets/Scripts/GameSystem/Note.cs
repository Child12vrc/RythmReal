using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Note
{
    public int trackIndex;          //노트가 속한 인덱스
    public float startTime;         //노트 시작 시간
    public float duration;          //노트 지속 시간
    public int noteValue;

    public Note(int trackIndex, float startTime, float duration, int noteValue)
    {
        this.trackIndex = trackIndex;
        this.startTime = startTime;
        this.duration = duration;
        this.noteValue = noteValue;
    }
}
