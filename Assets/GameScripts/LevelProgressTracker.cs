using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelProgressTracker : MonoBehaviour
{
    private bool hasReachedFinish = false;
    private bool levelCompleted = false;
    private bool isProcessingCompletion = false;
    private bool hasShownReturnMessage = false;

    public GameObject crosshair; // Crosshair image
    public GameObject levelCompleteUI; // "Mission Complete" panel
    public float delayBeforeStoppingGame = 1.0f; // Wait before freezing game

    public Animator playerAnimator; // Optional
    public MonoBehaviour playerInputHandler; // ?? Assign your input script here
    public TextMeshProUGUI returnMessage;


    private void OnTriggerEnter(Collider other)
    {
        if (levelCompleted || isProcessingCompletion) return;

        if (other.CompareTag("FinishPlatform"))
        {
            hasReachedFinish = true;
            Debug.Log("Finish platform reached.");

            if (returnMessage != null && !hasShownReturnMessage)
            {
                hasShownReturnMessage = true;
                StartCoroutine(ShowReturnMessage());
            }
               
        }

        if (other.CompareTag("StartPlatform") && hasReachedFinish)
        {
            isProcessingCompletion = true;
            StartCoroutine(CompleteLevelWithDelay());
        }
    }

    private IEnumerator ShowReturnMessage()
    {
        returnMessage.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        returnMessage.gameObject.SetActive(false);
    }

    private IEnumerator CompleteLevelWithDelay()
    {
        // ?? Show UI immediately
        if (levelCompleteUI != null)
            levelCompleteUI.SetActive(true);

        // ?? Hide crosshair
        if (crosshair != null)
            crosshair.SetActive(false);

        // ?? Disable player input right away
        if (playerInputHandler != null)
            playerInputHandler.enabled = false;

        levelCompleted = true;
        Debug.Log("Returned to Start after Finish — Level Completed!");

        // Let Rigidbody continue for natural stopping (e.g., sliding, rotation)
        yield return new WaitForSeconds(delayBeforeStoppingGame);

        // ?? THEN freeze game completely
        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
