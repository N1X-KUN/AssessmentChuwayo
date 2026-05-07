using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 

public class LevelManager : MonoBehaviour
{
    [Header("Dynamic Spawning System")]
    public bool isTutorialLevel = false; 
    public Transform playerSpawnPoint; 
    public GameObject kommyPrefab;
    public GameObject tigPrefab;
    
    [Header("Dynamic UI Overlays")]
    public GameObject kommyHeroUI;
    public GameObject tigHeroUI;
    public GameObject kommyScoreUI; 
    public GameObject tigScoreUI;   

    [Header("UI Elements")]
    public Slider progressBar;
    public Image progressFill; 
    public TMP_Text introText; 
    public Animator handleAnimator; 

    [Header("Colors")]
    public Color normalColor = Color.yellow;
    public Color slowMoColor = Color.magenta; 

    [Header("Level Settings")]
    public float levelDuration = 60f; 
    private float timeElapsed = 0f;
    public bool gameIsActive = false; 

    [Header("References")]
    public KommyController kommy;
    public WordManager wordManager;
    public ThiefController thief;

    void Awake()
    {
        SetupPlayerAndUI();
    }

    void SetupPlayerAndUI()
    {
        string equippedChar = PlayerPrefs.GetString("EquippedCharacter", "Tig");

        if (isTutorialLevel)
        {
            equippedChar = "Kommy";
        }

        if (kommyHeroUI != null) kommyHeroUI.SetActive(false);
        if (tigHeroUI != null) tigHeroUI.SetActive(false);
        if (kommyScoreUI != null) kommyScoreUI.SetActive(false);
        if (tigScoreUI != null) tigScoreUI.SetActive(false);

        GameObject spawnedPlayer = null;

        // --- FIXED: We now use playerSpawnPoint.rotation so she faces the right way! ---
        if (equippedChar == "Tig" && tigPrefab != null)
        {
            spawnedPlayer = Instantiate(tigPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            if (tigHeroUI != null) tigHeroUI.SetActive(true);
            if (tigScoreUI != null) tigScoreUI.SetActive(true); 
        }
        else 
        {
            spawnedPlayer = Instantiate(kommyPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            if (kommyHeroUI != null) kommyHeroUI.SetActive(true);
            if (kommyScoreUI != null) kommyScoreUI.SetActive(true); 
        }

        if (spawnedPlayer != null)
        {
            kommy = spawnedPlayer.GetComponent<KommyController>();
            
            // --- FIXED: Tell the WordManager about the NEW live player! ---
            if (wordManager != null) 
            {
                wordManager.kommy = kommy; 
            }
        }
    }

    void Start()
    {
        if (progressBar != null)
        {
            progressBar.maxValue = levelDuration;
            progressBar.value = 0f;
        }
        
        if (progressFill != null) progressFill.color = normalColor; 
        
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
        if (wordManager != null) wordManager.StartSpawning();

        DialogueManager dm = FindAnyObjectByType<DialogueManager>();
        if (dm != null && dm.isTutorialMode)
        {
            dm.PlayDialogue("IntroProgression");
        }
    }

    void Update()
    {
        if (!gameIsActive) return; 

        if (wordManager != null && wordManager.isControlledTutorialActive) return;

        if (kommy != null && kommy.currentState != KommyController.CharacterState.Dead && kommy.currentState != KommyController.CharacterState.Victory)
        {
            timeElapsed += Time.deltaTime;
            
            if (progressBar != null) progressBar.value = timeElapsed;

            if (progressFill != null)
            {
                progressFill.color = kommy.isAbilityActive ? slowMoColor : normalColor;
            }

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