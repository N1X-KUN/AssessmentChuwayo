using UnityEngine;
using TMPro;
using System.Collections;

public class FoodItem : MonoBehaviour
{
    public enum FoodState { Falling, Magnetizing, Swatted, Rolling }
    private FoodState state = FoodState.Falling;

    [Header("Food Data")]
    public string currentWord;
    public string originalWord; 
    public bool isRotten = false; 
    
    private TMP_Text floatingText; 
    private SpriteRenderer foodImage;
    public WordManager wordManager; 

    [Header("Toss & Float Settings")]
    public float launchUpForce = 7f;      
    public float launchLeftForce = 3.5f;  
    public float gravityScale = 1.2f;     
    public float groundYLevel = -3.5f; 

    [Header("Magnet Settings")]
    public float magnetSpeed = 20f;

    [Header("Swat & Roll Settings")]
    public float rollSpeed = 2f; 

    private Vector3 velocity; 
    private bool hasHitGround = false;
    [HideInInspector] public bool isFading = false;
    private float scrambleTimer = 0f;

    void Awake()
    {
        floatingText = GetComponentInChildren<TMP_Text>();
        foodImage = GetComponent<SpriteRenderer>();
        // Removed hardcoded velocity here so the dynamic throw works!
    }

    public void SetupFood(string newWord, Sprite newSprite, WordManager manager, bool makeRotten = false)
    {
        currentWord = newWord;
        originalWord = newWord; 
        wordManager = manager;
        isRotten = makeRotten;

        if (foodImage != null && newSprite != null) foodImage.sprite = newSprite;

        // --- NEW: TOXIC GREEN TINT FOR ROTTEN FOOD ---
        if (isRotten && foodImage != null)
        {
            foodImage.color = new Color(0.4f, 1f, 0.4f, 1f); 
        }
        else if (foodImage != null)
        {
            foodImage.color = Color.white; 
        }

        UpdateVisuals();

        // --- NEW: DYNAMIC TARGETED THROWING ---
        if (wordManager != null && wordManager.kommy != null)
        {
            // Calculate distance and time to perfectly arc the throw!
            float distanceToKommy = transform.position.x - wordManager.kommy.transform.position.x;
            float timeInAir = (launchUpForce / gravityScale) * 1.8f; 
            float dynamicLeftForce = distanceToKommy / timeInAir;

            velocity = new Vector3(-dynamicLeftForce, launchUpForce, 0);
        }
        else
        {
            velocity = new Vector3(-launchLeftForce, launchUpForce, 0);
        }
    }

    void Update()
    {
        if (floatingText != null) floatingText.transform.rotation = Quaternion.identity;

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
            case FoodState.Rolling:
                HandleRolling();
                break;
        }

        if (wordManager != null && wordManager.isPlayerDizzy && !isFading && state == FoodState.Falling)
        {
            scrambleTimer -= Time.deltaTime;
            if (scrambleTimer <= 0)
            {
                floatingText.text = ScrambleWord(currentWord);
                scrambleTimer = 0.1f;
            }
        }
        
        transform.Rotate(0, 0, 180f * Time.deltaTime);
    }

    void HandleFalling()
    {
        if (hasHitGround) return; 

        velocity.y -= gravityScale * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

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

        Vector3 targetPos = wordManager.kommy.transform.position + Vector3.up;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, magnetSpeed * Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 0.5f, 5f * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            Destroy(gameObject); 
        }
    }

    public void SwatAway()
    {
        state = FoodState.Swatted;
        velocity = new Vector3(6f, 4f, 0); 
        if (floatingText != null) floatingText.text = ""; 
    }

    void HandleSwat()
    {
        velocity.y -= gravityScale * 2f * Time.deltaTime; 
        transform.position += velocity * Time.deltaTime;

        if (transform.position.y <= groundYLevel)
        {
            transform.position = new Vector3(transform.position.x, groundYLevel, transform.position.z);
            state = FoodState.Rolling;
            velocity = new Vector3(rollSpeed, 0, 0); 
            if (!isFading) StartCoroutine(FadeOutAndDestroy());
        }
    }

    void HandleRolling()
    {
        transform.position += velocity * Time.deltaTime;
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
            floatingText.transform.localScale = Vector3.one * 1.15f; 
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
        if (floatingText != null) floatingText.text = ""; 
    }

    public void TriggerMistake() 
    { 
        gravityScale += 0.4f; 
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

    private string ScrambleWord(string word)
    {
        char[] chars = word.ToCharArray();
        for (int i = 0; i < chars.Length; i++) {
            int randomIndex = Random.Range(0, chars.Length);
            char temp = chars[i];
            chars[i] = chars[randomIndex];
            chars[randomIndex] = temp;
        }
        return new string(chars);
    }

    private IEnumerator FadeOutAndDestroy()
    {
        isFading = true;
        if (floatingText != null) floatingText.text = "";
        
        float elapsed = 0f;
        while(elapsed < 1.5f) 
        {
            elapsed += Time.deltaTime;
            foodImage.color = new Color(1, 1, 1, 1f - (elapsed / 1.5f));
            yield return null;
        }
        Destroy(gameObject);
    }
}