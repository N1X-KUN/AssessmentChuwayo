using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Panels")]
    public GameObject scoreboardPanel; // Drag your 'Scoreboard' parent object here

    [Header("Text Elements")]
    public TMP_Text resultText; // VICTORY or DEFEAT
    public TMP_Text scoreNumberText;
    public TMP_Text coinNumberText; // Your 'ScoreNumberText (1)'

    [Header("Star Images")]
    public Image star1;
    public Image star2;
    public Image star3;
    public Sprite goldStarSprite; // Drag 'Icon_Small_Star' here

    [Header("Buttons")]
    public GameObject homeButton;
    public GameObject nextButton;
    public GameObject retryButton;

    private WordManager wordManager;

    void Awake()
    {
        Instance = this; 
    }

    void Start()
    {
        if (scoreboardPanel != null) scoreboardPanel.SetActive(false);
        wordManager = FindAnyObjectByType<WordManager>();
    }

    public void TriggerEndGame(bool isVictory)
    {
        StartCoroutine(EndGameSequence(isVictory));
    }

    private IEnumerator EndGameSequence(bool isVictory)
    {
        // Wait 1.5 seconds for the character's Win/Die animation to finish playing
        yield return new WaitForSecondsRealtime(1.5f);

        bool isFirstTime = PlayerPrefs.GetInt("HasFinishedTutorial", 0) == 0;

        if (!isVictory && isFirstTime)
        {
            // FIRST TIME LOSE: Hide scoreboard, play dialogue!
            DialogueManager dm = FindAnyObjectByType<DialogueManager>();
            if (dm != null) dm.PlayDialogue("TutorialLose");
        }
        else
        {
            // WIN OR REPLAY LOSE: Show the Scoreboard!
            ShowScoreboard(isVictory);
        }
    }

    private void ShowScoreboard(bool isVictory)
    {
        scoreboardPanel.SetActive(true);
        Time.timeScale = 0f; // Freeze game background

        homeButton.SetActive(true);
        
        if (isVictory)
        {
            resultText.text = "VICTORY";
            nextButton.SetActive(true);
            retryButton.SetActive(false);
            
            // Mark Level 1 as beaten permanently!
            PlayerPrefs.SetInt("HasFinishedTutorial", 1); 
            PlayerPrefs.Save();
        }
        else
        {
            resultText.text = "DEFEAT";
            nextButton.SetActive(false);
            retryButton.SetActive(true);
        }

        StartCoroutine(AnimateScoreboard(isVictory));
    }

    private IEnumerator AnimateScoreboard(bool isVictory)
    {
        int finalScore = 0;
        if (wordManager != null) finalScore = wordManager.currentScore;

        // CALCULATE COINS (10 points = 1 coin)
        int targetCoins = Mathf.FloorToInt(finalScore / 10f);
        
        // IF DEFEAT: Half the coins!
        if (!isVictory) targetCoins = Mathf.FloorToInt(targetCoins / 2f); 

        // 1. ANIMATE THE SCORE COUNTING UP
        float duration = 1.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            // Lerp handles the smooth number climbing
            int currentDisplayScore = Mathf.FloorToInt(Mathf.Lerp(0, finalScore, elapsed / duration));
            scoreNumberText.text = currentDisplayScore.ToString();
            yield return null;
        }
        scoreNumberText.text = finalScore.ToString(); // Snap to exact score

        // 2. POP COINS
        coinNumberText.text = targetCoins.ToString();
        if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop);
        yield return new WaitForSecondsRealtime(0.5f);

        // 3. POP STARS (Only if they won!)
        if (isVictory)
        {
            if (finalScore >= 35)
            {
                star1.sprite = goldStarSprite;
                if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop);
                yield return new WaitForSecondsRealtime(0.4f);
            }
            if (finalScore >= 65)
            {
                star2.sprite = goldStarSprite;
                if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop);
                yield return new WaitForSecondsRealtime(0.4f);
            }
            if (finalScore >= 100)
            {
                star3.sprite = goldStarSprite;
                if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop);
            }
        }
        
        // 4. SAVE COINS TO THE BANK
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        PlayerPrefs.SetInt("TotalCoins", totalCoins + targetCoins);
        PlayerPrefs.Save();
    }

    // --- BUTTON FUNCTIONS ---
    public void Button_Home()
    {
        Time.timeScale = 1f;
        if (LoadingManager.Instance != null) LoadingManager.Instance.LoadNewScene("MenuScene");
        else SceneManager.LoadScene("MenuScene"); 
    }

    public void Button_Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Button_NextLevel()
    {
        Time.timeScale = 1f;
        if (LoadingManager.Instance != null) LoadingManager.Instance.LoadNewScene("MapScene");
        else SceneManager.LoadScene("MapScene"); 
    }
}