using UnityEngine;
using TMPro;

public class FoodItem : MonoBehaviour
{
    [Header("Food Data")]
    public string currentWord;
    public string originalWord; 
    public bool isRotten = false; 
    
    private TMP_Text floatingText; 
    private SpriteRenderer foodImage;
    private WordManager wordManager; 

    [Header("Settings")]
    public float floatSpeed = 1.5f;    
    public float fallSpeed = 2f;       
    public float groundYLevel = -2.5f; 

    [Header("Dizzy Effect Settings")]
    public float shakeIntensity = 0.05f; 
    public float scrambleSpeed = 0.1f;   

    private bool hasHitGround = false;
    private Vector3 textStartingLocalPos; 
    private float scrambleTimer = 0f;
    private bool isCurrentlyDizzy = false;

    void Awake()
    {
        floatingText = GetComponentInChildren<TMP_Text>();
        foodImage = GetComponent<SpriteRenderer>();

        if (floatingText != null)
        {
            textStartingLocalPos = floatingText.transform.localPosition;
        }
    }

    public void SetupFood(string newWord, Sprite newSprite, WordManager manager, bool makeRotten = false)
    {
        currentWord = newWord;
        originalWord = newWord; 
        wordManager = manager;
        isRotten = makeRotten;
        
        if (floatingText != null) floatingText.text = currentWord;
        
        if (foodImage != null && newSprite != null) 
        {
            foodImage.sprite = newSprite;
        }

        // Apply a sickly green tint IF and ONLY IF it's gross and rotten!
        if (isRotten && foodImage != null)
        {
            foodImage.color = new Color(0.3f, 0.6f, 0.3f); // Dark sickly green
        }
        else if (foodImage != null)
        {
            foodImage.color = Color.white; // Pure normal colors for fresh food
        }

        // --- NEW TEXT RULE: Even if rotten, the word stays pure white! ---
        if (floatingText != null) 
        {
            floatingText.color = Color.white; 
        }
    }

    void Update()
    {
        transform.Translate(Vector3.left * floatSpeed * Time.deltaTime);

        if (!hasHitGround)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            if (transform.position.y <= groundYLevel)
            {
                hasHitGround = true;
                
                if (wordManager != null && !isRotten)
                {
                    wordManager.MissedFood(this, originalWord);
                }
                
                Destroy(gameObject);
            }
        }

        bool managerSaysDizzy = (wordManager != null && wordManager.isPlayerDizzy);

        if (managerSaysDizzy)
        {
            isCurrentlyDizzy = true; 
            
            if (floatingText != null)
            {
                float shakeX = Random.Range(-shakeIntensity, shakeIntensity);
                float shakeY = Random.Range(-shakeIntensity, shakeIntensity);
                floatingText.transform.localPosition = textStartingLocalPos + new Vector3(shakeX, shakeY, 0);
                
                scrambleTimer -= Time.deltaTime;
                if (scrambleTimer <= 0)
                {
                    floatingText.text = ScrambleWord(currentWord);
                    scrambleTimer = scrambleSpeed; 
                }
            }
        }
        else
        {
            if (isCurrentlyDizzy)
            {
                isCurrentlyDizzy = false; 
                
                if (floatingText != null)
                {
                    floatingText.transform.localPosition = textStartingLocalPos;
                    floatingText.text = currentWord; 
                }
            }
        }
    }

    private string ScrambleWord(string word)
    {
        char[] chars = word.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            int randomIndex = Random.Range(0, chars.Length);
            char temp = chars[i];
            chars[i] = chars[randomIndex];
            chars[randomIndex] = temp;
        }
        return new string(chars);
    }

    public void RemoveFirstLetter()
    {
        currentWord = currentWord.Substring(1);
        
        if (wordManager != null && wordManager.isPlayerDizzy)
        {
            if (floatingText != null) 
            {
                floatingText.text = ScrambleWord(currentWord);
            }
            scrambleTimer = scrambleSpeed; 
        }
        else if (floatingText != null)
        {
            floatingText.text = currentWord;
        }
    }
}