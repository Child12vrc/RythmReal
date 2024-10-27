using UnityEngine;

public class JudgeManager : MonoBehaviour
{
    private NoteManager noteManager;
    private float judgeRange = 3.0f;

    // �� ���� �Ÿ� ����
    private float perfectRange = 1f;
    private float goodRange = 2f;
    private float badRange = 3f;

    // Inspector���� �� ���κ� ����Ʈ ����
    public ParticleSystem[] hitEffects = new ParticleSystem[4];

    void Start()
    {
        noteManager = FindObjectOfType<NoteManager>();
        if (noteManager == null)
            Debug.LogError("NoteManager not found!");
    }

    void Update()
    {
        // �� Ű�� ��Ʈ ����
        if (Input.GetKeyDown(KeyCode.S)) JudgeNote(0); // Ʈ�� 0
        if (Input.GetKeyDown(KeyCode.D)) JudgeNote(1); // Ʈ�� 1
        if (Input.GetKeyDown(KeyCode.J)) JudgeNote(2); // Ʈ�� 2
        if (Input.GetKeyDown(KeyCode.K)) JudgeNote(3); // Ʈ�� 3
    }

    void JudgeNote(int trackIndex)
    {
        if (noteManager == null || noteManager.hitPoints == null ||
            trackIndex >= noteManager.hitPoints.Length) return;

        Vector3 hitPosition = noteManager.hitPoints[trackIndex].position;

        // �ش� Ʈ���� ���ϴ� ���� ����� ��Ʈ ã��
        NoteObject closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (NoteObject note in FindObjectsOfType<NoteObject>())
        {
            if (note != null && note.noteData.trackIndex == trackIndex) // �ش� Ʈ���� ��Ʈ�� �˻�
            {
                float distance = Vector3.Distance(note.transform.position, hitPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNote = note;
                }
            }
        }

        // ����Ʈ ���
        if (trackIndex < hitEffects.Length && hitEffects[trackIndex] != null)
        {
            hitEffects[trackIndex].Play();
        }

        // ��Ʈ ����
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
            Debug.Log($"Track {trackIndex} - {result}");

            // ��Ʈ ��Ʈ �� ����
            closestNote.Hit();
        }
    }
}
