using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuUIController : MonoBehaviour
{
    #region Publics

    public bool m_isInitialized =>
        _root != null &&
        _startButton != null;

    #endregion


    #region API Unity

    private void OnEnable()
    {
        InitializeUI();
        RegisterEvents();
    }

    private void OnDisable()
    {
        UnregisterEvents();
    }

    #endregion


    #region Utils

    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(_gameSceneName))
        {
            Debug.LogError(
                $"[{nameof(MainMenuUIController)}] " +
                "Aucun nom de scène n'est configuré.",
                this);

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(_gameSceneName))
        {
            Debug.LogError(
                $"[{nameof(MainMenuUIController)}] " +
                $"La scène '{_gameSceneName}' est introuvable. " +
                "Vérifie son nom et son ajout dans les Build Profiles.",
                this);

            return;
        }

        Debug.Log(
            $"[{nameof(MainMenuUIController)}] " +
            $"Chargement de la scène '{_gameSceneName}'.",
            this);

        SceneManager.LoadScene(_gameSceneName);
    }

    public void OpenCredits()
    {
        if (_creditsContainer == null)
        {
            Debug.LogWarning(
                $"[{nameof(MainMenuUIController)}] " +
                $"Le conteneur de crédits '{_creditsContainerName}' " +
                "n'existe pas dans le document UXML.",
                this);

            return;
        }

        _creditsContainer.style.display = DisplayStyle.Flex;
    }

    public void CloseCredits()
    {
        if (_creditsContainer == null)
        {
            return;
        }

        _creditsContainer.style.display = DisplayStyle.None;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion


    #region Main Methods

    private void InitializeUI()
    {
        _uiDocument = GetComponent<UIDocument>();

        if (_uiDocument == null)
        {
            Debug.LogError(
                $"[{nameof(MainMenuUIController)}] " +
                "Aucun UIDocument n'est présent sur ce GameObject.",
                this);

            return;
        }

        _root = _uiDocument.rootVisualElement;

        if (_root == null)
        {
            Debug.LogError(
                $"[{nameof(MainMenuUIController)}] " +
                "Le rootVisualElement du UIDocument est introuvable.",
                this);

            return;
        }

        _startButton = _root.Q<Button>(_startButtonName);
        _creditsButton = _root.Q<Button>(_creditsButtonName);
        _quitButton = _root.Q<Button>(_quitButtonName);

        _creditsContainer =
            _root.Q<VisualElement>(_creditsContainerName);

        _closeCreditsButton =
            _root.Q<Button>(_closeCreditsButtonName);

        ValidateVisualElements();
        CloseCredits();
    }

    private void RegisterEvents()
    {
        if (_startButton != null)
        {
            _startButton.clicked += StartGame;

            _startButton.RegisterCallback<PointerEnterEvent>(
                OnStartPointerEntered);

            _startButton.RegisterCallback<PointerDownEvent>(
                OnStartPointerDown);
        }

        if (_creditsButton != null)
        {
            _creditsButton.clicked += OpenCredits;
        }

        if (_quitButton != null)
        {
            _quitButton.clicked += QuitGame;
        }

        if (_closeCreditsButton != null)
        {
            _closeCreditsButton.clicked += CloseCredits;
        }
    }

    private void UnregisterEvents()
    {
        if (_startButton != null)
        {
            _startButton.clicked -= StartGame;

            _startButton.UnregisterCallback<PointerEnterEvent>(
                OnStartPointerEntered);

            _startButton.UnregisterCallback<PointerDownEvent>(
                OnStartPointerDown);
        }

        if (_creditsButton != null)
        {
            _creditsButton.clicked -= OpenCredits;
        }

        if (_quitButton != null)
        {
            _quitButton.clicked -= QuitGame;
        }

        if (_closeCreditsButton != null)
        {
            _closeCreditsButton.clicked -= CloseCredits;
        }
    }

    private void ValidateVisualElements()
    {
        if (_startButton == null)
        {
            Debug.LogError(
                $"[{nameof(MainMenuUIController)}] " +
                $"Bouton Start introuvable : '{_startButtonName}'.",
                this);
        }

        if (_creditsButton == null)
        {
            Debug.LogError(
                $"[{nameof(MainMenuUIController)}] " +
                $"Bouton Credits introuvable : '{_creditsButtonName}'.",
                this);
        }

        if (_quitButton == null)
        {
            Debug.LogError(
                $"[{nameof(MainMenuUIController)}] " +
                $"Bouton Quit introuvable : '{_quitButtonName}'.",
                this);
        }

        if (_creditsContainer == null)
        {
            Debug.LogWarning(
                $"[{nameof(MainMenuUIController)}] " +
                $"Panneau de crédits optionnel introuvable : " +
                $"'{_creditsContainerName}'.",
                this);
        }

        if (_closeCreditsButton == null)
        {
            Debug.LogWarning(
                $"[{nameof(MainMenuUIController)}] " +
                $"Bouton de fermeture des crédits optionnel introuvable : " +
                $"'{_closeCreditsButtonName}'.",
                this);
        }
    }
    
    private void OnStartPointerEntered(PointerEnterEvent evt)
    {
        Debug.Log(
            $"[{nameof(MainMenuUIController)}] " +
            "Le pointeur XR survole le bouton Start.",
            this);
    }

    private void OnStartPointerDown(PointerDownEvent evt)
    {
        Debug.Log(
            $"[{nameof(MainMenuUIController)}] " +
            "Le bouton Start reçoit un PointerDown.",
            this);
    }

    #endregion


    #region Private and Protected

    [Header("Scene")]
    [SerializeField] private string _gameSceneName = "Game";

    [Header("UI Names")]
    [SerializeField]
    private string _startButtonName = "start_menu-btn_start";

    [SerializeField]
    private string _creditsButtonName = "start_menu-btn_credits";

    [SerializeField]
    private string _quitButtonName = "start_menu-btn_quit";

    [SerializeField]
    private string _creditsContainerName = "CreditsPanel";

    [SerializeField]
    private string _closeCreditsButtonName = "CloseCreditsButton";

    private UIDocument _uiDocument;
    private VisualElement _root;

    private Button _startButton;
    private Button _creditsButton;
    private Button _quitButton;
    private Button _closeCreditsButton;

    private VisualElement _creditsContainer;

    #endregion
}