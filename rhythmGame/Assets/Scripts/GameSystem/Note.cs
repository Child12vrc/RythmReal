using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Note
{
    public int trackIndex;   // 노트가 속한 트랙의 인덱스
    public float startTime;  // 노트 시작 시간(초 단위)
    public float duration;   // 노트 지속 시간(초 단위)
    public int noteValue;    // 노트의 타입 (2: 롱노트 시작, 3: 롱노트 종료, 기타 값: 일반 노트)

    // 기본 생성자
    public Note(int trackIndex, float startTime, float duration, int noteValue)
    {
        this.trackIndex = trackIndex;
        this.startTime = startTime;
        this.duration = duration;
        this.noteValue = noteValue;
    }

    // 일반 노트를 위한 생성자 (noteValue 기본값 설정)
    public Note(int trackIndex, float startTime, float duration) : this(trackIndex, startTime, duration, 0)
    {
    }
}
