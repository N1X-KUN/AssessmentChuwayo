using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Item Details")]
    public string displayName = "Kommy";
    [TextArea] public string description = "A cute companion!";
    public int price = 20;

    [Header("Purchase Logic")]
    public bool isJokeItem = false;
    public string saveKeyName = "Kommy_Unlocked"; 

    void Start()
    {
        if (PlayerPrefs.GetInt(saveKeyName, 0) == 1) gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShopManager.Instance.PreviewItem(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ShopManager.Instance.HidePreview();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ShopManager.Instance.LockItem(this);
    }
}