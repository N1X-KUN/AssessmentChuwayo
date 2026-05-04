using UnityEngine;
using UnityEngine.UI;

public class MapLevelButton : MonoBehaviour
{
    [Header("Level Settings")]
    public int levelNumber; // Type 1, 2, 3 etc. in the Inspector!
    
    [Header("The Button Background")]
    public Image buttonImage; 

    [Header("Drag Your Baked Sprites Here")]
    public Sprite lockedGrey;     // button01.png
    public Sprite unlocked0Stars; // button02.png (Blue, no stars)
    public Sprite unlocked1Star;  // Blue with 1 star
    public Sprite unlocked2Stars; // Blue with 2 stars
    public Sprite unlocked3Stars; // Blue with 3 stars

    void Start()
    {
        // Force Level 1 to ALWAYS be unlocked by default
        if (levelNumber == 1) PlayerPrefs.SetInt("Level1_Unlocked", 1);

        // Check brain memory: Is it unlocked? How many stars did they get?
        int isUnlocked = PlayerPrefs.GetInt("Level" + levelNumber + "_Unlocked", 0);
        int stars = PlayerPrefs.GetInt("Level" + levelNumber + "_Stars", 0);

        // Change the button image based on memory!
        if (isUnlocked == 0)
        {
            buttonImage.sprite = lockedGrey;
        }
        else
        {
            if (stars == 0) buttonImage.sprite = unlocked0Stars;
            else if (stars == 1) buttonImage.sprite = unlocked1Star;
            else if (stars == 2) buttonImage.sprite = unlocked2Stars;
            else if (stars >= 3) buttonImage.sprite = unlocked3Stars;
        }
    }

    // You will link this to the Button's "On Click()" event in the Inspector later!
    public void OnNodeClicked()
    {
        int isUnlocked = PlayerPrefs.GetInt("Level" + levelNumber + "_Unlocked", 0);
        
        if (isUnlocked == 1)
        {
            // Save which level they clicked so the next scene knows!
            PlayerPrefs.SetInt("SelectedLevel", levelNumber);
            
            // LoadingManager.Instance.LoadNewScene("LevelScene"); 
            Debug.Log("Player clicked to start Level: " + levelNumber);
        }
        else
        {
            Debug.Log("Level " + levelNumber + " is locked!");
        }
    }
}