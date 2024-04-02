using UnityEngine;
using TMPro;

///<summary>
/// Keeps track of the points scored by the player and displays these points
/// as text on the screen.
///<summary>
public class ScoreManager : MonoBehaviour
{
    private float score = 0;
    public float Score => score;
    private TextMeshProUGUI scoreText;

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
            Debug.Log("No TextMeshProUGUI component found attached to the GameObject.");
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
