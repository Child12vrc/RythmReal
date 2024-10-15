using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum Timing
{
    Perfect = 4,
    Great = 3,
    Good = 2,
    Bad = 1,
    Miss = 0
}
public class ScoreManager : MonoBehaviour
{
    public float HP { get; private set; }
    public float score { get; private set; }
    public int combo { get; private set; }
    public int MaxCombo { get; private set; }
    private int[] ScoreCount = new int[5];          //0번부터 Miss, Bad, Good, Great, Perfect의 개수

    public GameObject comboUI;

    private float targetScore;
    private Coroutine scoreAnimationCoroutine;

    private void Start()
    {
        ResetScore();
    }

    public void AddScore(Timing timing)
    {
        float newScore = (int)timing * (1 + combo * 0.1f);
        targetScore += newScore;
        ScoreCount[(int)timing] += 1;

        if (timing == Timing.Miss || timing == Timing.Bad)
        {
            combo = 0;
            comboUI.SetActive(false);
            if (timing == Timing.Miss)
            {
                HP -= 8;
            }
        }
        else
        {
            combo++;
            comboUI.SetActive(true);
            comboUI.transform.DOPunchScale(new Vector3(1.0f,1.0f,1.0f), 0.2f);
            if (combo > MaxCombo)
            {
                MaxCombo = combo;
                HP += 1;
            }
        }

        if (scoreAnimationCoroutine != null)
        {
            StopCoroutine(scoreAnimationCoroutine);
        }
        scoreAnimationCoroutine = StartCoroutine(AnimateScoreIncrease());
    }

    private IEnumerator AnimateScoreIncrease()
    {
        while (score < targetScore)
        {
            score = Mathf.MoveTowards(score, targetScore, Time.deltaTime * 100); // 초당 100점 속도로 증가
            SendScore();
            yield return null;
        }
        score = targetScore;
        SendScore();
    }

    public void ResetScore()
    {
        HP = 100;
        score = 0;
        targetScore = 0;
        MaxCombo = 0;
        combo = 0;
        ScoreCount = new int[5];
        SendScore();
    }

    public int[] CheckCount()
    {
        return ScoreCount;
    }

    public void SendScore()
    {
        GameManager.Instance.SendScore(Mathf.RoundToInt(score), combo, ScoreCount);
    }
}