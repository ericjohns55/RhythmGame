using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using System.Security.Cryptography;

public class GameManager : MonoBehaviour
{

    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject ScoreText;
    public GameObject endScreen;
   // public TMP_Text ScoreTextUI;
    public static bool isPaused = false;
    private static string state = "game";
    public GameObject playbackObject;
    private MidiOutput playback;
    private bool resumePlayback = false;
    private bool isGameCompleted = false; 
    private ProgressBar progressBar;
    [SerializeField] private int totalNotes;
    [SerializeField] private int destroyedNotes;
    private ScoreManager scoreManager;
    private float songEndDelay = 0.0f;

    Scene scene;
    public TMP_Text countdown;

    // Start is called before the first frame update
    void Start() {
        
        destroyedNotes = 0;
        scene = SceneManager.GetActiveScene();
        if (scene.name == "GameScene") {
            playback = (MidiOutput) playbackObject.GetComponent("MidiOutput");
            progressBar = (ProgressBar) this.GetComponent("ProgressBar");
            scoreManager = (ScoreManager) this.GetComponent("ScoreManager");
            settingsMenu.SetActive(false);
            pauseMenu.SetActive(false);
            totalNotes = GameObject.FindGameObjectsWithTag("Note").Length;
        } else {
            playback = null;
        }
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if (state != "countdown") {
                if (scene.name == "GameScene") {
                    Debug.Log("Escape");
                    Debug.Log("Is paused = " + isPaused);
                    if(isPaused) {
                        ResumeGame();
                    } else  {
                        resumePlayback = playback.GetPlaybackState();
                        Debug.Log("resumePlayback = " + resumePlayback);
                        PauseGame();
                    }
                }
            }
            
            Debug.Log("Time.timeScale = " + Time.timeScale);
            Debug.Log("TOTAL NOTES---------------: " + totalNotes);
        }
    }

    public void NoteDestroyed() 
    {
        destroyedNotes++;
        if (destroyedNotes >= totalNotes){
            StartCoroutine(EndGameWithDelay());
        }
    }

    IEnumerator EndGameWithDelay() {
        yield return new WaitForSeconds(songEndDelay);
        playback.ReleaseOutputDevice();
        EndGame();
    }

    private void EndGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (scene.name == "GameScene") {
            playback.ReleaseOutputDevice();
            string midiFilePath = PlayerPrefs.GetString("SelectedMidiFilePath", "");
            string hash = ComputeMD5Hash(midiFilePath);
            string difficulty = PlayerPrefs.GetString("SelectedDifficulty");
            if (PlayerPrefs.GetInt(hash + "_" + difficulty + "_Best", 0) < scoreManager.GetScore()) {
                PlayerPrefs.SetInt(hash + "_" + difficulty + "_Best", scoreManager.GetScore());
            }
            PlayerPrefs.SetInt(hash + "_" + difficulty + "_Current", scoreManager.GetScore());
            scoreManager.SaveHits();
        }
        SceneManager.LoadScene("EndGame");
    }

    public void PauseGame() {
        settingsMenu.SetActive(false);
        // ScoreText.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        playback.StopPlayback();
    }

    public void ResumeGame() {
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        StartCoroutine(Resume());        
    }

    // Separate function to resume the game
    // Called with a coroutine to allow for timer countdown
    IEnumerator Resume() {
        state = "countdown";

        for (int i = 3; i > 0; i--) {
            Debug.Log(i);
            countdown.text = i.ToString();
            yield return new WaitForSecondsRealtime(1);
        }

        countdown.text = "";

        Time.timeScale = 1f;
        isPaused = false;
        state = "game";
        if (resumePlayback) {
            playback.StartPlayback();
        }
        // ScoreText.SetActive(true);
    }

    public void GoToSettings() {
        settingsMenu.SetActive(true);
        SceneManager.LoadScene("SettingsScreen");
    }

    public void GoToMidiList() {
        SceneManager.LoadScene("MidiHandler");
    }

    public void MidiListToSettings() {
        SceneManager.LoadScene("SettingsScreen");
    }

    public void GoToMainMenu() {
        Time.timeScale = 1f;
        if (scene.name == "GameScene") {
            Debug.Log("RELEASING PLAYBACK DEVICE");
            playback.ReleaseOutputDevice();
        }
        SceneManager.LoadScene("HomeScreen");
    }
    
    public void GoToGameScene() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        isPaused = false;
    }

    public void GoToLatencyTest() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LatencyTest");
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        isPaused = false;
    }

    public void QuitGame() {
        Debug.Log("Quitting Game");
        Application.Quit();
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
    public void SetNoteCount(int totalNoteCount) {
        totalNotes = totalNoteCount;
    }

    public void SetSongEndDelay(float delay) {
        songEndDelay = delay;
    }
}