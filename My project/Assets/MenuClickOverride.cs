using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.EventSystems; // Add this secret library!

public class MenuClickOverride : MonoBehaviour
{
    [Header("Drag your Red Box here")]
    public RectTransform headHitbox;
    
    [Header("Drag KommyGreet here")]
    public MenuCharacter kommyScript;

    void Update()
    {
        // NEW LINE: If the mouse is hovering over ANY UI panel/box, STOP checking for clicks!
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (RectTransformUtility.RectangleContainsScreenPoint(headHitbox, mousePos, null))
            {
                kommyScript.OnCharacterClicked();
            }
        }
    }
}