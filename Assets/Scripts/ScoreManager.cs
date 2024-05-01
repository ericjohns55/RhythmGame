using UnityEngine;
using TMPro;

///<summary>
/// Keeps track of the points scored by the player and displays these points
/// as text on the screen.
///<summary>
public class ScoreManager : MonoBehaviour
{
    [SerializeField] private int score = 0;
    public int Score => score;
    [SerializeField] private TMP_Text scoreText;

    [SerializeField] private int comboStreak = 0;
    [SerializeField] private float comboMultiplier = 1;

    private int miss = 0;
    private int awful = 0;
    private int good = 0;
    private int excellent = 0;

    public void AddPoints(int points)
    {
        score += points;
        UpdateScoreText();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();
    }

    void Start()
    {
        miss = 0;
        awful = 0;
        good = 0;
        excellent = 0;
        PlayerPrefs.SetInt("miss", 0);
        PlayerPrefs.SetInt("awful", 0);
        PlayerPrefs.SetInt("good", 0);
        PlayerPrefs.SetInt("excellent", 0);
        UpdateScoreText();
    }

    void Update() {
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {

            scoreText.text = "<mspace=0.75em> Score " + score.ToString() + "\nStreak " + comboStreak.ToString() + "\n Combo " + comboMultiplier.ToString() + "</mspace>";

        }
    }

    public void IncrementComboAndScore(int points)
    {
        comboStreak++;
        AddPoints((int) (points * comboMultiplier));
        if (comboStreak % 10 == 0) // every 10 hits
        {
            UpdateComboMultiplier();
        }
    }

    private void UpdateComboMultiplier()
    {
        if (comboMultiplier < 4.0f)
        {
            comboMultiplier += 0.5f;
        }
    }

    public void ResetCombo()
    {
        comboStreak = 0;
        comboMultiplier = 1;
    }

    public void ShowText()
    {
        if (scoreText != null)
        {
            scoreText.enabled = true;
        }
    }

    public void HideText()
    {
        if(scoreText != null)
        {
            scoreText.enabled = false;
        }
    }

    public int GetScore() {
        return score;
    }

    public void IncrementMiss() {
        miss++;
    }

    public void IncrementAwful() {
        awful++;
    }

    public void IncrementGood() {
        good++;
    }

    public void IncrementExcellent() {
        excellent++;
    }

    public int GetMiss() {
        return miss;
    }

    public int GetAwful() {
        return awful;
    }

    public int GetGood() {
        return good;
    }

    public int GetExcellent() {
        return excellent;
    }

    public void SaveHits() {
        PlayerPrefs.SetInt("miss", miss);
        PlayerPrefs.SetInt("awful", awful);
        PlayerPrefs.SetInt("good", good);
        PlayerPrefs.SetInt("excellent", excellent);
    }
}
