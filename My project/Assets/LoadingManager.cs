using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance; 

    [Header("UI References")]
    public CanvasGroup loadingCanvasGroup;
    public VideoPlayer backgroundVideo;
    
    [Header("Dancing Characters")]
    public GameObject char1;
    public GameObject char2;
    public GameObject char3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadNewScene(string sceneName)
    {
        StartCoroutine(LoadingSequence(sceneName));
    }

    private IEnumerator LoadingSequence(string sceneName)
    {
        // 1. UNFREEZE TIME! This stops your next scene from being frozen.
        Time.timeScale = 1f;

        char1.SetActive(false);
        char2.SetActive(false);
        char3.SetActive(false);
        loadingCanvasGroup.blocksRaycasts = true;

        // 2. Fade In the Loading Screen 
        backgroundVideo.Play();
        float fadeTime = 1.5f;
        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime; 
            loadingCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeTime);
            yield return null;
        }

        // 3. Start loading! Char 1 appears instantly.
        char1.SetActive(true);
        
        // Give the UI a tiny moment to start animating before the heavy loading hits
        yield return new WaitForSecondsRealtime(0.2f);

        // 4. Tell Unity to load gently so the dancing doesn't stutter!
        Application.backgroundLoadingPriority = ThreadPriority.Low;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; 

        // 5. Monitor actual Unity loading progress in real-time
        while (asyncLoad.progress < 0.9f)
        {
            if (asyncLoad.progress >= 0.45f && !char2.activeSelf)
            {
                char2.SetActive(true);
            }
            yield return null; 
        }

        // 6. Loading is 100% finished in the background! 
        char2.SetActive(true); 
        char3.SetActive(true);

        // 7. Wait exactly 3 seconds REALTIME
        yield return new WaitForSecondsRealtime(3f);

        // 8. Open the new scene!
        asyncLoad.allowSceneActivation = true;

        yield return new WaitUntil(() => asyncLoad.isDone);

        // Reset CPU power back to normal now that the scene is loaded
        Application.backgroundLoadingPriority = ThreadPriority.Normal;

        yield return new WaitForSecondsRealtime(0.5f); 

        // 9. Fade Out 
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