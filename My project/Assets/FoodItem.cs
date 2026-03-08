using UnityEngine;
using TMPro;
using System.Collections;

public class FoodItem : MonoBehaviour
{
    public enum FoodState { Falling, Magnetizing, Swatted }
    private FoodState state = FoodState.Falling;

    [Header("Food Data")]
    public string currentWord;
    public string originalWord; 
    public bool isRotten = false; 
    
    private TMP_Text floatingText; 
    private SpriteRenderer foodImage;
    public WordManager wordManager; 

    [Header("Toss & Float Settings")]
    public float launchUpForce = 7f;      // High initial toss arc
    public float launchLeftForce = 3.5f;  // Thrown back behind the Thief
    public float gravityScale = 1.2f;     // Floatiness in the air
    public float groundYLevel = -3.5f; 

    [Header("Magnet Settings")]
    public float magnetSpeed = 20f;       // Fast zip to Kommy

    private Vector3 velocity; 
    private bool hasHitGround = false;
    [HideInInspector] public bool isFading = false;

    void Awake()
    {
        floatingText = GetComponentInChildren<TMP_Text>();
        foodImage = GetComponent<SpriteRenderer>();

        // INITIAL TOSS: Creates the parabolic "hurl" curve behind the thief
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
        // Force text to stay horizontal even when sprite spins
        if (floatingText != null) floatingText.transform.rotation = Quaternion.identity;

        // Execute physics based on current state
        switch (state)
        {
            case FoodState.Falling:
                HandleFalling();
                break;
            case FoodState.Magnetizing:
                HandleMagnet();
                break;
            case FoodState.Swatted:
                HandleSwat();
                break;
        }
        
        // Constant sprite rotation for "juice"
        transform.Rotate(0, 0, 180f * Time.deltaTime);
    }

    void HandleFalling()
    {
        if (hasHitGround) return; 

        // Physics: V = V + (G * dt)
        velocity.y -= gravityScale * Time.deltaTime;
        
        // Movement: P = P + (V * dt)
        transform.position += velocity * Time.deltaTime;

        // Ground check
        if (transform.position.y <= groundYLevel)
        {
            hasHitGround = true;
            velocity = Vector3.zero;
            transform.position = new Vector3(transform.position.x, groundYLevel, transform.position.z);
            
            if (wordManager != null && !isRotten) wordManager.MissedFood(this, originalWord);
            if (!isFading) StartCoroutine(FadeOutAndDestroy());
        }
    }

    void HandleMagnet()
    {
        if (wordManager == null || wordManager.kommy == null) return;

        // Fly straight into Kommy's hands (offset up slightly for chest height)
        Vector3 targetPos = wordManager.kommy.transform.position + Vector3.up;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, magnetSpeed * Time.deltaTime);

        // Shrink slightly as she catches it
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 0.5f, 5f * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            Destroy(gameObject); // Caught!
        }
    }

    void HandleSwat()
    {
        // High-speed violent launch away
        transform.position += velocity * Time.deltaTime;
        
        // Swatted items have heavier gravity for a "smashed" look
        velocity.y -= gravityScale * 3f * Time.deltaTime; 
        
        if (transform.position.y < -10f || transform.position.x < -20f)
        {
            Destroy(gameObject);
        }
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
            floatingText.transform.localScale = Vector3.one * 1.15f; // Small pop while typing
        }
        else
        {
            floatingText.text = originalWord;
            floatingText.color = Color.white;
            floatingText.transform.localScale = Vector3.one;
        }
    }

    public void VanishOnSuccess()
    {
        state = FoodState.Magnetizing;
        if (floatingText != null) floatingText.text = ""; // Hide words while magnetizing
    }

    public void SwatAway()
    {
        state = FoodState.Swatted;
        // Launch: Violent burst to the left and slightly up
        velocity = new Vector3(-18f, 9f, 0); 
        if (floatingText != null) floatingText.text = ""; 
    }

    public void TriggerMistake() 
    { 
        gravityScale += 0.4f; // Penalty: Drops faster on mistake
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