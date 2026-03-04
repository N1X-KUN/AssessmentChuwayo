using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    // We added Kommy so the background can watch her!
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
        // IF Kommy is hooked up, AND she is either Running or Attacking... move!
        // (If she is Stunned, Dead, or Victorious, it completely freezes)
        if (kommy != null && (kommy.currentState == KommyController.CharacterState.Running || kommy.currentState == KommyController.CharacterState.Attacking))
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