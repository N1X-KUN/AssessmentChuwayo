using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public KommyController kommy; 
    
    public float scrollSpeed = 2f;
    private float repeatWidth;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        // Your original perfect setup for getting the exact image size!
        repeatWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        LevelManager lm = FindAnyObjectByType<LevelManager>();
        if (lm != null && !lm.gameIsActive) return;
        // --- THE FIX ---
        // Added "Ability" so the background knows it is allowed to keep moving (in slow-mo) while she sleeps!
        if (kommy != null && (kommy.currentState == KommyController.CharacterState.Running || 
                              kommy.currentState == KommyController.CharacterState.Attacking || 
                              kommy.currentState == KommyController.CharacterState.Jumping ||
                              kommy.currentState == KommyController.CharacterState.Ability)) // <-- Added this!
        {
            // Because you are using Time.deltaTime, this naturally slows down to 40% speed automatically!
            transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

            // Your original perfect gap-prevention math!
            if (transform.position.x < startPosition.x - repeatWidth)
            {
                float overshoot = transform.position.x - (startPosition.x - repeatWidth);
                transform.position = new Vector3(startPosition.x + overshoot, transform.position.y, transform.position.z);
            }
        }
    }
}