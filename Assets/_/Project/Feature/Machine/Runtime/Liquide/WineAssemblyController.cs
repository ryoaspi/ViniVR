using UnityEngine;
using UnityEngine.Events;

namespace Machine.Runtime
{
    public class WineAssemblyController : MonoBehaviour
    {
        #region Publics

        [Header("Événements")]
        public UnityEvent m_onDebourbageStepCompleted;
        public UnityEvent m_onAssemblyCompleted;
        public UnityEvent m_onAssemblyInvalid;

        public bool m_isRunning =>
            _isRunning;

        public bool m_isDebourbageStepCompleted =>
            _currentStep == AssemblyStep.Reserve ||
            _currentStep == AssemblyStep.Completed;

        public bool m_isAssemblyCompleted =>
            _currentStep == AssemblyStep.Completed;

        public bool m_isAssemblyInvalid =>
            _currentStep == AssemblyStep.Invalid;

        public float m_debourbageTransferredAmount =>
            GetTransferredAmount(
                _initialDebourbageVolume,
                _debourbageContainer);

        public float m_reserveTransferredAmount =>
            GetTransferredAmount(
                _initialReserveVolume,
                _reserveContainer);

        #endregion


        #region API Unity

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }

            UpdateLiquidTransfer();
        }

        private void OnDisable()
        {
            _isRunning = false;
            UnregisterEvents();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            _debourbagePercentage =
                Mathf.Clamp01(_debourbagePercentage);

            _reservePercentage =
                Mathf.Clamp01(_reservePercentage);

            _flowRatePerSecond =
                Mathf.Max(0f, _flowRatePerSecond);

            _volumeTolerance =
                Mathf.Max(0f, _volumeTolerance);
        }

