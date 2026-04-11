using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public KommyController kommy; 
    
    [Header("Ground Settings")]
    public bool isGroundLayer = false; // NEW: Check this box ONLY on your Grass/Ground pieces!
    
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

        // NEW: If this is the ground, and Kommy is stunned, STOP moving!
        if (isGroundLayer && kommy != null && kommy.currentState == KommyController.CharacterState.Stunned)
        {
            return; 
        }

        if (kommy != null && (kommy.currentState == KommyController.CharacterState.Running || 
                              kommy.currentState == KommyController.CharacterState.Attacking || 
                              kommy.currentState == KommyController.CharacterState.Jumping ||
                              kommy.currentState == KommyController.CharacterState.Ability)) 
        {
            transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

            if (transform.position.x < startPosition.x - repeatWidth)
            {
                float overshoot = transform.position.x - (startPosition.x - repeatWidth);
                transform.position = new Vector3(startPosition.x + overshoot, transform.position.y, transform.position.z);
            }
        }
    }
}