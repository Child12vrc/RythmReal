using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NoteManager : MonoBehaviour
{
    public static NoteManager instance;

    public ScoreManager scoreManager;
    public Player player;
    public PoolManager poolManager;

    public AudioClip audioClip;
    public List<Note> notes = new List<Note>();
    public float bpm = 120f;
    public float speed = 1f;
    public GameObject notePrefabs;

    public float audioLatency = 0.1f;
    public float hitPosition = -8.0f;
    public float noteSpeed = 10;

    private AudioSource audioSource;
    private float startTime;
    [SerializeField] private List<Note> activeNotes = new List<Note>();

    public List<NoteObject> nowNotes = new List<NoteObject>();

    private float spawnOffset;

    public bool debugMode = false;
    public GameObject hitPositionMarker;

    public float initialDelay = 3f;

    public Queue<NoteObject> notePool = new Queue<NoteObject>();

    private bool GameOver = false;

    private void Awake()
    {
        instance = this;
    }

    // 게임 초기화
    public void Initialized()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        startTime = Time.time + initialDelay;
        activeNotes.Clear();
        activeNotes.AddRange(notes);
        spawnOffset = (10 - hitPosition) / noteSpeed;

        if (debugMode)
        {
            CreateHitPositionMarker();
        }

        AudioPlay();
    }

    private void AudioPlay()
    {
        double StartTime = AudioSettings.dspTime + initialDelay;
        audioSource.PlayScheduled(StartTime);
    }

    void Update()
    {
        float currentTime = Time.time - startTime;

        if (currentTime >= audioSource.clip.length + 2f || scoreManager.HP <= 0)
        {
            if (!GameOver)
            {
                GameOver = true;
                scoreManager.SendScore();
                if (currentTime >= audioSource.clip.length + 2f)
                {
                    SceneManager.LoadScene("ScoreScene");
                }
                else if (scoreManager.HP <= 0)
                {
                    SceneManager.LoadScene("GameOverScene");
                }
            }
            return;
        }

        // 활성화된 노트를 처리
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            Note note = activeNotes[i];
            if (currentTime >= note.startTime - spawnOffset && currentTime < note.startTime + note.duration)
            {
                GameObject temp = poolManager.SpawnFromPool("Note", note.startPosition, Quaternion.identity);
                temp.GetComponent<NoteObject>().Initialized(note, noteSpeed, note.startPosition, note.endPosition, startTime);
                nowNotes.Add(temp.GetComponent<NoteObject>());
                activeNotes.RemoveAt(i);
            }
            else if (currentTime >= note.startTime + note.duration)
            {
                activeNotes.RemoveAt(i);
            }
        }
    }

    public void AddNote(Note note)
    {
        notes.Add(note);
    }

    public void SpawnNoteObject(Note note)
    {
        GameObject noteObject = Instantiate(notePrefabs, note.startPosition, Quaternion.identity);
        noteObject.GetComponent<NoteObject>().Initialized(note, noteSpeed, note.startPosition, note.endPosition, startTime);
        nowNotes.Add(noteObject.GetComponent<NoteObject>());
    }

    // 노트를 풀로 되돌리는 메서드
    public void notePoolEnqueue(NoteObject targetNote)
    {
        notePool.Enqueue(targetNote);
        targetNote.gameObject.SetActive(false);
        nowNotes.Remove(targetNote);
    }

    // 노트 이동 속도 설정 메서드
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    private void CreateHitPositionMarker()
    {
        hitPositionMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hitPositionMarker.transform.position = new Vector3(hitPosition, 0, 0);
        hitPositionMarker.transform.localScale = new Vector3(0.1f, 10.0f, 1.0f);
    }
}
