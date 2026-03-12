using System.Collections;
using UnityEngine;

public class ThiefController : MonoBehaviour
{
    public Animator anim;
    public WordManager wordManager; 
    public KommyController kommy; 

    [Header("Emoticon Settings")]
    public Animator emoticonAnimator; // DRAG YOUR EmoticonBubble HERE!

    [Header("Cycle Timers")]
    public float runDuration = 3f;      
    public float takeoffDuration = 0.5f; 
    public float flyDuration = 5f;       
    public float fallDuration = 1f;      

    [Header("Flight & Escape Settings")]
    public float flightHeightOffset = 3f; 
    public float escapeDistance = 2f;     
    public float escapeSpeed = 2f;        
    public float escapeLimitX = 7f; 

    private bool isDefeated = false;
    private float groundY; 
    private float airY; 
    private float targetX;
    private bool wasStunned = false; 

    void Start()
    {
        anim = GetComponent<Animator>();
        groundY = transform.position.y; 
        airY = groundY + flightHeightOffset; 
        targetX = transform.position.x; 
        
        // Hide the bubble at the start
        if (emoticonAnimator != null) emoticonAnimator.gameObject.SetActive(false);
        
        StartCoroutine(JetpackCycle());
    }

    void Update()
    {
        if (transform.position.x != targetX)
        {
            float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * escapeSpeed);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        if (kommy != null && !isDefeated)
        {
            // --- REACTION: KOMMY STUNNED ---
            if (kommy.currentState == KommyController.CharacterState.Stunned)
            {
                if (!wasStunned) 
                {
                    targetX += escapeDistance; 
                    wasStunned = true;
                    // Play the laughing animation!
                    ShowEmoticon("EmoticonLaugh"); 
                }
            }
            else { wasStunned = false; }

            if (transform.position.x >= escapeLimitX && kommy.currentState != KommyController.CharacterState.Dead)
            {
                kommy.Die();
                TriggerDefeat();
            }
        }
    }

    IEnumerator JetpackCycle()
    {
        while (!isDefeated)
        {
            anim.Play("ThiefMove"); 
            yield return new WaitForSeconds(runDuration);
            if (isDefeated) break; 

            // --- REACTION: PREPARING TO FLY ---
            ShowEmoticon("EmoticonPrep");
            
            anim.Play("ThiefPrep");
            float elapsed = 0f;
            float startY = transform.position.y;

            while (elapsed < takeoffDuration)
            {
                float newY = Mathf.Lerp(startY, airY, elapsed / takeoffDuration);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                elapsed += Time.deltaTime;
                yield return null; 
            }
            transform.position = new Vector3(transform.position.x, airY, transform.position.z);
            if (isDefeated) break;

            anim.Play("ThiefFlight");
            wordManager.StartSpawning(); 
            yield return new WaitForSeconds(flyDuration);
            if (isDefeated) break;

            wordManager.StopSpawning(); 
            anim.Play("ThiefStun");
            elapsed = 0f;
            startY = transform.position.y;
            
            float dropTime = 0.3f; 
            while (elapsed < dropTime)
            {
                float newY = Mathf.Lerp(startY, groundY, elapsed / dropTime);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
            yield return new WaitForSeconds(fallDuration - dropTime);
        }
    }

    // This method now tells the Animator which state to play
    public void ShowEmoticon(string animName)
    {
        if (emoticonAnimator == null) return;
        
        emoticonAnimator.gameObject.SetActive(true);
        emoticonAnimator.Play(animName); // Plays the specific animated clip
        
        StopCoroutine(nameof(HideEmoticonRoutine));
        StartCoroutine(HideEmoticonRoutine());
    }

    private IEnumerator HideEmoticonRoutine()
    {
        yield return new WaitForSeconds(2.0f); // How long the reaction stays up
        emoticonAnimator.gameObject.SetActive(false);
    }

    public void TriggerDefeat()
    {
        isDefeated = true;
        StopAllCoroutines(); 
        wordManager.StopSpawning(); 
        
        // --- REACTION: CRYING ON DEFEAT ---
        ShowEmoticon("EmoticonCry");
        
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        anim.Play("ThiefDie"); 
    }

    // NEW: Called by WordManager when Kommy hits acid
    public void TriggerPoisonEscape()
    {
        if (!isDefeated)
        {
            targetX += escapeDistance; // Moves him forward
            ShowEmoticon("Emoticon_Laugh"); // Pops the laughing bubble!
        }
    }
}