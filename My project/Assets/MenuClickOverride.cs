using UnityEngine;
using UnityEngine.InputSystem; // This forces Unity to use the New Input System directly

public class MenuClickOverride : MonoBehaviour
{
    [Header("Drag your Red Box here")]
    public RectTransform headHitbox;
    
    [Header("Drag KommyGreet here")]
    public MenuCharacter kommyScript;

    void Update()
    {
        // 1. Check if the physical mouse was clicked this exact frame
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 2. Get the exact pixel coordinate of your mouse pointer
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // 3. Check if that pointer is inside your red box
            if (RectTransformUtility.RectangleContainsScreenPoint(headHitbox, mousePos, null))
            {
                // 4. Force the bonk!
                kommyScript.OnCharacterClicked();
            }
        }
    }
}