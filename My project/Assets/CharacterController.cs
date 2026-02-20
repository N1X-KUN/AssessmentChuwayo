using UnityEngine;
using System.Collections;

public class KommyController : MonoBehaviour
{
    private Animator animator;
    
    [Header("Game Settings")]
    public float runSpeed = 2f;
    public int maxHp = 5;
    private int currentHp;

    // This is the "Brain" of the auto-runner
    public enum CharacterState { Idle, Running, Attacking, Stunned, Dead, Victory }
    public CharacterState currentState = CharacterState.Idle;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHp = maxHp;
        
        // Updated to match your exact file name
        PlayAnimation("KommyIdle");
    }

    void Update()
    {
        if (currentState == CharacterState.Running)
        {
            // Moves the character to the left automatically
            transform.Translate(Vector3.left * runSpeed * Time.deltaTime);
        }

        // --- TEMPORARY TESTING CONTROLS ---
        if (Input.GetKeyDown(KeyCode.Return) && currentState == CharacterState.Idle) StartGame();
        if (Input.GetKeyDown(KeyCode.Space)) TypeCorrectLetter();
        if (Input.GetKeyDown(KeyCode.Backspace)) TypeWrongLetter();
    }

    public void StartGame()
    {
        currentState = CharacterState.Running;
        PlayAnimation("KommyMove");
    }

    public void TypeCorrectLetter()
    {
        if (currentState == CharacterState.Dead || currentState == CharacterState.Victory) return;
        
        StopAllCoroutines(); 
        StartCoroutine(AttackRoutine());
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

    // --- COROUTINES (Timers for animations) ---

    private IEnumerator AttackRoutine()
    {
        currentState = CharacterState.Attacking;
        PlayAnimation("KommyAttack"); 
        
        // Wait for 0.2 seconds (a quick attack flash)
        yield return new WaitForSeconds(0.2f);

        if (currentState != CharacterState.Dead && currentState != CharacterState.Stunned)
        {
            currentState = CharacterState.Running;
            PlayAnimation("KommyMove");
        }
    }

    private IEnumerator StunRoutine()
    {
        currentState = CharacterState.Stunned;
        currentHp--; // Lose 1 HP
        PlayAnimation("KommyStun"); 
        
        // Stunned for 1 full second
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
        Debug.Log("GAME OVER - Show Try Again Screen");
    }

    private void PlayAnimation(string animName)
    {
        if (animator != null) animator.Play(animName);
    }
}