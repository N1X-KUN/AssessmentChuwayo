using UnityEngine;
using TMPro;
using System.Collections;

public class FoodItem : MonoBehaviour
{
    [Header("Food Data")]
    public string currentWord;
    public string originalWord; 
    public bool isRotten = false; 
    
    private TMP_Text floatingText; 
    private SpriteRenderer foodImage;
    private WordManager wordManager; 

    [Header("Toss & Float Settings")]
    public float launchUpForce = 3f;      // How high he throws it up
    public float launchLeftForce = 4f;    // How hard he throws it back
    public float gravityScale = 1.5f;     // LOWER this to make it float more!
    public float groundYLevel = -3.5f; 

    private Vector3 velocity; // Tracks the current movement
    private bool hasHitGround = false;
    private bool isMistActive = false;
    [HideInInspector] public bool isFading = false;

    void Awake()
    {
        floatingText = GetComponentInChildren<TMP_Text>();
        foodImage = GetComponent<SpriteRenderer>();
        
        // INITIAL TOSS: Set the starting speed (Up and Left)
        velocity = new Vector3(-launchLeftForce, launchUpForce, 0);
    }

    public void SetupFood(string newWord, Sprite newSprite, WordManager manager, bool makeRotten = false)
    {
        currentWord = newWord;
        originalWord = newWord; 
        wordManager = manager;
        isRotten = makeRotten;
        if (foodImage != null && newSprite != null) foodImage.sprite = newSprite;
        UpdateVisuals();
    }

    void Update()
    {
        // 1. Keep text upright
        if (floatingText != null) floatingText.transform.rotation = Quaternion.identity;

        if (!hasHitGround)
        {
            // 2. PROJECTILE PHYSICS
            // Apply gravity to the vertical speed over time
            velocity.y -= gravityScale * Time.deltaTime;
            
            // Move the object based on the velocity
            transform.position += velocity * Time.deltaTime;

            // 3. GROUND CHECK
            if (transform.position.y <= groundYLevel)
            {
                hasHitGround = true;
                velocity = Vector3.zero; // Stop moving
                transform.position = new Vector3(transform.position.x, groundYLevel, transform.position.z);
                
                if (wordManager != null && !isRotten) wordManager.MissedFood(this, originalWord);
                if (!isFading) StartCoroutine(FadeOutAndDestroy());
            }
        }
        
        // Rotate sprite for "juice"
        transform.Rotate(0, 0, 150f * Time.deltaTime);
    }

    public void UpdateVisuals()
    {
        if (floatingText == null || isFading) return;
        int typed = originalWord.Length - currentWord.Length;
        if (typed > 0)
        {
            string typedPart = originalWord.Substring(0, typed);
            string remainingPart = originalWord.Substring(typed);
            floatingText.text = $"<color=yellow>{typedPart}</color>{remainingPart}";
        }
        else
        {
            floatingText.text = originalWord;
            floatingText.color = Color.white;
        }
    }

    public void TriggerMistake()
    {
        // Increase gravity on mistake so it drops faster as punishment!
        gravityScale += 0.5f;
        StartCoroutine(MistakeFlash());
    }

    private IEnumerator MistakeFlash()
    {
        if (floatingText != null) floatingText.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        UpdateVisuals();
    }

    public void RemoveFirstLetter()
    {
        if (currentWord.Length > 0)
        {
            currentWord = currentWord.Substring(1);
            UpdateVisuals();
        }
    }

    public void CancelLockOn()
    {
        currentWord = originalWord;
        UpdateVisuals();
    }

    public void VanishOnSuccess()
    {
        StartCoroutine(SuccessVanishRoutine());
    }

    private IEnumerator SuccessVanishRoutine()
    {
        isFading = true;
        if (floatingText != null) floatingText.text = "";
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        while(elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, startScale * 1.5f, elapsed / 0.2f);
            yield return null;
        }
        Destroy(gameObject);
    }

    private IEnumerator FadeOutAndDestroy()
    {
        isFading = true;
        if (floatingText != null) floatingText.text = "";
        float elapsed = 0f;
        while(elapsed < 1.0f)
        {
            elapsed += Time.deltaTime;
            foodImage.color = new Color(1, 1, 1, 1f - elapsed);
            yield return null;
        }
        Destroy(gameObject);
    }
}