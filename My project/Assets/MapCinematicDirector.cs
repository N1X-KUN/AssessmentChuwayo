using UnityEngine;
using System.Collections;

public class MapCinematicDirector : MonoBehaviour
{
    [Header("Phase 3: Unlock Cinematic")]
    public string phase3Part1Seq = "Level2"; 
    public string phase3Part2Seq = "MapIntro3";
    public float fadeSpeed = 0.3f;

    [Header("UI to Hide")]
    public GameObject phase1UI; 

    [Header("The Silhouettes")]
    public SpriteRenderer[] islandSilhouettes; 

    [Header("Testing Cheats")]
    public bool forcePhase3Test = false; 

    void Awake()
    {
        if (forcePhase3Test)
        {
            PlayerPrefs.SetInt("HasFinishedTutorial", 1);
            PlayerPrefs.SetInt("IslandsUnlocked", 0);
            
            // FIX: THE CHEAT BOX NOW UNLOCKS LEVEL 2 SO BUTTONS WORK!
            PlayerPrefs.SetInt("UnlockedLevel", 2); 
            
            PlayerPrefs.Save();
        }
    }

    void Start()
    {
        StartCoroutine(CheckMapState());
    }

    private void SetSilhouettesAlpha(float alpha)
    {
        foreach (var rootSil in islandSilhouettes)
        {
            if (rootSil != null)
            {
                SpriteRenderer[] allRenderers = rootSil.GetComponentsInChildren<SpriteRenderer>(true);
                foreach(var sr in allRenderers)
                {
                    Color c = sr.color; 
                    c.a = alpha; 
                    sr.color = c;
                }
            }
        }
    }

    private IEnumerator CheckMapState()
    {
        yield return null; 

        int hasFinishedLevel1 = PlayerPrefs.GetInt("HasFinishedTutorial", 0);
        int islandsUnlocked = PlayerPrefs.GetInt("IslandsUnlocked", 0);
        DialogueManager dm = FindAnyObjectByType<DialogueManager>();

        if (hasFinishedLevel1 == 0)
        {
            SetSilhouettesAlpha(1f);
        }
        else if (hasFinishedLevel1 == 1 && islandsUnlocked == 0)
        {
            if (phase1UI != null) phase1UI.SetActive(false);

            SetSilhouettesAlpha(1f);

            if (dm != null)
            {
                dm.PlayDialogue(phase3Part1Seq);
                yield return new WaitUntil(() => !dm.dialogueIsActive);

                float alpha = 1f;
                while (alpha > 0f)
                {
                    alpha -= Time.unscaledDeltaTime * fadeSpeed;
                    SetSilhouettesAlpha(alpha);
                    yield return null;
                }

                dm.PlayDialogue(phase3Part2Seq);
                yield return new WaitUntil(() => !dm.dialogueIsActive);
            }

            PlayerPrefs.SetInt("IslandsUnlocked", 1);
            PlayerPrefs.Save();
            
            foreach (var sil in islandSilhouettes)
            {
                if (sil != null) sil.gameObject.SetActive(false); 
            }
        }
        else if (hasFinishedLevel1 == 1 && islandsUnlocked == 1)
        {
            if (phase1UI != null) phase1UI.SetActive(false);

            SetSilhouettesAlpha(0f);
            foreach (var sil in islandSilhouettes)
            {
                if (sil != null) sil.gameObject.SetActive(false); 
            }
        }
    }
}