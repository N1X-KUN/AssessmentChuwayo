using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DizzyOverlayAnimator : MonoBehaviour
{
    private Image targetImage;

    [Header("Sprites")]
    public List<Sprite> poisonVignettes = new List<Sprite>();

    [Header("Settings")]
    public float timePerFrame = 0.05f; 
    public int flickerCount = 6; // How many times it flashes before vanishing

    void Awake()
    {
        targetImage = GetComponent<Image>();
    }

    public IEnumerator PlayPoisonIntro()
    {
        if (poisonVignettes.Count < 2) yield break;

        targetImage.color = Color.white; // Ensure it's visible
        for (int i = 0; i < poisonVignettes.Count; i++)
        {
            targetImage.sprite = poisonVignettes[i];
            yield return new WaitForSeconds(timePerFrame);
        }
    }

    public IEnumerator PlayPoisonOutro()
    {
        if (poisonVignettes.Count < 2) yield break;

        // --- NEW: THE FLICKER WARNING ---
        // Rapidly toggle the image on and off to warn the player
        for (int i = 0; i < flickerCount; i++)
        {
            targetImage.enabled = !targetImage.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        targetImage.enabled = true; // Ensure it stays on for the final fade

        // --- THE SMOOTH FADE OUT ---
        float elapsed = 0f;
        float fadeTime = 0.5f;
        Color startColor = targetImage.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            targetImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }
}