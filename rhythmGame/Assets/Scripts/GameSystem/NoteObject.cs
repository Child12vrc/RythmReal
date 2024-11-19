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
    private float endTime; // �ճ�Ʈ�� �� �ð�
    private bool isLongNote => noteData.duration > 0; // �ճ�Ʈ ���� Ȯ��

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
        endTime = exactHitTime + note.duration; // �ճ�Ʈ�� ���� �ð�

        isInitialized = true;
        isMissed = false;

        if (isLongNote)
        {
            SetupLongNoteVisuals();
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        float currentTime = Time.time;
        float progress = (currentTime - startJourneyTime) / (exactHitTime - startJourneyTime);

        // ��Ʈ�� �̵� ó��
        if (progress <= 1.0f)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
        }
        else if (isLongNote && currentTime <= endTime)
        {
            // �ճ�Ʈ�� ���� �ð� ���� ��ġ ����
            transform.position = targetPosition;
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

                if (overDistance > 2.0f)
                {
                    ReturnToPool();
                }
            }
        }

        transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    private void SetupLongNoteVisuals()
    {
        // �ð������� �ճ�Ʈ�� ����
        float longNoteLength = noteData.duration * speed; // �ճ�Ʈ ���̸� ���
        Transform visualTransform = transform.Find("Visual"); // �ճ�Ʈ ���־� ������Ʈ
        if (visualTransform != null)
        {
            visualTransform.localScale = new Vector3(visualTransform.localScale.x, visualTransform.localScale.y, longNoteLength);
        }

        // ������ �ð������� ǥ��
        Transform endMarker = transform.Find("EndMarker");
        if (endMarker != null)
        {
            endMarker.localPosition = new Vector3(0, 0, longNoteLength);
        }
    }

    private void OnNoteMissed()
    {
        Debug.Log($"Note Missed! Track: {noteData.trackIndex}");

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.gray;
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
