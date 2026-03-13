using UnityEngine;
using System.Collections;

public class PlayerProjectile : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private ThiefController thief;
    
    private Vector3 targetPos;
    private bool willHit;
    private float speed = 8f; // LOWERED: Normal throwing speed!

    public void Setup(Sprite foodSprite)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        
        spriteRenderer.sprite = foodSprite;
        spriteRenderer.sortingOrder = 5;
        
        // REMOVED the scaling! It will now be normal size.
        transform.localScale = Vector3.one; 

        thief = FindAnyObjectByType<ThiefController>();
        
        if (thief != null)
        {
            if (thief.transform.position.y > -2f) 
            {
                willHit = true;
                targetPos = thief.transform.position; 
            }
            else
            {
                willHit = false;
                targetPos = thief.transform.position + new Vector3(3f, 5f, 0f); 
                thief.ShowEmoticon("EmoticonLaugh"); 
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