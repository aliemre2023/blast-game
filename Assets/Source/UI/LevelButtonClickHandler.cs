using UnityEngine;
using UnityEngine.UI;

public class LevelButtonClickHandler : MonoBehaviour
{
    public Button levelButton; // Button to start the game
    public Canvas mainScreen;  // Main menu canvas
    public Canvas gameScreen;  // Game screen canvas

    private GameInitalizer gameInitalizer; // Reference to the GameInitalizer script

    void Start()
    {
        // Ensure the level button is assigned
        if (levelButton == null)
        {
            Debug.LogError("LevelButton is not assigned in the Inspector!");
            return;
        }

        // Add a listener to the button
        levelButton.onClick.AddListener(OpenCanvas);

        // Find the GameInitalizer in the scene
        gameInitalizer = FindObjectOfType<GameInitalizer>();
        if (gameInitalizer == null)
        {
            Debug.LogError("GameInitalizer not found in the scene!");
            return;
        }

        // Set the game screen to inactive initially
        if (gameScreen != null)
        {
            gameScreen.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("GameScreen is not assigned in the Inspector!");
        }
    }

    void OpenCanvas()
    {
        // Ensure the canvases are assigned
        if (mainScreen == null || gameScreen == null)
        {
            Debug.LogError("MainScreen or GameScreen is not assigned in the Inspector!");
            return;
        }

        // Switch canvases
        mainScreen.gameObject.SetActive(false);
        gameScreen.gameObject.SetActive(true);

        // Initialize the game
        if (gameInitalizer != null)
        {
            gameInitalizer.InitializeGame(); // Call the InitializeGame method
        }
        else
        {
            Debug.LogError("GameInitalizer is not assigned or found!");
        }
    }
}