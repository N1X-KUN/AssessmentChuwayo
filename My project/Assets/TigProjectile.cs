using UnityEngine;
using System.Collections; 

public class TigProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 8f;
    public float lifeTime = 4f; 
    public float hitDistance = 1.0f; 

    private ThiefController thief;
    private bool hasHitSomething = false;
    private Animator anim; 

    void Start()
    {
        thief = FindAnyObjectByType<ThiefController>();
        anim = GetComponent<Animator>(); 
        
        // Start the timer for when it flies off the screen
        StartCoroutine(FlyOutOfBoundsRoutine(lifeTime));
    }

    void Update()
    {
        // Stop moving forward if it exploded!
        if (hasHitSomething) return; 

        // Move Right
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Check distance to Thief
        if (thief != null)
        {
            if (Mathf.Abs(transform.position.x - thief.transform.position.x) <= hitDistance)
            {
                if (!thief.isFlying)
                {
                    hasHitSomething = true;
                    
                    thief.TriggerTumbleHit(); 
                    
                    KommyController player = FindAnyObjectByType<KommyController>();
                    if (player != null) player.TriggerHappyFace();

                    // Play Slash 4 and destroy
                    StartCoroutine(PlayImpactAndDestroy());
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Trap") || col.CompareTag("Obstacle"))
        {
            // Destroy the rock, keep flying!
            Destroy(col.gameObject);
        }
    }

    // Plays the Slash4 animation, waits 0.2 seconds to show it, then deletes it
    private IEnumerator PlayImpactAndDestroy()
    {
        if (anim != null) anim.Play("SlashHit"); 
        yield return new WaitForSeconds(0.2f); 
        Destroy(gameObject);
    }

    // If it survives 4 seconds without hitting the thief, explode at the edge of the screen
    private IEnumerator FlyOutOfBoundsRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!hasHitSomething)
        {
            hasHitSomething = true;
            StartCoroutine(PlayImpactAndDestroy()); 
        }
    }
}