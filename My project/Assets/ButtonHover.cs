using UnityEngine;
using UnityEngine.EventSystems; 

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 startScale;
    private Quaternion startRotation; // <-- This will memorize your custom slant!

    void Start()
    {
        // Remember exactly how big and slanted the button is when the game starts
        startScale = transform.localScale;
        startRotation = transform.localRotation; 
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = startScale * 1.1f; // Make it 10% bigger
        // Add a 3-degree tilt to whatever slant it ALREADY has
        transform.localRotation = startRotation * Quaternion.Euler(0, 0, 3f); 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = startScale; // Return to normal size
        transform.localRotation = startRotation; // <-- Return to your exact custom slant!
    }
}