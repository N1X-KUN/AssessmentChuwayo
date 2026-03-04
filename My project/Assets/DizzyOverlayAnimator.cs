using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Force this GameObject to have an Image component, so we can't break it
[RequireComponent(typeof(Image))]
public class DizzyOverlayAnimator : MonoBehaviour
{
    private Image targetImage;

    [Header("Sprites")]
    // --- THIS IS WHERE YOU DRAG YOUR 10 SPRITES ---
    public List<Sprite> poisonVignettes = new List<Sprite>();
    // ----------------------------------------------

    [Header("Settings")]
    // The delay between frames (10 frames * 0.05s = 0.5s fade in time)
    public float timePerFrame = 0.05f; 

    void Awake()
    {
        targetImage = GetComponent<Image>();
    }

    // --- COROUTINE 1: The Intro (Fades in, 1-10) ---
    public IEnumerator PlayPoisonIntro()
    {
        if (poisonVignettes.Count < 2) yield break; // Failsafe

        // Cycle through sprites from the beginning to the end
        for (int i = 0; i < poisonVignettes.Count; i++)
        {
            targetImage.sprite = poisonVignettes[i];
            yield return new WaitForSeconds(timePerFrame);
        }
    }

    // --- COROUTINE 2: The Outro (Fades out, 10-1) ---
    public IEnumerator PlayPoisonOutro()
    {
        if (poisonVignettes.Count < 2) yield break; // Failsafe

        // Cycle through sprites BACKWARDS (from the end to the beginning)
        for (int i = poisonVignettes.Count - 1; i >= 0; i--)
        {
            targetImage.sprite = poisonVignettes[i];
            yield return new WaitForSeconds(timePerFrame);
        }
    }
}