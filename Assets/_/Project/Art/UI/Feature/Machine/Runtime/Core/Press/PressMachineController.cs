using UnityEngine;
using UnityEngine.Events;

namespace Machine.Runtime
{
    public class PressMachineController : MonoBehaviour
    {
        #region Publics

        [Header("Événements d'alimentation")]
        public UnityEvent m_onMachinePoweredOn;
        public UnityEvent m_onMachinePoweredOff;
        public UnityEvent m_onMachineReady;

        [Header("Événements du cycle")]
        public UnityEvent m_onCycleStarted;
        public UnityEvent m_onDoorsClosed;
        public UnityEvent m_onPressStarted;
        public UnityEvent m_onPressFinished;
        public UnityEvent m_onDoorsOpened;
        public UnityEvent m_onFunnelRemoved;
        public UnityEvent m_onCycleCompleted;

        public bool m_isMachinePoweredOn => _isMachinePoweredOn;

        public bool m_isCycleRunning =>
            _currentState != PressMachineState.Ready &&
            _currentState != PressMachineState.Completed;

        public bool m_canStartCycle =>
            _isMachinePoweredOn &&
            _currentState == PressMachineState.Ready &&
            _animator != null;

        #endregion


        #region API Unity

        private void Awake()
        {
            ValidateReferences();
            InitializeMachineState();
        }

        private void OnEnable()
        {
            if (_funnelSocket != null)
            {
                _funnelSocket.m_onSocketEmptied.AddListener(OnFunnelRemoved);
            }
        }

        private void OnDisable()
        {
            if (_funnelSocket != null)
            {
                _funnelSocket.m_onSocketEmptied.RemoveListener(OnFunnelRemoved);
            }
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            _requiredRotationCount = Mathf.Max(1, _requiredRotationCount);
        }

#endif

        #endregion


        #region Utils

        /// <summary>
        /// Active ou désactive la machine.
        /// Cette méthode doit être appelée par le bouton physique.
        /// </summary>
        public void ToggleMachinePower()
        {
            Debug.Log(
                $"[{nameof(PressMachineController)}] ToggleMachinePower appelé.",
                this);

            if (_isMachinePoweredOn)
            {
                PowerOffMachine();
                return;
            }

            PowerOnMachine();
        }

        public void PowerOnMachine()
        {
            
            Debug.Log(
                $"[{nameof(PressMachineController)}] PowerOnMachine appelé.",
                this);
            
            if (_isMachinePoweredOn)
            {
                return;
            }

            _isMachinePoweredOn = true;
            _currentRotationCount = 0;
            _currentState = PressMachineState.Ready;

            ResetAnimatorParameters();

            m_onMachinePoweredOn?.Invoke();
            m_onMachineReady?.Invoke();
        }

        public void PowerOffMachine()
        {
            if (!_isMachinePoweredOn)
            {
                return;
            }

            if (m_isCycleRunning)
            {
                Debug.LogWarning(
                    $"[{nameof(PressMachineController)}] " +
                    "La machine ne peut pas être éteinte pendant son cycle.",
                    this);

                return;
            }

            ApplyPoweredOffState();
        }

        /// <summary>
        /// Démarre le cycle de la presse.
        /// Cette méthode doit être appelée par le bouton Start du UI Document.
        /// </summary>
        public void StartMachineCycle()
        {
            if (!_isMachinePoweredOn)
            {
                Debug.LogWarning(
                    $"[{nameof(PressMachineController)}] " +
                    "Le cycle ne peut pas démarrer car la machine est éteinte.",
                    this);

                return;
            }

            if (_animator == null)
            {
                Debug.LogError(
                    $"[{nameof(PressMachineController)}] " +
                    "La référence Animator est manquante.",
                    this);

                return;
            }

            if (_currentState != PressMachineState.Ready)
            {
                Debug.LogWarning(
                    $"[{nameof(PressMachineController)}] " +
                    $"Le cycle ne peut pas démarrer depuis l'état {_currentState}.",
                    this);

                return;
            }

            _currentRotationCount = 0;
            _currentState = PressMachineState.ClosingDoors;

            _animator.SetBool(_pressRotatingParameter, false);
            _animator.SetBool(_doorClosedParameter, true);

            m_onCycleStarted?.Invoke();
        }

        public void ResetMachineState()
        {
            _currentRotationCount = 0;
            _currentState = PressMachineState.Ready;

            ResetAnimatorParameters();

            if (_isMachinePoweredOn)
            {
                m_onMachineReady?.Invoke();
            }
        }

        /// <summary>
        /// Appelée par un Animation Event placé à la fin
        /// du clip de fermeture des portes.
        /// </summary>
        public void NotifyDoorsClosed()
        {
            Debug.Log(
                $"[{nameof(PressMachineController)}] " +
                $"NotifyDoorsClosed reçu. État : {_currentState}.",
                this);

            if (_currentState != PressMachineState.ClosingDoors)
            {
                Debug.LogWarning(
                    $"[{nameof(PressMachineController)}] " +
                    $"La fermeture a été signalée depuis l'état {_currentState}.",
                    this);

                return;
            }

            if (_animator == null)
            {
                Debug.LogError(
                    $"[{nameof(PressMachineController)}] " +
                    "Impossible de lancer la rotation : Animator manquant.",
                    this);

                return;
            }

            _currentState = PressMachineState.Pressing;

            Debug.Log(
                $"[{nameof(PressMachineController)}] " +
                $"Animator piloté : '{_animator.gameObject.name}'. " +
                $"Paramètre demandé : '{_pressRotatingParameter}'.",
                _animator);

            _animator.SetBool(_pressRotatingParameter, true);

            Debug.Log(
                $"[{nameof(PressMachineController)}] " +
                $"Valeur après SetBool : " +
                $"{_animator.GetBool(_pressRotatingParameter)}.",
                _animator);

            m_onDoorsClosed?.Invoke();
            m_onPressStarted?.Invoke();
        }

