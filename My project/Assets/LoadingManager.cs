using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance; // The magic Singleton!

    [Header("UI References")]
    public CanvasGroup loadingCanvasGroup;
    public VideoPlayer backgroundVideo;
    
    [Header("Dancing Characters")]
    public GameObject char1;
    public GameObject char2;
    public GameObject char3;

    private void Awake()
    {
        // This ensures only ONE Loading Manager ever exists in your game
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Survive scene changes!
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this from anywhere in your game to change scenes!
    public void LoadNewScene(string sceneName)
    {
        StartCoroutine(LoadingSequence(sceneName));
    }

    private IEnumerator LoadingSequence(string sceneName)
    {
        // 1. Reset everything
        char1.SetActive(false);
        char2.SetActive(false);
        char3.SetActive(false);
        loadingCanvasGroup.blocksRaycasts = true;

        // 2. Fade In the Loading Screen (Using Unscaled Time)
        backgroundVideo.Play();
        float fadeTime = 1.5f;
        float timer = 0f;
        while (timer < fadeTime)
        {
            // unscaledDeltaTime ignores the game being paused!
            timer += Time.unscaledDeltaTime; 
            loadingCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeTime);
            yield return null;
        }

        // 3. Start loading! Char 1 appears instantly.
        char1.SetActive(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; 

        // 4. Monitor actual Unity loading progress in real-time
        while (asyncLoad.progress < 0.9f)
        {
            if (asyncLoad.progress >= 0.45f && !char2.activeSelf)
            {
                char2.SetActive(true);
            }
            yield return null; 
        }

        // 5. Loading is 100% finished in the background! 
        char2.SetActive(true); 
        char3.SetActive(true);

        // 6. Wait exactly 3 seconds REALTIME
        yield return new WaitForSecondsRealtime(3f);

        // 7. Open the new scene!
        asyncLoad.allowSceneActivation = true;

        // CRITICAL: Wait until Unity confirms the scene has fully swapped
        yield return new WaitUntil(() => asyncLoad.isDone);

        yield return new WaitForSecondsRealtime(0.5f); 

        // 8. Fade Out (Using Unscaled Time)
        timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            loadingCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeTime);
            yield return null;
        }

        loadingCanvasGroup.blocksRaycasts = false;
        backgroundVideo.Pause();
    }
}