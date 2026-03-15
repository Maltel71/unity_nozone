using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("Canvas")]
    [SerializeField] private Canvas pauseCanvas;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Input")]
    [SerializeField] private Key pauseKey = Key.Escape;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool _isPaused;
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

        if (pauseCanvas != null)
            pauseCanvas.gameObject.SetActive(false);
    }

    private void Start()
    {
        continueButton?.onClick.AddListener(Resume);
        restartButton?.onClick.AddListener(Restart);
        mainMenuButton?.onClick.AddListener(GoToMainMenu);
        quitButton?.onClick.AddListener(Quit);

        BuildNavigation();
    }

    private void Update()
    {
        DetectInputDevice();

        var kb = Keyboard.current;
        if (kb != null && kb[pauseKey].wasPressedThisFrame)
            TogglePause();

        var pad = Gamepad.current;
        if (pad != null && pad.startButton.wasPressedThisFrame)
            TogglePause();

        if (_isPaused)
            ManageSelection();
    }

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

        if (!_isPaused) return;

        if (_usingController)
        {
            if (EventSystem.current != null && continueButton != null)
                EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
        }
        else
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void ManageSelection()
    {
        if (EventSystem.current == null) return;

        if (_usingController)
        {
            if (EventSystem.current.currentSelectedGameObject == null && continueButton != null)
                EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
        }
        else
        {
            if (EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void TogglePause()
    {
        if (_isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (_isPaused) return;
        _isPaused = true;

        Time.timeScale = 0f;

        if (pauseCanvas != null)
            pauseCanvas.gameObject.SetActive(true);

        if (_usingController && continueButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
    }

    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused = false;

        Time.timeScale = 1f;

        if (pauseCanvas != null)
            pauseCanvas.gameObject.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void Restart()
    {
        SpeedrunTimer.Instance?.Restart();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        Debug.Log("[PauseManager] Quit called in Editor — stopping play mode.");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void BuildNavigation()
    {
        if (continueButton == null || restartButton == null ||
            mainMenuButton == null || quitButton == null) return;

        SetExplicitNav(continueButton, up: quitButton, down: restartButton);
        SetExplicitNav(restartButton, up: continueButton, down: mainMenuButton);
        SetExplicitNav(mainMenuButton, up: restartButton, down: quitButton);
        SetExplicitNav(quitButton, up: mainMenuButton, down: continueButton);
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