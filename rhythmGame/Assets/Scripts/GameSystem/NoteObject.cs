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
    private float endTime;
    private LineRenderer lineRenderer;

    private bool isLongNote => noteData != null && (noteData.noteValue == 2 || noteData.noteValue == 3);
    private bool isLongNoteStart => noteData != null && noteData.noteValue == 2;
    private bool isLongNoteEnd => noteData != null && noteData.noteValue == 3;

    // 롱노트 끝점 관련 변수들
    private Vector3 nextNotePosition;
    private bool hasNextNote;
    private float distanceToNext;

    public void Initialize(Note note, float noteSpeed, Transform startPos, Transform targetPos, Transform nextNotePos, float gameStartTime, NotePool notePool, float beatDur)
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
        hasNextNote = nextNotePos != null;
        if (hasNextNote)
        {
            nextNotePosition = nextNotePos.position;
            distanceToNext = Vector3.Distance(targetPosition, nextNotePosition);
        }

        startTime = gameStartTime;
        pool = notePool;
        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(90, 0, 0);
        journeyLength = Vector3.Distance(startPosition, targetPosition);
        startJourneyTime = Time.time;
        exactHitTime = startTime + note.startTime + beatDuration;
        endTime = exactHitTime + note.duration;
        isInitialized = true;
        isMissed = false;

        if (isLongNoteStart)
        {
            SetupLineRenderer();
        }
        else
        {
            if (lineRenderer != null)
            {
                Destroy(lineRenderer);
                lineRenderer = null;
            }
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            if (isLongNoteStart)
            {
                material.color = Color.yellow;
                transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }
            else if (isLongNoteEnd)
            {
                material.color = Color.red;
                transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            }
            else
            {
                material.color = Color.white;
                transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            }
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

            if (isLongNoteStart && lineRenderer != null)
            {
                UpdateLongNoteLine(currentPosition, progress);
            }
        }
        else if (isLongNoteStart && currentTime <= endTime)
        {
            transform.position = targetPosition;
            if (lineRenderer != null && hasNextNote)
            {
                lineRenderer.SetPosition(0, targetPosition);
                lineRenderer.SetPosition(1, nextNotePosition);
            }
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

    private void UpdateLongNoteLine(Vector3 currentPosition, float progress)
    {
        if (lineRenderer != null && hasNextNote)
        {
            lineRenderer.SetPosition(0, currentPosition);

            // 다음 노트 위치까지의 보간
            Vector3 endPos = hasNextNote ? nextNotePosition : targetPosition;
            lineRenderer.SetPosition(1, endPos);

            // 라인 색상 그라데이션
            float alpha = 0.8f;
            Color startColor = new Color(1f, 1f, 0f, alpha); // 노란색
            Color endColor = new Color(1f, 0f, 0f, alpha);   // 빨간색
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
        }
    }

    private void SetupLineRenderer()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer);
        }

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"))
        {
            renderQueue = 3000
        };
        lineRenderer.material = lineMaterial;

        Vector3 endPos = hasNextNote ? nextNotePosition : targetPosition;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPos);

        float alpha = 0.8f;
        lineRenderer.startColor = new Color(1f, 1f, 0f, alpha);
        lineRenderer.endColor = new Color(1f, 0f, 0f, alpha);
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
            if (lineRenderer != null)
            {
                Destroy(lineRenderer);
                lineRenderer = null;
            }
            pool.ReturnNote(this);
        }
        else
        {
            if (lineRenderer != null)
            {
                Destroy(lineRenderer);
            }
            Destroy(gameObject);
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

    private void OnDestroy()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer);
        }
    }
}