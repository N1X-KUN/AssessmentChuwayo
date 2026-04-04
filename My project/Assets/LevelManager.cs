using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Needed for the countdown timer!

public class LevelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider progressBar;
    public TMP_Text introText; // NEW: For the 3-2-1 GO countdown
    public Animator handleAnimator; // NEW: For the running UI character

    [Header("Level Settings")]
    public float levelDuration = 60f; 
    private float timeElapsed = 0f;
    public bool gameIsActive = false; // Prevents the game from running during the intro

    [Header("References")]
    public KommyController kommy;
    public WordManager wordManager;
    public ThiefController thief;

    void Start()
    {
        if (progressBar != null)
        {
            progressBar.maxValue = levelDuration;
            progressBar.value = 0f;
        }
        
        // Starts the 3-2-1 Countdown before anything else can happen!
        StartCoroutine(LevelIntroRoutine()); 
    }

    private IEnumerator LevelIntroRoutine()
    {
        gameIsActive = false; // Freeze the game
        
        if (introText != null)
        {
            introText.gameObject.SetActive(true);
            introText.text = "Go";
            
            // Sync these delays with your Board Drop animation!
            yield return new WaitForSeconds(1f); 
            
            introText.text = "Get";
            yield return new WaitForSeconds(1f);
            
            introText.text = "HIM!";
            yield return new WaitForSeconds(1f);
            
            introText.gameObject.SetActive(false); // Hide the text
        }

        // UNFREEZE AND START!
        gameIsActive = true;
        kommy.StartGame();
        wordManager.StartSpawning();
    }

    void Update()
    {
        // If the intro is still playing, do not run the timer!
        if (!gameIsActive) return; 

        if (kommy.currentState != KommyController.CharacterState.Dead && kommy.currentState != KommyController.CharacterState.Victory)
        {
            // By using Time.deltaTime, the progress bar AUTOMATICALLY slows down when Kommy is in slow-mo!
            timeElapsed += Time.deltaTime;
            
            if (progressBar != null)
            {
                progressBar.value = timeElapsed;
            }

            // --- VICTORY CONDITION MET ---
            if (timeElapsed >= levelDuration)
            {
                kommy.WinGame(); 
                wordManager.CancelInvoke(); 
                gameIsActive = false; // Stop the timer
                
                if (thief != null) thief.TriggerDefeat();
                if (handleAnimator != null) handleAnimator.Play("LoadingWIN"); // Trigger UI Dance!
            }
        }
        // --- DEFEAT CONDITION MET ---
        else if (kommy.currentState == KommyController.CharacterState.Dead)
        {
            gameIsActive = false; // Stop the timer
            wordManager.CancelInvoke();
            
            if (handleAnimator != null) handleAnimator.Play("LoadingLOS"); // Trigger UI Crying!
        }
    }
}