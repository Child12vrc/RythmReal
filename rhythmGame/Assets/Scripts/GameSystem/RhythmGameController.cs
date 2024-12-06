using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class RhythmGameController : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        Ready,
        Playing,
        Paused,
        GameOver
    }

    [Header("Game Management")]
    public RhythmGameManager gameManager;
    public JudgeManager judgeManager;
    public KeyCode startKey = KeyCode.Return;  // Enter 키로 시작
    public KeyCode nextTrackKey = KeyCode.RightArrow;
    public KeyCode previousTrackKey = KeyCode.LeftArrow;
    public KeyCode restartKey = KeyCode.R;
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Game State")]
    private bool isGameStarted = false;
    private bool isPaused = false;
    private bool isGameActive = false;
    private GameState currentGameState = GameState.Menu;


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

    [Header("Guide UI")]
    public TextMeshProUGUI pressToStartText;  // TextMeshPro로 변경

    public CameraPositionController cameraPositionController;
    public MenuScroll menuScroll;

    [Header("Combo Fade Settings")]
    public float comboFadeDuration = 1f; // 서서히 사라지는 데 걸리는 시간
    private Coroutine fadeComboCoroutine; // 진행 중인 페이드 코루틴
    private CanvasGroup comboCanvasGroup; // Combo 텍스트를 감싸는 CanvasGroup

    void Start()
    {
        ResetScore();

        if (comboText != null)
        {
            comboCanvasGroup = comboText.GetComponent<CanvasGroup>();
            if (comboCanvasGroup == null)
            {
                comboCanvasGroup = comboText.gameObject.AddComponent<CanvasGroup>();
            }
            comboCanvasGroup.alpha = 0f; // 처음에는 투명
        }

        isGameActive = true;

        // 게임 시작 전에는 매니저들 비활성화
        if (gameManager != null) gameManager.enabled = false;
        if (judgeManager != null) judgeManager.enabled = false;
        SetGameState(GameState.Menu);
        ShowPressToStart(true);  // 시작 시 안내 텍스트 표시
    }

    private void ShowPressToStart(bool show)
    {
        if (pressToStartText != null)
        {
            pressToStartText.text = "Press Enter to Start";
            pressToStartText.gameObject.SetActive(show);
        }
    }

    public void StartGame(int trackIndex = 0)
    {
        StartCoroutine(GameStartSequence(trackIndex));
        ShowPressToStart(false);  // 게임 시작 시 안내 텍스트 숨김
    }

    // 게임 완료 시 호출될 메서드
    public void OnSongComplete()
    {
        ReturnToMenu();
        ShowPressToStart(true);  // 메뉴로 돌아갈 때 안내 텍스트 다시 표시
    }

    private IEnumerator GameStartSequence(int trackIndex)
    {
        SetGameState(GameState.Ready);
        ResetScore();

        // 게임 매니저들 활성화
        if (gameManager != null)
        {
            gameManager.enabled = true;
            gameManager.ChangeTrack(trackIndex);
        }
        if (judgeManager != null) judgeManager.enabled = true;

        // 여기에 게임 시작 연출을 위한 대기 시간 추가 가능
        yield return new WaitForSeconds(1f); // 연출을 위한 대기 시간

        // 게임 시작
        isGameStarted = true;
        isGameActive = true;
        SetGameState(GameState.Playing);
        gameManager.Initialize(); // 이제 초기화하면 음악과 노트가 시작됨
    }

    void Update()
    {
        switch (currentGameState)
        {
            case GameState.Menu:
                if (Input.GetKeyDown(startKey))
                {
                    StartGame(gameManager.GetCurrentTrackIndex());
                }
  
                break;

            case GameState.Playing:
                HandleGameControls();
                // HandleNoteInput 제거 - JudgeManager가 직접 처리
                break;

            case GameState.Paused:
                if (Input.GetKeyDown(pauseKey))
                {
                    ResumeGame();
                }
                break;
        }
    }

    private void HandleGameControls()
    {
        if (Input.GetKeyDown(pauseKey))  // ESC 키
        {
            // 음악 멈추고 메뉴로 복귀
            // 게임 종료 처리
            gameManager.EndGame();  // 게임 매니저의 EndGame 호출
            ReturnToMenu();
            menuScroll.ReturnToMenu();
            cameraPositionController.MoveCameraToPosition(1);
        }

        if (isPaused) return;

        if (Input.GetKeyDown(nextTrackKey))
        {
            StartNewTrack(() => gameManager.NextTrack());
        }
        else if (Input.GetKeyDown(previousTrackKey))
        {
            StartNewTrack(() => gameManager.PreviousTrack());
        }

        if (Input.GetKeyDown(restartKey))
        {
            RestartCurrentTrack();
        }
    }

    // JudgeManager의 판정 결과를 받아서 점수 처리
    public void OnJudgeResult(string result, float timing)
    {
        switch (result)
        {
            case "Perfect":
                AddScore(perfectScore, "Perfect");
                perfectHits++;
                break;
            case "Good":
                AddScore(goodScore, "Good");
                goodHits++;
                break;
            case "Bad":
            case "Miss":
                OnNoteMiss();
                break;
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
        if (comboText != null && currentCombo >= 1)
        {
            comboText.text = $"{currentCombo} Combo!";

            // 페이드 효과 적용
            if (fadeComboCoroutine != null)
            {
                StopCoroutine(fadeComboCoroutine);
            }
            fadeComboCoroutine = StartCoroutine(FadeComboText());
        }
        else
        {
            comboText.text = "";
        }

     
    }

    private IEnumerator FadeComboText()
    {
        if (comboCanvasGroup != null)
        {
            comboCanvasGroup.alpha = 1f; // 완전히 보이도록 설정

            // 지정된 시간 동안 유지
            yield return new WaitForSeconds(0.1f);

            // 서서히 알파 값 감소
            float elapsedTime = 0f;
            while (elapsedTime < comboFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                comboCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / comboFadeDuration);
                yield return null;
            }
            comboCanvasGroup.alpha = 0f; // 완전히 사라짐
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

    private void SetGameState(GameState newState)
    {
        currentGameState = newState;
        switch (newState)
        {
            case GameState.Menu:
                Time.timeScale = 1;
                isGameActive = false;
                break;

            case GameState.Ready:
                Time.timeScale = 1;
                isGameActive = false;
                break;

            case GameState.Playing:
                Time.timeScale = 1;
                isGameActive = true;
                break;

            case GameState.Paused:
                Time.timeScale = 0;
                break;

            case GameState.GameOver:
                Time.timeScale = 1;
                isGameActive = false;
                break;
        }
    }

    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
            isPaused = true;
            gameManager.PauseGame();
        }
    }

    public void ResumeGame()
    {
        if (currentGameState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
            isPaused = false;
            gameManager.ResumeGame();
        }
    }

    public void ReturnToMenu()
    {
        StopAllCoroutines();

        // 게임 매니저 정리
        if (gameManager != null)
        {
            gameManager.EndGame();
            gameManager.enabled = false;
        }

        // 저지 매니저 정리
        if (judgeManager != null)
        {
            judgeManager.ClearAllNotes();  // 노트 매니저 클리어 함수 필요
            judgeManager.enabled = false;
        }

        // 게임 상태 초기화
        SaveCurrentScore();
        SetGameState(GameState.Menu);
        isGameStarted = false;
        isGameActive = false;
        isPaused = false;

        // 시간 스케일 리셋
        Time.timeScale = 1;
    }

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

    // 외부에서 접근 가능한 현재 게임 상태 정보
    public int GetCurrentScore() => currentScore;
    public int GetMaxCombo() => maxCombo;
    public float GetAccuracy() => hitNotes > 0 ? (perfectHits * 100.0f + goodHits * 60.0f) / (hitNotes + misses) : 0f;
    public (int perfect, int good, int miss) GetHitCounts() => (perfectHits, goodHits, misses);
}