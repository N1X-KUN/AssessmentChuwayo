using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FoodEntry
{
    public string word;
    public Sprite foodSprite;
}

public class WordManager : MonoBehaviour
{
    [Header("References")]
    public GameObject foodPrefab;
    public Transform spawnPoint;
    public KommyController kommy;

    [Header("Food Database (Put all Fruits, Meats, Veggies here!)")]
    public List<FoodEntry> allFoods = new List<FoodEntry>();
    public float spawnRate = 2.5f;

    [Header("Rotten Food Settings")]
    public bool canDropRottenFood = false; 
    public float rottenChance = 30f; 
    public GameObject dizzyOverlay; 
    public float dizzyDuration = 4f; 

    [Header("Typing Mechanics")]
    private List<FoodItem> activeFoods = new List<FoodItem>(); 
    private FoodItem targetedFood = null; 
    
    public List<FoodEntry> thiefPocket = new List<FoodEntry>();

    [HideInInspector]
    public bool isPlayerDizzy = false; 

    void Start()
    {
        foreach (var food in allFoods)
        {
            food.word = food.word.ToLower();
        }
        
        thiefPocket.AddRange(allFoods);
        if (dizzyOverlay != null) dizzyOverlay.SetActive(false);
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
        // 1. If the pocket is empty, wait for him to reload!
        if (thiefPocket.Count == 0) return; 

        // 2. Roll the dice to see if the food he drops will be poisoned
        bool droppingRotten = false;
        if (canDropRottenFood)
        {
            float randomRoll = Random.Range(0f, 100f);
            if (randomRoll <= rottenChance)
            {
                droppingRotten = true;
            }
        }

        // 3. Reach into the pocket and pull out a normal food (like Apple or Melon)
        int randomIndex = Random.Range(0, thiefPocket.Count);
        FoodEntry chosenEntry = thiefPocket[randomIndex];
        thiefPocket.RemoveAt(randomIndex);

        // 4. Spawn it!
        GameObject newFoodObj = Instantiate(foodPrefab, spawnPoint.position, Quaternion.identity);
        FoodItem newFood = newFoodObj.GetComponent<FoodItem>();
        
        // 5. Send it to the FoodItem! 
        // If droppingRotten is TRUE, the FoodItem script automatically tints this sprite Dark Green!
        newFood.SetupFood(chosenEntry.word, chosenEntry.foodSprite, this, droppingRotten);
        activeFoods.Add(newFood); 
    }

    public void MissedFood(FoodItem missedFood, string originalWord)
    {
        // Only normal, safe foods go back into the pocket. Traps vanish forever!
        if (!missedFood.isRotten)
        {
            FoodEntry originalEntry = allFoods.Find(x => x.word == originalWord);
            if (originalEntry != null)
            {
                thiefPocket.Add(originalEntry);
            }
        }
        
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
                if (targetedFood.isRotten)
                {
                    kommy.TypeWrongLetter(); 
                    StartCoroutine(TriggerDizzyEffect());
                    
                    activeFoods.Remove(targetedFood);
                    Destroy(targetedFood.gameObject);
                    targetedFood = null;
                    return; 
                }

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

    private IEnumerator TriggerDizzyEffect()
    {
        isPlayerDizzy = true; 
        if (dizzyOverlay != null) dizzyOverlay.SetActive(true);
        
        yield return new WaitForSeconds(dizzyDuration);
        
        isPlayerDizzy = false; 
        if (dizzyOverlay != null) dizzyOverlay.SetActive(false);
    }
}