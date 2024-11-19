using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public Note noteData;
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
    private float endTime; // 롱노트의 끝 시간
    private bool isLongNote => noteData.duration > 0; // 롱노트 여부 확인
    private LineRenderer lineRenderer;

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
        exactHitTime = startTime + note.startTime + beatDuration;
        endTime = exactHitTime + note.duration; // 롱노트의 종료 시간

        isInitialized = true;
        isMissed = false;

        if (isLongNote)
        {
            SetupLineRenderer();
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        float currentTime = Time.time;
        float progress = (currentTime - startJourneyTime) / (exactHitTime - startJourneyTime);

        if (progress <= 1.0f)
        {
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            transform.position = currentPosition;

            // 롱노트의 LineRenderer 업데이트
            if (isLongNote && lineRenderer != null)
            {
                lineRenderer.SetPosition(1, currentPosition);
            }
        }
        else if (isLongNote && currentTime <= endTime)
        {
            transform.position = targetPosition;
        }
        else
        {
            if (!isMissed)
            {
                isMissed = true;
                OnNoteMissed();
                ReturnToPool();
            }
        }
    }

    private void SetupLineRenderer()
    {
        // 기존 LineRenderer 제거 후 새로 설정
        if (lineRenderer != null)
        {
            Destroy(lineRenderer);
        }

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition); // 초기화 시 끝점은 시작점
    }

    private void OnNoteMissed()
    {
        Debug.Log($"Note Missed! Track: {noteData.trackIndex}");
    }

    public void ReturnToPool()
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

        if (lineRenderer != null)
        {
            Destroy(lineRenderer);
        }
    }

    public void Hit(bool isLongNoteHold = false)
    {
        if (isInitialized)
        {
            if (!isLongNoteHold || Time.time >= endTime)
            {
                ReturnToPool();
            }
        }
    }
}