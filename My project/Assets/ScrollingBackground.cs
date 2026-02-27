using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public float scrollSpeed = 2f;
    private float repeatWidth;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        // This automatically measures how wide your background picture is!
        repeatWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        // Move to the left
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // If the background has moved off the screen, teleport it back to the start!
        if (transform.position.x < startPosition.x - repeatWidth)
        {
            float overshoot = transform.position.x - (startPosition.x - repeatWidth);
            transform.position = new Vector3(startPosition.x + overshoot, transform.position.y, transform.position.z);
        }
    }
}