#endif

        #endregion


        #region Utils (méthodes publics)

        /// <summary>
        /// Initialise le suivi lorsque la cuve de débourbage
        /// a terminé son remplissage.
        /// </summary>
        public void BeginAssemblyTracking()
        {
            if (_currentStep != AssemblyStep.NotInitialized)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Le suivi de l'assemblage a déjà été initialisé.",
                    this);

                return;
            }

            InitializeAssembly();
        }

        /// <summary>
        /// Démarre la pompe après vérification des branchements.
        /// </summary>
        public void StartPump()
        {
            if (_isRunning)
            {
                return;
            }

            if (_currentStep == AssemblyStep.NotInitialized)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Le suivi de l'assemblage n'est pas encore initialisé. " +
                    "La cuve de débourbage doit d'abord être remplie.",
                    this);

                return;
            }

            if (_currentStep == AssemblyStep.Completed)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "L'assemblage est déjà terminé.",
                    this);

                return;
            }

            if (_currentStep == AssemblyStep.Invalid)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "L'assemblage est dans un état invalide.",
                    this);

                return;
            }

            if (!ValidateReferences())
            {
                return;
            }

            if (!TryResolveConnectedContainers(
                    out LiquidContainer source,
                    out LiquidContainer destination))
            {
                return;
            }

            if (!ValidateConnectedContainers(
                    source,
                    destination))
            {
                return;
            }

            _activeSource = source;
            _isRunning = true;

            Debug.Log(
                $"[{nameof(WineAssemblyController)}] " +
                $"Pompe démarrée. Source : " +
                $"{_activeSource.m_containerName} | " +
                $"Destination : {_assemblyContainer.m_containerName}.",
                this);
        }

        /// <summary>
        /// Arrête manuellement le transfert.
        /// </summary>
        public void StopPump()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            _activeSource = null;

            Debug.Log(
                $"[{nameof(WineAssemblyController)}] " +
                "Pompe arrêtée.",
                this);
        }

        /// <summary>
        /// Réinitialise le suivi à partir des volumes actuels.
        /// </summary>
        public void ResetAssembly()
        {
            _isRunning = false;
            _activeSource = null;

            _currentStep =
                AssemblyStep.NotInitialized;

            InitializeAssembly();
        }

        /// <summary>
        /// Force une nouvelle vérification des volumes.
        /// </summary>
        public void RefreshAssemblyState()
        {
            EvaluateAssembly();
        }

        /// <summary>
        /// Affiche les cuves actuellement détectées
        /// à travers les deux tuyaux.
        /// </summary>
        public void LogCurrentConnections()
        {
            if (!TryResolveConnectedContainers(
                    out LiquidContainer source,
                    out LiquidContainer destination))
            {
                return;
            }

            Debug.Log(
                $"[{nameof(WineAssemblyController)}] " +
                $"Source connectée : {source.m_containerName} | " +
                $"Destination connectée : {destination.m_containerName}.",
                this);
        }

        #endregion


        #region Main Methods (méthodes private)

        private void InitializeAssembly()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            _initialDebourbageVolume =
                _debourbageContainer.m_currentVolume;

            _initialReserveVolume =
                _reserveContainer.m_currentVolume;

            _initialAssemblyVolume =
                _assemblyContainer.m_currentVolume;

            _requiredDebourbageAmount =
                _initialDebourbageVolume *
                _debourbagePercentage;

            _requiredReserveAmount =
                _initialReserveVolume *
                _reservePercentage;

            _requiredFinalAssemblyVolume =
                _initialAssemblyVolume +
                _requiredDebourbageAmount +
                _requiredReserveAmount;

            if (_requiredFinalAssemblyVolume >
                _assemblyContainer.m_maximumVolume +
                _volumeTolerance)
            {
                Debug.LogError(
                    $"[{nameof(WineAssemblyController)}] " +
                    "La cuve d'assemblage ne possède pas une capacité " +
                    "suffisante pour recevoir les quantités demandées. " +
                    $"Volume nécessaire : " +
                    $"{_requiredFinalAssemblyVolume:F2}. " +
                    $"Capacité disponible : " +
                    $"{_assemblyContainer.m_maximumVolume:F2}.",
                    this);

                SetAssemblyInvalid();
                return;
            }

            _currentStep =
                AssemblyStep.Debourbage;

            _activeSource = null;
            _isRunning = false;
            _hasRaisedInvalidEvent = false;

            Debug.Log(
                $"[{nameof(WineAssemblyController)}] " +
                "Suivi de l'assemblage initialisé. " +
                $"Débourbage initial : " +
                $"{_initialDebourbageVolume:F2}. " +
                $"Débourbage attendu : " +
                $"{_requiredDebourbageAmount:F2}. " +
                $"Réserve attendue : " +
                $"{_requiredReserveAmount:F2}.",
                this);

            EvaluateAssembly();
        }

        private void UpdateLiquidTransfer()
        {
            if (!TryResolveConnectedContainers(
                    out LiquidContainer source,
                    out LiquidContainer destination))
            {
                StopPump();
                return;
            }

            if (source != _activeSource ||
                destination != _assemblyContainer)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Les branchements ont changé pendant le transfert.",
                    this);

                StopPump();
                return;
            }

            if (!ValidateConnectedContainers(
                    source,
                    destination))
            {
                StopPump();
                return;
            }

            float remainingAmount =
                GetRemainingAmountForCurrentStep();

            if (remainingAmount <= _volumeTolerance)
            {
                StopPump();
                EvaluateAssembly();
                return;
            }

            if (source.m_isEmpty)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    $"La cuve source {source.m_containerName} est vide.",
                    source);

                StopPump();
                return;
            }

            if (destination.m_isFull)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "La cuve d'assemblage est pleine.",
                    destination);

                StopPump();
                return;
            }

            float requestedAmount =
                _flowRatePerSecond *
                Time.deltaTime;

            float destinationCapacity =
                destination.m_maximumVolume -
                destination.m_currentVolume;

            float transferableAmount =
                Mathf.Min(
                    requestedAmount,
                    remainingAmount,
                    destinationCapacity);

            if (transferableAmount <= 0f)
            {
                StopPump();
                return;
            }

            float removedAmount =
                source.RemoveLiquid(
                    transferableAmount);

            if (removedAmount <= 0f)
            {
                StopPump();
                return;
            }

            float addedAmount =
                destination.AddLiquid(
                    removedAmount);

            if (addedAmount < removedAmount)
            {
                float amountToReturn =
                    removedAmount - addedAmount;

                source.AddLiquid(
                    amountToReturn);
            }

            if (GetRemainingAmountForCurrentStep() <=
                _volumeTolerance)
            {
                _isRunning = false;
                _activeSource = null;

                EvaluateAssembly();
            }
        }

        private bool TryResolveConnectedContainers(
            out LiquidContainer source,
            out LiquidContainer destination)
        {
            source = null;
            destination = null;

            if (_inputSocket == null ||
                _outputSocket == null)
            {
                Debug.LogError(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Les sockets IN et OUT ne sont pas assignés.",
                    this);

                return false;
            }

            if (!_inputSocket.m_isConnected ||
                !_outputSocket.m_isConnected)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Les deux tuyaux doivent être branchés sur la pompe.",
                    this);

                return false;
            }

            LiquidHoseEnd inputTankEnd =
                ResolveOppositeHoseEnd(
                    _inputSocket.m_connectedHoseEnd,
                    _inputHoseEndA,
                    _inputHoseEndB);

            LiquidHoseEnd outputTankEnd =
                ResolveOppositeHoseEnd(
                    _outputSocket.m_connectedHoseEnd,
                    _outputHoseEndA,
                    _outputHoseEndB);

            if (inputTankEnd == null)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "L'extrémité connectée au socket IN " +
                    "ne correspond pas au tuyau d'entrée assigné.",
                    this);

                return false;
            }

            if (outputTankEnd == null)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "L'extrémité connectée au socket OUT " +
                    "ne correspond pas au tuyau de sortie assigné.",
                    this);

                return false;
            }

            source =
                inputTankEnd.m_connectedContainer;

            destination =
                outputTankEnd.m_connectedContainer;

            if (source == null)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "L'autre extrémité du tuyau d'entrée " +
                    "n'est branchée à aucune cuve.",
                    this);

                return false;
            }

            if (destination == null)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "L'autre extrémité du tuyau de sortie " +
                    "n'est branchée à aucune cuve.",
                    this);

                return false;
            }

            return true;
        }

        private LiquidHoseEnd ResolveOppositeHoseEnd(
            LiquidHoseEnd connectedPumpEnd,
            LiquidHoseEnd hoseEndA,
            LiquidHoseEnd hoseEndB)
        {
            if (connectedPumpEnd == null ||
                hoseEndA == null ||
                hoseEndB == null)
            {
                return null;
            }

            if (connectedPumpEnd == hoseEndA)
            {
                return hoseEndB;
            }

            if (connectedPumpEnd == hoseEndB)
            {
                return hoseEndA;
            }

            return null;
        }

        private bool ValidateConnectedContainers(
            LiquidContainer source,
            LiquidContainer destination)
        {
            if (destination != _assemblyContainer)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Le tuyau branché sur OUT doit être relié " +
                    "à la cuve d'assemblage.",
                    this);

                return false;
            }

            if (_currentStep == AssemblyStep.Debourbage)
            {
                if (source != _debourbageContainer)
                {
                    Debug.LogWarning(
                        $"[{nameof(WineAssemblyController)}] " +
                        "Pendant la première étape, le tuyau IN doit être " +
                        "relié à la cuve de débourbage.",
                        this);

                    return false;
                }

                return true;
            }

            if (_currentStep == AssemblyStep.Reserve)
            {
                if (source != _reserveContainer)
                {
                    Debug.LogWarning(
                        $"[{nameof(WineAssemblyController)}] " +
                        "Pendant la seconde étape, le tuyau IN doit être " +
                        "relié à la cuve de réserve.",
                        this);

                    return false;
                }

                return true;
            }

            return false;
        }

        private float GetRemainingAmountForCurrentStep()
        {
            if (_currentStep == AssemblyStep.Debourbage)
            {
                return Mathf.Max(
                    0f,
                    _requiredDebourbageAmount -
                    m_debourbageTransferredAmount);
            }

            if (_currentStep == AssemblyStep.Reserve)
            {
                return Mathf.Max(
                    0f,
                    _requiredReserveAmount -
                    m_reserveTransferredAmount);
            }

            return 0f;
        }

        private void RegisterEvents()
        {
            if (_debourbageContainer != null)
            {
                _debourbageContainer.m_onVolumeChanged.AddListener(
                    HandleVolumeChanged);
            }

            if (_reserveContainer != null)
            {
                _reserveContainer.m_onVolumeChanged.AddListener(
                    HandleVolumeChanged);
            }

            if (_assemblyContainer != null)
            {
                _assemblyContainer.m_onVolumeChanged.AddListener(
                    HandleVolumeChanged);
            }
        }

        private void UnregisterEvents()
        {
            if (_debourbageContainer != null)
            {
                _debourbageContainer.m_onVolumeChanged.RemoveListener(
                    HandleVolumeChanged);
            }

            if (_reserveContainer != null)
            {
                _reserveContainer.m_onVolumeChanged.RemoveListener(
                    HandleVolumeChanged);
            }

            if (_assemblyContainer != null)
            {
                _assemblyContainer.m_onVolumeChanged.RemoveListener(
                    HandleVolumeChanged);
            }
        }

        private void HandleVolumeChanged(
            float normalizedVolume)
        {
            EvaluateAssembly();
        }

        private void EvaluateAssembly()
        {
            if (_currentStep == AssemblyStep.NotInitialized ||
                _currentStep == AssemblyStep.Completed ||
                _currentStep == AssemblyStep.Invalid)
            {
                return;
            }

            float debourbageTransferred =
                m_debourbageTransferredAmount;

            float reserveTransferred =
                m_reserveTransferredAmount;

            float assemblyReceived =
                Mathf.Max(
                    0f,
                    _assemblyContainer.m_currentVolume -
                    _initialAssemblyVolume);

            if (_currentStep == AssemblyStep.Debourbage)
            {
                EvaluateDebourbageStep(
                    debourbageTransferred,
                    reserveTransferred,
                    assemblyReceived);

                return;
            }

            if (_currentStep == AssemblyStep.Reserve)
            {
                EvaluateReserveStep(
                    debourbageTransferred,
                    reserveTransferred,
                    assemblyReceived);
            }
        }

        private void EvaluateDebourbageStep(
            float debourbageTransferred,
            float reserveTransferred,
            float assemblyReceived)
        {
            if (reserveTransferred > _volumeTolerance)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Du vin de réserve a été transféré avant la fin " +
                    "de l'étape de débourbage.",
                    this);

                SetAssemblyInvalid();
                return;
            }

            if (debourbageTransferred >
                _requiredDebourbageAmount +
                _volumeTolerance)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "La quantité de débourbage transférée dépasse " +
                    "la quantité demandée.",
                    this);

                SetAssemblyInvalid();
                return;
            }

            bool hasTransferredRequiredAmount =
                IsAmountReached(
                    debourbageTransferred,
                    _requiredDebourbageAmount);

            bool hasAssemblyReceivedRequiredAmount =
                assemblyReceived >=
                _requiredDebourbageAmount -
                _volumeTolerance;

            if (!hasTransferredRequiredAmount ||
                !hasAssemblyReceivedRequiredAmount)
            {
                return;
            }

            _isRunning = false;
            _activeSource = null;

            _currentStep =
                AssemblyStep.Reserve;

            Debug.Log(
                $"[{nameof(WineAssemblyController)}] " +
                "Étape de débourbage terminée. " +
                "Branche maintenant la cuve de réserve sur IN.",
                this);

            m_onDebourbageStepCompleted?.Invoke();
        }

        private void EvaluateReserveStep(
            float debourbageTransferred,
            float reserveTransferred,
            float assemblyReceived)
        {
            if (debourbageTransferred >
                _requiredDebourbageAmount +
                _volumeTolerance)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Du débourbage supplémentaire a été transféré " +
                    "pendant l'étape de réserve.",
                    this);

                SetAssemblyInvalid();
                return;
            }

            if (reserveTransferred >
                _requiredReserveAmount +
                _volumeTolerance)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "La quantité de réserve transférée dépasse " +
                    "la quantité demandée.",
                    this);

                SetAssemblyInvalid();
                return;
            }

            float requiredTotalAmount =
                _requiredDebourbageAmount +
                _requiredReserveAmount;

            bool hasCorrectDebourbageAmount =
                IsAmountReached(
                    debourbageTransferred,
                    _requiredDebourbageAmount);

            bool hasCorrectReserveAmount =
                IsAmountReached(
                    reserveTransferred,
                    _requiredReserveAmount);

            bool hasAssemblyReceivedEverything =
                assemblyReceived >=
                requiredTotalAmount -
                _volumeTolerance;

            if (!hasCorrectDebourbageAmount ||
                !hasCorrectReserveAmount ||
                !hasAssemblyReceivedEverything)
            {
                return;
            }

            _isRunning = false;
            _activeSource = null;

            _currentStep =
                AssemblyStep.Completed;

            Debug.Log(
                $"[{nameof(WineAssemblyController)}] " +
                "Assemblage terminé avec succès : " +
                $"{_debourbagePercentage * 100f:F0} % de débourbage et " +
                $"{_reservePercentage * 100f:F0} % de réserve.",
                this);

            m_onAssemblyCompleted?.Invoke();
        }

        private float GetTransferredAmount(
            float initialVolume,
            LiquidContainer container)
        {
            if (container == null)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                initialVolume -
                container.m_currentVolume);
        }

        private bool IsAmountReached(
            float currentAmount,
            float requiredAmount)
        {
            return Mathf.Abs(
                currentAmount -
                requiredAmount) <=
                _volumeTolerance;
        }

        private void SetAssemblyInvalid()
        {
            _isRunning = false;
            _activeSource = null;

            _currentStep =
                AssemblyStep.Invalid;

            if (_hasRaisedInvalidEvent)
            {
                return;
            }

            _hasRaisedInvalidEvent = true;

            m_onAssemblyInvalid?.Invoke();
        }

        private bool ValidateReferences()
        {
            bool isValid = true;

            if (_inputSocket == null ||
                _outputSocket == null)
            {
                Debug.LogError(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Les sockets IN et OUT ne sont pas assignés.",
                    this);

                isValid = false;
            }

            if (_inputHoseEndA == null ||
                _inputHoseEndB == null)
            {
                Debug.LogError(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Les deux extrémités du tuyau d'entrée " +
                    "ne sont pas assignées.",
                    this);

                isValid = false;
            }

            if (_outputHoseEndA == null ||
                _outputHoseEndB == null)
            {
                Debug.LogError(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Les deux extrémités du tuyau de sortie " +
                    "ne sont pas assignées.",
                    this);

                isValid = false;
            }

            if (_debourbageContainer == null ||
                _reserveContainer == null ||
                _assemblyContainer == null)
            {
                Debug.LogError(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Les trois cuves ne sont pas assignées.",
                    this);

                isValid = false;
            }

            if (_debourbageContainer == _reserveContainer ||
                _debourbageContainer == _assemblyContainer ||
                _reserveContainer == _assemblyContainer)
            {
                Debug.LogError(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Les trois références doivent correspondre " +
                    "à trois cuves différentes.",
                    this);

                isValid = false;
            }

            return isValid;
        }

        #endregion


        #region Private and Protected

        private enum AssemblyStep
        {
            NotInitialized,
            Debourbage,
            Reserve,
            Completed,
            Invalid
        }

        [Header("Sockets de la pompe")]
        [SerializeField]
        private PumpSocketConnection _inputSocket;

        [SerializeField]
        private PumpSocketConnection _outputSocket;

        [Header("Tuyau branché sur IN")]
        [Tooltip(
            "Première extrémité du tuyau utilisé sur l'entrée de la pompe.")]
        [SerializeField]
        private LiquidHoseEnd _inputHoseEndA;

        [Tooltip(
            "Seconde extrémité du tuyau utilisé sur l'entrée de la pompe.")]
        [SerializeField]
        private LiquidHoseEnd _inputHoseEndB;

        [Header("Tuyau branché sur OUT")]
        [Tooltip(
            "Première extrémité du tuyau utilisé sur la sortie de la pompe.")]
        [SerializeField]
        private LiquidHoseEnd _outputHoseEndA;

        [Tooltip(
            "Seconde extrémité du tuyau utilisé sur la sortie de la pompe.")]
        [SerializeField]
        private LiquidHoseEnd _outputHoseEndB;

        [Header("Cuves")]
        [SerializeField]
        private LiquidContainer _debourbageContainer;

        [SerializeField]
        private LiquidContainer _reserveContainer;

        [SerializeField]
        private LiquidContainer _assemblyContainer;

        [Header("Proportions")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _debourbagePercentage = 0.70f;

        [Range(0f, 1f)]
        [SerializeField]
        private float _reservePercentage = 0.30f;

        [Header("Pompe")]
        [Min(0f)]
        [SerializeField]
        private float _flowRatePerSecond = 10f;

        [Header("Tolérance")]
        [Tooltip(
            "Écart accepté entre la quantité demandée " +
            "et la quantité transférée.")]
        [Min(0f)]
        [SerializeField]
        private float _volumeTolerance = 0.1f;

        private AssemblyStep _currentStep =
            AssemblyStep.NotInitialized;

        private LiquidContainer _activeSource;

        private float _initialDebourbageVolume;
        private float _initialReserveVolume;
        private float _initialAssemblyVolume;

        private float _requiredDebourbageAmount;
        private float _requiredReserveAmount;
        private float _requiredFinalAssemblyVolume;

        private bool _isRunning;
        private bool _hasRaisedInvalidEvent;

        #endregion
    }
}