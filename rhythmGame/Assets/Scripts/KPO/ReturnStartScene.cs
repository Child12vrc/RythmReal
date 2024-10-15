using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ReturnStartScene : MonoBehaviour
{
    // UI 텍스트 요소들
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI missText;
    public TextMeshProUGUI badText;
    public TextMeshProUGUI goodText;
    public TextMeshProUGUI greatText;
    public TextMeshProUGUI perfectText;

    // 애니메이션 관련 변수들
    public float animationDuration = 2f; // 애니메이션 지속 시간 (초)
    public AnimationCurve animationCurve; // 애니메이션 곡선 (에디터에서 설정 가능)

    public bool isAnimateNumber = true;

    private void Start()
    {
        // 게임 시작시 통계 애니메이션 코루틴 실행
        if(isAnimateNumber)
        StartCoroutine(AnimateStats());
    }

    private IEnumerator AnimateStats()
    {
        // GameManager에서 최종 통계값들을 가져옴
        int targetScore = GameManager.Instance.score;
        int targetMiss = GameManager.Instance.MissCount;
        int targetBad = GameManager.Instance.BadCount;
        int targetGood = GameManager.Instance.GoodCount;
        int targetGreat = GameManager.Instance.GreatCount;
        int targetPerfect = GameManager.Instance.PerfectCount;

        float elapsedTime = 0f;

        // 애니메이션 지속 시간동안 반복
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            float curvedT = animationCurve.Evaluate(t); // 애니메이션 곡선 적용

            // 각 통계에 대해 현재 값을 계산하고 UI를 업데이트
            UpdateText(scoreText, 0, targetScore, curvedT);
            UpdateText(missText, 0, targetMiss, curvedT);
            UpdateText(badText, 0, targetBad, curvedT);
            UpdateText(goodText, 0, targetGood, curvedT);
            UpdateText(greatText, 0, targetGreat, curvedT);
            UpdateText(perfectText, 0, targetPerfect, curvedT);

            yield return null; // 다음 프레임까지 대기
        }

        // 애니메이션 종료 후 정확한 최종값으로 설정
        SetFinalValue(scoreText, targetScore);
        SetFinalValue(missText, targetMiss);
        SetFinalValue(badText, targetBad);
        SetFinalValue(goodText, targetGood);
        SetFinalValue(greatText, targetGreat);
        SetFinalValue(perfectText, targetPerfect);
    }

    // 텍스트 UI를 현재 애니메이션 진행도에 따라 업데이트
    private void UpdateText(TextMeshProUGUI text, int start, int end, float t)
    {
        if (text != null)
        {
            int current = Mathf.RoundToInt(Mathf.Lerp(start, end, t));
            text.text = current.ToString();
        }
    }

    // 텍스트 UI에 최종값 설정
    private void SetFinalValue(TextMeshProUGUI text, int value)
    {
        if (text != null)
        {
            text.text = value.ToString();
        }
    }

    void Update()
    {
        // 스페이스바를 누르면 시작 씬으로 돌아감
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("StartScene");
        }
    }
}