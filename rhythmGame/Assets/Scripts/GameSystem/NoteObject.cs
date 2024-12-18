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
    private JudgeManager judgeManager;

    // New variable to track the delay before marking the note as missed
    private float missDelayTime = 0.3f; 
    private float missTimer = 0f;

    public void Initialize(Note note, float noteSpeed, Transform startPos, Transform targetPos,
        float gameStartTime, NotePool notePool, float beatDur)
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

        isInitialized = true;
        isMissed = false;

        judgeManager = FindObjectOfType<JudgeManager>();
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
        }
        else
        {
            // Only start the miss timer if the note has passed its target position
            if (!isMissed)
            {
                missTimer += Time.deltaTime;

                // Check if the delay time has passed and the note should be missed
                if (missTimer >= missDelayTime)
                {
                    isMissed = true;
                    if (judgeManager != null)
                    {
                        judgeManager.OnNoteMissed(noteData.trackIndex);
                    }
                    ReturnToPool();
                }
            }
        }
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
    }

    public void Hit()
    {
        if (isInitialized)
        {
            ReturnToPool();
        }
    }
}
