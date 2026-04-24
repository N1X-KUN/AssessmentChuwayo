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

    [HideInInspector] public bool isPlayerDizzy = false; 
    [HideInInspector] public bool onlySpawnTraps = false; 
    
    [HideInInspector] public bool isControlledTutorialActive = false; 
    
    [HideInInspector] public bool hasFinishedScriptedTutorial = false; 

    private List<FoodItem> activeFoods = new List<FoodItem>(); 
    private FoodItem targetedFood = null; 

    void Start()
    {
        foreach (var food in allFoods) { food.word = food.word.ToLower(); }
        if (dizzyAnimator != null) dizzyAnimator.gameObject.SetActive(false);
        UpdateAmmoUI();
        UpdateScoreUI();
    }

    public void StartSpawning() 
    { 
        DialogueManager dm = FindAnyObjectByType<DialogueManager>();
        
        if (dm != null && dm.isTutorialMode && !hasFinishedScriptedTutorial)
        {
            if (!isControlledTutorialActive)
            {
                StartCoroutine(MasterTutorialSequence(dm));
            }
        }
        else
        {
            if (!IsInvoking(nameof(SpawnFood))) 
            {
                InvokeRepeating(nameof(SpawnFood), 0.5f, spawnRate); 
            }
        }
    }
    
    public void StopSpawning() 
    { 
        if (isControlledTutorialActive) return; 

        CancelInvoke(nameof(SpawnFood)); 
    }

    private IEnumerator MasterTutorialSequence(DialogueManager dm)
    {
        isControlledTutorialActive = true; 
        ThiefController thief = FindAnyObjectByType<ThiefController>();

        yield return new WaitUntil(() => !dm.dialogueIsActive);
        yield return new WaitForSeconds(1.0f);

        // 1. POISON TRAP
        SpawnSpecificTrap(0); 
        yield return new WaitForSeconds(0.5f); 
        dm.PlayDialogue("PoisonTutorial");
        yield return new WaitUntil(() => !dm.dialogueIsActive);
        yield return new WaitUntil(() => FindAnyObjectByType<GroundTrap>() == null);

        // 2. PUNCH TRAP 
        yield return new WaitForSeconds(1.0f);
        SpawnSpecificTrap(1); 
        yield return new WaitForSeconds(0.5f); 
        dm.PlayDialogue("PunchTutorial");
        yield return new WaitUntil(() => !dm.dialogueIsActive);
        yield return new WaitUntil(() => FindAnyObjectByType<GroundTrap>() == null);

        // 3. FOOD
        yield return new WaitForSeconds(1.0f);
        SpawnSpecificFood();
        yield return new WaitForSeconds(0.8f);
        dm.PlayDialogue("FoodTutorial");
        yield return new WaitUntil(() => !dm.dialogueIsActive);

        // 4. WAIT FOR AMMO
        while (ammoBackpack.Count == 0)
        {
            if (activeFoods.Count == 0 && FindAnyObjectByType<FoodItem>() == null) SpawnSpecificFood();
            yield return null;
        }
        yield return new WaitForSeconds(0.6f);
        dm.PlayDialogue("AmmoTutorial");
        yield return new WaitUntil(() => !dm.dialogueIsActive);

        // 5. WAIT FOR THIEF KNOCKDOWN
        yield return new WaitUntil(() => thief != null && thief.isTumbling);
        yield return new WaitForSeconds(0.5f);
        dm.PlayDialogue("ThiefKnockedTutorial");
        yield return new WaitUntil(() => !dm.dialogueIsActive);

        // 6. DELAY 1.5 SECONDS -> ABILITY READY
        yield return new WaitForSeconds(1.5f); 
        kommy.currentCharge = kommy.maxCharge;
        kommy.UpdateUI();
        dm.PlayDialogue("AbilityReady");
        yield return new WaitUntil(() => !dm.dialogueIsActive);

        // 7. WAIT FOR ABILITY TO ACTIVATE THEN DEACTIVATE
        yield return new WaitUntil(() => kommy.isAbilityActive);
        yield return new WaitUntil(() => !kommy.isAbilityActive);
        
        // 8. ABILITY DONE
        dm.PlayDialogue("AbilityDone");
        yield return new WaitUntil(() => !dm.dialogueIsActive);

        // 9. ENTER "TUTORIAL NORMAL" PHASE
        isControlledTutorialActive = false; 
        hasFinishedScriptedTutorial = true; 
        
        InvokeRepeating(nameof(SpawnFood), 0.5f, spawnRate);
    }

    private Vector3 GetDropPosition()
    {
        ThiefController thief = FindAnyObjectByType<ThiefController>();
        if (thief != null)
        {
            return thief.transform.position + new Vector3(0, -0.5f, 0); 
        }
        return spawnPoint.position; 
    }

    private void SpawnSpecificTrap(int index)
    {
        if (physicalTrapPrefabs.Length > index)
        {
            GameObject newTrapObj = Instantiate(physicalTrapPrefabs[index], GetDropPosition(), Quaternion.identity);
            GroundTrap trapScript = newTrapObj.GetComponent<GroundTrap>();
            if (trapScript != null) trapScript.SetupTrap(kommy);
        }
    }

    private void SpawnSpecificFood()
    {
        if (allFoods.Count > 0)
        {
            FoodEntry chosenEntry = allFoods[0];
            GameObject newFoodObj = Instantiate(foodPrefab, GetDropPosition(), Quaternion.identity);
            FoodItem newFood = newFoodObj.GetComponent<FoodItem>();
            newFood.SetupFood(chosenEntry.word, chosenEntry.foodSprite, this, false);
            activeFoods.Add(newFood);
        }
    }

    void SpawnFood()
    {
        if (isControlledTutorialActive) return; 

        if (canDropPhysicalTraps && physicalTrapPrefabs.Length > 0 && trapCooldownTimer <= 0f)
        {
            if (Random.Range(0f, 100f) <= physicalTrapChance)
            {
                int randomTrapIndex = Random.Range(0, physicalTrapPrefabs.Length);
                GameObject newTrapObj = Instantiate(physicalTrapPrefabs[randomTrapIndex], GetDropPosition(), Quaternion.identity);
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

            GameObject newFoodObj = Instantiate(foodPrefab, GetDropPosition(), Quaternion.identity);
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

        DialogueManager dm = FindAnyObjectByType<DialogueManager>();
        if (dm != null && dm.dialogueIsActive) return;

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

                    // --- NEW: AUDIO TRIGGER (Score Up Sound) ---
                    if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.scoreUp);

                    UpdateScoreUI();
                    activeFoods.Remove(targetedFood);
                    
                    if (ammoBackpack.Count < maxAmmo)
                    {
                        ammoBackpack.Add(targetedFood.GetComponent<SpriteRenderer>().sprite);
                        targetedFood.VanishOnSuccess(); 
                        UpdateAmmoUI(); 
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

        // --- NEW: AUDIO TRIGGER (Poison Overlay Sound) ---
        if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.poisonOverlay);

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
}