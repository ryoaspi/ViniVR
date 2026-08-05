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
        

        private void OnDisable()
        {
            UnregisterEvents();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            _debourbagePercentage =
                Mathf.Clamp01(_debourbagePercentage);

            _reservePercentage =
                Mathf.Clamp01(_reservePercentage);

            _volumeTolerance =
                Mathf.Max(0f, _volumeTolerance);
        }

#endif

        #endregion


        #region Utils (méthodes publics)

        /// <summary>
        /// Réinitialise le suivi en utilisant les volumes actuellement
        /// présents dans les trois cuves comme nouvelles valeurs de départ.
        /// </summary>
        public void ResetAssembly()
        {
            InitializeAssembly();
        }

        /// <summary>
        /// Force une nouvelle vérification des volumes.
        /// Utile pour les tests ou les événements Unity.
        /// </summary>
        public void RefreshAssemblyState()
        {
            EvaluateAssembly();
        }
        
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
                    $"Volume nécessaire : {_requiredFinalAssemblyVolume:F2}. " +
                    $"Capacité : {_assemblyContainer.m_maximumVolume:F2}.",
                    this);

                SetAssemblyInvalid();
                return;
            }

            _currentStep = AssemblyStep.Debourbage;
            _hasRaisedInvalidEvent = false;

            Debug.Log(
                $"[{nameof(WineAssemblyController)}] " +
                "Suivi de l'assemblage initialisé. " +
                $"Débourbage attendu : {_requiredDebourbageAmount:F2}. " +
                $"Réserve attendue : {_requiredReserveAmount:F2}.",
                this);

            EvaluateAssembly();
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

        private void HandleVolumeChanged(float normalizedVolume)
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
                _requiredDebourbageAmount + _volumeTolerance)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "La quantité de débourbage transférée dépasse " +
                    $"la quantité demandée. Transféré : " +
                    $"{debourbageTransferred:F2}. Attendu : " +
                    $"{_requiredDebourbageAmount:F2}.",
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

            _currentStep = AssemblyStep.Reserve;

            Debug.Log(
                $"[{nameof(WineAssemblyController)}] " +
                "Étape de débourbage terminée. " +
                "Le joueur peut maintenant transférer le vin de réserve.",
                this);

            m_onDebourbageStepCompleted?.Invoke();
        }

        private void EvaluateReserveStep(
            float debourbageTransferred,
            float reserveTransferred,
            float assemblyReceived)
        {
            if (debourbageTransferred >
                _requiredDebourbageAmount + _volumeTolerance)
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
                _requiredReserveAmount + _volumeTolerance)
            {
                Debug.LogWarning(
                    $"[{nameof(WineAssemblyController)}] " +
                    "La quantité de vin de réserve transférée dépasse " +
                    $"la quantité demandée. Transféré : " +
                    $"{reserveTransferred:F2}. Attendu : " +
                    $"{_requiredReserveAmount:F2}.",
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

            _currentStep = AssemblyStep.Completed;

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
                initialVolume - container.m_currentVolume);
        }

        private bool IsAmountReached(
            float currentAmount,
            float requiredAmount)
        {
            return Mathf.Abs(
                currentAmount - requiredAmount) <=
                _volumeTolerance;
        }

        private void SetAssemblyInvalid()
        {
            _currentStep = AssemblyStep.Invalid;

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

            if (_debourbageContainer == null)
            {
                Debug.LogError(
                    $"[{nameof(WineAssemblyController)}] " +
                    "La cuve de débourbage n'est pas assignée.",
                    this);

                isValid = false;
            }

            if (_reserveContainer == null)
            {
                Debug.LogError(
                    $"[{nameof(WineAssemblyController)}] " +
                    "La cuve de réserve n'est pas assignée.",
                    this);

                isValid = false;
            }

            if (_assemblyContainer == null)
            {
                Debug.LogError(
                    $"[{nameof(WineAssemblyController)}] " +
                    "La cuve d'assemblage n'est pas assignée.",
                    this);

                isValid = false;
            }

            if (_debourbageContainer == _reserveContainer ||
                _debourbageContainer == _assemblyContainer ||
                _reserveContainer == _assemblyContainer)
            {
                Debug.LogError(
                    $"[{nameof(WineAssemblyController)}] " +
                    "Les trois références doivent correspondre à " +
                    "trois cuves différentes.",
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

        [Header("Tolérance")]
        [Tooltip(
            "Écart accepté entre la quantité demandée et la quantité transférée.")]
        [Min(0f)]
        [SerializeField]
        private float _volumeTolerance = 1f;

        private AssemblyStep _currentStep =
            AssemblyStep.NotInitialized;

        private float _initialDebourbageVolume;
        private float _initialReserveVolume;
        private float _initialAssemblyVolume;

        private float _requiredDebourbageAmount;
        private float _requiredReserveAmount;
        private float _requiredFinalAssemblyVolume;

        private bool _hasRaisedInvalidEvent;

        #endregion
    }
}