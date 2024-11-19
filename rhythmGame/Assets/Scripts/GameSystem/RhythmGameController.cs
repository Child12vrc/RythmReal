using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RhythmGameController : MonoBehaviour
{
    [Header("Game Management")]
    public RhythmGameManager gameManager;
    public KeyCode nextTrackKey = KeyCode.RightArrow;
    public KeyCode previousTrackKey = KeyCode.LeftArrow;
    public KeyCode restartKey = KeyCode.R;
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Score UI")]
    public Text scoreText;
    public Text comboText;
    public Text gradeText;
    public Text accuracyText;

    [Header("Score Settings")]
    public int perfectScore = 100;
    public int goodScore = 50;
    public int missScore = 0;
    public float perfectTiming = 0.05f;  // ±50ms
    public float goodTiming = 0.1f;      // ±100ms

    private int currentScore = 0;
    private int currentCombo = 0;
    private int maxCombo = 0;
    private int totalNotes = 0;
    private int hitNotes = 0;
    private int perfectHits = 0;
    private int goodHits = 0;
    private int misses = 0;

    private bool isPaused = false;
    private bool isGameActive = false;

    void Start()
    {
        ResetScore();
        isGameActive = true;
    }

    void Update()
    {
        if (!isGameActive) return;

        // 게임 컨트롤 입력 처리
        HandleGameControls();

        // 노트 입력 처리 (예시 - 실제 구현은 노트 판정 시스템과 연동 필요)
        HandleNoteInput();
    }

    private void HandleGameControls()
    {
        // 일시정지
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }

        if (isPaused) return;

        // 트랙 전환
        if (Input.GetKeyDown(nextTrackKey))
        {
            StartNewTrack(() => gameManager.NextTrack());
        }
        else if (Input.GetKeyDown(previousTrackKey))
        {
            StartNewTrack(() => gameManager.PreviousTrack());
        }

        // 재시작
        if (Input.GetKeyDown(restartKey))
        {
            RestartCurrentTrack();
        }
    }

    private void HandleNoteInput()
    {
        // 여기에 실제 노트 판정 로직 구현
        // 예시: 각 트랙에 해당하는 키 입력 처리
        // if (Input.GetKeyDown(KeyCode.D)) CheckNoteHit(0);
        // if (Input.GetKeyDown(KeyCode.F)) CheckNoteHit(1);
        // 등등...
    }

    // 노트 판정 처리 (NoteManager와 연동 필요)
    public void OnNoteHit(float timing)
    {
        if (Mathf.Abs(timing) <= perfectTiming)
        {
            AddScore(perfectScore, "Perfect");
            perfectHits++;
        }
        else if (Mathf.Abs(timing) <= goodTiming)
        {
            AddScore(goodScore, "Good");
            goodHits++;
        }
        else
        {
            OnNoteMiss();
        }
        hitNotes++;
        UpdateAccuracy();
    }

    public void OnNoteMiss()
    {
        currentCombo = 0;
        misses++;
        UpdateComboText("Miss");
        UpdateAccuracy();
    }

    private void AddScore(int score, string judgement)
    {
        currentScore += score * (1 + currentCombo / 50); // 콤보 보너스
        currentCombo++;
        maxCombo = Mathf.Max(maxCombo, currentCombo);

        UpdateScoreText();
        UpdateComboText(judgement);
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore:N0}";
        }
    }

    private void UpdateComboText(string judgement)
    {
        if (comboText != null)
        {
            comboText.text = currentCombo > 1 ? $"{currentCombo} Combo!\n{judgement}" : judgement;
        }
    }

    private void UpdateAccuracy()
    {
        if (accuracyText != null && hitNotes > 0)
        {
            float accuracy = (perfectHits * 100.0f + goodHits * 60.0f) / (hitNotes + misses) / 100.0f;
            accuracyText.text = $"Accuracy: {accuracy:F2}%";
            UpdateGrade(accuracy);
        }
    }

    private void UpdateGrade(float accuracy)
    {
        if (gradeText != null)
        {
            string grade;
            if (accuracy >= 95) grade = "S";
            else if (accuracy >= 90) grade = "A";
            else if (accuracy >= 80) grade = "B";
            else if (accuracy >= 70) grade = "C";
            else grade = "D";

            gradeText.text = $"Grade: {grade}";
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            gameManager.PauseGame();
        }
        else
        {
            gameManager.ResumeGame();
        }
    }

    private void StartNewTrack(System.Action trackChangeAction)
    {
        // 현재 점수 저장 또는 처리
        SaveCurrentScore();

        // 트랙 변경
        trackChangeAction?.Invoke();

        // 새로운 트랙 시작 준비
        ResetScore();
    }

    private void RestartCurrentTrack()
    {
        gameManager.RestartTrack();
        ResetScore();
    }

    private void ResetScore()
    {
        currentScore = 0;
        currentCombo = 0;
        maxCombo = 0;
        totalNotes = 0;
        hitNotes = 0;
        perfectHits = 0;
        goodHits = 0;
        misses = 0;

        UpdateScoreText();
        UpdateComboText("");
        UpdateAccuracy();
    }

    private void SaveCurrentScore()
    {
        // 여기에 점수 저장 로직 구현
        // 예: PlayerPrefs 사용 또는 데이터베이스 저장
        string trackName = gameManager.GetCurrentTrackInfo();
        PlayerPrefs.SetInt($"HighScore_{trackName}", Mathf.Max(PlayerPrefs.GetInt($"HighScore_{trackName}", 0), currentScore));
        PlayerPrefs.Save();
    }

    // 게임 종료 시 점수 저장
    void OnDisable()
    {
        SaveCurrentScore();
    }

    // 외부에서 접근 가능한 현재 게임 상태 정보
    public int GetCurrentScore() => currentScore;
    public int GetMaxCombo() => maxCombo;
    public float GetAccuracy() => hitNotes > 0 ? (perfectHits * 100.0f + goodHits * 60.0f) / (hitNotes + misses) : 0f;
    public (int perfect, int good, int miss) GetHitCounts() => (perfectHits, goodHits, misses);
}