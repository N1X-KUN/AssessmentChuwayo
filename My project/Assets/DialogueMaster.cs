using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Needed to change Image sprites!

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public enum SpeakerSide { Left, Right }
        public SpeakerSide side;
        
        [Header("Character & Box Setup")]
        public RuntimeAnimatorController characterController; // Swaps the "Brain" (Burumi, Erufinn, etc.)
        public Sprite boxColor; // Swaps the UI Box (Green, Purple, Red)
        public string avatarAnimationName; // The exact box name (e.g., "Burumi_Happy")
        
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

        // INSTANT TEST: Plays the Intro sequence right when the game starts
        PlayDialogue("Intro"); 
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
            
            // 1. SWAP THE BOX COLOR
            if (line.boxColor != null) 
                leftBox.GetComponent<Image>().sprite = line.boxColor;

            // 2. SWAP THE ANIMATOR CONTROLLER
            if (line.characterController != null) 
                leftAvatarAnim.runtimeAnimatorController = line.characterController;

            // 3. PLAY THE REACTION
            if (leftAvatarAnim != null && !string.IsNullOrEmpty(line.avatarAnimationName))
                leftAvatarAnim.Play(line.avatarAnimationName);
                
            typingCoroutine = StartCoroutine(TypeLine(leftText, line.text));
        }
        else
        {
            rightBox.SetActive(true);
            
            // 1. SWAP THE BOX COLOR
            if (line.boxColor != null) 
                rightBox.GetComponent<Image>().sprite = line.boxColor;

            // 2. SWAP THE ANIMATOR CONTROLLER
            if (line.characterController != null) 
                rightAvatarAnim.runtimeAnimatorController = line.characterController;

            // 3. PLAY THE REACTION
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
        dialogueOverlay.SetActive(false);
        leftBox.SetActive(false);
        rightBox.SetActive(false);
        
        Time.timeScale = 1f; // UNFREEZE GAME
    }
}