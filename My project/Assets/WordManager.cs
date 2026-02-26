using System.Collections.Generic;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    [Header("References")]
    public GameObject foodPrefab;
    public Transform spawnPoint;
    public KommyController kommy;

    [Header("Spawning Settings")]
    public string[] wordDatabase = { "apple", "banana", "pear", "peach", "grape" };
    public float spawnRate = 2.5f;

    [Header("Typing Mechanics")]
    // This list tracks every food currently on the screen
    private List<FoodItem> activeFoods = new List<FoodItem>(); 
    // This remembers which word you are currently attacking
    private FoodItem targetedFood = null; 

    void Start()
    {
        InvokeRepeating(nameof(SpawnFood), 1f, spawnRate);
    }

    void SpawnFood()
    {
        GameObject newFoodObj = Instantiate(foodPrefab, spawnPoint.position, Quaternion.identity);
        FoodItem newFood = newFoodObj.GetComponent<FoodItem>();
        string randomWord = wordDatabase[Random.Range(0, wordDatabase.Length)];
        
        newFood.SetupFood(randomWord, null);
        activeFoods.Add(newFood); // Add it to our active list!
    }

    void Update()
    {
        // Unity's built-in way to catch exactly what letter you typed on your keyboard
        string input = Input.inputString.ToLower();

        foreach (char c in input)
        {
            // We only care about normal alphabet letters (ignore backspaces, enters, etc)
            if (c >= 'a' && c <= 'z')
            {
                TypeLetter(c);
            }
        }
    }

    void TypeLetter(char letter)
    {
        // 1. IF WE DON'T HAVE A TARGET: Find the first word that starts with the typed letter
        if (targetedFood == null)
        {
            foreach (FoodItem food in activeFoods)
            {
                if (food.currentWord.StartsWith(letter.ToString()))
                {
                    targetedFood = food;
                    break; // Target locked!
                }
            }
        }

        // 2. IF WE HAVE A TARGET: Check if the typed letter matches the next letter needed
        if (targetedFood != null)
        {
            if (targetedFood.currentWord[0] == letter)
            {
                // CORRECT! 
                kommy.TypeCorrectLetter(); 
                targetedFood.RemoveFirstLetter();

                // Did we completely finish the spelling?
                if (targetedFood.currentWord.Length == 0)
                {
                    activeFoods.Remove(targetedFood); // Remove from list
                    Destroy(targetedFood.gameObject); // Destroy the apple!
                    targetedFood = null; // Reset target so we can type the next one
                }
            }
            else
            {
                // WRONG! User made a typo
                kommy.TypeWrongLetter(); 
            }
        }
    }
}