using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour
{
    [Header("Level Setup")]
    public string sceneToLoad = "Level1Scene"; // Type the EXACT name of your level scene here
    public int levelNumber = 1;
    public bool isAlwaysUnlocked = false; // Check this ONLY for Level 1!

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
            // If you have a grey/color sprite change, you can do it here!
        }
        else
        {
            isUnlocked = false;
            myButton.interactable = false; // Locks the button so they can't click it
        }
    }

    public void OnNodeClicked()
    {
        if (isUnlocked)
        {
            // If you have your LoadingManager from the Menu scene, use it!
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.LoadNewScene(sceneToLoad);
            }
            else
            {
                // Fallback just in case
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}