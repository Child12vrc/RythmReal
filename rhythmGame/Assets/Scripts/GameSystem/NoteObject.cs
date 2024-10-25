using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public Sprite[] NoteSprites = new Sprite[3];
    public Note note;
    public float speed;
    public float hitPosition;
    public float startTime;

    public GameObject[] HitCheckEffect = new GameObject[5];

    private NoteManager noteManager;
    private SpriteRenderer noteImage;

    // yPos 값을 매핑하는 Dictionary 추가
    private Dictionary<int, float> yPosMap = new Dictionary<int, float>
    {
        { 1, -1.5f },
        { 2, 0.2f },
        { 3, -0.65f }
    };

    private void Awake()
    {
        noteImage = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        noteManager = NoteManager.instance;
    }

    // NoteObject 초기화 메서드 수정 (5개의 매개변수 추가)
    public void Initialized(Note note, float speed, Vector3 startPosition, Vector3 endPosition, float startTime)
    {
        this.note = note;
        this.speed = speed;
        this.startTime = startTime;

        // 초기 위치 설정
        transform.position = new Vector3(startPosition.x, GetYPos(note.noteValue), startPosition.z);

        // 노트 이미지 설정
        noteImage.sprite = NoteSprites[note.noteValue - 1];
    }

    void Update()
    {
        // 노트 이동
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // 판정 위치를 지나면 처리
        if (transform.position.x <= hitPosition - 1)
        {
            ReturnNoteToPool();
            NoteManager.instance.scoreManager.AddScore(Timing.Miss);
            Instantiate(HitCheckEffect[4], transform.position, HitCheckEffect[4].transform.rotation);
        }
    }

    // 판정 체크
    public void HitCheck(int noteIndex)
    {
        float distance = Mathf.Abs(transform.position.x - hitPosition);

        if (distance > 2) return;

        if (note.noteValue == noteIndex)
        {
            Timing scoreTiming;
            GameObject hitEffect;

            if (distance < 0.5f)
            {
                scoreTiming = Timing.Perfect;
                hitEffect = HitCheckEffect[0];
            }
            else if (distance < 0.8f)
            {
                scoreTiming = Timing.Great;
                hitEffect = HitCheckEffect[1];
            }
            else if (distance < 1.1f)
            {
                scoreTiming = Timing.Good;
                hitEffect = HitCheckEffect[2];
            }
            else
            {
                scoreTiming = Timing.Bad;
                hitEffect = HitCheckEffect[3];
            }

            NoteManager.instance.scoreManager.AddScore(scoreTiming);
            Instantiate(hitEffect, transform.position, hitEffect.transform.rotation);
        }
        else
        {
            NoteManager.instance.scoreManager.AddScore(Timing.Miss);
            Instantiate(HitCheckEffect[4], transform.position, HitCheckEffect[4].transform.rotation);
        }

        ReturnNoteToPool();
    }

    // 노트를 풀로 되돌리는 공통 메서드
    private void ReturnNoteToPool()
    {
        noteManager.notePoolEnqueue(this);
        noteManager.poolManager.ReturnToPool(this.gameObject);
        noteManager.nowNotes.Remove(this);
    }

    // yPos 매핑 값을 반환
    private float GetYPos(int value)
    {
        return yPosMap.ContainsKey(value) ? yPosMap[value] : 0f;
    }
}
