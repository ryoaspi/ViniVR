using UnityEngine;
using UnityEngine.UIElements;

namespace Machine.Runtime
{
    [RequireComponent(typeof(UIDocument))]
    public class PressMachineUIController : MonoBehaviour
    {
        #region Publics

        public bool m_isInterfaceVisible =>
            _interfaceContainer != null &&
            _interfaceContainer.resolvedStyle.display == DisplayStyle.Flex;

        #endregion


        #region API Unity

        private void OnEnable()
        {
            InitializeVisualElements();
            RegisterEvents();
            HideInterface();
        }

        private void OnDisable()
        {
            UnregisterEvents();
        }

        #endregion


        #region Utils

        public void ShowInterface()
        {
            if (_interfaceContainer == null)
            {
                Debug.LogWarning(
                    $"[{nameof(PressMachineUIController)}] " +
                    $"Le conteneur possédant la classe '{_interfaceContainerClass}' est introuvable.",
                    this);

                return;
            }

            _interfaceContainer.style.display = DisplayStyle.Flex;

            SetStartButtonEnabled(true);
        }

        public void HideInterface()
        {
            if (_interfaceContainer == null)
            {
                return;
            }

            _interfaceContainer.style.display = DisplayStyle.None;
        }

        public void SetStartButtonEnabled(bool isEnabled)
        {
            _startButton?.SetEnabled(isEnabled);
        }

        #endregion


        #region Main Methods

        private void InitializeVisualElements()
        {
            _uiDocument = GetComponent<UIDocument>();

            if (_uiDocument == null)
            {
                Debug.LogError(
                    $"[{nameof(PressMachineUIController)}] " +
                    "Aucun composant UI Document n'est présent.",
                    this);

                return;
            }

            VisualElement rootVisualElement =
                _uiDocument.rootVisualElement;

            if (rootVisualElement == null)
            {
                Debug.LogError(
                    $"[{nameof(PressMachineUIController)}] " +
                    "Le Root Visual Element du UI Document est introuvable.",
                    this);

                return;
            }

            _interfaceContainer =
                rootVisualElement.Q<VisualElement>(
                    className: _interfaceContainerClass);

            _startButton =
                rootVisualElement.Q<Button>(
                    className: _startButtonClass);

            ValidateVisualElements();
        }

        private void RegisterEvents()
        {
            if (_startButton != null)
            {
                _startButton.clicked += OnStartButtonClicked;
            }

            if (_lightSequence != null)
            {
                _lightSequence.m_onLightsReady.AddListener(ShowInterface);
            }

            if (_pressMachineController != null)
            {
                _pressMachineController.m_onCycleStarted.AddListener(OnCycleStarted);
                _pressMachineController.m_onMachinePoweredOff.AddListener(HideInterface);
                _pressMachineController.m_onDoorsClosed.AddListener(HideInterface);
            }
            
        }

        private void UnregisterEvents()
        {
            if (_startButton != null)
            {
                _startButton.clicked -= OnStartButtonClicked;
            }

            if (_lightSequence != null)
            {
                _lightSequence.m_onLightsReady.RemoveListener(ShowInterface);
            }

            if (_pressMachineController != null)
            {
                _pressMachineController.m_onCycleStarted.RemoveListener(OnCycleStarted);
                _pressMachineController.m_onMachinePoweredOff.RemoveListener(HideInterface);
                _pressMachineController.m_onDoorsClosed.RemoveListener(HideInterface);
            }
        }

        private void OnStartButtonClicked()
        {
            if (_pressMachineController == null)
            {
                Debug.LogError(
                    $"[{nameof(PressMachineUIController)}] " +
                    "La référence PressMachineController est manquante.",
                    this);

                return;
            }

            if (!_pressMachineController.m_canStartCycle)
            {
                Debug.LogWarning(
                    $"[{nameof(PressMachineUIController)}] " +
                    "La presse n'est pas prête à démarrer.",
                    this);

                return;
            }

            SetStartButtonEnabled(false);

            _pressMachineController.StartMachineCycle();
        }

        private void OnCycleStarted()
        {
            SetStartButtonEnabled(false);
        }

        private void ValidateVisualElements()
        {
            if (_interfaceContainer == null)
            {
                Debug.LogError(
                    $"[{nameof(PressMachineUIController)}] " +
                    $"Aucun VisualElement possédant la classe " +
                    $"'{_interfaceContainerClass}' n'a été trouvé dans le document UXML.",
                    this);
            }

            if (_startButton == null)
            {
                Debug.LogError(
                    $"[{nameof(PressMachineUIController)}] " +
                    $"Aucun Button possédant la classe " +
                    $"'{_startButtonClass}' n'a été trouvé dans le document UXML.",
                    this);
            }
        }

        #endregion


        #region Private and Protected

        [Header("Références")]
        [SerializeField] private PressMachineController _pressMachineController;
        [SerializeField] private EmissiveLightSequence _lightSequence;

        [Header("Classes UI Toolkit")]
        [SerializeField] private string _interfaceContainerClass = "btn-container";
        [SerializeField] private string _startButtonClass = "btn-item";

        private UIDocument _uiDocument;
        private VisualElement _interfaceContainer;
        private Button _startButton;

        #endregion
    }
}