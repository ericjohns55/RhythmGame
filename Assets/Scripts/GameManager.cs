using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public GameObject pauseMenu;
    public static bool isPaused;

    Scene scene;

    // Start is called before the first frame update
    void Start() {
        pauseMenu.SetActive(false);
        Debug.Log("Hola");
        scene = SceneManager.GetActiveScene();
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if (scene.name == "GameScene") {
                Debug.Log("Escape");
                if(isPaused){
                    ResumeGame();
                }
                else  {
                    PauseGame();
                }
            }
        }
    }

    #region PauseFunctions

    public void PauseGame() {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame() {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void GoToMainMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    #endregion
    
    public void GoToGameScene() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame() {
        Debug.Log("Quitting Game");
        Application.Quit();
    }
}