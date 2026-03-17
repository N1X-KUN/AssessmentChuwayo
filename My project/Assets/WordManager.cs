using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

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
    public GameObject playerProjectilePrefab; 
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

    [Header("Typing Mechanics & Ammo")]
    public TMP_Text ammoCounterText; 
    public int maxAmmo = 5; 
    public List<Sprite> ammoBackpack = new List<Sprite>(); 
    
    private List<FoodItem> activeFoods = new List<FoodItem>(); 
    private FoodItem targetedFood = null; 
    
    [HideInInspector] public bool isPlayerDizzy = false; 
    [HideInInspector] public bool onlySpawnTraps = false; 

    void Start()
    {
        foreach (var food in allFoods) { food.word = food.word.ToLower(); }
        if (dizzyAnimator != null) dizzyAnimator.gameObject.SetActive(false);
        UpdateAmmoUI();
    }

    public void StartSpawning() { InvokeRepeating(nameof(SpawnFood), 0.5f, spawnRate); }
    public void StopSpawning() { CancelInvoke(nameof(SpawnFood)); }

    void SpawnFood()
    {
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

        if (!onlySpawnTraps)
        {
            if (allFoods.Count == 0) return; 

            bool droppingRotten = canDropRottenFood && (Random.Range(0f, 100f) <= rottenChance);
            int randomIndex = Random.Range(0, allFoods.Count);
            FoodEntry chosenEntry = allFoods[randomIndex];

            GameObject newFoodObj = Instantiate(foodPrefab, spawnPoint.position, Quaternion.identity);
            FoodItem newFood = newFoodObj.GetComponent<FoodItem>();
            newFood.SetupFood(chosenEntry.word, chosenEntry.foodSprite, this, droppingRotten);
            activeFoods.Add(newFood); 
        }
    }

    public void MissedFood(FoodItem missedFood, string originalWord)
    {
        // Bonk and Escape are completely removed from here! 
        // FoodItem.cs handles it only on a direct head hit now.
        activeFoods.Remove(missedFood);
        if (targetedFood == missedFood) targetedFood = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            FoodItem foodToSwat = GetClosestFood();
            
            if (foodToSwat != null && foodToSwat.transform.position.x < kommy.transform.position.x + 3f)
            {
                activeFoods.Remove(foodToSwat);
                foodToSwat.SwatAway();
                kommy.TriggerSwipeAnimation(); 
                if (targetedFood == foodToSwat) targetedFood = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (ammoBackpack.Count > 0 && playerProjectilePrefab != null)
            {
                Sprite ammoToThrow = ammoBackpack[0];
                ammoBackpack.RemoveAt(0);
                UpdateAmmoUI(); 
                
                kommy.TriggerSwipeAnimation(); 
                
                GameObject proj = Instantiate(playerProjectilePrefab, kommy.transform.position + Vector3.up, Quaternion.identity);
                proj.GetComponent<PlayerProjectile>().Setup(ammoToThrow);
            }
        }

        string input = Input.inputString.ToLower();
        foreach (char c in input) 
        { 
            if (c >= 'a' && c <= 'z') TypeLetter(c); 
        }
    }

    void TypeLetter(char letter)
    {
        if (targetedFood == null)
        {
            FoodItem closestFood = GetClosestFood();
            if (closestFood != null && closestFood.currentWord.StartsWith(letter.ToString()))
            {
                targetedFood = closestFood;
                targetedFood.UpdateVisuals();
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

                targetedFood.RemoveFirstLetter();
                
                if (targetedFood.currentWord.Length == 0)
                {
                    activeFoods.Remove(targetedFood);
                    
                    if (ammoBackpack.Count < maxAmmo)
                    {
                        ammoBackpack.Add(targetedFood.GetComponent<SpriteRenderer>().sprite);
                        targetedFood.VanishOnSuccess(); 
                        UpdateAmmoUI(); 
                    }
                    else 
                    {
                        if (kommy != null) kommy.TriggerBonk(); 
                        ThiefController thief = FindAnyObjectByType<ThiefController>();
                        if (thief != null) thief.TriggerPoisonEscape();
                        Destroy(targetedFood.gameObject);
                    }
                    targetedFood = null;
                }
            }
            else 
            { 
                targetedFood.TriggerMistake(); 
                kommy.TypeWrongLetter(); 
            }
        }
    }

    private FoodItem GetClosestFood()
    {
        FoodItem closest = null;
        float minX = float.MaxValue;

        foreach (FoodItem food in activeFoods)
        {
            if (food != null && !food.isFading && food.transform.position.x < minX)
            {
                minX = food.transform.position.x;
                closest = food;
            }
        }
        return closest;
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

        foreach (FoodItem food in activeFoods)
        {
            if (food != null) food.UpdateVisuals();
        }
    }

    public void TriggerPoisonFromTrap()
    {
        StartCoroutine(TriggerDizzyEffect());
        ThiefController thief = FindAnyObjectByType<ThiefController>();
        if (thief != null) thief.TriggerPoisonEscape();
    }

    public void UpdateAmmoUI()
    {
        if (ammoCounterText != null)
        {
            ammoCounterText.text = ammoBackpack.Count + " / " + maxAmmo;
        }
    }
}