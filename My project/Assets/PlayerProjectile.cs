using UnityEngine;
using System.Collections;

public class PlayerProjectile : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private ThiefController thief;
    private KommyController kommy; // Added Kommy reference
    
    private Vector3 targetPos;
    private bool willHit;
    private float speed = 8f; 

    public void Setup(Sprite foodSprite)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        
        spriteRenderer.sprite = foodSprite;
        spriteRenderer.sortingOrder = 5;
        transform.localScale = Vector3.one; 

        thief = FindAnyObjectByType<ThiefController>();
        kommy = FindAnyObjectByType<KommyController>();
        
        if (thief != null)
        {
            // If thief is flying, we hit him!
            if (thief.transform.position.y > -2f) 
            {
                willHit = true;
                targetPos = thief.transform.position; 
            }
            else
            {
                // If thief is on ground, we miss!
                willHit = false;
                targetPos = thief.transform.position + new Vector3(3f, 5f, 0f); 
                // Thief laughs at your miss!
                thief.ShowEmoticon("EmoticonLaugh", 2.05f);
            }
        }
    }

    void Update()
    {
        if (thief == null) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        transform.Rotate(0, 0, -300f * Time.deltaTime); 

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            // The Moment of Impact!
            if (kommy != null) kommy.EndSwipeAnimation(); // Return Kommy to Run pose

            if (willHit)
            {
                thief.TriggerTumbleHit(); 
                Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject, 1f); 
            }
        }
    }
}