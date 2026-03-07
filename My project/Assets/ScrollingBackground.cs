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
        repeatWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        // --- THE FIX ---
        // We added "Jumping" to the list of allowed movement states!
        if (kommy != null && (kommy.currentState == KommyController.CharacterState.Running || 
                              kommy.currentState == KommyController.CharacterState.Attacking || 
                              kommy.currentState == KommyController.CharacterState.Jumping))
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