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
        StartCoroutine(FlyOutOfBoundsRoutine(lifeTime));
    }

    void Update()
    {
        if (hasHitSomething) return; 

        transform.Translate(Vector3.right * speed * Time.deltaTime);

        if (thief != null)
        {
            if (Mathf.Abs(transform.position.x - thief.transform.position.x) <= hitDistance)
            {
                if (!thief.isFlying)
                {
                    hasHitSomething = true;
                    
                    // --- NEW: HEAVY GROUND STUN ---
                    thief.StepBackward(); // Push 1
                    thief.StepBackward(); // Push 2 (Double distance!)
                    thief.anim.Play("ThiefStun"); // Force the visual stun
                    thief.ShowEmoticon("EmoticonCry", 2.05f); // Make him cry
                    
                    KommyController player = FindAnyObjectByType<KommyController>();
                    if (player != null) player.TriggerHappyFace();

                    StartCoroutine(PlayImpactAndDestroy());
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Trap") || col.CompareTag("Obstacle"))
        {
            // --- NEW: DESTROY THE ROOT PARENT ---
            // This guarantees the visual sprite is destroyed instantly, not just the collider child!
            if (col.transform.parent != null)
            {
                Destroy(col.transform.parent.gameObject);
            }
            else
            {
                Destroy(col.gameObject);
            }
        }
    }

    private IEnumerator PlayImpactAndDestroy()
    {
        if (anim != null) anim.Play("SlashHit"); 
        yield return new WaitForSeconds(0.2f); 
        Destroy(gameObject);
    }

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