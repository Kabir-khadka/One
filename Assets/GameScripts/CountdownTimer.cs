using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Or TMPro if using TMP

public class CountdownTimer : MonoBehaviour
{
    public TextMeshProUGUI countdownText; // Or TextMeshProUGUI if using TMP
    private AnimandMovement playerInputHandler; // Your input/movement script

    private void Start()
    {

        // Freeze time completely
        Time.timeScale = 0f;

        playerInputHandler = GetComponent<AnimandMovement>(); // change this to your actual script name
        if (playerInputHandler != null)
            playerInputHandler.enabled = false;

        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSecondsRealtime(1f); // << THIS

        countdownText.text = "2";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "1";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "GO!";
        yield return new WaitForSecondsRealtime(0.6f);

        countdownText.gameObject.SetActive(false);

        // Resume game and inputs
        Time.timeScale = 1f;

        if (playerInputHandler != null)
            playerInputHandler.enabled = true;
    }

}
