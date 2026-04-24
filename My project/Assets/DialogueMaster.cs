using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public enum SpeakerSide { Left, Right }
        public SpeakerSide side;
        public RuntimeAnimatorController characterController; 
        public Sprite boxColor; 
        public string avatarAnimationName; 
        [TextArea(3, 10)] public string text;
    }

    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceName; 
        public DialogueLine[] lines;
    }

    [Header("All Game Dialogues")]
    public DialogueSequence[] allSequences;

    [Header("UI Elements")]
    public GameObject dialogueOverlay; 
    public GameObject leftBox;
    public TextMeshProUGUI leftText;
    public Animator leftAvatarAnim;
    public GameObject rightBox;
    public TextMeshProUGUI rightText;
    public Animator rightAvatarAnim;

    [Header("Settings")]
    public float typingSpeed = 0.03f;

    [Header("Tutorial Mode Flags (The Checklist)")]
    public bool isTutorialMode = true; 
    // We don't need the other booleans anymore because the Master Sequence handles it!

    [Header("End Game Buttons")]
    public GameObject winNextLevelButton; 
    public GameObject loseTryAgainButton; 
    public GameObject loseGiveUpButton;   

    [HideInInspector] public bool dialogueIsActive = false;
    
    private DialogueSequence currentSequence;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        dialogueOverlay.SetActive(false);
        leftBox.SetActive(false);
        rightBox.SetActive(false);

        if (winNextLevelButton != null) winNextLevelButton.SetActive(false);
        if (loseTryAgainButton != null) loseTryAgainButton.SetActive(false);
        if (loseGiveUpButton != null) loseGiveUpButton.SetActive(false);

        if (PlayerPrefs.HasKey("TutorialMode"))
        {
            isTutorialMode = PlayerPrefs.GetInt("TutorialMode") == 1;
        }

        if (isTutorialMode) PlayDialogue("Intro"); 
    }

    void Update()
    {
        if (!dialogueIsActive) return;

        // --- NEW: Changed to 0 for Left-Click! ---
        if (Input.GetMouseButtonDown(0))
        {
            // Don't let them left-click through the text if the end buttons are on screen!
            if (winNextLevelButton != null && winNextLevelButton.activeSelf) return;
            if (loseTryAgainButton != null && loseTryAgainButton.activeSelf) return;

            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                SetInstantText();
            }
            else
            {
                NextLine();
            }
        }
    }

    public void PlayDialogue(string seqName)
    {
        if (!isTutorialMode && seqName != "TutorialWin" && seqName != "TutorialLose") return;

        foreach (DialogueSequence seq in allSequences)
        {
            if (seq.sequenceName == seqName)
            {
                currentSequence = seq;
                dialogueIsActive = true;
                dialogueOverlay.SetActive(true);
                currentLineIndex = 0;
                
                Time.timeScale = 0f; 
                PlayLine(currentLineIndex);
                return;
            }
        }
    }

    private void PlayLine(int index)
    {
        DialogueLine line = currentSequence.lines[index];
        leftBox.SetActive(false);
        rightBox.SetActive(false);
        leftText.text = "";
        rightText.text = "";

        if (line.side == DialogueLine.SpeakerSide.Left)
        {
            leftBox.SetActive(true);
            if (line.boxColor != null) leftBox.GetComponent<Image>().sprite = line.boxColor;
            if (line.characterController != null) leftAvatarAnim.runtimeAnimatorController = line.characterController;
            if (leftAvatarAnim != null && !string.IsNullOrEmpty(line.avatarAnimationName)) leftAvatarAnim.Play(line.avatarAnimationName);
            typingCoroutine = StartCoroutine(TypeLine(leftText, line.text));
        }
        else
        {
            rightBox.SetActive(true);
            if (line.boxColor != null) rightBox.GetComponent<Image>().sprite = line.boxColor;
            if (line.characterController != null) rightAvatarAnim.runtimeAnimatorController = line.characterController;
            if (rightAvatarAnim != null && !string.IsNullOrEmpty(line.avatarAnimationName)) rightAvatarAnim.Play(line.avatarAnimationName);
            typingCoroutine = StartCoroutine(TypeLine(rightText, line.text));
        }
    }

    private IEnumerator TypeLine(TextMeshProUGUI textComponent, string line)
    {
        isTyping = true;
        textComponent.text = line; 
        textComponent.maxVisibleCharacters = 0; 
        for (int i = 0; i <= line.Length; i++)
        {
            textComponent.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(typingSpeed); 
        }
        isTyping = false;
        CheckForEndGameButtons(); 
    }

    private void SetInstantText()
    {
        DialogueLine line = currentSequence.lines[currentLineIndex];
        if (line.side == DialogueLine.SpeakerSide.Left)
        {
            leftText.text = line.text;
            leftText.maxVisibleCharacters = line.text.Length; 
        }
        else 
        {
            rightText.text = line.text;
            rightText.maxVisibleCharacters = line.text.Length; 
        }
        isTyping = false;
        CheckForEndGameButtons(); 
    }

    private void CheckForEndGameButtons()
    {
        if (currentLineIndex == currentSequence.lines.Length - 1)
        {
            if (currentSequence.sequenceName == "TutorialWin")
            {
                if (winNextLevelButton != null) winNextLevelButton.SetActive(true); 
                if (loseGiveUpButton != null) loseGiveUpButton.SetActive(true); 
            }
            else if (currentSequence.sequenceName == "TutorialLose")
            {
                if (loseTryAgainButton != null) loseTryAgainButton.SetActive(true); 
                if (loseGiveUpButton != null) loseGiveUpButton.SetActive(true); 
            }
        }
    }

    private void NextLine()
    {
        currentLineIndex++;
        if (currentLineIndex < currentSequence.lines.Length) PlayLine(currentLineIndex);
        else EndDialogue();
    }

    private void EndDialogue()
    {
        dialogueIsActive = false;
        dialogueOverlay.SetActive(false);
        leftBox.SetActive(false);
        rightBox.SetActive(false);
        Time.timeScale = 1f; 
    }

    public void Button_TryAgain()
    {
        PlayerPrefs.SetInt("TutorialMode", 0); 
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Button_GiveUp()
    {
        PlayerPrefs.SetInt("TutorialMode", 1); 
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); 
    }

    public void Button_OkLetsGo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MapScene"); 
    }
}