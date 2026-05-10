using UnityEngine;

public class CharacterUnlockSync : MonoBehaviour
{
    [Tooltip("This MUST match the exact spelling from your ShopItem script!")]
    public string saveKey = "Kommy_Unlocked"; 
    
    public GameObject lockedSilhouette; // The black shadow with the padlock
    public GameObject unlockedButton;   // The colored, clickable character

    void Start()
    {
        // Check the memory when the panel opens!
        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            // They bought it! Hide the lock, show the character.
            lockedSilhouette.SetActive(false);
            unlockedButton.SetActive(true);
        }
        else
        {
            // Not bought yet. Show the lock, hide the character.
            lockedSilhouette.SetActive(true);
            unlockedButton.SetActive(false);
        }
    }
}