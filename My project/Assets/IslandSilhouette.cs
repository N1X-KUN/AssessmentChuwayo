using System.Collections;
using UnityEngine;

public class IslandSilhouette : MonoBehaviour
{
    [Header("Island Setup")]
    public int levelNumber; // Type 1, 2, 3, etc.

    private SpriteRenderer[] allChildSprites;

    void Start()
    {
        // Find every single piece of art inside this island group!
        allChildSprites = GetComponentsInChildren<SpriteRenderer>();

        // Level 1 is always unlocked.
        if (levelNumber == 1) PlayerPrefs.SetInt("Level1_Unlocked", 1);

        int isUnlocked = PlayerPrefs.GetInt("Level" + levelNumber + "_Unlocked", 0);

        if (isUnlocked == 0)
        {
            // LOCKED: Turn everything pitch black instantly
            SetAllSpritesColor(Color.black);
        }
        else
        {
            // UNLOCKED: Check if it was JUST unlocked (needs smooth reveal animation)
            int needsRevealAnim = PlayerPrefs.GetInt("Level" + levelNumber + "_NeedsReveal", 0);

            if (needsRevealAnim == 1)
            {
                // Start black, then smoothly fade to full color!
                SetAllSpritesColor(Color.black);
                StartCoroutine(SmoothReveal());
                
                // Turn off the flag so it doesn't animate again next time
                PlayerPrefs.SetInt("Level" + levelNumber + "_NeedsReveal", 0); 
            }
            else
            {
                // Already unlocked previously, just show it normally
                SetAllSpritesColor(Color.white); 
            }
        }
    }

    private void SetAllSpritesColor(Color targetColor)
    {
        foreach (SpriteRenderer sprite in allChildSprites)
        {
            sprite.color = targetColor;
        }
    }

    private IEnumerator SmoothReveal()
    {
        // Wait 2 seconds for the camera to pan and the scene to settle
        yield return new WaitForSeconds(2f); 

        float duration = 2.5f; // Takes 2.5 seconds to fade in
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float lerpValue = elapsedTime / duration;

            // Smoothly transition from Black to White (full color)
            Color currentColor = Color.Lerp(Color.black, Color.white, lerpValue);
            SetAllSpritesColor(currentColor);

            yield return null;
        }

        SetAllSpritesColor(Color.white); // Ensure it finishes perfectly white
    }
}