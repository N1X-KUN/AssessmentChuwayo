using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 

public class LevelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider progressBar;
    public TMP_Text introText; 
    public Animator handleAnimator; 

    [Header("Level Settings")]
    public float levelDuration = 60f; 
    private float timeElapsed = 0f;
    public bool gameIsActive = false; 

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
        StartCoroutine(LevelIntroRoutine()); 
    }

    private IEnumerator LevelIntroRoutine()
    {
        gameIsActive = false; 
        
        if (introText != null)
        {
            introText.gameObject.SetActive(true);
            introText.text = "Go";
            yield return new WaitForSeconds(1f); 
            
            introText.text = "Get";
            yield return new WaitForSeconds(1f);
            
            introText.text = "HIM!";
            yield return new WaitForSeconds(1f);
            
            introText.gameObject.SetActive(false); 
        }

        gameIsActive = true;
        if (kommy != null) kommy.StartGame();
    }

    void Update()
    {
        if (!gameIsActive) return; 

        // Added null checks here so the game never crashes!
        if (kommy != null && kommy.currentState != KommyController.CharacterState.Dead && kommy.currentState != KommyController.CharacterState.Victory)
        {
            timeElapsed += Time.deltaTime;
            
            if (progressBar != null) progressBar.value = timeElapsed;

            if (timeElapsed >= levelDuration)
            {
                kommy.WinGame(); 
                if (wordManager != null) wordManager.CancelInvoke(); 
                gameIsActive = false; 
                
                if (thief != null) thief.TriggerDefeat();
                if (handleAnimator != null) handleAnimator.Play("LoadingWIN"); 
            }
        }
        else if (kommy != null && kommy.currentState == KommyController.CharacterState.Dead)
        {
            gameIsActive = false; 
            if (wordManager != null) wordManager.CancelInvoke();
            if (handleAnimator != null) handleAnimator.Play("LoadingLOS"); 
        }
    }
}