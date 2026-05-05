using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapPhaseManager : MonoBehaviour
{
    [Header("Phase 1: Input Setup")]
    [Tooltip("Drag the Phase1 folder from your Canvas here")]
    public GameObject phase1Folder; 
    public TMP_InputField nameInputField;
    public Button submitNameButton;

    [Header("Phase 2: Camera Setup")]
    public Camera mainCamera;
    public float autoScrollSpeed = 15f; // How fast it scrolls up at the beginning
    public float manualScrollSpeed = 10f; // How fast mouse scrolling is
    public float topCameraLimit = 0f;    
    public float bottomCameraLimit = -20f; 

    // Internal states to track what the game is currently doing
    private enum MapState { AutoScrolling, PlayingIntro1, WaitingForName, PlayingIntro2, FreeRoam }
    private MapState currentState;
    private DialogueManager dm;

    void Start()
    {
        dm = FindAnyObjectByType<DialogueManager>();
        
        // Hide the typing box immediately when the scene loads
        phase1Folder.SetActive(false); 

        string savedName = PlayerPrefs.GetString("PlayerName", "");

        if (savedName == "")
        {
            // --- IT IS THE PLAYER's FIRST TIME ---
            // 1. Force the camera to the very bottom of the map
            Vector3 startPos = mainCamera.transform.position;
            startPos.y = bottomCameraLimit;
            mainCamera.transform.position = startPos;
            
            // 2. Tell the Update loop to start scrolling up
            currentState = MapState.AutoScrolling;

            // 3. Connect the Submit button to our function
            submitNameButton.onClick.AddListener(SaveNameAndStartPart2);
        }
        else
        {
            // --- THEY ALREADY PLAYED ---
            // Put camera at the top and unlock mouse immediately
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
                // Move the camera UP automatically
                Vector3 autoPos = mainCamera.transform.position;
                autoPos.y += autoScrollSpeed * Time.deltaTime;

                if (autoPos.y >= topCameraLimit)
                {
                    // Reached the top! Stop the camera.
                    autoPos.y = topCameraLimit;
                    mainCamera.transform.position = autoPos;
                    
                    // START DIALOGUE PART 1
                    if (dm != null) 
                    {
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
                // Wait for the player to click through Dialogue Part 1
                if (dm != null && !dm.dialogueIsActive)
                {
                    // DIALOGUE 1 FINISHED! Show the Name Input box!
                    phase1Folder.SetActive(true);
                    currentState = MapState.WaitingForName;
                }
                break;

            case MapState.WaitingForName:
                // Doing nothing. Waiting for player to click Submit.
                break;

            case MapState.PlayingIntro2:
                // Wait for the player to click through Dialogue Part 2
                if (dm != null && !dm.dialogueIsActive)
                {
                    // DIALOGUE 2 FINISHED! Unlock the mouse.
                    currentState = MapState.FreeRoam;
                }
                break;

            case MapState.FreeRoam:
                // --- MANUAL MOUSE SCROLLING ---
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
        // Only run if they actually typed something
        if (nameInputField.text.Length > 0)
        {
            // Save it to memory
            PlayerPrefs.SetString("PlayerName", nameInputField.text);
            PlayerPrefs.Save();

            // Hide the Name Input UI
            phase1Folder.SetActive(false);

            // START DIALOGUE PART 2 (This is where the narrator will say "@playername")
            if (dm != null) dm.PlayDialogue("MapIntro2");
            currentState = MapState.PlayingIntro2;
        }
    }
}