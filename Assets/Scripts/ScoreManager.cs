using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps track of the player's score and displays combo count and
/// multiplier.
/// </summary>

public class ScoreManager : MonoBehaviour
{
    private int score = 0;
    public int Score => score;

    public void AddPoints(int points)
    {
        score += points;
        Debug.Log("Points added: " + points);
    }

    public void ResetScore()
    {
        score = 0;
        Debug.Log("Points reset.");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (score > 0)
        {
            Debug.Log("Score: " + score);
        }
    }
}
