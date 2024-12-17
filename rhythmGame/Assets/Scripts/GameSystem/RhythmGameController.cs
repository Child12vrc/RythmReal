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
    public NoteManager noteManager;
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

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalGradeText;
    public TextMeshProUGUI finalAccuracyText;
    public TextMeshProUGUI finalComboText;
    public TextMeshProUGUI finalPerfectText;
    public TextMeshProUGUI finalGoodText;
    public TextMeshProUGUI finalMissText;
    public Button retryButton;
    public Button menuButton;

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
    public TextMeshProUGUI pressToStartText;

    public CameraPositionController cameraPositionController;
    public MenuScroll menuScroll;

    [Header("Combo Fade Settings")]
    public float comboFadeDuration = 1f;
    private Coroutine fadeComboCoroutine;
    private CanvasGroup comboCanvasGroup;

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
            comboCanvasGroup.alpha = 0f;
        }

        isGameActive = true;

        if (gameManager != null) gameManager.enabled = false;
        if (judgeManager != null) judgeManager.enabled = false;
        SetGameState(GameState.Menu);
        ShowPressToStart(true);

        // 게임 오버 UI 초기화
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
            retryButton.onClick.AddListener(() => {
                RestartCurrentTrack();
                Debug.Log("Retry button clicked");
            });
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
            menuButton.onClick.AddListener(() => {
                ReturnToMenu();
                Debug.Log("Menu button clicked");
            });
        }
    }

    private void ShowPressToStart(bool show)
    {
        if (pressToStartText != null)
        {
            pressToStartText.text = "Press Enter to Start";
            pressToStartText.gameObject.SetActive(show);
        }
    }

    private void ShowGameOverUI()
    {
        gameOverPanel.SetActive(true);
        finalScoreText.gameObject.SetActive(true);
        finalGradeText.gameObject.SetActive(true);
        finalAccuracyText.gameObject.SetActive(true);
        finalComboText.gameObject.SetActive(true);
        finalPerfectText.gameObject.SetActive(true);
        finalGoodText.gameObject.SetActive(true);
        finalMissText.gameObject.SetActive(true);
    }

    private void SaveAndUpdateHighScore(int score, string grade)
    {
        string trackName = gameManager.GetCurrentTrackInfo();
        int highScore = PlayerPrefs.GetInt($"HighScore_{trackName}", 0);
        if (score > highScore)
        {
            PlayerPrefs.SetInt($"HighScore_{trackName}", score);
            PlayerPrefs.SetString($"HighScore_{trackName}_Grade", grade);
            PlayerPrefs.Save();
        }
    }

    private string GetGradeFromAccuracy(float accuracy)
    {
        if (accuracy >= 95) return "S";
        else if (accuracy >= 90) return "A";
        else if (accuracy >= 80) return "B";
        else if (accuracy >= 70) return "C";
        else return "D";
    }

    private IEnumerator AnimateScoreText(TextMeshProUGUI scoreText, int finalScore)
    {
        scoreText.gameObject.SetActive(true);
        int startScore = 0;
        float elapsedTime = 0f;
        float animationDuration = 1f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            int currentScore = Mathf.FloorToInt(Mathf.Lerp(startScore, finalScore, elapsedTime / animationDuration));
            scoreText.text = $"Score: {currentScore:N0}";
            yield return null;
        }

        scoreText.text = $"Score: {finalScore:N0}";
    }

    private IEnumerator AnimateComboText(TextMeshProUGUI comboText, int finalCombo)
    {
        int startCombo = 0;
        float elapsedTime = 0f;
        float animationDuration = 1f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            int currentCombo = Mathf.FloorToInt(Mathf.Lerp(startCombo, finalCombo, elapsedTime / animationDuration));
            comboText.text = $"Max Combo: {currentCombo}";
            yield return null;
        }

        comboText.text = $"Max Combo: {finalCombo}";
    }

    private IEnumerator AnimateAccuracyText(TextMeshProUGUI accuracyText, float finalAccuracy)
    {
        float startAccuracy = 0f;
        float elapsedTime = 0f;
        float animationDuration = 1f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentAccuracy = Mathf.Lerp(startAccuracy, finalAccuracy, elapsedTime / animationDuration);
            accuracyText.text = $"Accuracy: {currentAccuracy:F2}%";
            yield return null;
        }

        accuracyText.text = $"Accuracy: {finalAccuracy:F2}%";
    }

    private IEnumerator AnimatePerfectGoodMissText(TextMeshProUGUI perfectText, int perfectCount, TextMeshProUGUI goodText, int goodCount, TextMeshProUGUI missText, int missCount)
    {
        perfectText.text = $"Perfect: {perfectCount}";
        goodText.text = $"Good: {goodCount}";
        missText.text = $"Miss: {missCount}";

        float elapsedTime = 0f;
        float animationDuration = 0.5f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            perfectText.alpha = Mathf.Lerp(0f, 1f, elapsedTime / animationDuration);
            goodText.alpha = Mathf.Lerp(0f, 1f, elapsedTime / animationDuration);
            missText.alpha = Mathf.Lerp(0f, 1f, elapsedTime / animationDuration);
            yield return null;
        }

        perfectText.alpha = 1f;
        goodText.alpha = 1f;
        missText.alpha = 1f;
    }

    public IEnumerator GameOverSequence()
    {
        // 게임 오버 UI 표시
        ShowGameOverUI();

        // 점수 계산
        float accuracy = (perfectHits * 100.0f + goodHits * 60.0f) / (hitNotes + misses) / 100.0f;
        string grade = GetGradeFromAccuracy(accuracy);

        // 점수 저장 및 최고 점수 업데이트
        SaveAndUpdateHighScore(currentScore, grade);

        // 점수, 콤보, 정확도, 등급 애니메이션 효과
        yield return StartCoroutine(AnimateScoreText(finalScoreText, currentScore));
        yield return StartCoroutine(AnimateComboText(finalComboText, maxCombo));
        yield return StartCoroutine(AnimateAccuracyText(finalAccuracyText, accuracy));
        yield return StartCoroutine(AnimateGradeText(finalGradeText, grade));
        yield return StartCoroutine(AnimatePerfectGoodMissText(finalPerfectText, perfectHits, finalGoodText, goodHits, finalMissText, misses));
    }

    private IEnumerator AnimateGradeText(TextMeshProUGUI gradeText, string grade)
    {
        gradeText.gameObject.SetActive(true);
        string startGrade = "D";
        float elapsedTime = 0f;
        float animationDuration = 0.5f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            string currentGrade = Mathf.Lerp(0, 1, t) < 0.25f ? "D" : Mathf.Lerp(0, 1, t) < 0.5f ? "C" : Mathf.Lerp(0, 1, t) < 0.75f ? "B" : "A";
            gradeText.text = $"Grade: {currentGrade}";
            yield return null;
        }

        gradeText.text = $"Grade: {grade}";
    }

    private void HideGameOverUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void StartGame(int trackIndex = 0)
    {
        StartCoroutine(GameStartSequence(trackIndex));
        ShowPressToStart(false);
    }

    public void OnSongComplete()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameStartSequence(int trackIndex)
    {
        Debug.Log($"[RGC] GameStartSequence for track {trackIndex} started");
        SetGameState(GameState.Ready);
        ResetScore();

        if (gameManager != null)
        {
            gameManager.enabled = true;

            // 트랙 변경과 초기화를 한 번에 처리
            if (trackIndex != gameManager.GetCurrentTrackIndex())
            {
                gameManager.ChangeTrack(trackIndex);
            }
            else
            {
                gameManager.Initialize();
            }
        }

        if (noteManager != null)
        {
            noteManager.enabled = true;
        }

        if (judgeManager != null)
        {
            judgeManager.enabled = true;
        }

        yield return new WaitForSeconds(1f);

        isGameStarted = true;
        isGameActive = true;
        SetGameState(GameState.Playing);

        Debug.Log("[RGC] GameStartSequence complete");
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
            case GameState.GameOver:
                HandleGameControls();
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
        if (Input.GetKeyDown(pauseKey))
        {
            if (currentGameState == GameState.GameOver)
            {
                HideGameOverUI();
                ReturnToMenu();
                menuScroll.ReturnToMenu();
                cameraPositionController.MoveCameraToPosition(1);
            }
            else
            {
                gameManager.EndGame();
                ReturnToMenu();
                menuScroll.ReturnToMenu();
                cameraPositionController.MoveCameraToPosition(1);
            }
        }

        if (currentGameState == GameState.GameOver) return;

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
        currentScore += score * (1 + currentCombo / 50);
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
            comboCanvasGroup.alpha = 1f;

            yield return new WaitForSeconds(0.1f);

            float elapsedTime = 0f;
            while (elapsedTime < comboFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                comboCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / comboFadeDuration);
                yield return null;
            }
            comboCanvasGroup.alpha = 0f;
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
        SaveCurrentScore();
        trackChangeAction?.Invoke();
        ResetScore();
    }

    private void RestartCurrentTrack()
    {
        // 게임 오버 UI 숨기기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 점수 초기화
        ResetScore();

        // 게임 상태 초기화
        SetGameState(GameState.Playing);
        isGameStarted = true;
        isGameActive = true;
        isPaused = false;

        // 게임 매니저와 저지 매니저 활성화
        if (gameManager != null)
        {
            gameManager.enabled = true;
            gameManager.RestartTrack();
        }


        if (noteManager != null)
        {
            noteManager.enabled = true;
        }


        if (judgeManager != null)
        {
            judgeManager.enabled = true;
            judgeManager.ClearAllNotes();
        }
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
        string trackName = gameManager.GetCurrentTrackInfo();
        PlayerPrefs.SetInt($"HighScore_{trackName}", Mathf.Max(PlayerPrefs.GetInt($"HighScore_{trackName}", 0), currentScore));
        PlayerPrefs.Save();
    }

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

        // 게임 오버 UI 숨기기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 게임 매니저 정리
        if (gameManager != null)
        {
            gameManager.EndGame();
            gameManager.enabled = false;
        }

        // 저지 매니저 정리
        if (judgeManager != null)
        {
            judgeManager.ClearAllNotes();
            judgeManager.enabled = false;
        }

        // 게임 상태 초기화
        SaveCurrentScore();
        SetGameState(GameState.Menu);
        isGameStarted = false;
        isGameActive = false;
        isPaused = false;

        // UI 초기화
        ShowPressToStart(true);

        // 메뉴로 이동
        menuScroll.ReturnToMenu();
        cameraPositionController.MoveCameraToPosition(1);

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
    public int GetCurrentScore() => currentScore;
    public int GetMaxCombo() => maxCombo;
    public float GetAccuracy() => hitNotes > 0 ? (perfectHits * 100.0f + goodHits * 60.0f) / (hitNotes + misses) : 0f;
    public (int perfect, int good, int miss) GetHitCounts() => (perfectHits, goodHits, misses);
}