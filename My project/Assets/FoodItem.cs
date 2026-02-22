using UnityEngine;
using TMPro;

public class FoodItem : MonoBehaviour
{
    [Header("Food Data")]
    public string currentWord;
    
    // Made these private so Unity hides them in the Inspector. No more dragging!
    private TMP_Text floatingText; 
    private SpriteRenderer foodImage;

    [Header("Settings")]
    public float floatSpeed = 1.5f;

    void Awake()
    {
        // MAGIC TRICK: This tells the code to automatically scan the object 
        // and link the Text and the Image the exact millisecond the game starts!
        floatingText = GetComponentInChildren<TMP_Text>();
        foodImage = GetComponent<SpriteRenderer>();
    }

    public void SetupFood(string newWord, Sprite newSprite)
    {
        currentWord = newWord;
        
        if (floatingText != null) floatingText.text = currentWord;
        if (foodImage != null && newSprite != null) foodImage.sprite = newSprite;
    }

    void Update()
    {
        transform.Translate(Vector3.left * floatSpeed * Time.deltaTime);
    }
}