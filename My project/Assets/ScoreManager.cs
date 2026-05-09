using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Panels")]
    public GameObject scoreboardPanel; 

    [Header("Text Elements")]
    public TMP_Text resultText; 
    public TMP_Text scoreNumberText;
    public TMP_Text coinNumberText; 

    [Header("Star Images")]
    public Image star1;
    public Image star2;
    public Image star3;
    public Sprite goldStarSprite; 

    [Header("Buttons")]
    public GameObject homeButton;
    public GameObject nextButton;
    public GameObject retryButton;

    private WordManager wordManager;

    void Awake()
    {
        // Safety lock: Instantly destroy itself if it accidentally ends up in the Tutorial
        if (SceneManager.GetActiveScene().name == "Tutorial") { Destroy(gameObject); return; }
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
        yield return new WaitForSecondsRealtime(1.5f);

        bool isFirstTime = PlayerPrefs.GetInt("HasFinishedLevel1", 0) == 0; // FIXED THE MEMORY KEY

        if (!isVictory && isFirstTime)
        {
            DialogueManager dm = FindAnyObjectByType<DialogueManager>();
            if (dm != null) dm.PlayDialogue("TutorialLose");
        }
        else
        {
            ShowScoreboard(isVictory);
        }
    }

    private void ShowScoreboard(bool isVictory)
    {
        scoreboardPanel.SetActive(true);
        Time.timeScale = 0f; 

        homeButton.SetActive(true);
        
        if (isVictory)
        {
            resultText.text = "VICTORY";
            nextButton.SetActive(true);
            retryButton.SetActive(false);
            
            PlayerPrefs.SetInt("HasFinishedLevel1", 1); // FIXED THE MEMORY KEY
            PlayerPrefs.SetInt("UnlockedLevel", 2); 
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

        int targetCoins = Mathf.FloorToInt(finalScore / 10f);
        if (!isVictory) targetCoins = Mathf.FloorToInt(targetCoins / 2f); 

        float duration = 1.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            int currentDisplayScore = Mathf.FloorToInt(Mathf.Lerp(0, finalScore, elapsed / duration));
            scoreNumberText.text = currentDisplayScore.ToString();
            yield return null;
        }
        scoreNumberText.text = finalScore.ToString(); 

        coinNumberText.text = targetCoins.ToString();
        if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop);
        yield return new WaitForSecondsRealtime(0.5f);

        if (isVictory)
        {
            if (finalScore >= 35) { star1.sprite = goldStarSprite; if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop); yield return new WaitForSecondsRealtime(0.4f); }
            if (finalScore >= 65) { star2.sprite = goldStarSprite; if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop); yield return new WaitForSecondsRealtime(0.4f); }
            if (finalScore >= 100) { star3.sprite = goldStarSprite; if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop); }
        }
        
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        PlayerPrefs.SetInt("TotalCoins", totalCoins + targetCoins);
        PlayerPrefs.Save();
    }

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