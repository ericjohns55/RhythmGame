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
    [SerializeField] private TextMeshProUGUI scoreText;

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
        scoreText = GetComponent<TextMeshProUGUI>();
        if (scoreText == null)
        {
            Debug.LogError("No TextMeshProUGUI component found attached to the GameObject.");
        }
        else
        {
            UpdateScoreText();
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    public void IncrementComboAndScore(int points)
    {
        comboStreak++;
        AddPoints(points * comboMultiplier);
        UpdateComboMultiplier();
    }

    private void UpdateComboMultiplier()
    {
        if (comboStreak % 5 == 0)
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
