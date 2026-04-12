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

    [Header("Homing Projectile Settings")]
    public float flightTime = 1.2f; // How fast it hits Kommy
    [Tooltip("Increase this number to make the throw curve HIGHER into the air!")]
    public float arcCurveStrength = 15f; // The secret to the perfect "Pop Up" curve!
    
    [Header("Magnet Settings")]
    public float magnetSpeed = 20f;

    [Header("Swat & Roll Settings")]
    public float rollSpeed = 2f; 
    public float groundYLevel = -3.5f; 

    private Vector3 velocity; 
    private bool hasHitGround = false;
    [HideInInspector] public bool isFading = false;
    private float scrambleTimer = 0f;

    // The exact coordinate of Kommy's head!
    private Vector3 targetHeadPos = new Vector3(-6.5f, -1.5f, 0f);

    void Awake()
    {
        floatingText = GetComponentInChildren<TMP_Text>();
        foodImage = GetComponent<SpriteRenderer>();
    }

    public void SetupFood(string newWord, Sprite newSprite, WordManager manager, bool makeRotten = false)
    {
        currentWord = newWord;
        originalWord = newWord; 
        wordManager = manager;
        isRotten = makeRotten;

        if (foodImage != null && newSprite != null) foodImage.sprite = newSprite;
        if (isRotten && foodImage != null) foodImage.color = new Color(0.4f, 1f, 0.4f, 1f); 
        else if (foodImage != null) foodImage.color = Color.white; 

        UpdateVisuals();

        // --- THE PERFECT CURVE MATH ---
        float distanceX = targetHeadPos.x - transform.position.x;
        float distanceY = targetHeadPos.y - transform.position.y;

        float vx = distanceX / flightTime;
        
        // This calculates exactly how hard to throw it UP to fight your "Arc Curve Strength" 
        // and still land perfectly on her head!
        float vy = (distanceY + (0.5f * arcCurveStrength * flightTime * flightTime)) / flightTime;

        velocity = new Vector3(vx, vy, 0);
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

        velocity.y -= arcCurveStrength * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        if (transform.position.x <= targetHeadPos.x)
        {
            hasHitGround = true;
            transform.position = new Vector3(targetHeadPos.x, targetHeadPos.y, transform.position.z); 
            
            if (wordManager != null) 
            {
                // --- THE iFRAME CHECK ---
                // If Kommy is already stunned or dead, the food "passes through" her head
                if (wordManager.kommy.currentState == KommyController.CharacterState.Stunned || 
                    wordManager.kommy.currentState == KommyController.CharacterState.Dead)
                {
                    // Do nothing! No bonk, no thief movement.
                }
                else 
                {
                    wordManager.kommy.TriggerBonk(); 
                }
                
                wordManager.MissedFood(this, originalWord);
            }
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
        velocity.y -= arcCurveStrength * 2f * Time.deltaTime; 
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
        arcCurveStrength += 10f; // Make it drop much faster if they make a typo!
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