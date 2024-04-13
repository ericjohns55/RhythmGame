using UnityEngine;
using TMPro;

///<summary>
/// Keeps track of the points scored by the player and displays these points
/// as text on the screen.
///<summary>
public class ScoreManager : MonoBehaviour
{
    [SerializeField] private float score = 0;
    public float Score => score;
    [SerializeField] private TMP_Text scoreText;

    [SerializeField] private float comboStreak = 0;
    [SerializeField] private float comboMultiplier = 1;

    public void AddPoints(float points)
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
        AddPoints(points * comboMultiplier);
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
}
