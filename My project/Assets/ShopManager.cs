using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShopManager : MonoBehaviour
{   
    [Header("Pages")]
    public GameObject Page1;
    public GameObject Page2;
    public static ShopManager Instance;

    [Header("UI Text")]
    public TMP_Text playerCoinsText;

    [Header("Purchase Pop-up Panel")]
    public GameObject purchasePanel;
    public GameObject closeBlocker; 
    public TMP_Text popupNameText;
    public TMP_Text popupDescText;
    public TMP_Text popupPriceText;
    public Button priceButton; 
    public Image priceButtonImage; 
    private Color originalButtonColor;

    [Header("Shopkeeper System")]
    public Animator shopkeeperAnim;
    public CanvasGroup dialogueGroup; 
    public TMP_Text dialogueText;
    
    [TextArea] public string[] idlePhrases = new string[3];
    [TextArea] public string acceptPhrase = "Thank you for your purchase!";
    [TextArea] public string rejectPhrase = "You don't have enough coins!";

    private ShopItem currentSelectedItem;
    private int currentCoins;
    private bool isPanelLocked = false; 
    private Coroutine shakeCoroutine;
    private Coroutine shopkeeperRoutine;
    private Coroutine coinRoutine;

    void Awake() { Instance = this; }

    void Start()
    {
        currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        playerCoinsText.text = currentCoins.ToString("D4"); 
        
        purchasePanel.SetActive(false);
        closeBlocker.SetActive(false);
        if (priceButtonImage != null) originalButtonColor = priceButtonImage.color;

        // STARTUP GREETING: Show the first dialogue phrase immediately!
        if (dialogueGroup != null) dialogueGroup.alpha = 1f;
        if (dialogueText != null && idlePhrases.Length > 0) dialogueText.text = idlePhrases[0];
        
        shopkeeperRoutine = StartCoroutine(IdleDialogueRoutine(true));
    }

    // --- HOVER & CLICK LOGIC ---
    public void PreviewItem(ShopItem item)
    {
        if (isPanelLocked) return; 
        
        currentSelectedItem = item;
        popupNameText.text = item.displayName;
        popupDescText.text = item.description;
        popupPriceText.text = item.price.ToString(); 
        
        purchasePanel.SetActive(true);
    }

    public void HidePreview()
    {
        if (isPanelLocked) return; 
        purchasePanel.SetActive(false);
    }

    public void LockItem(ShopItem item)
    {
        isPanelLocked = true;
        PreviewItem(item);
        closeBlocker.SetActive(true); 
    }

    public void UnlockAndClosePanel()
    {
        isPanelLocked = false;
        closeBlocker.SetActive(false);
        purchasePanel.SetActive(false);
    }

    // --- PURCHASE LOGIC ---
    public void TryPurchase()
    {
        if (currentSelectedItem == null) return;

        if (currentCoins >= currentSelectedItem.price)
        {
            int oldCoins = currentCoins;
            currentCoins -= currentSelectedItem.price;
            PlayerPrefs.SetInt("TotalCoins", currentCoins);
            
            if (coinRoutine != null) StopCoroutine(coinRoutine);
            coinRoutine = StartCoroutine(AnimateCoins(oldCoins, currentCoins));

            if (!currentSelectedItem.isJokeItem)
            {
                PlayerPrefs.SetInt(currentSelectedItem.saveKeyName, 1);
                PlayerPrefs.Save();
            }

            currentSelectedItem.gameObject.SetActive(false); 
            UnlockAndClosePanel();

            if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop);
            TriggerShopkeeperReaction("Accept", acceptPhrase);
        }
        else
        {
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(FailPurchaseRoutine());
            TriggerShopkeeperReaction("Reject", rejectPhrase);
        }
    }

    private IEnumerator AnimateCoins(int startValue, int endValue)
    {
        float duration = 1.0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int currentVal = Mathf.FloorToInt(Mathf.Lerp(startValue, endValue, elapsed / duration));
            playerCoinsText.text = currentVal.ToString("D4");
            yield return null;
        }
        playerCoinsText.text = endValue.ToString("D4");
    }

    private IEnumerator FailPurchaseRoutine()
    {
        if (priceButtonImage != null) priceButtonImage.color = Color.red;
        if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop); 

        RectTransform btnRect = priceButton.GetComponent<RectTransform>();
        Vector3 originalPos = btnRect.anchoredPosition;

        float shakeAmount = 15f;
        float shakeTime = 0.05f;

        btnRect.anchoredPosition = originalPos + new Vector3(-shakeAmount, 0, 0);
        yield return new WaitForSecondsRealtime(shakeTime);
        btnRect.anchoredPosition = originalPos + new Vector3(shakeAmount, 0, 0);
        yield return new WaitForSecondsRealtime(shakeTime);
        btnRect.anchoredPosition = originalPos;

        if (priceButtonImage != null) priceButtonImage.color = originalButtonColor;
    }

    // --- SHOPKEEPER LOGIC ---
    private void TriggerShopkeeperReaction(string animTrigger, string phrase)
    {
        if (shopkeeperRoutine != null) StopCoroutine(shopkeeperRoutine);
        shopkeeperRoutine = StartCoroutine(ReactionRoutine(animTrigger, phrase));
    }

    private IEnumerator ReactionRoutine(string animName, string phrase)
    {
        if (shopkeeperAnim != null) shopkeeperAnim.Play(animName);
        if (dialogueText != null) dialogueText.text = phrase;
        if (dialogueGroup != null) dialogueGroup.alpha = 1f;

        // Fades out the reaction after 4 seconds so you aren't stuck waiting 30 seconds to see her go back to Idle
        yield return new WaitForSeconds(4f);

        if (dialogueGroup != null) yield return StartCoroutine(FadeDialogue(0f));
        if (shopkeeperAnim != null) shopkeeperAnim.Play("Idle");

        shopkeeperRoutine = StartCoroutine(IdleDialogueRoutine(false));
    }

    private IEnumerator IdleDialogueRoutine(bool isIntro)
    {
        // If it's the startup intro, wait 5 seconds then fade it out
        if (isIntro)
        {
            yield return new WaitForSeconds(5f);
            if (dialogueGroup != null) yield return StartCoroutine(FadeDialogue(0f));
        }

        // Loop forever: Wait 15 seconds, say something random, wait 5 seconds, fade out
        while (true)
        {
            yield return new WaitForSeconds(15f); 
            if (idlePhrases.Length > 0 && dialogueText != null)
            {
                dialogueText.text = idlePhrases[Random.Range(0, idlePhrases.Length)];
                if (dialogueGroup != null) yield return StartCoroutine(FadeDialogue(1f)); 
                yield return new WaitForSeconds(5f); 
                if (dialogueGroup != null) yield return StartCoroutine(FadeDialogue(0f)); 
            }
        }
    }

    private IEnumerator FadeDialogue(float targetAlpha)
    {
        if (dialogueGroup == null) yield break;
        float startAlpha = dialogueGroup.alpha;
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            dialogueGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        dialogueGroup.alpha = targetAlpha;
    }
    
    // Make sure your Left/Right arrows use these!
    public void ShowPage1() { Page1.SetActive(true); if(Page2 != null) Page2.SetActive(false); purchasePanel.SetActive(false); }
    public void ShowPage2() { Page1.SetActive(false); if(Page2 != null) Page2.SetActive(true); purchasePanel.SetActive(false); }

    public void ExitShop()
    {
        Time.timeScale = 1f; // Just to be safe!
        
        // Use your LoadingManager to transition back to the MapScene safely
        if (LoadingManager.Instance != null) 
        {
            LoadingManager.Instance.LoadNewScene("MapScene");
        }
        else 
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene");
        }
    }
}