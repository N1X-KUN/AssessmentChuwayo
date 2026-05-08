using UnityEngine;
using TMPro;

public class CoinDisplay : MonoBehaviour
{
    public TMP_Text coinText;

    void Start()
    {
        UpdateCoins();
    }

    // Call this if they buy something in the shop to refresh the number!
    public void UpdateCoins()
    {
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (coinText != null) coinText.text = totalCoins.ToString();
    }
}