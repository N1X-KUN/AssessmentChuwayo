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
    
    // THE THIEF'S POCKET
    public List<string> thiefPocket = new List<string>();

    void Start()
    {
        thiefPocket.AddRange(wordDatabase);
    }

    public void StartSpawning()
    {
        InvokeRepeating(nameof(SpawnFood), 0.5f, spawnRate);
    }

    public void StopSpawning()
    {
        CancelInvoke(nameof(SpawnFood));
    }

    void SpawnFood()
    {
        if (thiefPocket.Count == 0) return;

        int randomIndex = Random.Range(0, thiefPocket.Count);
        string chosenWord = thiefPocket[randomIndex];
        
        thiefPocket.RemoveAt(randomIndex);

        GameObject newFoodObj = Instantiate(foodPrefab, spawnPoint.position, Quaternion.identity);
        FoodItem newFood = newFoodObj.GetComponent<FoodItem>();
        
        newFood.SetupFood(chosenWord, null, this);
        activeFoods.Add(newFood); 
    }

    public void MissedFood(FoodItem missedFood, string originalWord)
    {
        thiefPocket.Add(originalWord);
        activeFoods.Remove(missedFood);
        
        if (targetedFood == missedFood)
        {
            targetedFood = null;
        }
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