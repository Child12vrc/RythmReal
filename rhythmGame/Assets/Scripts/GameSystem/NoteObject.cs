using System.Collections.Generic;
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
    private float endNoteTime;
    private Vector3 endNotePosition;
    private bool hasEndNote;
    public GameObject temporaryEndPosObject;

    private GameObject longNoteLine;
    private LineRenderer longNoteLineRenderer;
    private static List<GameObject> activeLongNoteLines = new List<GameObject>();

    private bool isLongNoteStart => noteData != null && noteData.noteValue == 2;
    private bool isLongNoteEnd => noteData != null && noteData.noteValue == 3;

    public void Initialize(Note note, float noteSpeed, Transform startPos, Transform targetPos,
        Transform endNotePos, float gameStartTime, NotePool notePool, float beatDur, float endTime = 0f)
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
        this.endTime = exactHitTime + note.duration;

        hasEndNote = endNotePos != null;
        if (hasEndNote)
        {
            endNotePosition = endNotePos.position;
            endNoteTime = endTime;
        }

        isInitialized = true;
        isMissed = false;

        if (isLongNoteStart && hasEndNote)
        {
            SetupLongNoteLine();
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
            }
            else if (isLongNoteEnd)
            {
                material.color = Color.red;
            }
            else
            {
                material.color = Color.white;
            }
        }
    }

    private void SetupLongNoteLine()
    {
        longNoteLine = new GameObject($"LongNoteLine_Track{noteData.trackIndex}");
        activeLongNoteLines.Add(longNoteLine);

        longNoteLineRenderer = longNoteLine.AddComponent<LineRenderer>();
        longNoteLineRenderer.positionCount = 2;
        longNoteLineRenderer.startWidth = 0.2f;
        longNoteLineRenderer.endWidth = 0.2f;

        Material lineMaterial = new Material(Shader.Find("Sprites/Default"))
        {
            renderQueue = 3000
        };
        longNoteLineRenderer.material = lineMaterial;

        float alpha = 0.8f;
        longNoteLineRenderer.startColor = new Color(1f, 1f, 0f, alpha);
        longNoteLineRenderer.endColor = new Color(1f, 0f, 0f, alpha);

        longNoteLineRenderer.SetPosition(0, transform.position);
        longNoteLineRenderer.SetPosition(1, endNotePosition);
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

            if (isLongNoteStart && longNoteLineRenderer != null && hasEndNote)
            {
                float endProgress = (Time.time - startJourneyTime) / (endNoteTime - startTime);
                endProgress = Mathf.Clamp01(endProgress);
                Vector3 endPos = Vector3.Lerp(startPosition, endNotePosition, endProgress);

                longNoteLineRenderer.SetPosition(0, currentPosition);
                longNoteLineRenderer.SetPosition(1, endPos);
            }
        }
        else if (!isMissed)
        {
            isMissed = true;
            OnNoteMissed();
            ReturnToPool();
        }
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

            if (temporaryEndPosObject != null)
            {
                Destroy(temporaryEndPosObject);
                temporaryEndPosObject = null;
            }

            if (isLongNoteEnd)
            {
                GameObject lineToRemove = null;
                foreach (var line in activeLongNoteLines)
                {
                    if (line != null && line.name == $"LongNoteLine_Track{noteData.trackIndex}")
                    {
                        lineToRemove = line;
                        break;
                    }
                }

                if (lineToRemove != null)
                {
                    activeLongNoteLines.Remove(lineToRemove);
                    Destroy(lineToRemove);
                }
            }

            pool.ReturnNote(this);
        }
        else
        {
            if (temporaryEndPosObject != null)
            {
                Destroy(temporaryEndPosObject);
            }
            if (longNoteLine != null)
            {
                Destroy(longNoteLine);
            }
            Destroy(gameObject);
        }
    }

    public void Hit(bool isLongNoteHold = false)
    {
        if (isInitialized)
        {
            ReturnToPool();
        }
    }

    private void OnDestroy()
    {
        if (temporaryEndPosObject != null)
        {
            Destroy(temporaryEndPosObject);
        }
        if (longNoteLine != null)
        {
            Destroy(longNoteLine);
        }
    }
}