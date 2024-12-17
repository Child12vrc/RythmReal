using System.Collections.Generic;
using UnityEngine;

public class NotePool : MonoBehaviour
{
    private GameObject notePrefab;
    private List<NoteObject> pool = new List<NoteObject>();
    private const int initialPoolSize = 20;

    public void Initialize(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[NotePool] Prefab is null!");
            return;
        }

        notePrefab = prefab;
        pool.Clear();

        // �ʱ� Ǯ ����
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewNote();
        }
    }

    private NoteObject CreateNewNote()
    {
        if (notePrefab == null)
        {
            Debug.LogError("[NotePool] Prefab is null in CreateNewNote!");
            return null;
        }

        GameObject noteObj = Instantiate(notePrefab, transform);
        noteObj.SetActive(false);
        NoteObject note = noteObj.GetComponent<NoteObject>();
        pool.Add(note);
        return note;
    }

    public NoteObject GetNote()
    {
        // null�� ��Ʈ ����
        pool.RemoveAll(note => note == null);

        // ��� ������ ��Ʈ ã��
        NoteObject note = pool.Find(n => n != null && !n.gameObject.activeSelf);

        // ������ ���� ����
        if (note == null)
        {
            note = CreateNewNote();
        }

        if (note != null)
        {
            note.gameObject.SetActive(true);
        }

        return note;
    }

    public void ReturnNote(NoteObject note)
    {
        if (note != null && note.gameObject != null)
        {
            note.gameObject.SetActive(false);
        }
    }

    public void ClearPool()
    {
        foreach (var note in pool)
        {
            if (note != null && note.gameObject != null)
            {
                Destroy(note.gameObject);
            }
        }
        pool.Clear();
    }

    private void OnDestroy()
    {
        ClearPool();
    }
}