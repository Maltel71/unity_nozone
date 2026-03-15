using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    [Header("Input")]
    [SerializeField] private Key startKey = Key.Enter;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    private bool _usingController;
    private Vector2 _lastMousePosition;

    private void Start()
    {
        startButton?.onClick.AddListener(StartGame);
        exitButton?.onClick.AddListener(Exit);

        BuildNavigation();
    }

    private void Update()
    {
        DetectInputDevice();
        ManageSelection();

        var kb = Keyboard.current;
        if (kb != null && kb[startKey].wasPressedThisFrame)
            StartGame();
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
                pad.startButton.isPressed)
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

        if (_usingController)
        {
            if (EventSystem.current != null && startButton != null)
                EventSystem.current.SetSelectedGameObject(startButton.gameObject);
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
            if (EventSystem.current.currentSelectedGameObject == null && startButton != null)
                EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
        else
        {
            if (EventSystem.current.currentSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        Debug.Log("[MainMenuManager] Exit called in Editor — stopping play mode.");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Explicit wrapping navigation: Start → Exit → (wraps back to) Start
    /// </summary>
    private void BuildNavigation()
    {
        if (startButton == null || exitButton == null) return;

        SetExplicitNav(startButton, up: exitButton, down: exitButton);
        SetExplicitNav(exitButton, up: startButton, down: startButton);
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