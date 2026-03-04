using System.Collections;
using UnityEngine;

public class ThiefController : MonoBehaviour
{
    public Animator anim;
    public WordManager wordManager; 
    public KommyController kommy; // <--- The Thief watches her get stunned!

    [Header("Cycle Timers")]
    public float runDuration = 3f;      
    public float takeoffDuration = 0.5f; 
    public float flyDuration = 5f;       
    public float fallDuration = 1f;      

    [Header("Flight & Escape Settings")]
    public float flightHeightOffset = 3f; // Change his flight height right here!
    public float escapeDistance = 2f;     // How much further right he moves per stun
    public float escapeSpeed = 2f;        // How fast he glides to the new spot

    private bool isDefeated = false;
    private float groundY; 
    private float airY; 
    
    // Memory for the X-axis escape logic
    private float targetX;
    private bool wasStunned = false; 

    void Start()
    {
        anim = GetComponent<Animator>();
        groundY = transform.position.y; 
        
        // It now uses your custom Offset to calculate how high to fly
        airY = groundY + flightHeightOffset; 
        
        targetX = transform.position.x; 
        
        StartCoroutine(JetpackCycle());
    }

    void Update()
    {
        // 1. Smoothly glide to the targetX position horizontally!
        if (transform.position.x != targetX)
        {
            float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * escapeSpeed);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        // 2. Watch Kommy! If she gets Stunned, push targetX further to the right.
        if (kommy != null)
        {
            if (kommy.currentState == KommyController.CharacterState.Stunned)
            {
                if (!wasStunned) // Makes sure we only add distance ONCE per stun!
                {
                    targetX += escapeDistance; 
                    wasStunned = true;
                }
            }
            else
            {
                wasStunned = false; // Reset the memory when she recovers
            }
        }
    }

    IEnumerator JetpackCycle()
    {
        while (!isDefeated)
        {
            // 1. RUNNING
            anim.Play("ThiefMove"); 
            yield return new WaitForSeconds(runDuration);
            if (isDefeated) break; 

            // 2. TAKEOFF
            anim.Play("ThiefPrep");
            float elapsed = 0f;
            float startY = transform.position.y;

            // Notice we only Lerp the Y axis now, so it doesn't fight the Escape X axis!
            while (elapsed < takeoffDuration)
            {
                float newY = Mathf.Lerp(startY, airY, elapsed / takeoffDuration);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                elapsed += Time.deltaTime;
                yield return null; 
            }
            transform.position = new Vector3(transform.position.x, airY, transform.position.z);
            if (isDefeated) break;

            // 3. FLYING & DROPPING
            anim.Play("ThiefFlight");
            wordManager.StartSpawning(); 
            yield return new WaitForSeconds(flyDuration);
            if (isDefeated) break;

            // 4. FUEL EMPTY / FALLING 
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