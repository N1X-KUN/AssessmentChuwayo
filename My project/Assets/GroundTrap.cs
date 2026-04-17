using UnityEngine;

public class GroundTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    public float moveSpeed = 3f; 
    public bool isPoisonTrap = false; 
    public float triggerDistance = 0.5f; 
    public float fallSpeed = 8f; 
    
    // Set this to -3 in the Inspector to match Kommy!
    public float floorY = -3f; 

    [Header("Visual Effects")]
    public GameObject poisonSplashPrefab; // <--- NEW: Drag your Poison Splash Prefab here in the Inspector!

    private KommyController kommy;
    private bool hasTriggered = false;

    public void SetupTrap(KommyController target)
    {
        kommy = target;
    }

    void Update()
    {
        LevelManager lm = FindAnyObjectByType<LevelManager>();
        if (lm != null && !lm.gameIsActive) return;

        // 1. GRAVITY: Fall down until it hits exactly floorY
        if (transform.position.y > floorY)
        {
            // Space.World guarantees it falls straight down, even if the sprite is rotated!
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
            
            // Snap perfectly to floorY
            if (transform.position.y < floorY)
            {
                transform.position = new Vector3(transform.position.x, floorY, transform.position.z);
            }
        }

        // 2. Move left along the treadmill
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.World);

        if (kommy == null || hasTriggered) return;

        // 3. DYNAMIC POSITION CHECK (Kommy's X axis)
        if (transform.position.x <= kommy.transform.position.x + triggerDistance &&
            transform.position.x >= kommy.transform.position.x - triggerDistance)
        {
            // 4. Safety Checks: Is Kommy jumping? Is the trap on the floor yet?
            if (kommy.currentState != KommyController.CharacterState.Jumping)
            {
                // Ensures she doesn't get hit if the trap is still falling
                if (transform.position.y <= floorY + 0.1f) 
                {
                    hasTriggered = true; 

                    if (isPoisonTrap)
                    {
                        // <--- NEW: Spawn the poison splash exactly where Kommy is! --->
                        if (poisonSplashPrefab != null)
                        {
                            Instantiate(poisonSplashPrefab, kommy.transform.position, Quaternion.identity);
                        }

                        WordManager wm = FindAnyObjectByType<WordManager>();
                        if (wm != null) wm.TriggerPoisonFromTrap();
                    }
                    else
                    {
                        kommy.HitByTrap(); 
                    }

                    Destroy(gameObject); 
                }
            }
        }

        // 5. Cleanup when off-screen
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }
}