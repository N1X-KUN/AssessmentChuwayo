using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuCharacter : MonoBehaviour
{
    [Header("Animations")]
    public Animator anim;
    public string defaultAnimation = "KommyHappy"; 
    public string clickAnimation = "KommyBonk";    
    public float animationDuration = 1.0f; // How long before she goes back to normal?

    [Header("Audio (Optional)")]
    public AudioClip clickSound; 

    private bool isReacting = false;

    public void OnCharacterClicked()
    {
        if (isReacting) return; // Prevents spam-clicking from breaking the animation!
        StartCoroutine(ReactionRoutine());
    }

    private IEnumerator ReactionRoutine()
    {
        isReacting = true;
        
        // 1. Play the animation
        if (anim != null) anim.Play(clickAnimation);

        // 2. Play the sound (using your awesome AudioManager!)
        if (AudioManager.instance != null && clickSound != null)
        {
            AudioManager.instance.PlaySFX(clickSound);
        }

        // 3. Wait for the animation to finish
        yield return new WaitForSeconds(animationDuration);

        // 4. Go back to normal
        if (anim != null) anim.Play(defaultAnimation);
        isReacting = false;
    }
}