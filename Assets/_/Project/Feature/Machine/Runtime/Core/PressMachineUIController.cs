using UnityEngine;
using UnityEngine.UIElements;

namespace Machine.Runtime
{
    [RequireComponent(typeof(UIDocument))]
    public class PressMachineUIController : MonoBehaviour
    {
        #region Publics

        [Header("Références")]
        [SerializeField] private PressMachineController _pressMachineController;
        [SerializeField] private EmissiveLightSequence _lightSequence;

        [Header("Noms UI Toolkit")]
        [SerializeField] private string _interfaceContainerName = "machine-interface";
        [SerializeField] private string _startButtonName = "start-button";
        [SerializeField] private string _statusLabelName = "status-label";

        [Header("Textes")]
        [SerializeField] private string _readyText = "Machine prête";
        [SerializeField] private string _cycleRunningText = "Cycle en cours";

        public bool m_isInterfaceVisible =>
            _interfaceContainer != null &&
            _interfaceContainer.style.display == DisplayStyle.Flex;

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
                    $"Le conteneur '{_interfaceContainerName}' est introuvable.",
                    this);

                return;
            }

            _interfaceContainer.style.display = DisplayStyle.Flex;

            SetStartButtonEnabled(true);
            SetStatusText(_readyText);
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

            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _interfaceContainer =
                rootVisualElement.Q<VisualElement>(_interfaceContainerName);

            _startButton =
                rootVisualElement.Q<Button>(_startButtonName);

            _statusLabel =
                rootVisualElement.Q<Label>(_statusLabelName);

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
            SetStatusText(_cycleRunningText);
            HideInterface();
        }

        private void SetStatusText(string text)
        {
            if (_statusLabel == null)
            {
                return;
            }

            _statusLabel.text = text;
        }

        private void ValidateVisualElements()
        {
            if (_interfaceContainer == null)
            {
                Debug.LogError(
                    $"[{nameof(PressMachineUIController)}] " +
                    $"Le conteneur '{_interfaceContainerName}' est introuvable dans le document UXML.",
                    this);
            }

            if (_startButton == null)
            {
                Debug.LogError(
                    $"[{nameof(PressMachineUIController)}] " +
                    $"Le bouton '{_startButtonName}' est introuvable dans le document UXML.",
                    this);
            }

            if (_statusLabel == null)
            {
                Debug.LogWarning(
                    $"[{nameof(PressMachineUIController)}] " +
                    $"Le Label facultatif '{_statusLabelName}' est introuvable.",
                    this);
            }
        }

        #endregion


        #region Private and Protected

        private UIDocument _uiDocument;
        private VisualElement _interfaceContainer;
        private Button _startButton;
        private Label _statusLabel;

        #endregion
    }
}