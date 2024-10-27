using UnityEngine;

public class JudgeManager : MonoBehaviour
{
    private NoteManager noteManager;
    private float judgeRange = 3.0f;

    // 각 판정 거리 범위
    private float perfectRange = 1f;
    private float goodRange = 2f;
    private float badRange = 3f;

    // Inspector에서 각 레인별 이펙트 설정
    public ParticleSystem[] hitEffects = new ParticleSystem[4];

    void Start()
    {
        noteManager = FindObjectOfType<NoteManager>();
        if (noteManager == null)
            Debug.LogError("NoteManager not found!");
    }

    void Update()
    {
        // 각 키별 노트 판정
        if (Input.GetKeyDown(KeyCode.S)) JudgeNote(0); // 트랙 0
        if (Input.GetKeyDown(KeyCode.D)) JudgeNote(1); // 트랙 1
        if (Input.GetKeyDown(KeyCode.J)) JudgeNote(2); // 트랙 2
        if (Input.GetKeyDown(KeyCode.K)) JudgeNote(3); // 트랙 3
    }

    void JudgeNote(int trackIndex)
    {
        if (noteManager == null || noteManager.hitPoints == null ||
            trackIndex >= noteManager.hitPoints.Length) return;

        Vector3 hitPosition = noteManager.hitPoints[trackIndex].position;

        // 해당 트랙에 속하는 가장 가까운 노트 찾기
        NoteObject closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (NoteObject note in FindObjectsOfType<NoteObject>())
        {
            if (note != null && note.noteData.trackIndex == trackIndex) // 해당 트랙의 노트만 검사
            {
                float distance = Vector3.Distance(note.transform.position, hitPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNote = note;
                }
            }
        }

        // 이펙트 재생
        if (trackIndex < hitEffects.Length && hitEffects[trackIndex] != null)
        {
            hitEffects[trackIndex].Play();
        }

        // 노트 판정
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

            // 노트 히트 및 삭제
            closestNote.Hit();
        }
    }
}
