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
        public GameObject custom2DPrefab; 
        
        [Tooltip("Since we are back in UI mode, set this to a low number like 1, 2, or 3!")]
        public float prefabScaleMultiplier = 1.5f; 

        public Sprite boxColor; 
        public string avatarAnimationName; 
        public AudioClip customVoiceLine; 
        [TextArea(3, 10)] public string text;
    }

    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceName; 
        public Sprite backgroundImage; 
        public DialogueLine[] lines;
    }

    [Header("All Game Dialogues")]
    public DialogueSequence[] allSequences;

    [Header("Left Side UI")]
    public GameObject leftBox;
    public TextMeshProUGUI leftText;
    public Animator leftAvatarGiant; 
    public Animator leftAvatarHead;  
    public Transform left2DSpawnPoint; 

    [Header("Right Side UI")]
    public GameObject rightBox;
    public TextMeshProUGUI rightText;
    public Animator rightAvatarGiant; 
    public Animator rightAvatarHead;  
    public Transform right2DSpawnPoint; 

    [Header("Other UI Elements")]
    public GameObject dialogueOverlay; 
    public Image dialogueBackgroundImage; 

    [Header("Settings")]
    public float typingSpeed = 0.03f;
    public bool isTutorialMode = true; 

    [Header("End Game Buttons")]
    public GameObject winNextLevelButton; 
    public GameObject loseTryAgainButton; 
    public GameObject loseGiveUpButton;   

    [HideInInspector] public bool dialogueIsActive = false;
    [HideInInspector] public bool keepOpenOnEnd = false; 
    
    private DialogueSequence currentSequence;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    
    private GameObject active2DWrapper; 
    
    // --- NEW: THE PROXY UI VARIABLES ---
    private SpriteRenderer activeProxyRenderer;
    private Image activeProxyImage;

    void Start()
    {
        if (dialogueOverlay != null) dialogueOverlay.SetActive(false);
        if (leftBox != null) leftBox.SetActive(false);
        if (rightBox != null) rightBox.SetActive(false);
        if (leftAvatarGiant != null) leftAvatarGiant.gameObject.SetActive(false);
        if (rightAvatarGiant != null) rightAvatarGiant.gameObject.SetActive(false);
        if (dialogueBackgroundImage != null) dialogueBackgroundImage.gameObject.SetActive(false);
        if (winNextLevelButton != null) winNextLevelButton.SetActive(false);
        if (loseTryAgainButton != null) loseTryAgainButton.SetActive(false);
        if (loseGiveUpButton != null) loseGiveUpButton.SetActive(false);

        if (PlayerPrefs.HasKey("TutorialMode")) isTutorialMode = PlayerPrefs.GetInt("TutorialMode") == 1;
        if (isTutorialMode) PlayDialogue("Intro"); 
    }

    void Update()
    {
        // --- NEW: COPY THE 2D SPRITE TO THE UI IMAGE FRAME BY FRAME ---
        if (activeProxyImage != null && activeProxyRenderer != null)
        {
            if (activeProxyImage.sprite != activeProxyRenderer.sprite)
            {
                activeProxyImage.sprite = activeProxyRenderer.sprite;
                activeProxyImage.SetNativeSize(); // Automatically size it correctly!
            }
        }

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
            else NextLine();
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

                if (dialogueBackgroundImage != null)
                {
                    if (seq.backgroundImage != null) { dialogueBackgroundImage.sprite = seq.backgroundImage; dialogueBackgroundImage.gameObject.SetActive(true); }
                    else dialogueBackgroundImage.gameObject.SetActive(false);
                }
                
                Time.timeScale = 0f; 
                if (AudioManager.instance != null) AudioManager.instance.MuffleMusic(true);
                PlayLine(currentLineIndex);
                return;
            }
        }
    }

    private void PlayLine(int index)
    {
        DialogueLine line = currentSequence.lines[index];

        if (AudioManager.instance != null)
        {
            if (line.customVoiceLine != null) AudioManager.instance.PlayUI(line.customVoiceLine); 
            else AudioManager.instance.PlayUI(AudioManager.instance.dialoguePop);
        }

        leftBox.SetActive(false);
        rightBox.SetActive(false);
        if (leftAvatarGiant != null) leftAvatarGiant.gameObject.SetActive(false);
        if (rightAvatarGiant != null) rightAvatarGiant.gameObject.SetActive(false);
        if (leftAvatarHead != null) leftAvatarHead.gameObject.SetActive(false); 
        if (rightAvatarHead != null) rightAvatarHead.gameObject.SetActive(false); 

        activeProxyRenderer = null;
        activeProxyImage = null;
        if (active2DWrapper != null) Destroy(active2DWrapper);

        leftText.text = "";
        rightText.text = "";

        string actualText = line.text;
        if (actualText.Contains("@playername"))
        {
            string savedName = PlayerPrefs.GetString("PlayerName", "Player");
            actualText = actualText.Replace("@playername", savedName);
        }

        if (line.side == DialogueLine.SpeakerSide.Left)
        {
            leftBox.SetActive(true);
            if (line.boxColor != null) leftBox.GetComponent<Image>().sprite = line.boxColor;

            if (line.custom2DPrefab != null && left2DSpawnPoint != null)
            {
                left2DSpawnPoint.gameObject.SetActive(true); 
                Image spawnImage = left2DSpawnPoint.GetComponent<Image>();
                if (spawnImage != null) spawnImage.enabled = false;

                active2DWrapper = new GameObject("ScaleWrapper");
                active2DWrapper.transform.SetParent(left2DSpawnPoint, false);
                active2DWrapper.transform.localPosition = Vector3.zero; 
                
                // Add the fake UI Image that will display the Thief
                activeProxyImage = active2DWrapper.AddComponent<Image>();
                activeProxyImage.preserveAspect = true;

                float scale = line.prefabScaleMultiplier;
                active2DWrapper.transform.localScale = new Vector3(scale, scale, scale);

                GameObject thiefClone = Instantiate(line.custom2DPrefab, active2DWrapper.transform);
                thiefClone.SetActive(false); // Surgery time

                ThiefController tc = thiefClone.GetComponentInChildren<ThiefController>(true);
                if (tc != null) DestroyImmediate(tc);

                Rigidbody2D[] rbs = thiefClone.GetComponentsInChildren<Rigidbody2D>(true);
                foreach(var rb in rbs) DestroyImmediate(rb);

                thiefClone.SetActive(true); 
                thiefClone.transform.localPosition = Vector3.zero;
                thiefClone.transform.localScale = Vector3.one; 
                
                // --- HIDE THE ACTUAL SPRITE, WE WILL COPY IT TO THE UI IMAGE ---
                activeProxyRenderer = thiefClone.GetComponentInChildren<SpriteRenderer>(true);
                if (activeProxyRenderer != null) activeProxyRenderer.enabled = false; 
                
                Animator anim = thiefClone.GetComponentInChildren<Animator>(true);
                if (anim != null && anim.runtimeAnimatorController != null) 
                { 
                    anim.gameObject.SetActive(true);
                    anim.enabled = true; 
                    anim.updateMode = AnimatorUpdateMode.UnscaledTime;
                    anim.Update(0f); 
                    if (!string.IsNullOrEmpty(line.avatarAnimationName)) { try { anim.Play(line.avatarAnimationName, 0, 0f); } catch {} }
                }
            }
            else if (line.characterController != null) 
            {
                if (left2DSpawnPoint != null) {
                    Image spawnImage = left2DSpawnPoint.GetComponent<Image>();
                    if (spawnImage != null) spawnImage.enabled = true;
                }

                if (leftAvatarGiant != null) { leftAvatarGiant.gameObject.SetActive(true); leftAvatarGiant.runtimeAnimatorController = line.characterController; }
                if (leftAvatarHead != null) { leftAvatarHead.gameObject.SetActive(true); leftAvatarHead.runtimeAnimatorController = line.characterController; }
                if (!string.IsNullOrEmpty(line.avatarAnimationName)) 
                {
                    if (leftAvatarGiant != null) leftAvatarGiant.Play(line.avatarAnimationName);
                    if (leftAvatarHead != null) leftAvatarHead.Play(line.avatarAnimationName);
                }
            }

            typingCoroutine = StartCoroutine(TypeLine(leftText, actualText));
        }
        else
        {
            rightBox.SetActive(true);
            if (line.boxColor != null) rightBox.GetComponent<Image>().sprite = line.boxColor;

            if (line.custom2DPrefab != null && right2DSpawnPoint != null)
            {
                right2DSpawnPoint.gameObject.SetActive(true); 
                Image spawnImage = right2DSpawnPoint.GetComponent<Image>();
                if (spawnImage != null) spawnImage.enabled = false;

                active2DWrapper = new GameObject("ScaleWrapper");
                active2DWrapper.transform.SetParent(right2DSpawnPoint, false);
                active2DWrapper.transform.localPosition = Vector3.zero; 
                
                // Add the fake UI Image that will display the Thief
                activeProxyImage = active2DWrapper.AddComponent<Image>();
                activeProxyImage.preserveAspect = true;

                float scale = line.prefabScaleMultiplier;
                active2DWrapper.transform.localScale = new Vector3(-scale, scale, scale); 

                GameObject thiefClone = Instantiate(line.custom2DPrefab, active2DWrapper.transform);
                thiefClone.SetActive(false); 

                ThiefController tc = thiefClone.GetComponentInChildren<ThiefController>(true);
                if (tc != null) DestroyImmediate(tc);

                Rigidbody2D[] rbs = thiefClone.GetComponentsInChildren<Rigidbody2D>(true);
                foreach(var rb in rbs) DestroyImmediate(rb);

                thiefClone.SetActive(true); 
                thiefClone.transform.localPosition = Vector3.zero;
                thiefClone.transform.localScale = Vector3.one; 
                
                // --- HIDE THE ACTUAL SPRITE, WE WILL COPY IT TO THE UI IMAGE ---
                activeProxyRenderer = thiefClone.GetComponentInChildren<SpriteRenderer>(true);
                if (activeProxyRenderer != null) activeProxyRenderer.enabled = false; 

                Animator anim = thiefClone.GetComponentInChildren<Animator>(true);
                if (anim != null && anim.runtimeAnimatorController != null) 
                { 
                    anim.gameObject.SetActive(true);
                    anim.enabled = true; 
                    anim.updateMode = AnimatorUpdateMode.UnscaledTime;
                    anim.Update(0f); 
                    if (!string.IsNullOrEmpty(line.avatarAnimationName)) { try { anim.Play(line.avatarAnimationName, 0, 0f); } catch {} }
                }
            }
            else if (line.characterController != null) 
            {
                if (right2DSpawnPoint != null) {
                    Image spawnImage = right2DSpawnPoint.GetComponent<Image>();
                    if (spawnImage != null) spawnImage.enabled = true;
                }

                if (rightAvatarGiant != null) { rightAvatarGiant.gameObject.SetActive(true); rightAvatarGiant.runtimeAnimatorController = line.characterController; }
                if (rightAvatarHead != null) { rightAvatarHead.gameObject.SetActive(true); rightAvatarHead.runtimeAnimatorController = line.characterController; }
                if (!string.IsNullOrEmpty(line.avatarAnimationName)) 
                {
                    if (rightAvatarGiant != null) rightAvatarGiant.Play(line.avatarAnimationName);
                    if (rightAvatarHead != null) rightAvatarHead.Play(line.avatarAnimationName);
                }
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

        if (line.side == DialogueLine.SpeakerSide.Left) { leftText.text = actualText; leftText.maxVisibleCharacters = actualText.Length; }
        else { rightText.text = actualText; rightText.maxVisibleCharacters = actualText.Length; }
        isTyping = false;
        CheckForEndGameButtons(); 
    }

    private void CheckForEndGameButtons()
    {
        if (currentLineIndex == currentSequence.lines.Length - 1)
        {
            if (currentSequence.sequenceName == "TutorialWin") { if (winNextLevelButton != null) winNextLevelButton.SetActive(true); if (loseGiveUpButton != null) loseGiveUpButton.SetActive(true); }
            else if (currentSequence.sequenceName == "TutorialLose") { if (loseTryAgainButton != null) loseTryAgainButton.SetActive(true); if (loseGiveUpButton != null) loseGiveUpButton.SetActive(true); }
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

        activeProxyRenderer = null;
        activeProxyImage = null;
        if (active2DWrapper != null) Destroy(active2DWrapper); 

        if (!keepOpenOnEnd)
        {
            if (dialogueOverlay != null) dialogueOverlay.SetActive(false);
            if (leftBox != null) leftBox.SetActive(false);
            if (rightBox != null) rightBox.SetActive(false);
            if (leftAvatarGiant != null) leftAvatarGiant.gameObject.SetActive(false);
            if (rightAvatarGiant != null) rightAvatarGiant.gameObject.SetActive(false);
            if (dialogueBackgroundImage != null) dialogueBackgroundImage.gameObject.SetActive(false);
        }

        if (AudioManager.instance != null) AudioManager.instance.MuffleMusic(false);
        Time.timeScale = 1f; 
    }

    public void Button_TryAgain() { PlayerPrefs.SetInt("TutorialMode", 0); Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    public void Button_GiveUp() { if (currentSequence != null && currentSequence.sequenceName == "TutorialWin") { PlayerPrefs.SetInt("HasFinishedTutorial", 1); PlayerPrefs.Save(); } if (LoadingManager.Instance != null) { LoadingManager.Instance.LoadNewScene("MenuScene"); } else { SceneManager.LoadScene("MenuScene"); } }
    public void Button_YesLetsGo() { PlayerPrefs.SetInt("HasFinishedTutorial", 1); PlayerPrefs.Save(); if (LoadingManager.Instance != null) { LoadingManager.Instance.LoadNewScene("MapScene"); } else { SceneManager.LoadScene("MapScene"); } }
}