using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI; 

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
    private float trapCooldownTimer = 0f; 

    [Header("Typing Mechanics & Ammo")]
    public TMP_Text ammoCounterText; 
    public int maxAmmo = 5; 
    public List<Sprite> ammoBackpack = new List<Sprite>(); 
    
    public GameObject emptyBagObj;
    public GameObject partialBagObj; 
    public GameObject halfBagObj;    
    public GameObject fullBagObj;    
    public Animator bagAnimator; 
    
    [Header("Score System")]
    public TMP_Text scoreText; 
    public int currentScore = 0;

    [Header("Tutorial Sequencing")]
    public int tutorialPhase = 0; // The strict phase tracker

    private List<FoodItem> activeFoods = new List<FoodItem>(); 
    private FoodItem targetedFood = null; 
    
    [HideInInspector] public bool isPlayerDizzy = false; 
    [HideInInspector] public bool onlySpawnTraps = false; 

    void Start()
    {
        foreach (var food in allFoods) { food.word = food.word.ToLower(); }
        if (dizzyAnimator != null) dizzyAnimator.gameObject.SetActive(false);
        UpdateAmmoUI();
        UpdateScoreUI();
    }

    public void StartSpawning() { InvokeRepeating(nameof(SpawnFood), 0.5f, spawnRate); }
    public void StopSpawning() { CancelInvoke(nameof(SpawnFood)); }

    void SpawnFood()
    {
        DialogueManager dm = FindAnyObjectByType<DialogueManager>();
        bool isTutorial = dm != null && dm.isTutorialMode;

        // --- NEW: THE SCRIPTED TUTORIAL SEQUENCE ---
        if (isTutorial && tutorialPhase < 7)
        {
            // PACING: Wait until the screen is completely clear of traps before spawning the next step!
            if (FindAnyObjectByType<GroundTrap>() != null) return; 

            if (tutorialPhase == 0 && canDropPhysicalTraps && physicalTrapPrefabs.Length > 0)
            {
                GameObject newTrapObj = Instantiate(physicalTrapPrefabs[0], spawnPoint.position, Quaternion.identity); // MUST BE POISON
                GroundTrap trapScript = newTrapObj.GetComponent<GroundTrap>();
                if (trapScript != null) trapScript.SetupTrap(kommy);
                StartCoroutine(DelayedTrapDialogue(true));
                tutorialPhase = 1;
                return;
            }
            else if (tutorialPhase == 1 && canDropPhysicalTraps && physicalTrapPrefabs.Length > 1)
            {
                GameObject newTrapObj = Instantiate(physicalTrapPrefabs[1], spawnPoint.position, Quaternion.identity); // MUST BE PUNCH
                GroundTrap trapScript = newTrapObj.GetComponent<GroundTrap>();
                if (trapScript != null) trapScript.SetupTrap(kommy);
                StartCoroutine(DelayedTrapDialogue(false));
                tutorialPhase = 2;
                return;
            }
            else if (tutorialPhase == 2 && !onlySpawnTraps)
            {
                FoodEntry chosenEntry = allFoods[0];
                GameObject newFoodObj = Instantiate(foodPrefab, spawnPoint.position, Quaternion.identity);
                FoodItem newFood = newFoodObj.GetComponent<FoodItem>();
                newFood.SetupFood(chosenEntry.word, chosenEntry.foodSprite, this, false);
                activeFoods.Add(newFood); 
                StartCoroutine(DelayedFoodDialogue());
                tutorialPhase = 3;
                return;
            }
            else if (tutorialPhase == 3 && !onlySpawnTraps && FindAnyObjectByType<FoodItem>() == null)
            {
                // If they missed the first food, drop another one so they don't get softlocked!
                FoodEntry chosenEntry = allFoods[0];
                GameObject newFoodObj = Instantiate(foodPrefab, spawnPoint.position, Quaternion.identity);
                FoodItem newFood = newFoodObj.GetComponent<FoodItem>();
                newFood.SetupFood(chosenEntry.word, chosenEntry.foodSprite, this, false);
                activeFoods.Add(newFood); 
                return;
            }
            return; // Block all random default logic until tutorial is over!
        }

        // --- DEFAULT NORMAL SPAWNING LOGIC (HAPPENS AFTER PHASE 7 OR IF TUTORIAL IS OFF) ---
        if (canDropPhysicalTraps && physicalTrapPrefabs.Length > 0 && trapCooldownTimer <= 0f)
        {
            if (Random.Range(0f, 100f) <= physicalTrapChance)
            {
                int randomTrapIndex = Random.Range(0, physicalTrapPrefabs.Length);
                GameObject newTrapObj = Instantiate(physicalTrapPrefabs[randomTrapIndex], spawnPoint.position, Quaternion.identity);
                GroundTrap trapScript = newTrapObj.GetComponent<GroundTrap>();
                if (trapScript != null) trapScript.SetupTrap(kommy);
                trapCooldownTimer = 2.0f; 
                return; 
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
        activeFoods.Remove(missedFood);
        if (targetedFood == missedFood) targetedFood = null;
    }

    void Update()
    {
        if (trapCooldownTimer > 0) trapCooldownTimer -= Time.deltaTime;

        LevelManager lm = FindAnyObjectByType<LevelManager>();
        if (lm != null && !lm.gameIsActive) return;

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
                kommy.AddAttackBonusCharge(); 
                
                GameObject proj = Instantiate(playerProjectilePrefab, kommy.transform.position + Vector3.up, Quaternion.identity);
                proj.GetComponent<PlayerProjectile>().Setup(ammoToThrow);

                // --- TUTORIAL: ADVANCE TO ABILITY ON THROW ---
                if (tutorialPhase == 4)
                {
                    tutorialPhase = 5;
                    StartCoroutine(MaxAbilityRoutine());
                }
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
                    if (kommy.currentState != KommyController.CharacterState.Stunned && !kommy.isAbilityActive)
                    {
                        kommy.HitByTrap(); 
                        StartCoroutine(TriggerDizzyEffect());
                    }
                    activeFoods.Remove(targetedFood);
                    Destroy(targetedFood.gameObject);
                    targetedFood = null;
                    return; 
                }

                targetedFood.RemoveFirstLetter();
                
                if (targetedFood.currentWord.Length == 0)
                {
                    int pointsEarned = targetedFood.originalWord.Length * 2;
                    currentScore += pointsEarned;
                    UpdateScoreUI();
                    activeFoods.Remove(targetedFood);
                    
                    if (ammoBackpack.Count < maxAmmo)
                    {
                        ammoBackpack.Add(targetedFood.GetComponent<SpriteRenderer>().sprite);
                        targetedFood.VanishOnSuccess(); 
                        UpdateAmmoUI(); 

                        DialogueManager dmAmmo = FindAnyObjectByType<DialogueManager>();
                        if (dmAmmo != null && dmAmmo.isTutorialMode && tutorialPhase == 3)
                        {
                            StartCoroutine(DelayedAmmoDialogue());
                            tutorialPhase = 4;
                        }
                    }
                    else 
                    {
                        if (kommy.currentState != KommyController.CharacterState.Stunned && !kommy.isAbilityActive)
                        {
                            if (bagAnimator != null) bagAnimator.Play("BagShake");
                            kommy.TriggerBonk(); 
                        }
                        Destroy(targetedFood.gameObject);
                    }
                    targetedFood = null;
                }
            }
            else 
            { 
                if (kommy.currentState != KommyController.CharacterState.Stunned && !kommy.isAbilityActive)
                {
                    targetedFood.TriggerMistake(); 
                    kommy.HitByTrap(); 
                }
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
    }

    public void TriggerPoisonFromTrap()
    {
        if (kommy.currentState == KommyController.CharacterState.Stunned || kommy.isAbilityActive) return;

        StartCoroutine(TriggerDizzyEffect());
        ThiefController thief = FindAnyObjectByType<ThiefController>();
        if (thief != null) thief.ShowEmoticon("EmoticonLaugh", 2.05f);
    }

    public void UpdateAmmoUI()
    {
        if (ammoCounterText != null) ammoCounterText.text = ammoBackpack.Count + "/" + maxAmmo;

        if (emptyBagObj != null) emptyBagObj.SetActive(false);
        if (partialBagObj != null) partialBagObj.SetActive(false);
        if (halfBagObj != null) halfBagObj.SetActive(false);
        if (fullBagObj != null) fullBagObj.SetActive(false);

        if (ammoBackpack.Count == 0 && emptyBagObj != null) emptyBagObj.SetActive(true);
        else if (ammoBackpack.Count <= 2 && partialBagObj != null) partialBagObj.SetActive(true);
        else if (ammoBackpack.Count <= 4 && halfBagObj != null) halfBagObj.SetActive(true);
        else if (fullBagObj != null) fullBagObj.SetActive(true);
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = currentScore + "\nPOINTS";
    }

    // --- TUTORIAL DELAYS ---
    private IEnumerator DelayedTrapDialogue(bool isPoison)
    {
        yield return new WaitForSeconds(1.2f); 
        DialogueManager dmTrap = FindAnyObjectByType<DialogueManager>();
        if (dmTrap != null && dmTrap.isTutorialMode)
        {
            if (isPoison) dmTrap.PlayDialogue("PoisonTutorial");
            else dmTrap.PlayDialogue("PunchTutorial");
        }
    }

    private IEnumerator DelayedFoodDialogue()
    {
        yield return new WaitForSeconds(0.8f); 
        DialogueManager dmFood = FindAnyObjectByType<DialogueManager>();
        if (dmFood != null && dmFood.isTutorialMode) dmFood.PlayDialogue("FoodTutorial");
    }

    private IEnumerator DelayedAmmoDialogue()
    {
        yield return new WaitForSeconds(0.6f); 
        DialogueManager dmAmmo = FindAnyObjectByType<DialogueManager>();
        if (dmAmmo != null && dmAmmo.isTutorialMode) dmAmmo.PlayDialogue("AmmoTutorial");
    }

    private IEnumerator MaxAbilityRoutine()
    {
        yield return new WaitForSeconds(1.0f); // Give the thrown item time to hit!
        kommy.currentCharge = kommy.maxCharge;
        kommy.UpdateUI();
        
        DialogueManager dm = FindAnyObjectByType<DialogueManager>();
        if (dm != null && dm.isTutorialMode) dm.PlayDialogue("AbilityReady");
    }
}