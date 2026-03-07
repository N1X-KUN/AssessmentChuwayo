using System.Collections;
using UnityEngine;

public class ThiefController : MonoBehaviour
{
    public Animator anim;
    public WordManager wordManager; 
    public KommyController kommy; 

    [Header("Cycle Timers")]
    public float runDuration = 3f;      
    public float takeoffDuration = 0.5f; 
    public float flyDuration = 5f;       
    public float fallDuration = 1f;      

    [Header("Flight & Escape Settings")]
    public float flightHeightOffset = 3f; 
    public float escapeDistance = 2f;     
    public float escapeSpeed = 2f;        
    
    // --- THE NEW ENRAGE TIMER LIMIT ---
    public float escapeLimitX = 7f; // If his X hits this number, the player loses!

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
            // Watch for Stuns to push him further right
            if (kommy.currentState == KommyController.CharacterState.Stunned)
            {
                if (!wasStunned) 
                {
                    targetX += escapeDistance; 
                    wasStunned = true;
                }
            }
            else
            {
                wasStunned = false; 
            }

            // --- OUT OF BOUNDS GAME OVER TRIGGER ---
            if (transform.position.x >= escapeLimitX && kommy.currentState != KommyController.CharacterState.Dead)
            {
                kommy.Die(); // Instantly kill Kommy
                TriggerDefeat(); // Stop the Thief from flying
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

    public void TriggerDefeat()
    {
        isDefeated = true;
        StopAllCoroutines(); 
        wordManager.StopSpawning(); 
        
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        anim.Play("ThiefDie"); 
    }
}