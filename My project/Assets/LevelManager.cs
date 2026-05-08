using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 

public class LevelManager : MonoBehaviour
{
    // --- NEW: A flexible container for infinite cinematic steps! ---
    [System.Serializable]
    public class CinematicPhase
    {
        public string sequenceName; // e.g. "Level1Intro"
        [TextArea(2, 5)] 
        public string narrationAfter; // The text on the black screen. Leave empty to skip!
    }

    [Header("Dynamic Spawning System")]
    public bool isTutorialLevel = false; 
    public Transform playerSpawnPoint; 
    public GameObject kommyPrefab;
    public GameObject tigPrefab;

    [Header("Cinematic Intro Setup")]
    public bool playCinematicIntro = false; 
    
    [Tooltip("Add as many phases as you want here!")]
    public CinematicPhase[] cinematicPhases; 
    
    public Image fadeBlackScreen; 
    public TMP_Text cinematicNarrationText; 
    public float fadeSpeed = 1.5f; 
    
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
        // --- NEW: CINEMATIC MEMORY CHECK ---
        if (playCinematicIntro)
        {
            // If the game remembers we already saw the intro, force skip it!
            if (PlayerPrefs.GetInt("HasSeenLevel1Intro", 0) == 1)
            {
                playCinematicIntro = false;
            }
            else
            {
                // Otherwise, mark it in memory so we NEVER see it again on this save file
                PlayerPrefs.SetInt("HasSeenLevel1Intro", 1);
                PlayerPrefs.Save();
            }
        }
        // -----------------------------------

        SetupPlayerAndUI();
    }

    void SetupPlayerAndUI()
    {
        string equippedChar = PlayerPrefs.GetString("EquippedCharacter", "Tig");

        if (isTutorialLevel) equippedChar = "Kommy";

        if (kommyHeroUI != null) kommyHeroUI.SetActive(false);
        if (tigHeroUI != null) tigHeroUI.SetActive(false);
        if (kommyScoreUI != null) kommyScoreUI.SetActive(false);
        if (tigScoreUI != null) tigScoreUI.SetActive(false);

        GameObject spawnedPlayer = null;

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
            if (wordManager != null) wordManager.kommy = kommy; 
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
        
        if (fadeBlackScreen != null)
        {
            Color c = fadeBlackScreen.color;
            c.a = 0f;
            fadeBlackScreen.color = c;
            fadeBlackScreen.gameObject.SetActive(false);
        }

        if (cinematicNarrationText != null)
        {
            Color tc = cinematicNarrationText.color;
            tc.a = 0f;
            cinematicNarrationText.color = tc;
            cinematicNarrationText.gameObject.SetActive(false);
        }

        // THE FIX: Wait 1 frame before starting the movie!
        StartCoroutine(DelayedIntroStart()); 
    }

    private IEnumerator DelayedIntroStart()
    {
        yield return null; 
        StartCoroutine(LevelIntroRoutine());
    }

    private IEnumerator LevelIntroRoutine()
    {
        gameIsActive = false; 
        
        if (playCinematicIntro && cinematicPhases != null && cinematicPhases.Length > 0)
        {
            DialogueManager dm = FindAnyObjectByType<DialogueManager>();
            if (dm != null)
            {
                // 1. FADE TO BLACK BEFORE THE MOVIE STARTS! (Hides the game)
                if (fadeBlackScreen != null)
                {
                    fadeBlackScreen.gameObject.SetActive(true);
                    Color c = fadeBlackScreen.color;
                    while (c.a < 1f) 
                    { 
                        // Mathf.MoveTowards stops the Alt-Tab freezing bug!
                        c.a = Mathf.MoveTowards(c.a, 1f, Time.unscaledDeltaTime * fadeSpeed); 
                        fadeBlackScreen.color = c; 
                        yield return null; 
                    }
                }

                // 2. PLAY THE ENTIRE CINEMATIC
                foreach (CinematicPhase phase in cinematicPhases)
                {
                    // Play Dialogue (Custom backgrounds will draw over the black screen safely!)
                    if (!string.IsNullOrEmpty(phase.sequenceName))
                    {
                        dm.PlayDialogue(phase.sequenceName);
                        yield return new WaitUntil(() => !dm.dialogueIsActive);
                    }

                    // Play Narration (Black screen is already behind it!)
                    if (!string.IsNullOrEmpty(phase.narrationAfter))
                    {
                        if (cinematicNarrationText != null)
                        {
                            string finalNarration = phase.narrationAfter;
                            if (finalNarration.Contains("@playername"))
                            {
                                string savedName = PlayerPrefs.GetString("PlayerName", "Player");
                                finalNarration = finalNarration.Replace("@playername", savedName);
                            }
                            
                            cinematicNarrationText.text = finalNarration;
                            cinematicNarrationText.gameObject.SetActive(true);
                            Color tc = cinematicNarrationText.color;
                            
                            // Fade In Text
                            while (tc.a < 1f) { tc.a = Mathf.MoveTowards(tc.a, 1f, Time.unscaledDeltaTime * fadeSpeed); cinematicNarrationText.color = tc; yield return null; }
                            
                            yield return new WaitForSecondsRealtime(2.5f); // Read time
                            
                            // Fade Out Text
                            while (tc.a > 0f) { tc.a = Mathf.MoveTowards(tc.a, 0f, Time.unscaledDeltaTime * fadeSpeed); cinematicNarrationText.color = tc; yield return null; }
                            
                            cinematicNarrationText.gameObject.SetActive(false);
                        }
                    }
                }

                // 3. MOVIE FINISHED! FADE THE BLACK SCREEN AWAY TO REVEAL THE GAME
                if (fadeBlackScreen != null)
                {
                    Color c = fadeBlackScreen.color;
                    while (c.a > 0f) 
                    { 
                        c.a = Mathf.MoveTowards(c.a, 0f, Time.unscaledDeltaTime * fadeSpeed); 
                        fadeBlackScreen.color = c; 
                        yield return null; 
                    }
                    fadeBlackScreen.gameObject.SetActive(false);
                }
            }
        }

        // ==========================================
        // PHASE 3: THE "GO GET HIM" COUNTDOWN
        // ==========================================
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

        DialogueManager backupDm = FindAnyObjectByType<DialogueManager>();
        if (backupDm != null && backupDm.isTutorialMode && !playCinematicIntro)
        {
            backupDm.PlayDialogue("IntroProgression");
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