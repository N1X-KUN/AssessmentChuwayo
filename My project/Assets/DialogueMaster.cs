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

    [Header("Left Side UI")]
    public GameObject leftBox;
    public TextMeshProUGUI leftText;
    public Animator leftAvatarGiant; // FOR THE MASSIVE AVATAR
    public Animator leftAvatarHead;  // FOR THE CROPPED HEAD

    [Header("Right Side UI")]
    public GameObject rightBox;
    public TextMeshProUGUI rightText;
    public Animator rightAvatarGiant; // FOR THE MASSIVE AVATAR
    public Animator rightAvatarHead;  // FOR THE CROPPED HEAD

    [Header("Other UI Elements")]
    public GameObject dialogueOverlay; 

    [Header("Settings")]
    public float typingSpeed = 0.03f;

    [Header("Tutorial Mode Flags")]
    public bool isTutorialMode = true; 

    [Header("End Game Buttons (Leave Empty in Map Scene)")]
    public GameObject winNextLevelButton; 
    public GameObject loseTryAgainButton; 
    public GameObject loseGiveUpButton;   

    [HideInInspector] public bool dialogueIsActive = false;
    [HideInInspector] public bool keepOpenOnEnd = false; // KEEPS IT OPEN FOR TYPING!
    
    private DialogueSequence currentSequence;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (dialogueOverlay != null) dialogueOverlay.SetActive(false);
        if (leftBox != null) leftBox.SetActive(false);
        if (rightBox != null) rightBox.SetActive(false);
        
        // Hide giant avatars by default
        if (leftAvatarGiant != null) leftAvatarGiant.gameObject.SetActive(false);
        if (rightAvatarGiant != null) rightAvatarGiant.gameObject.SetActive(false);

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

        if (Input.GetMouseButtonDown(0))
        {
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
        foreach (DialogueSequence seq in allSequences)
        {
            if (seq.sequenceName == seqName)
            {
                currentSequence = seq;
                dialogueIsActive = true;
                dialogueOverlay.SetActive(true);
                currentLineIndex = 0;
                
                Time.timeScale = 0f; 

                if (AudioManager.instance != null) AudioManager.instance.MuffleMusic(true);

                PlayLine(currentLineIndex);
                return;
            }
        }
    }

    private void PlayLine(int index)
    {
        if (AudioManager.instance != null) AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop);

        DialogueLine line = currentSequence.lines[index];
        leftBox.SetActive(false);
        rightBox.SetActive(false);
        
        if (leftAvatarGiant != null) leftAvatarGiant.gameObject.SetActive(false);
        if (rightAvatarGiant != null) rightAvatarGiant.gameObject.SetActive(false);

        leftText.text = "";
        rightText.text = "";

        // --- THE MAGIC NAME CHECKER ---
        string actualText = line.text;
        if (actualText.Contains("@playername"))
        {
            string savedName = PlayerPrefs.GetString("PlayerName", "Player");
            actualText = actualText.Replace("@playername", savedName);
        }

        if (line.side == DialogueLine.SpeakerSide.Left)
        {
            leftBox.SetActive(true);
            if (leftAvatarGiant != null) leftAvatarGiant.gameObject.SetActive(true);

            if (line.boxColor != null) leftBox.GetComponent<Image>().sprite = line.boxColor;
            
            // PLAY ANIMATION ON BOTH GIANT AND HEAD!
            if (line.characterController != null) 
            {
                if (leftAvatarGiant != null) leftAvatarGiant.runtimeAnimatorController = line.characterController;
                if (leftAvatarHead != null) leftAvatarHead.runtimeAnimatorController = line.characterController;
            }
            if (!string.IsNullOrEmpty(line.avatarAnimationName)) 
            {
                if (leftAvatarGiant != null) leftAvatarGiant.Play(line.avatarAnimationName);
                if (leftAvatarHead != null) leftAvatarHead.Play(line.avatarAnimationName);
            }

            typingCoroutine = StartCoroutine(TypeLine(leftText, actualText));
        }
        else
        {
            rightBox.SetActive(true);
            if (rightAvatarGiant != null) rightAvatarGiant.gameObject.SetActive(true);

            if (line.boxColor != null) rightBox.GetComponent<Image>().sprite = line.boxColor;
            
            // PLAY ANIMATION ON BOTH GIANT AND HEAD!
            if (line.characterController != null) 
            {
                if (rightAvatarGiant != null) rightAvatarGiant.runtimeAnimatorController = line.characterController;
                if (rightAvatarHead != null) rightAvatarHead.runtimeAnimatorController = line.characterController;
            }
            if (!string.IsNullOrEmpty(line.avatarAnimationName)) 
            {
                if (rightAvatarGiant != null) rightAvatarGiant.Play(line.avatarAnimationName);
                if (rightAvatarHead != null) rightAvatarHead.Play(line.avatarAnimationName);
            }

            typingCoroutine = StartCoroutine(TypeLine(rightText, actualText));
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
        
        string actualText = line.text;
        if (actualText.Contains("@playername"))
        {
            string savedName = PlayerPrefs.GetString("PlayerName", "Player");
            actualText = actualText.Replace("@playername", savedName);
        }

        if (line.side == DialogueLine.SpeakerSide.Left)
        {
            leftText.text = actualText;
            leftText.maxVisibleCharacters = actualText.Length; 
        }
        else 
        {
            rightText.text = actualText;
            rightText.maxVisibleCharacters = actualText.Length; 
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

        // ONLY hide the UI if we didn't tell it to stay open for the typing box!
        if (!keepOpenOnEnd)
        {
            if (dialogueOverlay != null) dialogueOverlay.SetActive(false);
            if (leftBox != null) leftBox.SetActive(false);
            if (rightBox != null) rightBox.SetActive(false);
            if (leftAvatarGiant != null) leftAvatarGiant.gameObject.SetActive(false);
            if (rightAvatarGiant != null) rightAvatarGiant.gameObject.SetActive(false);
        }

        if (AudioManager.instance != null) AudioManager.instance.MuffleMusic(false);

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
        if (currentSequence != null && currentSequence.sequenceName == "TutorialWin")
        {
            PlayerPrefs.SetInt("HasFinishedTutorial", 1);
            PlayerPrefs.Save();
        }
        LoadingManager.Instance.LoadNewScene("MenuScene"); 
    }

    public void Button_YesLetsGo()
    {
        PlayerPrefs.SetInt("HasFinishedTutorial", 1);
        PlayerPrefs.Save();
        LoadingManager.Instance.LoadNewScene("MapScene");
    }
}