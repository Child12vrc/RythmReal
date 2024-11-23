using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeManager : MonoBehaviour
{
    private NoteManager noteManager;
    private RhythmGameController gameController;

    [Header("Judge Settings")]
    public float judgeRange = 4.0f;
    public float perfectRange = 1f;
    public float greatRange = 2.0f;
    public float goodRange = 3.5f;
    public float badRange = 4.0f;

    public bool debugMode = false;

    [Header("Effects")]
    public ParticleSystem[] hitEffects = new ParticleSystem[4];
    public Transform[] hitPositions = new Transform[4];
    public ButtonPress[] buttonPresses = new ButtonPress[4];

    [Header("Judge Effect Objects")]
    public GameObject perfectEffect;
    public GameObject greatEffect;
    public GameObject goodEffect;
    public GameObject badEffect;
    public GameObject missEffect;

    [Header("Judge Effect Settings")]
    public float effectDuration = 0.5f;
    public float effectHeight = 1f;
    public Vector3 effectScale = new Vector3(1f, 1f, 1f);

    private List<GameObject>[] activeEffects;
    private List<float>[] effectTimers;

    void Start()
    {
        noteManager = FindObjectOfType<NoteManager>();
        gameController = FindObjectOfType<RhythmGameController>();
        if (noteManager == null) Debug.LogError("NoteManager not found!");
        if (gameController == null) Debug.LogError("RhythmGameController not found!");

        activeEffects = new List<GameObject>[4];
        effectTimers = new List<float>[4];
        for (int i = 0; i < 4; i++)
        {
            activeEffects[i] = new List<GameObject>();
            effectTimers[i] = new List<float>();
        }
    }

    void Update()
    {
        UpdateEffectTimers();
        HandleKeyInputs();
    }

    private void UpdateEffectTimers()
    {
        for (int trackIndex = 0; trackIndex < 4; trackIndex++)
        {
            for (int i = activeEffects[trackIndex].Count - 1; i >= 0; i--)
            {
                if (activeEffects[trackIndex][i] != null)
                {
                    effectTimers[trackIndex][i] -= Time.deltaTime;
                    if (effectTimers[trackIndex][i] <= 0)
                    {
                        Destroy(activeEffects[trackIndex][i]);
                        activeEffects[trackIndex].RemoveAt(i);
                        effectTimers[trackIndex].RemoveAt(i);
                    }
                }
            }
        }
    }

    public void OnNoteMissed(int trackIndex)
    {
        if (debugMode) Debug.Log($"Note missed on track {trackIndex}");
        ShowJudgeEffect(trackIndex, missEffect);
        if (gameController != null)
        {
            gameController.OnJudgeResult("Miss", float.MaxValue);
        }
    }

    private void HandleKeyInputs()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (buttonPresses[0] != null) buttonPresses[0].PressButton();
            JudgeNote(0);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            if (buttonPresses[0] != null) buttonPresses[0].ReleaseButton();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (buttonPresses[1] != null) buttonPresses[1].PressButton();
            JudgeNote(1);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            if (buttonPresses[1] != null) buttonPresses[1].ReleaseButton();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (buttonPresses[2] != null) buttonPresses[2].PressButton();
            JudgeNote(2);
        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            if (buttonPresses[2] != null) buttonPresses[2].ReleaseButton();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (buttonPresses[3] != null) buttonPresses[3].PressButton();
            JudgeNote(3);
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            if (buttonPresses[3] != null) buttonPresses[3].ReleaseButton();
        }
    }

    void JudgeNote(int trackIndex)
    {
        if (noteManager == null || noteManager.hitPoints == null ||
            trackIndex >= noteManager.hitPoints.Length)
        {
            if (debugMode) Debug.LogError($"Invalid track index or missing components: {trackIndex}");
            return;
        }

        Vector3 hitPosition = noteManager.hitPoints[trackIndex].position;
        float closestDistance = float.MaxValue;
        NoteObject closestNote = null;

        NoteObject[] allNotes = FindObjectsOfType<NoteObject>();
        if (debugMode) Debug.Log($"Found {allNotes.Length} active notes");

        foreach (NoteObject note in allNotes)
        {
            if (note != null && note.noteData.trackIndex == trackIndex)
            {
                float distance = Mathf.Abs(note.transform.position.z - hitPosition.z);
                if (debugMode) Debug.Log($"Note distance: {distance}");

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNote = note;
                }
            }
        }

        if (closestNote != null && closestDistance <= judgeRange)
        {
            if (debugMode) Debug.Log($"Judging note with distance: {closestDistance}");

            string result = "";
            GameObject effectPrefab = null;

            if (closestDistance <= perfectRange)
            {
                result = "Perfect";
                effectPrefab = perfectEffect;
            }
            else if (closestDistance <= greatRange)
            {
                result = "Great";
                effectPrefab = greatEffect;
            }
            else if (closestDistance <= goodRange)
            {
                result = "Good";
                effectPrefab = goodEffect;
            }
            else if (closestDistance <= badRange)
            {
                result = "Bad";
                effectPrefab = badEffect;
            }

            if (result != "" && effectPrefab != null)
            {
                ShowJudgeEffect(trackIndex, effectPrefab);

                if (trackIndex < hitEffects.Length && hitEffects[trackIndex] != null)
                {
                    hitEffects[trackIndex].Play();
                }

                if (gameController != null)
                {
                    gameController.OnJudgeResult(result, closestDistance);
                }

                closestNote.Hit();
            }
        }
        // else 구문 제거 - 노트가 없을 때는 아무것도 하지 않음
    }

    private void ShowJudgeEffect(int trackIndex, GameObject effectPrefab)
    {
        if (hitPositions[trackIndex] == null || effectPrefab == null)
        {
            if (debugMode) Debug.LogError($"Missing references - hitPosition: {hitPositions[trackIndex]}, effectPrefab: {effectPrefab}");
            return;
        }

        Vector3 spawnPosition = hitPositions[trackIndex].position + Vector3.up * effectHeight;
        GameObject newEffect = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);

        if (Camera.main != null)
        {
            newEffect.transform.forward = -Camera.main.transform.forward;
        }

        newEffect.transform.localScale = effectScale;

        activeEffects[trackIndex].Add(newEffect);
        effectTimers[trackIndex].Add(effectDuration);

        StartCoroutine(FadeOutEffect(trackIndex, newEffect));
    }

    private IEnumerator FadeOutEffect(int trackIndex, GameObject effect)
    {
        if (effect == null) yield break;

        float elapsedTime = 0f;
        Vector3 startScale = new Vector3(150, 150, 150);
        Vector3 startPosition = effect.transform.position;

        while (elapsedTime < effectDuration)
        {
            if (effect == null) break;

            float normalizedTime = elapsedTime / effectDuration;
            effect.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, normalizedTime);

            float heightOffset = Mathf.Lerp(0f, 1f, normalizedTime);
            effect.transform.position = startPosition + Vector3.up * heightOffset;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (effect != null)
        {
            int index = activeEffects[trackIndex].IndexOf(effect);
            if (index != -1)
            {
                activeEffects[trackIndex].RemoveAt(index);
                effectTimers[trackIndex].RemoveAt(index);
            }
            Destroy(effect);
        }
    }
}