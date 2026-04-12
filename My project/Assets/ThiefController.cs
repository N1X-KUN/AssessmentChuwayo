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
    public float catchDistance = 2.5f; 
    
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

        if (kommy != null && !isDefeated)
        {
            // Thief Escapes (Thief Wins)
            if (transform.position.x >= escapeLimitX && kommy.currentState != KommyController.CharacterState.Dead)
            {
                TriggerThiefWin();
            }
            // Early Victory (Kommy catches Thief before time runs out)
            else if (transform.position.x <= kommy.transform.position.x + catchDistance && kommy.currentState != KommyController.CharacterState.Victory)
            {
                kommy.WinGame();
                TriggerDefeat();
            }
        }
    }

    public void StepForward(bool playLaugh = true)
    {
        if (!isDefeated)
        {
            targetX += escapeDistance; 
            if (playLaugh) ShowEmoticon("EmoticonLaugh", 2.05f);
        }
    }

    public void StepBackward()
    {
        if (!isDefeated)
        {
            targetX -= (escapeDistance * 2f); 
            ShowEmoticon("EmoticonCry", 2.05f);
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
            ShowEmoticon("EmoticonPrep", 2.05f);
            
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
        ShowEmoticon("EmoticonCry", 2.05f); 

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

        if (kommy != null) kommy.TriggerHappyFace();

        yield return new WaitForSeconds(2.5f);

        if (!isDefeated)
        {
            StartCoroutine(JetpackCycle());
        }
    }

    public void ShowEmoticon(string animName, float duration)
    {
        if (emoticonAnimator == null) return;
        emoticonAnimator.gameObject.SetActive(true);
        emoticonAnimator.Play(animName); 
        
        StopCoroutine(nameof(HideEmoticonRoutine));
        if (duration > 0f) StartCoroutine(HideEmoticonRoutine(duration));
    }

    private IEnumerator HideEmoticonRoutine(float duration)
    {
        yield return new WaitForSeconds(duration); 
        emoticonAnimator.gameObject.SetActive(false);
    }

    public void TriggerThiefWin()
    {
        isDefeated = true;
        kommy.Die();
        StopAllCoroutines();
        wordManager.StopSpawning();
        anim.Play("ThiefWin"); 
        ShowEmoticon("EmoticonLaugh", 0f); // 0 = Loops infinitely
    }

    public void TriggerDefeat()
    {
        isDefeated = true;
        StopAllCoroutines(); 
        wordManager.StopSpawning(); 
        ShowEmoticon("EmoticonCry", 0f); // 0 = Loops infinitely
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        anim.Play("ThiefDie"); 
    }
}