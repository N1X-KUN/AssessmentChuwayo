using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public enum SpeakerSide { Left, Right }
        public SpeakerSide side;
        
        [Header("Character & Box Setup")]
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
    
    [Header("Left Side")]
    public GameObject leftBox;
    public TextMeshProUGUI leftText;
    public Animator leftAvatarAnim;

    [Header("Right Side")]
    public GameObject rightBox;
    public TextMeshProUGUI rightText;
    public Animator rightAvatarAnim;

    [Header("Settings")]
    public float typingSpeed = 0.03f;

    [Header("Tutorial Mode Flags (The Checklist)")]
    public bool isTutorialMode = true; // Turn this OFF when they hit "Try Again"
    public bool seenCountdown = false;
    public bool seenFirstFood = false;
    public bool seenFirstPoison = false;
    public bool seenFirstPunch = false;
    public bool seenFirstAmmo = false;
    public bool seenThiefKnockdown = false;

    [Header("End Game Buttons")]
    public GameObject winNextLevelButton; 
    public GameObject loseTryAgainButton; 
    public GameObject loseGiveUpButton;   

    private DialogueSequence currentSequence;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private bool dialogueIsActive = false;

    void Start()
    {
        dialogueOverlay.SetActive(false);
        leftBox.SetActive(false);
        rightBox.SetActive(false);

        if (winNextLevelButton != null) winNextLevelButton.SetActive(false);
        if (loseTryAgainButton != null) loseTryAgainButton.SetActive(false);
        if (loseGiveUpButton != null) loseGiveUpButton.SetActive(false);

        // INSTANT TEST (Will only play if isTutorialMode is true)
        if (isTutorialMode) PlayDialogue("Intro"); 
    }

    void Update()
    {
        if (!dialogueIsActive) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
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
        // If tutorial mode is off, ignore dialogue requests EXCEPT Win/Lose
        if (!isTutorialMode && seqName != "TutorialWin" && seqName != "TutorialLose") return;

        foreach (DialogueSequence seq in allSequences)
        {
            if (seq.sequenceName == seqName)
            {
                currentSequence = seq;
                dialogueIsActive = true;
                dialogueOverlay.SetActive(true);
                currentLineIndex = 0;
                
                Time.timeScale = 0f; // FREEZE GAME
                PlayLine(currentLineIndex);
                return;
            }
        }
        Debug.LogWarning("Could not find dialogue sequence named: " + seqName);
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
            if (leftAvatarAnim != null && !string.IsNullOrEmpty(line.avatarAnimationName))
                leftAvatarAnim.Play(line.avatarAnimationName);
                
            typingCoroutine = StartCoroutine(TypeLine(leftText, line.text));
        }
        else
        {
            rightBox.SetActive(true);
            if (line.boxColor != null) rightBox.GetComponent<Image>().sprite = line.boxColor;
            if (line.characterController != null) rightAvatarAnim.runtimeAnimatorController = line.characterController;
            if (rightAvatarAnim != null && !string.IsNullOrEmpty(line.avatarAnimationName))
                rightAvatarAnim.Play(line.avatarAnimationName);
                
            typingCoroutine = StartCoroutine(TypeLine(rightText, line.text));
        }
    }

    private IEnumerator TypeLine(TextMeshProUGUI textComponent, string line)
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char c in line.ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed); 
        }

        isTyping = false;
    }

    private void SetInstantText()
    {
        DialogueLine line = currentSequence.lines[currentLineIndex];
        if (line.side == DialogueLine.SpeakerSide.Left) leftText.text = line.text;
        else rightText.text = line.text;
        
        isTyping = false;
    }

    private void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentSequence.lines.Length)
        {
            PlayLine(currentLineIndex);
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialogueIsActive = false;
        leftBox.SetActive(false);
        rightBox.SetActive(false);
        
        // CHECK IF IT WAS THE END OF THE GAME DIALOGUE
        if (currentSequence.sequenceName == "TutorialWin")
        {
            if (winNextLevelButton != null) winNextLevelButton.SetActive(true);
            return; // Leave overlay on, game frozen, wait for button click!
        }
        else if (currentSequence.sequenceName == "TutorialLose")
        {
            if (loseTryAgainButton != null) loseTryAgainButton.SetActive(true);
            if (loseGiveUpButton != null) loseGiveUpButton.SetActive(true);
            return; // Leave overlay on, game frozen, wait for button click!
        }

        // If it was just a mid-game tutorial dialogue, unfreeze and turn off overlay
        dialogueOverlay.SetActive(false);
        Time.timeScale = 1f; 
    }
}