using UnityEngine;
using UnityEngine.Events;

namespace Machine.Runtime
{
    public class LiquidPumpController : MonoBehaviour
    {
        #region Publics

        [Header("Événements")]
        public UnityEvent m_onPumpStarted;
        public UnityEvent m_onPumpStopped;
        public UnityEvent m_onTransferCompleted;
        public UnityEvent m_onTransferBlocked;

        public bool m_isRunning => _isRunning;

        #endregion


        #region API Unity

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }

            UpdateTransfer();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            _flowRatePerSecond =
                Mathf.Max(0.01f, _flowRatePerSecond);
        }

#endif

        #endregion


        #region Utils

        /// <summary>
        /// Tente de démarrer la pompe.
        /// </summary>
        public void StartPump()
        {
            if (_isRunning)
            {
                return;
            }

            if (!TryPrepareTransfer())
            {
                m_onTransferBlocked?.Invoke();
                return;
            }

            _isRunning = true;

            Debug.Log(
                $"[{nameof(LiquidPumpController)}] " +
                $"Transfert démarré : " +
                $"{_source.m_containerName} → " +
                $"{_destination.m_containerName}.",
                this);

            m_onPumpStarted?.Invoke();
        }

        /// <summary>
        /// Arrête manuellement la pompe.
        /// </summary>
        public void StopPump()
        {
            StopPumpInternal(false);
        }

        /// <summary>
        /// Active ou désactive la pompe.
        /// </summary>
        public void SetPumpActive(bool isActive)
        {
            if (isActive)
            {
                StartPump();
                return;
            }

            StopPump();
        }

        #endregion


        #region Main Methods

        private void UpdateTransfer()
        {
            if (!TryResolveConnectedContainers(
                    out LiquidContainer currentSource,
                    out LiquidContainer currentDestination))
            {
                StopPumpInternal(false);
                return;
            }

            if (currentSource != _source ||
                currentDestination != _destination)
            {
                StopPumpInternal(false);
                return;
            }

            if (!IsValveOpen())
            {
                StopPumpInternal(false);
                return;
            }

            if (_source.m_isEmpty ||
                _destination.m_isFull)
            {
                CompleteTransfer();
                return;
            }

            float requestedVolume =
                _flowRatePerSecond * Time.deltaTime;

            float availableVolume =
                _source.m_currentVolume;

            float remainingCapacity =
                _destination.m_maximumVolume -
                _destination.m_currentVolume;

            float transferVolume = Mathf.Min(
                requestedVolume,
                availableVolume,
                remainingCapacity);

            if (transferVolume <= 0f)
            {
                CompleteTransfer();
                return;
            }

            float removedVolume =
                _source.RemoveLiquid(transferVolume);

            float addedVolume =
                _destination.AddLiquid(removedVolume);

            if (addedVolume < removedVolume)
            {
                _source.AddLiquid(
                    removedVolume - addedVolume);
            }

            if (_source.m_isEmpty ||
                _destination.m_isFull)
            {
                CompleteTransfer();
            }
        }

        private bool TryPrepareTransfer()
        {
            if (!ValidateReferences())
            {
                return false;
            }

            if (!IsValveOpen())
            {
                LogBlockedTransfer(
                    "La vanne doit être ouverte.");

                return false;
            }

            if (!TryResolveConnectedContainers(
                    out LiquidContainer source,
                    out LiquidContainer destination))
            {
                return false;
            }

            if (source == destination)
            {
                LogBlockedTransfer(
                    "La source et la destination " +
                    "sont le même conteneur.");

                return false;
            }

            if (source.m_isEmpty)
            {
                LogBlockedTransfer(
                    "Le conteneur source est vide.");

                return false;
            }

            if (destination.m_isFull)
            {
                LogBlockedTransfer(
                    "Le conteneur destination est plein.");

                return false;
            }

            _source = source;
            _destination = destination;

            return true;
        }

        private bool TryResolveConnectedContainers(
            out LiquidContainer source,
            out LiquidContainer destination)
        {
            source = null;
            destination = null;

            if (!_inputSocket.m_isConnected ||
                !_outputSocket.m_isConnected)
            {
                LogBlockedTransfer(
                    "Un tuyau doit être connecté " +
                    "à l'entrée et à la sortie de la pompe.");

                return false;
            }

            source = GetContainerAtOppositeEnd(
                _inputSocket.m_connectedHoseEnd);

            destination = GetContainerAtOppositeEnd(
                _outputSocket.m_connectedHoseEnd);

            if (source == null)
            {
                LogBlockedTransfer(
                    "Le tuyau d'entrée n'est connecté " +
                    "à aucun conteneur source.");

                return false;
            }

            if (destination == null)
            {
                LogBlockedTransfer(
                    "Le tuyau de sortie n'est connecté " +
                    "à aucun conteneur destination.");

                return false;
            }

            return true;
        }

        private LiquidContainer GetContainerAtOppositeEnd(
            LiquidHoseEnd pumpHoseEnd)
        {
            if (pumpHoseEnd == null)
            {
                return null;
            }

            LiquidHose hose =
                pumpHoseEnd.GetComponentInParent<LiquidHose>();

            if (hose == null)
            {
                Debug.LogWarning(
                    $"[{nameof(LiquidPumpController)}] " +
                    $"{pumpHoseEnd.name} n'appartient à aucun " +
                    $"{nameof(LiquidHose)}.",
                    pumpHoseEnd);

                return null;
            }

            LiquidHoseEnd oppositeEnd =
                hose.GetOppositeEnd(pumpHoseEnd);

            if (oppositeEnd == null ||
                !oppositeEnd.m_isConnected)
            {
                return null;
            }

            return oppositeEnd.m_connectedContainer;
        }

        private bool ValidateReferences()
        {
            if (_inputSocket == null)
            {
                LogBlockedTransfer(
                    "Le socket d'entrée n'est pas assigné.");

                return false;
            }

            if (_outputSocket == null)
            {
                LogBlockedTransfer(
                    "Le socket de sortie n'est pas assigné.");

                return false;
            }

            return true;
        }

        private bool IsValveOpen()
        {
            if (!_requireOpenValve)
            {
                return true;
            }

            if (_valveHandle == null)
            {
                return false;
            }

            return _valveHandle.m_isOpen;
        }

        private void CompleteTransfer()
        {
            Debug.Log(
                $"[{nameof(LiquidPumpController)}] " +
                "Transfert terminé.",
                this);

            StopPumpInternal(true);
        }

        private void StopPumpInternal(
            bool transferCompleted)
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;

            m_onPumpStopped?.Invoke();

            if (transferCompleted)
            {
                m_onTransferCompleted?.Invoke();
            }

            _source = null;
            _destination = null;
        }

        private void LogBlockedTransfer(string message)
        {
            Debug.LogWarning(
                $"[{nameof(LiquidPumpController)}] " +
                message,
                this);
        }

        #endregion


        #region Private and Protected

        [Header("Sockets de la pompe")]
        [Tooltip("Socket correspondant à l'entrée IN.")]
        [SerializeField] private PumpSocketConnection _inputSocket;

        [Tooltip("Socket correspondant à la sortie OUT.")]
        [SerializeField] private PumpSocketConnection _outputSocket;

        [Header("Vanne")]
        [SerializeField] private VRValveHandle _valveHandle;

        [SerializeField] private bool _requireOpenValve = true;

        [Header("Transfert")]
        [Min(0.01f)]
        [SerializeField] private float _flowRatePerSecond = 10f;

        private LiquidContainer _source;
        private LiquidContainer _destination;

        private bool _isRunning;

        #endregion
    }
}