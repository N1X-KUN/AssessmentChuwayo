using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CharacterCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Card Visuals")]
    public Image layer0_Halo;
    public Image layer2_CharacterArt; 
    public Animator characterAnimator; 
    public Image layer3_Border;
    public Animator borderAnimator;
    public Sprite lockedBorderSprite;
    public Sprite unlockedBorderSprite;

    [Header("Global Indicator")]
    public TextMeshProUGUI globalIndicatorText;
    public Animator globalIndicatorAnimator; 

    [Header("Card Status")]
    public string characterName; 
    [Tooltip("Leave blank for default characters. Type Kommy_Unlocked for Kommy!")]
    public string unlockSaveKey; // <-- I ADDED THIS!
    public bool isUnlocked = true; 
    public bool isEquipped = false;

    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; 

    void Start()
    {
        // --- NEW: CHECK IF BOUGHT IN SHOP ---
        if (!string.IsNullOrEmpty(unlockSaveKey))
        {
            // If they have the key, set unlocked to true! Otherwise, false.
            isUnlocked = (PlayerPrefs.GetInt(unlockSaveKey, 0) == 1);
        }
        // ------------------------------------

        string savedCharacter = PlayerPrefs.GetString("EquippedCharacter", "Tig");
        isEquipped = (savedCharacter == characterName);
        UpdateCardVisuals();
    }

    public void UpdateCardVisuals()
    {
        if (!isUnlocked)
        {
            layer0_Halo.enabled = false;
            if (borderAnimator != null) borderAnimator.enabled = false;
            layer3_Border.sprite = lockedBorderSprite;
            
            if (layer2_CharacterArt != null) layer2_CharacterArt.color = new Color(0, 0, 0, 0.8f); 
            if (characterAnimator != null) characterAnimator.enabled = true; 
        }
        else if (isEquipped)
        {
            layer0_Halo.enabled = true;
            if (borderAnimator != null) borderAnimator.enabled = true; 
            
            if (layer2_CharacterArt != null) layer2_CharacterArt.color = Color.white;
            if (characterAnimator != null) characterAnimator.enabled = true; 
        }
        else 
        {
            layer0_Halo.enabled = false;
            if (borderAnimator != null) borderAnimator.enabled = false; 
            layer3_Border.sprite = unlockedBorderSprite; 
            
            if (layer2_CharacterArt != null) layer2_CharacterArt.color = Color.white;
            if (characterAnimator != null) characterAnimator.enabled = false; 
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isUnlocked) 
        {
            globalIndicatorText.text = "LOCKED";
            if (globalIndicatorAnimator != null) globalIndicatorAnimator.enabled = false;
        }
        else if (isEquipped) 
        {
            globalIndicatorText.text = "EQUIPPED";
            if (globalIndicatorAnimator != null) globalIndicatorAnimator.enabled = true; 
        }
        else 
        {
            globalIndicatorText.text = "EQUIP";
            if (globalIndicatorAnimator != null) globalIndicatorAnimator.enabled = false;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        globalIndicatorText.text = "---";
        if (globalIndicatorAnimator != null) globalIndicatorAnimator.enabled = false; 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isUnlocked) 
        {
            if (Time.unscaledTime - lastClickTime <= doubleClickThreshold)
            {
                CharacterCard[] allCards = transform.parent.GetComponentsInChildren<CharacterCard>();
                foreach (CharacterCard card in allCards)
                {
                    card.isEquipped = false;
                    card.UpdateCardVisuals();
                }

                isEquipped = true;
                UpdateCardVisuals();
                
                globalIndicatorText.text = "EQUIPPED";
                if (globalIndicatorAnimator != null) globalIndicatorAnimator.enabled = true;

                PlayerPrefs.SetString("EquippedCharacter", characterName);
            }
            lastClickTime = Time.unscaledTime;
        }
    }
}