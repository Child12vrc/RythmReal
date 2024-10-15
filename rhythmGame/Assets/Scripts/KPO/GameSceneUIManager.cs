using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSceneUIManager : MonoBehaviour
{
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI ComboText;
    public Slider HPSlider;

    private NoteManager noteManager;
    private ScoreManager scoreManager;

    // Start is called before the first frame update
    void Start()
    {
        noteManager = NoteManager.instance;
        scoreManager = noteManager.scoreManager;
    }

    // Update is called once per frame
    void Update()
    {
        float score = scoreManager.score;
        ScoreText.text = score == 0 ? "" : score.ToString("F0");
        int combo = scoreManager.combo;
        ComboText.text = combo == 0 ? "" : combo.ToString();
        HPSlider.value = scoreManager.HP;
    }
}
