using UnityEngine;
using TMPro;

public class FoodItem : MonoBehaviour
{
    [Header("Food Data")]
    public string currentWord;
    public string originalWord; 
    
    private TMP_Text floatingText; 
    private SpriteRenderer foodImage;
    private WordManager wordManager; 

    [Header("Settings")]
    public float floatSpeed = 1.5f;    
    public float fallSpeed = 2f;       
    public float groundYLevel = -2.5f; 

    private bool hasHitGround = false;

    void Awake()
    {
        floatingText = GetComponentInChildren<TMP_Text>();
        foodImage = GetComponent<SpriteRenderer>();
    }

    public void SetupFood(string newWord, Sprite newSprite, WordManager manager)
    {
        currentWord = newWord;
        originalWord = newWord; 
        wordManager = manager;
        
        if (floatingText != null) floatingText.text = currentWord;
        if (foodImage != null && newSprite != null) foodImage.sprite = newSprite;
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
                
                if (wordManager != null)
                {
                    wordManager.MissedFood(this, originalWord);
                }
                
                Destroy(gameObject);
            }
        }
    }

    public void RemoveFirstLetter()
    {
        currentWord = currentWord.Substring(1);
        if (floatingText != null) floatingText.text = currentWord;
    }
}