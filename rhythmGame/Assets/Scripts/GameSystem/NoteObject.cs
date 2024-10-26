using UnityEngine;

public class NoteObject : MonoBehaviour
{
    private Note noteData;
    private float speed;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float startTime;
    private float journeyLength;
    private float startJourneyTime;
    private bool isInitialized = false;
    private bool isMissed = false;
    private float exactHitTime;
    private float beatDuration;
    private NotePool pool;

    public void Initialize(Note note, float noteSpeed, Transform startPos, Transform targetPos, float gameStartTime, NotePool notePool, float beatDur)
    {
        if (startPos == null || targetPos == null)
        {
            Debug.LogError("Start or Target position is null!");
            return;
        }

        noteData = note;
        speed = noteSpeed;
        beatDuration = beatDur;
        startPosition = startPos.position;
        targetPosition = targetPos.position;
        startTime = gameStartTime;
        pool = notePool;

        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(90, 0, 0);

        journeyLength = Vector3.Distance(startPosition, targetPosition);
        startJourneyTime = Time.time;
        // 비트 타이밍을 고려한 정확한 히트 타임 계산
        exactHitTime = startTime + note.startTime + beatDuration;

        isInitialized = true;
        isMissed = false;
    }
    void Update()
    {
        if (!isInitialized) return;

        float currentTime = Time.time;
        float progress = (currentTime - startJourneyTime) / (exactHitTime - startJourneyTime);

        if (progress <= 1.0f)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
        }
        else
        {
            float overDistance = (progress - 1.0f) * journeyLength;
            Vector3 direction = (targetPosition - startPosition).normalized;
            transform.position = targetPosition + (direction * overDistance);

            if (!isMissed && overDistance > 1.0f)
            {
                isMissed = true;
                OnNoteMissed();
                // Miss 판정 후 일정 거리 이상 지나면 풀로 반환
                if (overDistance > 2.0f)
                {
                    ReturnToPool();
                }
            }
        }

        transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    private void OnNoteMissed()
    {
        Debug.Log($"Note Missed! Track: {noteData.trackIndex}");

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.gray;
        }

        // 여기에 Miss 이벤트 추가
        //GameManager.Instance?.OnNoteMissed(noteData);
    }

    public void Hit()
    {
        // 노트 히트 시 호출
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (pool != null)
        {
            isInitialized = false;
            pool.ReturnNote(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
