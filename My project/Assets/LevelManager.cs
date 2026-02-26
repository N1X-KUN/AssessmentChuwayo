using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider progressBar;
    public TMP_Text hpText;

    [Header("Level Settings")]
    public float levelDuration = 30f; // The level lasts 30 seconds
    private float timeElapsed = 0f;

    [Header("References")]
    public KommyController kommy;
    public WordManager wordManager;

    void Start()
    {
        // Setup the progression bar
        if (progressBar != null)
        {
            progressBar.maxValue = levelDuration;
            progressBar.value = 0f;
        }
    }

    void Update()
    {
        // 1. Constantly update the Health Text to match Kommy's actual HP
        if (hpText != null && kommy != null)
        {
            hpText.text = "HP: " + kommy.currentHp.ToString();
        }

        // 2. Fill the Progression Bar over time
        if (kommy.currentState != KommyController.CharacterState.Dead && kommy.currentState != KommyController.CharacterState.Victory)
        {
            timeElapsed += Time.deltaTime;
            
            if (progressBar != null)
            {
                progressBar.value = timeElapsed;
            }

            // 3. Victory Condition!
            if (timeElapsed >= levelDuration)
            {
                kommy.WinGame(); // Triggers her Victory animation
                wordManager.CancelInvoke(); // Stops the apples from spawning!
            }
        }
    }
}