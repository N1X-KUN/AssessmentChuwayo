using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapPhaseManager : MonoBehaviour
{
    [Header("Developer Testing")]
    public bool forcePlayIntro = false;

    [Header("Phase 1: Input Setup")]
    public GameObject phase1Folder; 
    public TMP_InputField nameInputField;
    public Button submitNameButton;

    [Header("Phase 2: Camera Setup")]
    public Camera mainCamera;
    public float autoScrollSpeed = 5f; 
    public float manualScrollSpeed = 10f; 
    public float topCameraLimit = 6.5f;    
    public float bottomCameraLimit = -15f; 

    private enum MapState { AutoScrolling, PlayingIntro1, WaitingForName, PlayingIntro2, FreeRoam }
    private MapState currentState;
    private DialogueManager dm;

    void Start()
    {
        dm = FindAnyObjectByType<DialogueManager>();
        if (phase1Folder != null) phase1Folder.SetActive(false); 

        string savedName = PlayerPrefs.GetString("PlayerName", "");
        int hasFinishedLevel1 = PlayerPrefs.GetInt("HasFinishedLevel1", 0); // FIXED THE MEMORY KEY

        if (forcePlayIntro || (savedName == "" && hasFinishedLevel1 == 0))
        {
            Vector3 startPos = mainCamera.transform.position;
            startPos.y = bottomCameraLimit;
            mainCamera.transform.position = startPos;
            
            currentState = MapState.AutoScrolling;
            
            if (submitNameButton != null) submitNameButton.onClick.AddListener(SaveNameAndStartPart2);
            if (nameInputField != null) nameInputField.onSubmit.AddListener((string input) => SaveNameAndStartPart2());
        }
        else
        {
            Vector3 startPos = mainCamera.transform.position;
            startPos.y = topCameraLimit;
            mainCamera.transform.position = startPos;
            currentState = MapState.FreeRoam;
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case MapState.AutoScrolling:
                Vector3 autoPos = mainCamera.transform.position;
                autoPos.y += autoScrollSpeed * Time.deltaTime;

                if (autoPos.y >= topCameraLimit)
                {
                    autoPos.y = topCameraLimit;
                    mainCamera.transform.position = autoPos;
                    
                    if (dm != null) 
                    {
                        dm.keepOpenOnEnd = true; 
                        dm.PlayDialogue("MapIntro1");
                    }
                    currentState = MapState.PlayingIntro1;
                }
                else
                {
                    mainCamera.transform.position = autoPos;
                }
                break;

            case MapState.PlayingIntro1:
                if (dm != null && !dm.dialogueIsActive)
                {
                    if (phase1Folder != null) phase1Folder.SetActive(true);
                    if (nameInputField != null)
                    {
                        nameInputField.Select();
                        nameInputField.ActivateInputField();
                    }
                    Time.timeScale = 1f; 
                    currentState = MapState.WaitingForName;
                }
                break;

            case MapState.WaitingForName:
                break;

            case MapState.PlayingIntro2:
                if (dm != null && !dm.dialogueIsActive)
                {
                    currentState = MapState.FreeRoam;
                }
                break;

            case MapState.FreeRoam:
                float scrollInput = 0f;
                if (Input.mousePosition.y >= Screen.height * 0.9f) scrollInput = 1f;
                else if (Input.mousePosition.y <= Screen.height * 0.1f) scrollInput = -1f;

                if (scrollInput != 0)
                {
                    Vector3 newPos = mainCamera.transform.position;
                    newPos.y += scrollInput * manualScrollSpeed * Time.deltaTime;
                    newPos.y = Mathf.Clamp(newPos.y, bottomCameraLimit, topCameraLimit);
                    mainCamera.transform.position = newPos;
                }
                break;
        }
    }

    public void SaveNameAndStartPart2()
    {
        if (currentState != MapState.WaitingForName) return;

        if (nameInputField != null && nameInputField.text.Length > 0)
        {
            PlayerPrefs.SetString("PlayerName", nameInputField.text);
            PlayerPrefs.Save();
            if (phase1Folder != null) phase1Folder.SetActive(false);

            if (dm != null) 
            {
                dm.keepOpenOnEnd = false; 
                dm.PlayDialogue("MapIntro2");
            }
            currentState = MapState.PlayingIntro2;
        }
    }
}