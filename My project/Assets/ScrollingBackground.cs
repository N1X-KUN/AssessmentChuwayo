using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public KommyController kommy; 
    
    [Header("Ground Settings")]
    public bool isGroundLayer = false; 
    
    [Header("Ability Settings")]
    public bool ignoreAbilitySlowdown = false; // NEW: Check this for BG1 and BG5!

    public float scrollSpeed = 2f;
    private float repeatWidth;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        repeatWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        LevelManager lm = FindAnyObjectByType<LevelManager>();
        if (lm != null && !lm.gameIsActive) return;

        if (isGroundLayer && kommy != null && kommy.currentState == KommyController.CharacterState.Stunned)
        {
            return; 
        }

        if (kommy != null && (kommy.currentState == KommyController.CharacterState.Running || 
                              kommy.currentState == KommyController.CharacterState.Attacking || 
                              kommy.currentState == KommyController.CharacterState.Jumping ||
                              kommy.currentState == KommyController.CharacterState.Ability)) 
        {
            float currentSpeed = scrollSpeed;

            // If the ultimate is active AND this isn't BG1/BG5, cut the speed exactly in half!
            if (kommy.isAbilityActive && !ignoreAbilitySlowdown)
            {
                currentSpeed = scrollSpeed / 2f;
            }

            // Using unscaledDeltaTime gives us total control so Unity doesn't double-slow it
            transform.Translate(Vector3.left * currentSpeed * Time.unscaledDeltaTime);

            if (transform.position.x < startPosition.x - repeatWidth)
            {
                float overshoot = transform.position.x - (startPosition.x - repeatWidth);
                transform.position = new Vector3(startPosition.x + overshoot, transform.position.y, transform.position.z);
            }
        }
    }
}