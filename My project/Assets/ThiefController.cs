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
    public float tumblePenaltyDistance = 3f; 

    private bool isDefeated = false;
    [HideInInspector] public bool isFlying = false; 
    
    private float groundY; 
    private float airY; 
    private float targetX;

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
        LevelManager lm = FindAnyObjectByType<LevelManager>();
        if (lm != null && !lm.gameIsActive) return;

        if (transform.position.x != targetX)
        {
            float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * escapeSpeed);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        // ONLY checks for the death limit now, we removed the clunky stun checking!
        if (kommy != null && !isDefeated)
        {
            if (transform.position.x >= escapeLimitX && kommy.currentState != KommyController.CharacterState.Dead)
            {
                kommy.Die();
                TriggerDefeat();
            }
        }
    }

    // Call this to move him exactly 1 step ahead (When Kommy gets hit/bonked)
    public void StepForward()
    {
        if (!isDefeated)
        {
            targetX += escapeDistance; 
            ShowEmoticon("EmoticonLaugh");
        }
    }

    // Call this to move him exactly 2 steps BACKWARDS (When Kommy hits him)
    public void StepBackward()
    {
        if (!isDefeated)
        {
            targetX -= (escapeDistance * 2f); // 2 steps back!
            ShowEmoticon("EmoticonCry");
        }
    }
    
    IEnumerator JetpackCycle()
    {   
        LevelManager lm = FindAnyObjectByType<LevelManager>();
        yield return new WaitUntil(() => lm != null && lm.gameIsActive);

        while (!isDefeated)
        {
            isFlying = false; 
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

    public void TriggerTumbleHit()
    {
        if (!isFlying || isDefeated) return; 

        StopAllCoroutines(); 
        wordManager.StopSpawning(); 
        isFlying = false;

        StartCoroutine(TumbleRoutine());
    }

    private IEnumerator TumbleRoutine()
    {
        anim.Play("ThiefPrep"); 
        ShowEmoticon("EmoticonCry"); 

        float elapsed = 0f;
        float dropTime = 0.15f; 
        float startY = transform.position.y;

        while (elapsed < dropTime)
        {
            float newY = Mathf.Lerp(startY, groundY, elapsed / dropTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

        anim.Play("ThiefStun");
        StepBackward(); 
        
        // NEW: Makes Kommy smile because the thief fell down!
        if (kommy != null) kommy.TriggerHappyFace();

        yield return new WaitForSeconds(2.5f);

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