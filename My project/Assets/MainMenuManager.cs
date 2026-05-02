using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Phase 1 & 3: Pop-up Panels")]
    public GameObject dimBackground;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject charactersPanel; // Controls the new Equip screen

    [Header("Phase 2: Reset System")]
    public GameObject resetConfirmPanel; 

    [Header("Phase 2: Volume Control")]
    public Slider volumeSlider;
    public AudioSource backgroundMusic; 
    private float maxAudioLevel = 0.05f; 

    void Start()
    {
        // --- DEV MODE WIPE ---
        // (Delete this line later when you actually want players to save their progress!)
        PlayerPrefs.DeleteAll(); 
        
        CloseAllPopups();

        // If we have a slider, load the saved volume (defaulting to 1, which is 100%)
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("SavedVolume", 1f);
            UpdateVolume(volumeSlider.value);
        }
    }

    // --- POP-UP NAVIGATION FUNCTIONS ---
    public void PlayGame()
    {
        int hasPlayed = PlayerPrefs.GetInt("HasFinishedTutorial", 0);
        if (hasPlayed == 0) SceneManager.LoadScene("TutorialLevel");
        else SceneManager.LoadScene("MapScene");
    }

    public void OpenSettings() { dimBackground.SetActive(true); settingsPanel.SetActive(true); }
    public void OpenCredits() { dimBackground.SetActive(true); creditsPanel.SetActive(true); }
    
    // Opens the Character menu and safely closes Settings so they don't overlap
    public void OpenCharacters() 
    { 
        CloseAllPopups(); 
        dimBackground.SetActive(true); 
        charactersPanel.SetActive(true); 
    }

    // This acts as a "Back" button to return to Settings
    public void CloseCharacters()
    {
        charactersPanel.SetActive(false); // Turn off the Character screen
        settingsPanel.SetActive(true);    // Turn Settings back on!
        
        // Notice we don't touch the dimBackground, because Settings still needs it!
    }

    public void CloseAllPopups()
    {
        dimBackground.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        if (resetConfirmPanel != null) resetConfirmPanel.SetActive(false);
        if (charactersPanel != null) charactersPanel.SetActive(false); // Hides the Character screen
    }

    // --- PHASE 2: RESET FUNCTIONS ---
    public void OpenResetConfirm() { resetConfirmPanel.SetActive(true); }
    public void CloseResetConfirm() { resetConfirmPanel.SetActive(false); }

    public void ExecuteHardReset()
    {
        // 1. Factory reset! Wipes all PlayerPrefs from the hard drive.
        PlayerPrefs.DeleteAll();
        Debug.Log("SUCCESS: Game has been factory reset!");
        
        // 2. Reset the slider visually AND audibly to 100%
        if (volumeSlider != null) volumeSlider.value = 1f; 
        UpdateVolume(1f);

        // 3. Force all Character Cards to reset instantly!
        if (charactersPanel != null)
        {
            // This finds every card, even if the Character Panel is currently hidden
            CharacterCard[] allCards = charactersPanel.GetComponentsInChildren<CharacterCard>(true); 
            
            foreach (CharacterCard card in allCards)
            {
                // If it's Kommy, unlock and equip her!
                if (card.characterName == "Kommy") 
                {
                    card.isUnlocked = true;
                    card.isEquipped = true;
                }
                // If it's anyone else, lock them and un-equip them!
                else 
                {
                    card.isUnlocked = false;
                    card.isEquipped = false;
                }
                
                // Tell the card to update its visuals immediately
                card.UpdateCardVisuals();
            }
        }

        // 4. Close the pop-up
        CloseResetConfirm();
    }

    // --- PHASE 2: VOLUME FUNCTIONS ---
    public void UpdateVolume(float sliderValue)
    {
        // Save the setting so the game remembers it next time
        PlayerPrefs.SetFloat("SavedVolume", sliderValue);

        // THE MASTER VOLUME SWITCH
        // This instantly controls Music, SFX, UI, and Videos all at once!
        AudioListener.volume = sliderValue; 
    }

    public void MuteVolume() { if (volumeSlider != null) volumeSlider.value = 0f; }
    public void MaxVolume() { if (volumeSlider != null) volumeSlider.value = 1f; }
}