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

    [Header("Phase 2: Reset System")]
    public GameObject resetConfirmPanel; 

    [Header("Phase 2: Volume Control")]
    public Slider volumeSlider;
    public AudioSource backgroundMusic; 
    private float maxAudioLevel = 0.05f; 

    void Start()
    {
        // DEV WIPE HAS BEEN REMOVED! Memory will now safely persist.
        CloseAllPopups();

        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("SavedVolume", 1f);
            UpdateVolume(volumeSlider.value);
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

    public void OpenSettings() { dimBackground.SetActive(true); settingsPanel.SetActive(true); }
    public void OpenCredits() { dimBackground.SetActive(true); creditsPanel.SetActive(true); }
    
    public void OpenCharacters() 
    { 
        CloseAllPopups(); 
        dimBackground.SetActive(true); 
        charactersPanel.SetActive(true); 
    }

    public void CloseCharacters()
    {
        charactersPanel.SetActive(false); 
        settingsPanel.SetActive(true);    
    }

    public void CloseAllPopups()
    {
        dimBackground.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        if (resetConfirmPanel != null) resetConfirmPanel.SetActive(false);
        if (charactersPanel != null) charactersPanel.SetActive(false); 
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
                if (card.characterName == "Kommy") 
                {
                    card.isUnlocked = true;
                    card.isEquipped = true;
                }
                else 
                {
                    card.isUnlocked = false;
                    card.isEquipped = false;
                }
                
                card.UpdateCardVisuals();
            }
        }

        CloseResetConfirm();
    }

    public void UpdateVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("SavedVolume", sliderValue);
        AudioListener.volume = sliderValue; 
    }

    public void MuteVolume() { if (volumeSlider != null) volumeSlider.value = 0f; }
    public void MaxVolume() { if (volumeSlider != null) volumeSlider.value = 1f; }
}