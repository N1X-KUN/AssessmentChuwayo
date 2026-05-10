using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Needed for the timer!

public class MapNode : MonoBehaviour
{
    [Header("Level Setup")]
    public string sceneToLoad = "Level1Scene"; 
    public int levelNumber = 1;
    public bool isAlwaysUnlocked = false; 

    [Header("Coming Soon Gag")]
    public bool isComingSoon = false; // Check this ONLY for Level 2 & Infinite!
    public GameObject burumiPopup; // Drag your UI panel here

    private Button myButton;
    private bool isUnlocked = false;

    void Start()
    {
        myButton = GetComponent<Button>();

        // Check if this level is unlocked in memory
        int unlockedStatus = PlayerPrefs.GetInt("Level" + levelNumber + "_Unlocked", 0);

        if (isAlwaysUnlocked || unlockedStatus == 1)
        {
            isUnlocked = true;
            myButton.interactable = true; // Button is clickable
        }
        else
        {
            isUnlocked = false;
            myButton.interactable = false; // Locks the button 
        }
    }

    public void OnNodeClicked()
    {
        if (isUnlocked)
        {
            // THE GAG INTERCEPT: If this is a fake level, do this instead!
            if (isComingSoon)
            {
                if (burumiPopup != null)
                {
                    StopAllCoroutines(); 
                    StartCoroutine(ShowBurumiRoutine());
                }
                // Play a funny sound if you have one!
                if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop);
                
                return; // <-- This "return" stops the code here so the scene NEVER loads!
            }

            // Normal Scene Loading (For Level 1 and Shop)
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.LoadNewScene(sceneToLoad);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    private IEnumerator ShowBurumiRoutine()
    {
        burumiPopup.SetActive(true);
        yield return new WaitForSeconds(4f); // Wait 4 seconds
        burumiPopup.SetActive(false); // Hide it again
    }
}