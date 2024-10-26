using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Note
{
    public int trackIndex;              //노트가 속한 트랙의 인덱스
    public float startTime;             //노트 시작 시간(초 단위)
    public float duration;              //노트 지속 시간(초 단위)

    //노트 생성자 New을 이용해서 생성할때 인수로 받은 값을 Note에 넣어 준다. 
    public Note(int trackIndex, float startTime, float duration)
    {
        this.trackIndex = trackIndex;
        this.startTime = startTime;
        this.duration = duration;
    }
}
