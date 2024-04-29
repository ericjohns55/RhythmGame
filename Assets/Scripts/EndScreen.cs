using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using TMPro;
using System.Security.Cryptography;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    // Start is called before the first frame update
    void Start()
    {
        scoreText = this.GetComponent<TMP_Text>();

        string hash = ComputeMD5Hash(PlayerPrefs.GetString("SelectedMidiFilePath"));
        string difficulty = PlayerPrefs.GetString("SelectedDifficulty");

        scoreText.text = "<mspace=0.75em>    Score " + PlayerPrefs.GetInt(hash + "_" + difficulty + "_Current")
        + "\n     Best " + PlayerPrefs.GetInt(hash + "_" + difficulty + "_Best")
        + "\nExcellent " + PlayerPrefs.GetInt("excellent")
        + "\n     Good " + PlayerPrefs.GetInt("good")
        + "\n    Awful " + PlayerPrefs.GetInt("awful")
        + "\n     Miss " + PlayerPrefs.GetInt("miss") + "</mspace>";
    }

    string ComputeMD5Hash(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                // Convert the byte array to hexadecimal string
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
