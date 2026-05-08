using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class KommyController : MonoBehaviour
{
    public Animator anim; 
    
    [Header("Character Identity")]
    [Tooltip("Type 'Kommy' for Kommy, or 'Tig' for Tig!")]
    public string animPrefix = "Kommy"; 

    [Header("Game Settings")]
    public float runSpeed = 2f;
    public int maxHp = 5;
    public int currentHp;

    public enum CharacterState { Running, Attacking, Jumping, Stunned, Dead, Victory, Ability }
    public CharacterState currentState = CharacterState.Running; 

    [Header("Jump Settings")]
    public float jumpHeight = 2.5f;   
    public float jumpDuration = 0.6f; 
    public float jumpCooldown = 0.8f; 
    private float nextJumpTime = 0f;
    private float originalY;          

    [Header("Ultimate Ability Settings")]
    public float maxCharge = 100f;
    public float currentCharge = 0f;
    public float passiveChargeRate = 2f; 
    public bool isAbilityActive = false;
    
    [Header("Kommy Specific (Time Stop)")]
    public float drainRate = 20f; 
    public float slowMotionSpeed = 0.4f; 
    public float endSoundPrecastTime = 1.5f; 
    private bool hasPlayedEndSound = false;
    public GameObject zawarudoIcon;     
    public GameObject zawarudoOverlay;  

    [Header("Tig Specific (Moon Slash)")]
    public GameObject tigProjectilePrefab;
    public GameObject tigOverlay; // The screen effect that stays for 2 seconds
    public float tigAbilityDuration = 2f; // Tig's ability only lasts 2 seconds

    [Header("UI Visuals")]
    public Slider hpSlider; 
    public Slider abilitySlider; 
    public Animator uiFaceAnimator;
    public Animator progressionFaceAnimator; 
    public Image abilityFillImage; 
    public RectTransform abilityBarRect; 
    public Color chargingColor = Color.cyan;
    public Color fullColor = Color.yellow;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.04f; 

    private float attackTimer = 0f;
    private float timeToStopAttacking = 1.0f; 

    private LevelManager lm;
    private DialogueManager dm;
    private WordManager wm;

    void Start()
    {
        anim = GetComponent<Animator>(); 
        currentHp = maxHp;
        currentCharge = 0f;
        originalY = transform.position.y; 

        lm = FindAnyObjectByType<LevelManager>();
        dm = FindAnyObjectByType<DialogueManager>();
        wm = FindAnyObjectByType<WordManager>();
        
        AutoLinkUI();

        if (hpSlider != null) { hpSlider.maxValue = maxHp; hpSlider.value = currentHp; }
        if (abilitySlider != null) { abilitySlider.maxValue = maxCharge; abilitySlider.value = currentCharge; }
        
        if (zawarudoIcon != null) zawarudoIcon.SetActive(false);
        if (zawarudoOverlay != null) zawarudoOverlay.SetActive(false);
        if (tigOverlay != null) tigOverlay.SetActive(false);

        UpdateUI();
    }

    private void AutoLinkUI()
    {
        GameObject activeHeroUI = GameObject.Find("HeroUI_" + animPrefix);

        if (activeHeroUI != null)
        {
            abilityBarRect = activeHeroUI.GetComponent<RectTransform>();

            Slider[] sliders = activeHeroUI.GetComponentsInChildren<Slider>(true);
            foreach (Slider s in sliders)
            {
                if (s.name.ToUpper().Contains("HP")) hpSlider = s;
                if (s.name.ToUpper().Contains("SKILL")) abilitySlider = s;
            }

            Animator[] animators = activeHeroUI.GetComponentsInChildren<Animator>(true);
            foreach (Animator a in animators)
            {
                if (a.name.Contains("Face") || a.name.Contains("Avatar")) uiFaceAnimator = a;
            }

            Image[] images = activeHeroUI.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (img.name.ToUpper().Contains("FILL")) abilityFillImage = img;
            }

            Canvas myCanvas = activeHeroUI.GetComponentInParent<Canvas>();
            if (myCanvas != null)
            {
                Transform[] allUIElements = myCanvas.GetComponentsInChildren<Transform>(true); 
                foreach (Transform t in allUIElements)
                {
                    if (t.name == "TimeSlow") zawarudoIcon = t.gameObject;
                    if (t.name == "TimeSlowEffect") zawarudoOverlay = t.gameObject;
                    if (t.name == "TigSlashOverlay") tigOverlay = t.gameObject; // LINKS TIG'S OVERLAY
                }
            }
        }

        if (lm != null && lm.handleAnimator != null) progressionFaceAnimator = lm.handleAnimator;
    }

    void Update()
    {   
        if (currentState == CharacterState.Dead || currentState == CharacterState.Victory) return;

        if (lm != null && !lm.gameIsActive) return; 
        if (dm != null && dm.dialogueIsActive) return; 

        bool isTutorialStalling = false;
        if (wm != null && wm.isControlledTutorialActive) isTutorialStalling = true;

        if (!isAbilityActive)
        {
            if (!isTutorialStalling && currentCharge < maxCharge)
            {
                currentCharge += passiveChargeRate * Time.deltaTime;
                
                if (currentCharge >= maxCharge)
                {
                    if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.abilityReady);
                }

                if (abilityFillImage != null) abilityFillImage.color = chargingColor;
                if (abilityBarRect != null) abilityBarRect.localScale = Vector3.one; 
            }
            else if (currentCharge >= maxCharge) 
            {
                currentCharge = maxCharge;
                if (abilityFillImage != null) abilityFillImage.color = fullColor; 
                
                if (abilityBarRect != null)
                {
                    float angle = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount; 
                    abilityBarRect.localRotation = Quaternion.Euler(0, 0, angle);
                }
            }
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && currentCharge >= maxCharge && !isAbilityActive && currentState == CharacterState.Running)
        {
            StartAbility();
        }

        if (isAbilityActive)
        {
            // DYNAMIC DRAIN: Tig drains in exactly 2 seconds. Kommy uses her normal drain rate.
            float currentDrainRate = (animPrefix == "Tig") ? (maxCharge / tigAbilityDuration) : drainRate;
            currentCharge -= currentDrainRate * Time.unscaledDeltaTime; 
            UpdateUI();

            if (animPrefix == "Kommy" && currentCharge <= (drainRate * endSoundPrecastTime) && !hasPlayedEndSound)
            {
                if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.timeSoundEnd);
                hasPlayedEndSound = true;
            }

            if (currentCharge <= 0f) StopAbility();
        }

        if (currentState == CharacterState.Attacking)
        {
            attackTimer -= Time.unscaledDeltaTime;
            if (attackTimer <= 0f)
            {
                currentState = CharacterState.Running;
                PlayAnimation(animPrefix + "Move");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Space) && !isAbilityActive) TryJump();
    }

    private void StartAbility()
    {
        isAbilityActive = true;
        hasPlayedEndSound = false; 
        currentState = CharacterState.Ability;
        
        if (abilityBarRect != null) 
        {
            abilityBarRect.localScale = Vector3.one; 
            abilityBarRect.localRotation = Quaternion.identity; 
        }
        
        if (wm != null)
        {
            wm.isPlayerDizzy = false;
            if (wm.dizzyAnimator != null) wm.dizzyAnimator.gameObject.SetActive(false);
        }

        // --- THE FORK IN THE ROAD ---
        if (animPrefix == "Kommy")
        {
            if (zawarudoIcon != null) zawarudoIcon.SetActive(true); 
            if (zawarudoOverlay != null) zawarudoOverlay.SetActive(true); 

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(AudioManager.instance.timeSoundStart);
                AudioManager.instance.SetUnderwaterMusic(true);
            }

            Time.timeScale = slowMotionSpeed; 
            Time.fixedDeltaTime = 0.02f * Time.timeScale; 
        }
        else if (animPrefix == "Tig")
        {
            if (tigOverlay != null) tigOverlay.SetActive(true); 
            
            // Spawn the Crescent Moon Projectile slightly in front of Tig
            if (tigProjectilePrefab != null)
            {
                Instantiate(tigProjectilePrefab, transform.position + new Vector3(1.5f, 0, 0), Quaternion.identity);
            }

            // ADD TIG'S ABILITY SOUND HERE IN THE FUTURE!
        }

        PlayAnimation(animPrefix + "Ability"); 
    }

    private void StopAbility()
    {
        isAbilityActive = false;
        currentCharge = 0f;
        
        // --- THE FORK IN THE ROAD ---
        if (animPrefix == "Kommy")
        {
            Time.timeScale = 1f; 
            Time.fixedDeltaTime = 0.02f;
            
            if (zawarudoIcon != null) zawarudoIcon.SetActive(false); 
            if (zawarudoOverlay != null) zawarudoOverlay.SetActive(false); 

            if (AudioManager.instance != null)
            {
                if (!hasPlayedEndSound) AudioManager.instance.PlaySFX(AudioManager.instance.timeSoundEnd);
                AudioManager.instance.SetUnderwaterMusic(false);
            }
        }
        else if (animPrefix == "Tig")
        {
            if (tigOverlay != null) tigOverlay.SetActive(false); // Hides overlay after 2 seconds
        }

        currentState = CharacterState.Running;
        PlayAnimation(animPrefix + "Move");
        UpdateUI();
    }

    private void ForceClearAllEffects()
    {
        isAbilityActive = false;
        if (zawarudoIcon != null) zawarudoIcon.SetActive(false); 
        if (zawarudoOverlay != null) zawarudoOverlay.SetActive(false); 
        if (tigOverlay != null) tigOverlay.SetActive(false); 
        
        if (wm != null)
        {
            wm.isPlayerDizzy = false;
            if (wm.dizzyAnimator != null) wm.dizzyAnimator.gameObject.SetActive(false);
        }

        if (AudioManager.instance != null) AudioManager.instance.SetUnderwaterMusic(false);
    }

    public void AddAttackBonusCharge()
    {
        if (!isAbilityActive && currentState != CharacterState.Dead && currentState != CharacterState.Victory)
        {
            float oldCharge = currentCharge; 
            currentCharge += 10f; 
            
            if (currentCharge >= maxCharge) 
            {
                currentCharge = maxCharge;
                if (oldCharge < maxCharge && AudioManager.instance != null) 
                {
                    AudioManager.instance.PlayUI(AudioManager.instance.abilityReady);
                }
            }
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (abilitySlider != null) abilitySlider.value = currentCharge; 
        if (hpSlider != null) hpSlider.value = currentHp;
    }

    public void TryJump()
    {
        if (Time.time >= nextJumpTime && (currentState == CharacterState.Running || currentState == CharacterState.Attacking))
        {
            StartCoroutine(JumpRoutine());
            nextJumpTime = Time.time + jumpCooldown;
        }
    }

    public void StartGame()
    {
        currentState = CharacterState.Running;
        PlayAnimation(animPrefix + "Move");
    }

    public void TriggerSwipeAnimation()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Victory || currentState == CharacterState.Stunned || isAbilityActive) return;
        
        attackTimer = timeToStopAttacking; 
        currentState = CharacterState.Attacking;
        PlayAnimation(animPrefix + "Attack"); 
    }

    public void HitByTrap()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Victory || currentState == CharacterState.Stunned || isAbilityActive) return;
        TriggerStun();
    }

    public void TriggerBonk()
    {
        if (currentState == CharacterState.Stunned || currentState == CharacterState.Dead || currentState == CharacterState.Victory || isAbilityActive) return;

        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.bonkSound);

        ThiefController thief = FindAnyObjectByType<ThiefController>();
        if (thief != null) 
        {
            if (currentState == CharacterState.Running || currentState == CharacterState.Attacking)
            {
                thief.StepForward(false); 
            }
        }

        PlayAnimation(animPrefix + "Bonk"); 
        StartCoroutine(ResetRunAfterBonk());
    }

    private IEnumerator ResetRunAfterBonk()
    {
        yield return new WaitForSeconds(1.0f); 
        if (currentState != CharacterState.Dead && currentState != CharacterState.Victory && currentState != CharacterState.Stunned && !isAbilityActive)
            PlayAnimation(animPrefix + "Move");
    }

    private void TriggerStun()
    {   
        ThiefController thief = FindAnyObjectByType<ThiefController>();
        if (thief != null) 
        {
            thief.ShowEmoticon("EmoticonLaugh", 2.05f);
            thief.StepForward(false);
        }

        StopAllCoroutines();
        transform.position = new Vector3(transform.position.x, originalY, transform.position.z); 
        StartCoroutine(StunRoutine());
    }

    public void WinGame()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Victory) return;
        currentState = CharacterState.Victory;
        ForceClearAllEffects(); 
        PlayAnimation(animPrefix + "Victory"); 
        
        if (dm != null) dm.PlayDialogue("TutorialWin");
        
        StartCoroutine(FreezeWorldRoutine());
    }

    public void Die()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Victory) return;
        currentState = CharacterState.Dead;
        ForceClearAllEffects(); 
        PlayAnimation(animPrefix + "Die");
        
        if (dm != null) dm.PlayDialogue("TutorialLose");
        
        StartCoroutine(FreezeWorldRoutine());
    }

    private IEnumerator FreezeWorldRoutine()
    {
        if (lm != null) lm.gameIsActive = false; 

        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 0f;
    }

    private IEnumerator JumpRoutine()
    {
        currentState = CharacterState.Jumping;
        PlayAnimation(animPrefix + "Jump"); 

        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.kommyJump);

        float halfTime = jumpDuration / 2f;
        float elapsed = 0f;

        while (elapsed < halfTime)
        {
            float newY = Mathf.Lerp(originalY, originalY + jumpHeight, elapsed / halfTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            elapsed += Time.unscaledDeltaTime; 
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < halfTime)
        {
            float newY = Mathf.Lerp(originalY + jumpHeight, originalY, elapsed / halfTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        transform.position = new Vector3(transform.position.x, originalY, transform.position.z);
        
        if(currentState != CharacterState.Stunned && currentState != CharacterState.Dead && currentState != CharacterState.Victory && !isAbilityActive)
        {
            currentState = CharacterState.Running;
            PlayAnimation(animPrefix + "Move");
        }
    }

    private IEnumerator StunRoutine()
    {
        currentState = CharacterState.Stunned;
        currentHp--; 
        UpdateUI(); 

        PlayAnimation(animPrefix + "Stun"); 
        yield return new WaitForSeconds(1.0f);

        if (currentHp <= 0) Die();
        else if (currentState != CharacterState.Dead && currentState != CharacterState.Victory && !isAbilityActive)
        {
            currentState = CharacterState.Running;
            PlayAnimation(animPrefix + "Move");
        }
    }

    public void TriggerHappyFace()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.kommyKnockdownVoice);
        StartCoroutine(HappyFaceRoutine());
    }

    private IEnumerator HappyFaceRoutine()
    {
        if (uiFaceAnimator != null && uiFaceAnimator.isActiveAndEnabled) uiFaceAnimator.Play(animPrefix + "Face_Happy");
        
        yield return new WaitForSeconds(2.0f); 
        
        if (currentState == CharacterState.Running || currentState == CharacterState.Attacking)
        {
            if (uiFaceAnimator != null && uiFaceAnimator.isActiveAndEnabled) uiFaceAnimator.Play(animPrefix + "Face_Idle");
        }
    }

    private void PlayAnimation(string animName)
    {
        try 
        {
            if (anim != null && anim.isActiveAndEnabled) anim.Play(animName); 

            if (uiFaceAnimator != null && uiFaceAnimator.isActiveAndEnabled)
            {
                string faceName = animPrefix + "Face_Idle";
                if (animName == animPrefix + "Stun" || animName == animPrefix + "Bonk") faceName = animPrefix + "Face_Sad";
                else if (animName == animPrefix + "Die") faceName = animPrefix + "Face_Defeat"; 
                else if (animName == animPrefix + "Victory") faceName = animPrefix + "Face_Victory";
                else if (animName == animPrefix + "Ability") faceName = animPrefix + "Face_Power";

                uiFaceAnimator.Play(faceName); 
            }

            if (progressionFaceAnimator != null && progressionFaceAnimator.isActiveAndEnabled)
            {
                if (animName == animPrefix + "Victory") 
                    progressionFaceAnimator.Play("LoadingWIN");
                else if (animName == animPrefix + "Die") 
                    progressionFaceAnimator.Play("LoadingLOS");
                else 
                    progressionFaceAnimator.Play("LoadingRUN");
            }
        }
        catch (System.Exception e) { Debug.LogWarning("Safe Catch: " + e.Message); }
    }
}