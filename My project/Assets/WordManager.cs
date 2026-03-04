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
    
    // --- THE NEW ANIMATOR REFERENCE ---
    // Instead of a simple GameObject, we are looking for the new Animator Script directly!
    public DizzyOverlayAnimator dizzyAnimator; 
    // ------------------------------------
    
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
        
        // Ensure the overlay is fully off on level start
        if (dizzyAnimator != null) 
        {
            dizzyAnimator.gameObject.SetActive(false);
        }
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

        bool droppingRotten = false;
        if (canDropRottenFood)
        {
            float randomRoll = Random.Range(0f, 100f);
            if (randomRoll <= rottenChance)
            {
                droppingRotten = true;
            }
        }

        int randomIndex = Random.Range(0, thiefPocket.Count);
        FoodEntry chosenEntry = thiefPocket[randomIndex];
        thiefPocket.RemoveAt(randomIndex);

        GameObject newFoodObj = Instantiate(foodPrefab, spawnPoint.position, Quaternion.identity);
        FoodItem newFood = newFoodObj.GetComponent<FoodItem>();
        
        newFood.SetupFood(chosenEntry.word, chosenEntry.foodSprite, this, droppingRotten);
        activeFoods.Add(newFood); 
    }

    public void MissedFood(FoodItem missedFood, string originalWord)
    {
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

    // --- THE POLISHED COROUTINE ---
    private IEnumerator TriggerDizzyEffect()
    {
        isPlayerDizzy = true; 
        
        // 1. Tell the Animator to run the Intro (Fades from sprite 1-10)
        if (dizzyAnimator != null) 
        {
            dizzyAnimator.gameObject.SetActive(true);
            yield return StartCoroutine(dizzyAnimator.PlayPoisonIntro()); 
        }
        
        // 2. Stay in full dizzy mode for the duration
        yield return new WaitForSeconds(dizzyDuration);
        
        // 3. Tell the Animator to run the Outro (Fades from sprite 10-1)
        if (dizzyAnimator != null) 
        {
            yield return StartCoroutine(dizzyAnimator.PlayPoisonOutro()); 
            dizzyAnimator.gameObject.SetActive(false);
        }
        
        isPlayerDizzy = false; 
    }
}