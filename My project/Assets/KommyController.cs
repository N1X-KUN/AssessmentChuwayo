using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class KommyController : MonoBehaviour
{
    public Animator anim; 
    
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
    public float drainRate = 20f; 
    public float slowMotionSpeed = 0.4f; 
    public bool isAbilityActive = false;

    [Header("UI Visuals")]
    public Slider hpSlider; 
    public Slider abilitySlider; 
    public Animator uiFaceAnimator;
    public Image abilityFillImage; 
    public RectTransform abilityBarRect; 
    public Color chargingColor = Color.cyan;
    public Color fullColor = Color.yellow;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.15f;

    // --- THESE WERE DELETED IN THE CRASH! ---
    private float attackTimer = 0f;
    private float timeToStopAttacking = 1.0f; 

    void Start()
    {
        anim = GetComponent<Animator>(); 
        currentHp = maxHp;
        currentCharge = 0f;
        originalY = transform.position.y; 
        
        if (hpSlider != null) { hpSlider.maxValue = maxHp; hpSlider.value = currentHp; }
        if (abilitySlider != null) { abilitySlider.maxValue = maxCharge; abilitySlider.value = currentCharge; }
        
        UpdateUI();
    }

    void Update()
    {   
        LevelManager lm = FindAnyObjectByType<LevelManager>();
        if (lm != null && !lm.gameIsActive) return; 

        if (!isAbilityActive && currentState != CharacterState.Dead)
        {
            if (currentCharge < maxCharge)
            {
                currentCharge += passiveChargeRate * Time.deltaTime;
                if (abilityFillImage != null) abilityFillImage.color = chargingColor;
                if (abilityBarRect != null) abilityBarRect.localScale = Vector3.one; 
            }
            else
            {
                currentCharge = maxCharge;
                if (abilityFillImage != null) abilityFillImage.color = fullColor; 
                
                if (abilityBarRect != null)
                {
                    float scale = 1f + Mathf.PingPong(Time.time * pulseSpeed, pulseAmount);
                    abilityBarRect.localScale = new Vector3(scale, scale, 1f);
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
            currentCharge -= drainRate * Time.unscaledDeltaTime; 
            UpdateUI();

            if (currentCharge <= 0f) StopAbility();
        }

        if (currentState == CharacterState.Attacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                currentState = CharacterState.Running;
                PlayAnimation("KommyMove");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Space) && !isAbilityActive) TryJump();
    }

    private void StartAbility()
    {
        isAbilityActive = true;
        currentState = CharacterState.Ability;
        if (abilityBarRect != null) abilityBarRect.localScale = Vector3.one; 
        
        Time.timeScale = slowMotionSpeed; 
        Time.fixedDeltaTime = 0.02f * Time.timeScale; 
        PlayAnimation("KommyAbility"); 
    }

    private void StopAbility()
    {
        isAbilityActive = false;
        currentCharge = 0f;
        Time.timeScale = 1f; 
        Time.fixedDeltaTime = 0.02f;
        currentState = CharacterState.Running;
        PlayAnimation("KommyMove");
        UpdateUI();
    }

    public void AddAttackBonusCharge()
    {
        if (!isAbilityActive)
        {
            currentCharge += 10f; 
            if (currentCharge > maxCharge) currentCharge = maxCharge;
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
        PlayAnimation("KommyMove");
    }

    public void TriggerSwipeAnimation()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Stunned || isAbilityActive) return;
        attackTimer = timeToStopAttacking; 
        currentState = CharacterState.Attacking;
        PlayAnimation("KommyAttack"); 
    }

    public void HitByTrap()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Victory || currentState == CharacterState.Stunned || isAbilityActive) return;
        TriggerStun();
    }

    public void TriggerBonk()
    {
        if (currentState == CharacterState.Stunned || currentState == CharacterState.Dead || currentState == CharacterState.Victory) return;

        ThiefController thief = FindAnyObjectByType<ThiefController>();
        if (thief != null) 
        {
            if (currentState == CharacterState.Running || currentState == CharacterState.Attacking)
            {
                thief.StepForward(false); 
            }
        }

        PlayAnimation("KommyBonk"); 
        StartCoroutine(ResetRunAfterBonk());
    }

    private IEnumerator ResetRunAfterBonk()
    {
        yield return new WaitForSeconds(1.0f); 
        if (currentState != CharacterState.Dead && currentState != CharacterState.Stunned && !isAbilityActive)
            PlayAnimation("KommyMove");
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
        if (currentState == CharacterState.Dead) return;
        currentState = CharacterState.Victory;
        PlayAnimation("KommyVictory"); 
    }

    private IEnumerator JumpRoutine()
    {
        currentState = CharacterState.Jumping;
        PlayAnimation("KommyJump"); 

        float halfTime = jumpDuration / 2f;
        float elapsed = 0f;

        while (elapsed < halfTime)
        {
            float newY = Mathf.Lerp(originalY, originalY + jumpHeight, elapsed / halfTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < halfTime)
        {
            float newY = Mathf.Lerp(originalY + jumpHeight, originalY, elapsed / halfTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = new Vector3(transform.position.x, originalY, transform.position.z);
        if(currentState != CharacterState.Stunned && currentState != CharacterState.Dead && !isAbilityActive)
        {
            currentState = CharacterState.Running;
            PlayAnimation("KommyMove");
        }
    }

    private IEnumerator StunRoutine()
    {
        currentState = CharacterState.Stunned;
        currentHp--; 
        UpdateUI(); 

        PlayAnimation("KommyStun"); 
        yield return new WaitForSeconds(1.0f);

        if (currentHp <= 0) Die();
        else if (currentState != CharacterState.Dead && !isAbilityActive)
        {
            currentState = CharacterState.Running;
            PlayAnimation("KommyMove");
        }
    }

    public void Die()
    {
        if (currentState == CharacterState.Dead) return;
        currentState = CharacterState.Dead;
        PlayAnimation("KommyDie");
    }

    public void TriggerHappyFace()
    {
        StartCoroutine(HappyFaceRoutine());
    }

    private IEnumerator HappyFaceRoutine()
    {
        if (uiFaceAnimator != null) uiFaceAnimator.Play("KommyFace_Happy");
        yield return new WaitForSeconds(2.0f); 
        
        if (currentState == CharacterState.Running || currentState == CharacterState.Attacking)
        {
            if (uiFaceAnimator != null) uiFaceAnimator.Play("KommyFace_Idle");
        }
    }

    private void PlayAnimation(string animName)
    {
        if (anim != null) anim.Play(animName); 

        if (uiFaceAnimator != null)
        {
            if (animName == "KommyStun" || animName == "KommyBonk") 
                uiFaceAnimator.Play("KommyFace_Sad");
            else if (animName == "KommyDie") 
                uiFaceAnimator.Play("KommyFace_Defeat"); 
            else if (animName == "KommyVictory") 
                uiFaceAnimator.Play("KommyFace_Victory");
            else if (animName == "KommyAbility") 
                uiFaceAnimator.Play("KommyFace_Power");
            else 
                uiFaceAnimator.Play("KommyFace_Idle"); 
        }
    }
}