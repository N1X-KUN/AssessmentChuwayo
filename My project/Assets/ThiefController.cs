using System.Collections;
using UnityEngine;

public class ThiefController : MonoBehaviour
{
    public Animator anim;
    public WordManager wordManager; // <--- We gave him access to the WordManager!

    [Header("Cycle Timers")]
    public float runDuration = 3f;      
    public float takeoffDuration = 0.5f; 
    public float flyDuration = 5f;       
    public float fallDuration = 1f;      

    private bool isDefeated = false;
    private float groundY; 
    private float airY; 

    void Start()
    {
        anim = GetComponent<Animator>();
        groundY = transform.position.y; 
        airY = groundY + 1.5f; 
        
        StartCoroutine(JetpackCycle());
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
            Vector3 startPos = transform.position;
            Vector3 targetPos = new Vector3(startPos.x, airY, startPos.z);

            while (elapsed < takeoffDuration)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / takeoffDuration);
                elapsed += Time.deltaTime;
                yield return null; 
            }
            transform.position = targetPos; 
            if (isDefeated) break;

            // 3. FLYING & DROPPING
            anim.Play("ThiefFlight");
            wordManager.StartSpawning(); // <--- TURN FOOD ON!
            yield return new WaitForSeconds(flyDuration);
            if (isDefeated) break;

            // 4. FUEL EMPTY / FALLING 
            wordManager.StopSpawning(); // <--- TURN FOOD OFF!
            anim.Play("ThiefStun");
            elapsed = 0f;
            startPos = transform.position;
            targetPos = new Vector3(startPos.x, groundY, startPos.z);
            
            float dropTime = 0.3f; 
            while (elapsed < dropTime)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / dropTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPos;
            
            yield return new WaitForSeconds(fallDuration - dropTime);
        }
    }

    public void TriggerDefeat()
    {
        isDefeated = true;
        StopAllCoroutines(); 
        wordManager.StopSpawning(); // <--- Make sure food stops dropping if he dies!
        
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        anim.Play("ThiefDie"); 
    }
}