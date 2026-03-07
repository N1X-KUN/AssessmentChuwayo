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

    [Header("Food Database")]
    public List<FoodEntry> allFoods = new List<FoodEntry>();
    public float spawnRate = 2.5f;

    [Header("Rotten Food Settings")]
    public bool canDropRottenFood = false; 
    public float rottenChance = 30f; 
    public DizzyOverlayAnimator dizzyAnimator; 
    public float dizzyDuration = 4f; 

    [Header("Physical Trap Settings")]
    public bool canDropPhysicalTraps = false; 
    public float physicalTrapChance = 20f; 
    public GameObject[] physicalTrapPrefabs; 

    [Header("Focus Zone Settings")]
    public float deadZoneX = -6f; 

    [Header("Typing Mechanics")]
    private List<FoodItem> activeFoods = new List<FoodItem>(); 
    private FoodItem targetedFood = null; 
    public List<FoodEntry> thiefPocket = new List<FoodEntry>();
    [HideInInspector] public bool isPlayerDizzy = false; 

    void Start()
    {
        foreach (var food in allFoods) { food.word = food.word.ToLower(); }
        thiefPocket.AddRange(allFoods);
        if (dizzyAnimator != null) dizzyAnimator.gameObject.SetActive(false);
    }

    public void StartSpawning() { InvokeRepeating(nameof(SpawnFood), 0.5f, spawnRate); }
    public void StopSpawning() { CancelInvoke(nameof(SpawnFood)); }

    void SpawnFood()
    {
        if (thiefPocket.Count == 0) return; 

        if (canDropPhysicalTraps && physicalTrapPrefabs.Length > 0)
        {
            if (Random.Range(0f, 100f) <= physicalTrapChance)
            {
                int randomTrapIndex = Random.Range(0, physicalTrapPrefabs.Length);
                GameObject newTrapObj = Instantiate(physicalTrapPrefabs[randomTrapIndex], spawnPoint.position, Quaternion.identity);
                GroundTrap trapScript = newTrapObj.GetComponent<GroundTrap>();
                if (trapScript != null) trapScript.SetupTrap(kommy);
            }
        }

        bool droppingRotten = canDropRottenFood && (Random.Range(0f, 100f) <= rottenChance);
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
        if (missedFood != null && !missedFood.isRotten)
        {
            FoodEntry originalEntry = allFoods.Find(x => x.word == originalWord);
            if (originalEntry != null) thiefPocket.Add(originalEntry);
        }
        activeFoods.Remove(missedFood);
        if (targetedFood == missedFood) targetedFood = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) && targetedFood != null)
        {
            if (targetedFood.originalWord.Length - targetedFood.currentWord.Length == 1)
            {
                targetedFood.CancelLockOn();
                targetedFood = null;
                return; 
            }
        }

        string input = Input.inputString.ToLower();
        foreach (char c in input) { if (c >= 'a' && c <= 'z') TypeLetter(c); }
    }

    void TypeLetter(char letter)
    {
        if (targetedFood == null)
        {
            foreach (FoodItem food in activeFoods)
            {
                if (food != null && !food.isFading && food.transform.position.x > deadZoneX)
                {
                    if (food.currentWord.StartsWith(letter.ToString()))
                    {
                        targetedFood = food;
                        targetedFood.UpdateVisuals();
                        break; 
                    }
                }
            }
        }

        if (targetedFood != null)
        {
            if (targetedFood.transform.position.x <= deadZoneX)
            {
                targetedFood.CancelLockOn();
                targetedFood = null;
                return;
            }

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
                    targetedFood.VanishOnSuccess(); // TRIGGER VANISH
                    targetedFood = null; 
                }
            }
            else
            {
                kommy.TypeWrongLetter(); 
                targetedFood.TriggerMistake();
            }
        }
    }

    private IEnumerator TriggerDizzyEffect()
    {
        isPlayerDizzy = true; 
        if (dizzyAnimator != null) {
            dizzyAnimator.gameObject.SetActive(true);
            yield return StartCoroutine(dizzyAnimator.PlayPoisonIntro()); 
        }
        yield return new WaitForSeconds(dizzyDuration);
        if (dizzyAnimator != null) {
            yield return StartCoroutine(dizzyAnimator.PlayPoisonOutro()); 
            dizzyAnimator.gameObject.SetActive(false);
        }
        isPlayerDizzy = false; 
    }
}