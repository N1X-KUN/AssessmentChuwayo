using UnityEngine;

public class GroundTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    public float moveSpeed = 2f; 
    public float kommyPositionX = -5f; 
    
    // --- NEW: GRAVITY SETTINGS ---
    public float fallSpeed = 8f;       // Fast falling speed like an anvil
    public float groundYLevel = -3.5f; // Where the dirt floor is

    public KommyController kommy; 
    private bool hasTriggered = false;
    private bool hasHitGround = false; // Prevents it from biting Kommy while still in the air!

    public void SetupTrap(KommyController playerRef)
    {
        kommy = playerRef;
    }

    void Update()
    {
        // 1. GRAVITY: Plunge to the ground first!
        if (!hasHitGround)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
            if (transform.position.y <= groundYLevel)
            {
                transform.position = new Vector3(transform.position.x, groundYLevel, transform.position.z);
                hasHitGround = true; 
            }
        }

        // 2. SLIDE: Move to the left, but ONLY if the background is moving
        if (kommy != null && (kommy.currentState == KommyController.CharacterState.Running || kommy.currentState == KommyController.CharacterState.Attacking || kommy.currentState == KommyController.CharacterState.Jumping))
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }

        // 3. BITE: Did the trap just slide under Kommy's feet?
        if (hasHitGround && !hasTriggered && transform.position.x <= kommyPositionX + 0.5f) 
        {
            hasTriggered = true; 

            // Did she forget to jump?!
            if (kommy != null && kommy.currentState != KommyController.CharacterState.Jumping) 
            {
                kommy.HitByTrap(); 
            }

            Destroy(gameObject, 2f);
        }
    }
}