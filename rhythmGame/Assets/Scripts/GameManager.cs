using RhythmGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int score;

    public int MissCount;
    public int BadCount;
    public int GoodCount;
    public int GreatCount;
    public int PerfectCount;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public LevelData LevelData { get; private set; }

    public float Score { get; private set; }

    public int MaxCombo { get; private set; }

    public void SetLevelData(LevelData levelData)
    {
        LevelData = levelData;
    }

    public void SendScore(float score ,int maxCombo, int[] Count)
    {
        Score = score;
        this.score = (int)score;
        MaxCombo = maxCombo;

        MissCount = Count[0];
        BadCount = Count[1];
        GoodCount = Count[2];
        GreatCount = Count[3];
        PerfectCount = Count[4];
    }
}
