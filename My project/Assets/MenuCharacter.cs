using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuCharacter : MonoBehaviour
{
    [Header("Animations")]
    public Animator anim;
    public string defaultAnimation = "KommyFace_Happy"; 
    public string clickAnimation = "KommyBonk";    
    public float animationDuration = 1.0f; 

    [Header("Audio (Optional)")]
    public AudioClip clickSound; 

    private Coroutine reactionCoroutine;

    public void OnCharacterClicked()
    {
        // SECRET WEAPON: This tells us if the button is actually working!
        Debug.Log("SUCCESS: The invisible button was clicked!"); 

        // If they spam click, stop the current timer so we can start over!
        if (reactionCoroutine != null) StopCoroutine(reactionCoroutine);
        
        reactionCoroutine = StartCoroutine(ReactionRoutine());
    }

    private IEnumerator ReactionRoutine()
    {
        // 1. Force the animation to play from the absolute beginning (Time: 0f)
        if (anim != null) anim.Play(clickAnimation, 0, 0f);

        // 2. Play the sound instantly
        if (AudioManager.instance != null && clickSound != null)
        {
            AudioManager.instance.PlaySFX(clickSound);
            Debug.Log("SUCCESS: Audio Manager played the sound!");
        }
        else
        {
            Debug.LogWarning("ERROR: AudioManager or Audio Clip is missing from the scene!");
        }

        // 3. Wait...
        yield return new WaitForSeconds(animationDuration);

        // 4. Go back to normal
        if (anim != null) anim.Play(defaultAnimation);
        reactionCoroutine = null;
    }
}