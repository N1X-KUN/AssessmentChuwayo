using UnityEngine;
using UnityEngine.UI;

public class MapUIManager : MonoBehaviour
{
    [Header("Settings UI")]
    public GameObject settingsPanel; 
    public Button settingsButton;    

    private bool isSettingsOpen = false;

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ToggleSettings);
        }
    }

    void Update()
    {
        // Listen for the ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }

    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            isSettingsOpen = !isSettingsOpen;
            settingsPanel.SetActive(isSettingsOpen);

            // THIS IS THE FIX: Freezes the map scrolling and locks the background!
            if (isSettingsOpen)
            {
                Time.timeScale = 0f; 
            }
            else
            {
                // Only unfreeze time if dialogue isn't currently playing
                DialogueManager dm = FindAnyObjectByType<DialogueManager>();
                if (dm == null || !dm.dialogueIsActive)
                {
                    Time.timeScale = 1f;
                }
            }
        }
    }
}