using UnityEngine;
using UnityEngine.SceneManagement; // This is the magic teleporting library!

public class MainMenuManagerMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Make sure "MapScene" is spelled exactly like your scene file!
        SceneManager.LoadScene("MapScene"); 
    }

    public void OpenSettings()
    {
        // We will build the actual settings menu later if we have time!
        Debug.Log("Opening Settings Menu..."); 
    }

    public void OpenCredits()
    {
        // We will build the credits panel later if we have time!
        Debug.Log("Opening Credits..."); 
    }
}