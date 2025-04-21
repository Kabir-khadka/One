using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button MenuButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    [SerializeField]  private static bool GameIsPaused = false;

    [SerializeField] GameObject PauseMenu;

    // Called to display the completion text

    private void Awake()
    {
        resumeButton.onClick.AddListener(() =>
        {
             Resume();
        });

        restartButton.onClick.AddListener(() =>
        {
             RestartGame();
        });

        MenuButton.onClick.AddListener(() =>
        {
             MainMenu();
        });

        quitButton.onClick.AddListener(() =>
        {
            QuitGame();
        });

        pauseButton.onClick.AddListener(() =>
        {
            Debug.Log("Paused");
            if(GameIsPaused){
                Resume();
            }else{
                Pause();
            }
        });

    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(GameIsPaused){
                Resume();
            }else{
                Pause();
            }
        }
    }

    public void Resume(){
        PauseMenu.SetActive(false);
        Time.timeScale = 1.0f;
        GameIsPaused = false;
    }

    public void Pause(){
        PauseMenu.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        if(Time.timeScale == 0f){
             Time.timeScale = 1.0f;
             GameIsPaused = false;
        }
    }

    public void StartGame(){
        SceneManager.LoadScene(1);
    }

    public void QuitGame(){
        Debug.Log("Quit game");
        Application.Quit();
    }

    public void MainMenu(){
        SceneManager.LoadScene(0);
    }
}
