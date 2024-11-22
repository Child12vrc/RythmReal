using UnityEngine;

public class JudgeManager : MonoBehaviour
{
    private NoteManager noteManager;
    private RhythmGameController gameController;
    private float judgeRange = 3.0f;
    private float perfectRange = 1f;
    private float goodRange = 2f;
    private float badRange = 3f;
    public ParticleSystem[] hitEffects = new ParticleSystem[4];

    // 버튼 오브젝트 참조 배열
    public ButtonPress[] buttonPresses = new ButtonPress[4];

    void Start()
    {
        noteManager = FindObjectOfType<NoteManager>();
        gameController = FindObjectOfType<RhythmGameController>();
        if (noteManager == null)
            Debug.LogError("NoteManager not found!");
        if (gameController == null)
            Debug.LogError("RhythmGameController not found!");
    }

    void Update()
    {
        // S키 (트랙 1)
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (buttonPresses[0] != null) buttonPresses[0].PressButton();
            JudgeNote(0);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            if (buttonPresses[0] != null) buttonPresses[0].ReleaseButton();
        }

        // D키 (트랙 2)
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (buttonPresses[1] != null) buttonPresses[1].PressButton();
            JudgeNote(1);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            if (buttonPresses[1] != null) buttonPresses[1].ReleaseButton();
        }

        // J키 (트랙 3)
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (buttonPresses[2] != null) buttonPresses[2].PressButton();
            JudgeNote(2);
        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            if (buttonPresses[2] != null) buttonPresses[2].ReleaseButton();
        }

        // K키 (트랙 4)
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
            trackIndex >= noteManager.hitPoints.Length) return;

        Vector3 hitPosition = noteManager.hitPoints[trackIndex].position;
        NoteObject closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (NoteObject note in FindObjectsOfType<NoteObject>())
        {
            if (note != null && note.noteData.trackIndex == trackIndex)
            {
                float distance = Vector3.Distance(note.transform.position, hitPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNote = note;
                }
            }
        }

        if (trackIndex < hitEffects.Length && hitEffects[trackIndex] != null)
        {
            hitEffects[trackIndex].Play();
        }

        if (closestNote != null && closestDistance <= judgeRange)
        {
            string result;
            if (closestDistance <= perfectRange)
            {
                result = "Perfect";
            }
            else if (closestDistance <= goodRange)
            {
                result = "Good";
            }
            else if (closestDistance <= badRange)
            {
                result = "Bad";
            }
            else
            {
                result = "Miss";
            }

            // 게임 컨트롤러에 판정 결과 전달
            if (gameController != null)
            {
                gameController.OnJudgeResult(result, closestDistance);
            }

            closestNote.Hit();
        }
    }
}