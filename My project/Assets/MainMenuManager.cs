using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Phase 1 & 3: Pop-up Panels")]
    public GameObject dimBackground;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject charactersPanel; 
    public GameObject noticePanel; 

    [Header("Phase 2: Reset System")]
    public GameObject resetConfirmPanel; 

    [Header("Phase 2: Volume Control")]
    public Slider volumeSlider;
    public AudioSource backgroundMusic; 

    void Start()
    {
        CloseAllPopups();

        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("SavedVolume", 1f);
            UpdateVolume(volumeSlider.value);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0f) return; 

            // IRONCLAD LOCK 1: Block ESC completely if the player hasn't finished the name intro!
            if (SceneManager.GetActiveScene().name == "MapScene")
            {
                if (PlayerPrefs.GetString("PlayerName", "") == "") return; 
            }

            // IRONCLAD LOCK 2: Block ESC if ANY dialogue is on screen!
            DialogueManager dm = FindFirstObjectByType<DialogueManager>();
            if (dm != null && (dm.dialogueIsActive || dm.keepOpenOnEnd)) return;
            
            if (!settingsPanel.activeSelf) OpenSettings();
            else CloseAllPopups();
        }
    }

    public void PlayGame()
    {
        int hasPlayed = PlayerPrefs.GetInt("HasFinishedTutorial", 0);

        if (hasPlayed == 0) 
        {
            LoadingManager.Instance.LoadNewScene("TutorialLevel");
        }
        else 
        {
            LoadingManager.Instance.LoadNewScene("MapScene");
        }
    }

    public void GoHome()
    {
        Time.timeScale = 1f; 
        CloseAllPopups();
        if (LoadingManager.Instance != null) LoadingManager.Instance.LoadNewScene("MenuScene");
        else SceneManager.LoadScene("MenuScene");
    }

    public void OpenNotice() { settingsPanel.SetActive(false); noticePanel.SetActive(true); }
    public void CloseNotice() { noticePanel.SetActive(false); settingsPanel.SetActive(true); }

    public void OpenSettings() { dimBackground.SetActive(true); settingsPanel.SetActive(true); }
    public void OpenCredits() { dimBackground.SetActive(true); creditsPanel.SetActive(true); }
    public void OpenCharacters() { CloseAllPopups(); dimBackground.SetActive(true); charactersPanel.SetActive(true); }

    public void CloseCharacters() { charactersPanel.SetActive(false); settingsPanel.SetActive(true); }

    public void CloseAllPopups()
    {
        dimBackground.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        if (resetConfirmPanel != null) resetConfirmPanel.SetActive(false);
        if (charactersPanel != null) charactersPanel.SetActive(false); 
        if (noticePanel != null) noticePanel.SetActive(false);
    }

    public void OpenResetConfirm() { resetConfirmPanel.SetActive(true); }
    public void CloseResetConfirm() { resetConfirmPanel.SetActive(false); }

    public void ExecuteHardReset()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("SUCCESS: Game has been factory reset!");
        
        if (volumeSlider != null) volumeSlider.value = 1f; 
        UpdateVolume(1f);

        if (charactersPanel != null)
        {
            CharacterCard[] allCards = charactersPanel.GetComponentsInChildren<CharacterCard>(true); 
            foreach (CharacterCard card in allCards)
            {
                if (card.characterName == "Tig") { card.isUnlocked = true; card.isEquipped = true; }
                else { card.isUnlocked = false; card.isEquipped = false; }
                card.UpdateCardVisuals();
            }
        }

        CloseResetConfirm();

        Time.timeScale = 1f;
        if (LoadingManager.Instance != null) LoadingManager.Instance.LoadNewScene("MenuScene");
        else SceneManager.LoadScene("MenuScene");
    }

    public void UpdateVolume(float sliderValue) { PlayerPrefs.SetFloat("SavedVolume", sliderValue); AudioListener.volume = sliderValue; }
}