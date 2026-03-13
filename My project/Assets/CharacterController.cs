using UnityEngine;
using System.Collections;

public class KommyController : MonoBehaviour
{
    public Animator anim; // FIXED: Now public so other scripts can talk to it!
    
    [Header("Game Settings")]
    public float runSpeed = 2f;
    public int maxHp = 5;
    public int currentHp;

    public enum CharacterState { Running, Attacking, Jumping, Stunned, Dead, Victory }
    public CharacterState currentState = CharacterState.Running;

    [Header("Jump Settings")]
    public float jumpHeight = 2.5f;   
    public float jumpDuration = 0.6f; 
    public float jumpCooldown = 0.8f; 
    private float nextJumpTime = 0f;
    private float originalY;          

    private float attackTimer = 0f;
    private float timeToStopAttacking = 1.0f; 

    void Start()
    {
        anim = GetComponent<Animator>(); // FIXED
        currentHp = maxHp;
        originalY = transform.position.y; 
        StartGame();
    }

    void Update()
    {
        if (currentState == CharacterState.Attacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                currentState = CharacterState.Running;
                PlayAnimation("KommyMove");
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }
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

    public void TypeCorrectLetter()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Victory || currentState == CharacterState.Stunned) return;
    }

    public void TriggerSwipeAnimation()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Stunned) return;
        
        attackTimer = timeToStopAttacking;
        currentState = CharacterState.Attacking;
        PlayAnimation("KommyAttack"); 
    }

    public void TypeWrongLetter()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Victory || currentState == CharacterState.Stunned) return;
        TriggerStun();
    }

    public void HitByTrap()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Victory || currentState == CharacterState.Stunned) return;
        TriggerStun();
    }

    // --- NEW: THE SAFE BONK (No Stun, No HP Loss) ---
    public void TriggerBonk()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Victory || currentState == CharacterState.Stunned) return;
        
        PlayAnimation("KommyBonk"); 
        StartCoroutine(ResetRunAfterBonk());
    }

    private IEnumerator ResetRunAfterBonk()
    {
        yield return new WaitForSeconds(1.0f); // Bonk lasts 1 second
        if (currentState != CharacterState.Dead && currentState != CharacterState.Stunned)
        {
            PlayAnimation("KommyMove");
        }
    }

    private void TriggerStun()
    {
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
        if(currentState != CharacterState.Stunned && currentState != CharacterState.Dead)
        {
            currentState = CharacterState.Running;
            PlayAnimation("KommyMove");
        }
    }

    private IEnumerator StunRoutine()
    {
        currentState = CharacterState.Stunned;
        currentHp--; 
        PlayAnimation("KommyStun"); 
        yield return new WaitForSeconds(1.0f);

        if (currentHp <= 0) Die();
        else if (currentState != CharacterState.Dead)
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

    private void PlayAnimation(string animName)
    {
        if (anim != null) anim.Play(animName); // FIXED
    }
}