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
    private List<FoodItem> activeFoods = new List<FoodItem>(); 
    private FoodItem targetedFood = null; 

    void Start()
    {
        // We removed the automatic InvokeRepeating from here!
        // The Thief will tell this script when to start now.
    }

    // --- NEW SWITCHES ---
    public void StartSpawning()
    {
        // Starts dropping food every few seconds
        InvokeRepeating(nameof(SpawnFood), 0.5f, spawnRate);
    }

    public void StopSpawning()
    {
        // Instantly stops the dropping
        CancelInvoke(nameof(SpawnFood));
    }
    // --------------------

    void SpawnFood()
    {
        GameObject newFoodObj = Instantiate(foodPrefab, spawnPoint.position, Quaternion.identity);
        FoodItem newFood = newFoodObj.GetComponent<FoodItem>();
        string randomWord = wordDatabase[Random.Range(0, wordDatabase.Length)];
        
        newFood.SetupFood(randomWord, null);
        activeFoods.Add(newFood); 
    }

    void Update()
    {
        string input = Input.inputString.ToLower();

        foreach (char c in input)
        {
            if (c >= 'a' && c <= 'z')
            {
                TypeLetter(c);
            }
        }
    }

    void TypeLetter(char letter)
    {
        if (targetedFood == null)
        {
            foreach (FoodItem food in activeFoods)
            {
                if (food.currentWord.StartsWith(letter.ToString()))
                {
                    targetedFood = food;
                    break; 
                }
            }
        }

        if (targetedFood != null)
        {
            if (targetedFood.currentWord[0] == letter)
            {
                kommy.TypeCorrectLetter(); 
                targetedFood.RemoveFirstLetter();

                if (targetedFood.currentWord.Length == 0)
                {
                    activeFoods.Remove(targetedFood); 
                    Destroy(targetedFood.gameObject); 
                    targetedFood = null; 
                }
            }
            else
            {
                kommy.TypeWrongLetter(); 
            }
        }
    }
}