        /// <summary>
        /// Appelée par un Animation Event à la fin
        /// de chaque rotation complète de la presse.
        /// </summary>
        public void NotifyPressRotationCompleted()
        {
            if (_currentState != PressMachineState.Pressing)
            {
                return;
            }

            _currentRotationCount++;
            
            Debug.Log(
                $"[{nameof(PressMachineController)}] " +
                $"Rotation {_currentRotationCount}/{_requiredRotationCount}.",
                this);

            if (_currentRotationCount < _requiredRotationCount)
            {
                return;
            }

            StopPressAndOpenDoors();
        }

        /// <summary>
        /// Appelée par un Animation Event placé à la fin
        /// du clip d'ouverture des portes.
        /// </summary>
        public void NotifyDoorsOpened()
        {
            Debug.Log(
                $"[{nameof(PressMachineController)}] " +
                $"NotifyDoorsOpened reçu. État : {_currentState}.",
                this);

            if (_currentState != PressMachineState.OpeningDoors)
            {
                Debug.LogWarning(
                    $"[{nameof(PressMachineController)}] " +
                    $"L'ouverture a été signalée depuis l'état {_currentState}.",
                    this);

                return;
            }

            _currentState = PressMachineState.Ready;

            m_onDoorsOpened?.Invoke();
            m_onMachineReady?.Invoke();

            Debug.Log(
                $"[{nameof(PressMachineController)}] " +
                "Les portes sont ouvertes. La machine est prête.",
                this);
        }
        
        public void NotifyDoorAnimationEndpoint()
        {
            switch (_currentState)
            {
                case PressMachineState.ClosingDoors:
                    NotifyDoorsClosed();
                    break;

                case PressMachineState.OpeningDoors:
                    NotifyDoorsOpened();
                    break;

                default:
                    Debug.LogWarning(
                        $"[{nameof(PressMachineController)}] " +
                        $"Une fin d'animation de porte a été reçue depuis l'état " +
                        $"'{_currentState}'.",
                        this);
                    break;
            }
        }

        #endregion


        #region Main Methods

        private void InitializeMachineState()
        {
            _isMachinePoweredOn = false;
            _currentRotationCount = 0;
            _currentState = PressMachineState.OpeningDoors;

            ResetAnimatorParameters();
        }

        private void StopPressAndOpenDoors()
        {
            _currentState = PressMachineState.OpeningDoors;

            _animator.SetBool(_pressRotatingParameter, false);
            _animator.SetBool(_doorClosedParameter, false);

            m_onPressFinished?.Invoke();
        }

        private bool MustWaitForFunnelRemoval()
        {
            return _waitForFunnelRemoval &&
                   _funnelSocket != null &&
                   _funnelSocket.m_isOccupied;
        }

        private void OnFunnelRemoved()
        {
            if (_currentState != PressMachineState.WaitingForFunnelRemoval)
            {
                return;
            }

            m_onFunnelRemoved?.Invoke();

            CompleteCycle();
        }

        private void CompleteCycle()
        {
            _currentState = PressMachineState.Completed;

            ResetAnimatorParameters();

            m_onCycleCompleted?.Invoke();

            ApplyPoweredOffState();
        }

        private void ApplyPoweredOffState()
        {
            _isMachinePoweredOn = false;
            _currentRotationCount = 0;
            _currentState = PressMachineState.Ready;

            ResetAnimatorParameters();

            m_onMachinePoweredOff?.Invoke();
        }

        private void ResetAnimatorParameters()
        {
            if (_animator == null)
            {
                return;
            }

            _animator.SetBool(_doorClosedParameter, false);
            _animator.SetBool(_pressRotatingParameter, false);
        }

        private void ValidateReferences()
        {
            if (_animator == null)
            {
                Debug.LogError(
                    $"[{nameof(PressMachineController)}] " +
                    "La référence Animator est manquante.",
                    this);

                return;
            }

            ValidateAnimatorParameter(_doorClosedParameter);
            ValidateAnimatorParameter(_pressRotatingParameter);
        }

        private void ValidateAnimatorParameter(string parameterName)
        {
            foreach (AnimatorControllerParameter parameter in _animator.parameters)
            {
                if (parameter.name == parameterName &&
                    parameter.type == AnimatorControllerParameterType.Bool)
                {
                    return;
                }
            }

            Debug.LogError(
                $"[{nameof(PressMachineController)}] " +
                $"Le paramètre Animator booléen '{parameterName}' est introuvable.",
                this);
        }

        #endregion


        #region Private and Protected
        
        [Header("Références")]
        [SerializeField] private Animator _animator;
        [SerializeField] private PressSnapRequirement _funnelSocket;

        [Header("Paramètres Animator")]
        [SerializeField] private string _doorClosedParameter = "IsDoorClosed";
        [SerializeField] private string _pressRotatingParameter = "IsPressRotating";

        [Header("Cycle")]
        [Min(1)]
        [SerializeField] private int _requiredRotationCount = 3;

        [SerializeField] private bool _waitForFunnelRemoval = true;

        private enum PressMachineState
        {
            Ready,
            ClosingDoors,
            Pressing,
            OpeningDoors,
            WaitingForFunnelRemoval,
            Completed
        }

        private PressMachineState _currentState = PressMachineState.Ready;
        private int _currentRotationCount;
        private bool _isMachinePoweredOn;

        #endregion
    }
}