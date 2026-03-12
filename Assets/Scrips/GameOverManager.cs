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

    private void Update()
    {
        if (!_isGameOver) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb[restartKey].wasPressedThisFrame)
            Retry();
        else if (kb[mainMenuKey].wasPressedThisFrame)
            GoToMainMenu();
    }

    public void ShowGameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        if (gameplayCanvas != null)
            gameplayCanvas.gameObject.SetActive(false);

        if (deathCanvas != null)
            deathCanvas.gameObject.SetActive(true);

        // Select the restart button so controller navigation starts at the top
        if (restartButton != null && EventSystem.current != null)
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
}