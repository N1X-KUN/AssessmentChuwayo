using UnityEngine;

public class GroundTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    public float moveSpeed = 2f; 
    public float kommyPositionX = -5f; 
    
    [Header("Trap Type")]
    public bool isAcidTrap = false; // CHECK THIS BOX ON THE ACID BARREL PREFAB!

    [Header("Gravity Settings")]
    public float fallSpeed = 8f;       
    public float groundYLevel = -3.5f; 

    public KommyController kommy; 
    private bool hasTriggered = false;
    private bool hasHitGround = false; 

    public void SetupTrap(KommyController playerRef)
    {
        kommy = playerRef;
    }

    void Update()
    {
        // 1. GRAVITY
        if (!hasHitGround)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
            if (transform.position.y <= groundYLevel)
            {
                transform.position = new Vector3(transform.position.x, groundYLevel, transform.position.z);
                hasHitGround = true; 
            }
        }

        // 2. SLIDE
        if (kommy != null && (kommy.currentState == KommyController.CharacterState.Running || 
            kommy.currentState == KommyController.CharacterState.Attacking || 
            kommy.currentState == KommyController.CharacterState.Jumping))
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }

        // 3. TRIGGER (BITE/POISON)
        if (hasHitGround && !hasTriggered && transform.position.x <= kommyPositionX + 0.5f) 
        {
            hasTriggered = true; 

            if (kommy != null && kommy.currentState != KommyController.CharacterState.Jumping) 
            {
                // Is this the Acid Trap?
                if (isAcidTrap)
                {
                    FindAnyObjectByType<WordManager>().TriggerPoisonFromTrap();
                }
                else
                {
                    kommy.HitByTrap(); 
                }
            }

            Destroy(gameObject, 2f);
        }
    }
}