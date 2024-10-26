using UnityEngine;
using System.Collections.Generic;

public class NotePool : MonoBehaviour
{
    private Queue<NoteObject> pooledNotes = new Queue<NoteObject>();
    private GameObject notePrefab;
    private int poolSize = 100;  // 초기 풀 크기

    public void Initialize(GameObject prefab)
    {
        notePrefab = prefab;
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewNote();
        }
    }

    private void CreateNewNote()
    {
        GameObject noteObj = Instantiate(notePrefab);
        noteObj.SetActive(false);
        noteObj.transform.SetParent(transform);
        pooledNotes.Enqueue(noteObj.GetComponent<NoteObject>());
    }

    public NoteObject GetNote()
    {
        if (pooledNotes.Count == 0)
        {
            CreateNewNote();
        }

        NoteObject note = pooledNotes.Dequeue();
        note.gameObject.SetActive(true);
        return note;
    }

    public void ReturnNote(NoteObject note)
    {
        note.gameObject.SetActive(false);
        note.transform.SetParent(transform);
        pooledNotes.Enqueue(note);
    }
}
