using UnityEngine;
using System.Collections;

public class KommyController : MonoBehaviour
{
    private Animator animator;
    
    [Header("Game Settings")]
    public float runSpeed = 2f;
    public int maxHp = 5;
    public int currentHp;

    public enum CharacterState { Running, Attacking, Stunned, Dead, Victory }
    
    // Default to Running instead of Idle!
    public CharacterState currentState = CharacterState.Running;

    // --- NEW TYPING TIMERS ---
    private float attackTimer = 0f;
    private float timeToStopAttacking = 0.3f; // How long she waits after your last keystroke to start running again

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHp = maxHp;
        
        // Auto-start running the millisecond the game loads
        StartGame();
    }

    void Update()
    {
        // If she is currently attacking, count down the timer
        if (currentState == CharacterState.Attacking)
        {
            attackTimer -= Time.deltaTime;
            
            // If you stopped typing and the timer hits 0, go back to running!
            if (attackTimer <= 0f)
            {
                currentState = CharacterState.Running;
                PlayAnimation("KommyMove");
            }
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
        
        // Reset the countdown timer every single time you hit a correct letter!
        attackTimer = timeToStopAttacking;

        // ONLY tell the Animator to play the attack if she isn't already attacking.
        // This stops the animation from glitching out and restarting on every keystroke!
        if (currentState != CharacterState.Attacking)
        {
            currentState = CharacterState.Attacking;
            PlayAnimation("KommyAttack"); 
        }
    }

    public void TypeWrongLetter()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Victory || currentState == CharacterState.Stunned) return;
        
        StopAllCoroutines();
        StartCoroutine(StunRoutine());
    }

    public void WinGame()
    {
        if (currentState == CharacterState.Dead) return;
        currentState = CharacterState.Victory;
        PlayAnimation("KommyVictory"); 
    }

    private IEnumerator StunRoutine()
    {
        currentState = CharacterState.Stunned;
        currentHp--; 
        PlayAnimation("KommyStun"); 
        
        yield return new WaitForSeconds(1.0f);

        if (currentHp <= 0)
        {
            Die();
        }
        else if (currentState != CharacterState.Dead)
        {
            currentState = CharacterState.Running;
            PlayAnimation("KommyMove");
        }
    }

    private void Die()
    {
        currentState = CharacterState.Dead;
        PlayAnimation("KommyDie");
        Debug.Log("GAME OVER");
    }

    private void PlayAnimation(string animName)
    {
        if (animator != null) animator.Play(animName);
    }
}