using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider progressBar;
    public TMP_Text hpText;

    [Header("Level Settings")]
    public float levelDuration = 20f; 
    private float timeElapsed = 0f;

    [Header("References")]
    public KommyController kommy;
    public WordManager wordManager;
    public ThiefController thief; // WE ADDED THE THIEF HERE!

    void Start()
    {
        if (progressBar != null)
        {
            progressBar.maxValue = levelDuration;
            progressBar.value = 0f;
        }
    }

    void Update()
    {
        if (hpText != null && kommy != null)
        {
            hpText.text = "HP: " + kommy.currentHp.ToString();
        }

        if (kommy.currentState != KommyController.CharacterState.Dead && kommy.currentState != KommyController.CharacterState.Victory)
        {
            timeElapsed += Time.deltaTime;
            
            if (progressBar != null)
            {
                progressBar.value = timeElapsed;
            }

            // VICTORY CONDITION MET (30 Seconds Passed)
            if (timeElapsed >= levelDuration)
            {
                kommy.WinGame(); 
                wordManager.CancelInvoke(); 
                
                // NEW: Tell the thief he lost!
                if (thief != null) 
                {
                    thief.TriggerDefeat();
                }
            }
        }
    }
}