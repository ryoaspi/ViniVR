using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

    [RequireComponent(typeof(UIDocument))]
    public class MainMenuUIController : MonoBehaviour
    {
        #region Publics

        public bool m_isInitialized => _root != null;

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
                    $"[{nameof(MainMenuUIController)}] Aucun nom de scène n'est configuré.",
                    this);

                return;
            }

            SceneManager.LoadScene(_gameSceneName);
        }

        public void OpenCredits()
        {
            if (_creditsContainer == null)
            {
                Debug.LogWarning(
                    $"[{nameof(MainMenuUIController)}] Impossible d'ouvrir les crédits.",
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
            _root = _uiDocument.rootVisualElement;

            _startButton = _root.Q<Button>(_startButtonName);
            _creditsButton = _root.Q<Button>(_creditsButtonName);
            _quitButton = _root.Q<Button>(_quitButtonName);

            _creditsContainer = _root.Q<VisualElement>(_creditsContainerName);
            _closeCreditsButton = _root.Q<Button>(_closeCreditsButtonName);

            CloseCredits();
        }

        private void RegisterEvents()
        {
            _startButton?.RegisterCallback<ClickEvent>(OnStartClicked);
            _creditsButton?.RegisterCallback<ClickEvent>(OnCreditsClicked);
            _quitButton?.RegisterCallback<ClickEvent>(OnQuitClicked);
            _closeCreditsButton?.RegisterCallback<ClickEvent>(OnCloseCreditsClicked);
        }

        private void UnregisterEvents()
        {
            _startButton?.UnregisterCallback<ClickEvent>(OnStartClicked);
            _creditsButton?.UnregisterCallback<ClickEvent>(OnCreditsClicked);
            _quitButton?.UnregisterCallback<ClickEvent>(OnQuitClicked);
            _closeCreditsButton?.UnregisterCallback<ClickEvent>(OnCloseCreditsClicked);
        }

        private void OnStartClicked(ClickEvent evt)
        {
            StartGame();
        }

        private void OnCreditsClicked(ClickEvent evt)
        {
            OpenCredits();
        }

        private void OnCloseCreditsClicked(ClickEvent evt)
        {
            CloseCredits();
        }

        private void OnQuitClicked(ClickEvent evt)
        {
            QuitGame();
        }

        #endregion


        #region Private and Protected

        [Header("Scene")]
        [SerializeField] private string _gameSceneName = "Game";

        [Header("UI Names")]
        [SerializeField] private string _startButtonName = "StartButton";
        [SerializeField] private string _creditsButtonName = "CreditsButton";
        [SerializeField] private string _quitButtonName = "QuitButton";
        [SerializeField] private string _creditsContainerName = "CreditsPanel";
        [SerializeField] private string _closeCreditsButtonName = "CloseCreditsButton";

        private UIDocument _uiDocument;
        private VisualElement _root;

        private Button _startButton;
        private Button _creditsButton;
        private Button _quitButton;
        private Button _closeCreditsButton;

        private VisualElement _creditsContainer;

        #endregion
    }
