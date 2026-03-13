using System.Collections;
using UnityEngine;

public class ThiefController : MonoBehaviour
{
    public Animator anim;
    public WordManager wordManager; 
    public KommyController kommy; 

    [Header("Emoticon Settings")]
    public Animator emoticonAnimator; 

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
    
    [Header("Tumble Settings")]
    public float tumblePenaltyDistance = 3f; // NEW: How far back he slides when hit

    private bool isDefeated = false;
    [HideInInspector] public bool isFlying = false; // NEW: Tracks if he is in the air!
    
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
        
        if (emoticonAnimator != null) emoticonAnimator.gameObject.SetActive(false);
        
        StartCoroutine(JetpackCycle());
    }

    void Update()
    {
        // Smoothly slides the thief to his target X position
        if (transform.position.x != targetX)
        {
            float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * escapeSpeed);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        if (kommy != null && !isDefeated)
        {
            if (kommy.currentState == KommyController.CharacterState.Stunned)
            {
                if (!wasStunned) 
                {
                    targetX += escapeDistance; 
                    wasStunned = true;
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
            isFlying = false; 
            
            // --- GROUND: TRAPS ONLY! ---
            wordManager.onlySpawnTraps = true; 
            wordManager.StartSpawning(); 
            
            anim.Play("ThiefMove"); 
            yield return new WaitForSeconds(runDuration);
            if (isDefeated) break; 

            wordManager.StopSpawning(); 
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

            // --- FLYING: FOOD AND TRAPS! ---
            isFlying = true; 
            wordManager.onlySpawnTraps = false; 
            wordManager.StartSpawning(); 
            
            anim.Play("ThiefFlight");
            yield return new WaitForSeconds(flyDuration);
            if (isDefeated) break;

            wordManager.StopSpawning(); 
            isFlying = false; 
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

    // --- NEW: THE TUMBLE HIT SEQUENCE ---
    public void TriggerTumbleHit()
    {
        if (!isFlying || isDefeated) return; // Only works if he is flying!

        // 1. Interrupt current flight!
        StopAllCoroutines(); 
        wordManager.StopSpawning(); 
        isFlying = false;

        // 2. Start the crash sequence
        StartCoroutine(TumbleRoutine());
    }

    private IEnumerator TumbleRoutine()
    {
        // Prep animation to show he is falling
        anim.Play("ThiefPrep"); 
        ShowEmoticon("EmoticonCry"); // Shocked/Cry face!

        // Drop him fast
        float elapsed = 0f;
        float dropTime = 0.15f; // Drops faster than a normal landing
        float startY = transform.position.y;

        while (elapsed < dropTime)
        {
            float newY = Mathf.Lerp(startY, groundY, elapsed / dropTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

        // STUNNED ON THE GROUND (The Background Illusion!)
        anim.Play("ThiefStun");
        targetX -= tumblePenaltyDistance; // Pushes him backwards, simulating loss of speed

        // Wait while he recovers on the ground
        yield return new WaitForSeconds(2.5f);

        // Go right back to the normal running loop!
        if (!isDefeated)
        {
            StartCoroutine(JetpackCycle());
        }
    }

    public void ShowEmoticon(string animName)
    {
        if (emoticonAnimator == null) return;
        
        emoticonAnimator.gameObject.SetActive(true);
        emoticonAnimator.Play(animName); 
        
        StopCoroutine(nameof(HideEmoticonRoutine));
        StartCoroutine(HideEmoticonRoutine());
    }

    private IEnumerator HideEmoticonRoutine()
    {
        yield return new WaitForSeconds(2.0f); 
        emoticonAnimator.gameObject.SetActive(false);
    }

    public void TriggerDefeat()
    {
        isDefeated = true;
        StopAllCoroutines(); 
        wordManager.StopSpawning(); 
        ShowEmoticon("EmoticonCry");
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        anim.Play("ThiefDie"); 
    }

    public void TriggerPoisonEscape()
    {
        if (!isDefeated)
        {
            targetX += escapeDistance; 
            ShowEmoticon("EmoticonLaugh"); 
        }
    }
}