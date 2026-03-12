using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Canvases")]
    [SerializeField] private Canvas gameplayCanvas;
    [SerializeField] private Canvas deathCanvas;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Keyboard Shortcuts")]
    [SerializeField] private Key restartKey = Key.R;
    [SerializeField] private Key mainMenuKey = Key.M;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool _isGameOver;
    private bool _usingController;
    private Vector2 _lastMousePosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (deathCanvas != null)
            deathCanvas.gameObject.SetActive(false);
    }

    private void Start()
    {
        restartButton?.onClick.AddListener(Retry);
        mainMenuButton?.onClick.AddListener(GoToMainMenu);
        quitButton?.onClick.AddListener(Quit);

        BuildNavigation();
    }

    private void Update()
    {
        if (!_isGameOver) return;

        DetectInputDevice();
        ManageSelection();

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb[restartKey].wasPressedThisFrame)
            Retry();
        else if (kb[mainMenuKey].wasPressedThisFrame)
            GoToMainMenu();
    }

    /// <summary>
    /// Switches the active input mode between controller and mouse/keyboard
    /// based on whichever device produced input most recently.
    /// </summary>
    private void DetectInputDevice()
    {
        var pad = Gamepad.current;
        if (pad != null)
        {
            if (pad.leftStick.ReadValue().sqrMagnitude > 0.01f ||
                pad.dpad.ReadValue().sqrMagnitude > 0.01f ||
                pad.buttonSouth.isPressed || pad.buttonNorth.isPressed ||
                pad.buttonEast.isPressed || pad.buttonWest.isPressed ||
                pad.startButton.isPressed || pad.selectButton.isPressed)
            {
                SetControllerMode(true);
                return;
            }
        }

        var mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 mousePos = mouse.position.ReadValue();
            if (mousePos != _lastMousePosition || mouse.leftButton.isPressed)
            {
                _lastMousePosition = mousePos;
                SetControllerMode(false);
            }
        }
    }

    private void SetControllerMode(bool controller)
    {
        if (_usingController == controller) return;
        _usingController = controller;

        if (!_isGameOver) return;

        if (_usingController)
        {
            if (EventSystem.current != null && restartButton != null)
                EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
        }
        else
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }

    /// <summary>
    /// While the death screen is up, keep selection cleared for mouse users
    /// and ensure a button is always selected for controller users.
    /// </summary>
    private void ManageSelection()
    {
        if (EventSystem.current == null) return;

        if (_usingController)
        {
            if (EventSystem.current.currentSelectedGameObject == null && restartButton != null)
                EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
        }
        else
        {
            if (EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void ShowGameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        if (gameplayCanvas != null)
            gameplayCanvas.gameObject.SetActive(false);

        if (deathCanvas != null)
            deathCanvas.gameObject.SetActive(true);

        // Only auto-select when already using a controller
        if (_usingController && restartButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        Debug.Log("[GameOverManager] Quit called in Editor — stopping play mode.");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Sets up explicit wrapping navigation so the controller can cycle
    /// through all three buttons in both directions without dead ends.
    /// Order: Restart → MainMenu → Quit → (wraps back to) Restart
    /// </summary>
    private void BuildNavigation()
    {
        if (restartButton == null || mainMenuButton == null || quitButton == null) return;

        SetExplicitNav(restartButton, up: quitButton, down: mainMenuButton);
        SetExplicitNav(mainMenuButton, up: restartButton, down: quitButton);
        SetExplicitNav(quitButton, up: mainMenuButton, down: restartButton);
    }

    private static void SetExplicitNav(Button button, Button up, Button down)
    {
        Navigation nav = button.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnUp = up;
        nav.selectOnDown = down;
        button.navigation = nav;
    }
}