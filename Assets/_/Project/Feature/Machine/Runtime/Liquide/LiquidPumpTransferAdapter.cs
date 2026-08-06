using UnityEngine;

namespace Machine.Runtime
{
    public class LiquidPumpTransferAdapter : MonoBehaviour
    {
        #region Publics

        public bool m_isRunning =>
            _isRunning;

        public LiquidContainer m_currentSource =>
            ResolveConnectedContainer(_inputSocket);

        public LiquidContainer m_currentDestination =>
            ResolveConnectedContainer(_outputSocket);

        #endregion


        #region API Unity

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }

            TransferLiquid();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            _flowRatePerSecond =
                Mathf.Max(0f, _flowRatePerSecond);

            _debugLogInterval =
                Mathf.Max(0.1f, _debugLogInterval);
        }

#endif

        #endregion


        #region Utils (méthodes publics)

        public void StartPump()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;

            Debug.Log(
                $"[{nameof(LiquidPumpTransferAdapter)}] " +
                $"Pompe démarrée sur {name}.",
                this);

            LogCurrentConnections();
        }

        public void StopPump()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;

            Debug.Log(
                $"[{nameof(LiquidPumpTransferAdapter)}] " +
                $"Pompe arrêtée sur {name}.",
                this);
        }

        public void LogCurrentConnections()
        {
            LiquidContainer source =
                ResolveConnectedContainer(_inputSocket);

            LiquidContainer destination =
                ResolveConnectedContainer(_outputSocket);

            string sourceName =
                source != null
                    ? source.m_containerName
                    : "NULL";

            string destinationName =
                destination != null
                    ? destination.m_containerName
                    : "NULL";

            Debug.Log(
                $"[{nameof(LiquidPumpTransferAdapter)}] " +
                $"Source : {sourceName} | " +
                $"Destination : {destinationName}.",
                this);
        }

        #endregion


        #region Main Methods (méthodes private)

        private void TransferLiquid()
        {
            if (_inputSocket == null ||
                _outputSocket == null)
            {
                LogMissingSocketReferences();
                return;
            }

            if (!_inputSocket.m_isConnected ||
                !_outputSocket.m_isConnected)
            {
                LogMissingPumpConnections();
                return;
            }

            LiquidContainer source =
                ResolveConnectedContainer(_inputSocket);

            LiquidContainer destination =
                ResolveConnectedContainer(_outputSocket);

            if (source == null ||
                destination == null)
            {
                LogMissingContainerConnection(
                    source,
                    destination);

                return;
            }

            if (source == destination)
            {
                LogSameContainerWarning();
                return;
            }

            if (source.m_isEmpty)
            {
                LogSourceEmpty(source);
                return;
            }

            if (destination.m_isFull)
            {
                LogDestinationFull(destination);
                return;
            }

            float requestedAmount =
                _flowRatePerSecond *
                Time.deltaTime;

            float destinationAvailableCapacity =
                destination.m_maximumVolume -
                destination.m_currentVolume;

            float transferableAmount =
                Mathf.Min(
                    requestedAmount,
                    destinationAvailableCapacity);

            if (transferableAmount <= 0f)
            {
                return;
            }

            float removedAmount =
                source.RemoveLiquid(
                    transferableAmount);

            if (removedAmount <= 0f)
            {
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
        }

        private LiquidContainer ResolveConnectedContainer(
            PumpSocketConnection pumpSocket)
        {
            if (pumpSocket == null ||
                !pumpSocket.m_isConnected)
            {
                return null;
            }

            LiquidHoseEnd pumpSideEnd =
                pumpSocket.m_connectedHoseEnd;

            if (pumpSideEnd == null)
            {
                return null;
            }

            if (pumpSideEnd.m_connectedContainer != null)
            {
                return pumpSideEnd.m_connectedContainer;
            }

            LiquidHose hose =
                pumpSideEnd.GetComponentInParent<LiquidHose>();

            if (hose == null)
            {
                hose =
                    pumpSideEnd.GetComponentInChildren<LiquidHose>();
            }

            if (hose == null)
            {
                LogMissingHose(pumpSideEnd);
                return null;
            }

            LiquidHoseEnd oppositeEnd =
                hose.GetOppositeEnd(
                    pumpSideEnd);

            if (oppositeEnd == null)
            {
                return null;
            }

            return oppositeEnd.m_connectedContainer;
        }

        private void LogMissingSocketReferences()
        {
            if (!CanWriteDebugLog())
            {
                return;
            }

            Debug.LogWarning(
                $"[{nameof(LiquidPumpTransferAdapter)}] " +
                "Les références Input Socket ou Output Socket " +
                "ne sont pas assignées.",
                this);
        }

        private void LogMissingPumpConnections()
        {
            if (!CanWriteDebugLog())
            {
                return;
            }

            string inputState =
                _inputSocket != null &&
                _inputSocket.m_isConnected
                    ? "connecté"
                    : "déconnecté";

            string outputState =
                _outputSocket != null &&
                _outputSocket.m_isConnected
                    ? "connecté"
                    : "déconnecté";

            Debug.LogWarning(
                $"[{nameof(LiquidPumpTransferAdapter)}] " +
                $"Transfert impossible. " +
                $"Entrée : {inputState} | " +
                $"Sortie : {outputState}.",
                this);
        }

        private void LogMissingContainerConnection(
            LiquidContainer source,
            LiquidContainer destination)
        {
            if (!CanWriteDebugLog())
            {
                return;
            }

            string sourceName =
                source != null
                    ? source.m_containerName
                    : "NULL";

            string destinationName =
                destination != null
                    ? destination.m_containerName
                    : "NULL";

            Debug.LogWarning(
                $"[{nameof(LiquidPumpTransferAdapter)}] " +
                $"Transfert impossible. " +
                $"Source : {sourceName} | " +
                $"Destination : {destinationName}. " +
                "Vérifie que l'autre extrémité de chaque tuyau " +
                "est connectée à une cuve.",
                this);
        }

        private void LogSameContainerWarning()
        {
            if (!CanWriteDebugLog())
            {
                return;
            }

            Debug.LogWarning(
                $"[{nameof(LiquidPumpTransferAdapter)}] " +
                "La source et la destination correspondent " +
                "au même LiquidContainer.",
                this);
        }

        private void LogSourceEmpty(
            LiquidContainer source)
        {
            if (!CanWriteDebugLog())
            {
                return;
            }

            Debug.LogWarning(
                $"[{nameof(LiquidPumpTransferAdapter)}] " +
                $"La cuve source {source.m_containerName} " +
                "est vide.",
                source);
        }

        private void LogDestinationFull(
            LiquidContainer destination)
        {
            if (!CanWriteDebugLog())
            {
                return;
            }

            Debug.LogWarning(
                $"[{nameof(LiquidPumpTransferAdapter)}] " +
                $"La cuve destination " +
                $"{destination.m_containerName} est pleine.",
                destination);
        }

        private void LogMissingHose(
            LiquidHoseEnd hoseEnd)
        {
            if (!CanWriteDebugLog())
            {
                return;
            }

            Debug.LogWarning(
                $"[{nameof(LiquidPumpTransferAdapter)}] " +
                $"Aucun {nameof(LiquidHose)} trouvé pour " +
                $"l'extrémité {hoseEnd.name}.",
                hoseEnd);
        }

        private bool CanWriteDebugLog()
        {
            if (Time.time <
                _nextDebugLogTime)
            {
                return false;
            }

            _nextDebugLogTime =
                Time.time +
                _debugLogInterval;

            return true;
        }

        #endregion


        #region Private and Protected

        [Header("Sockets de la pompe")]
        [SerializeField]
        private PumpSocketConnection _inputSocket;

        [SerializeField]
        private PumpSocketConnection _outputSocket;

        [Header("Débit")]
        [Min(0f)]
        [SerializeField]
        private float _flowRatePerSecond = 10f;

        [Header("Debug")]
        [Min(0.1f)]
        [SerializeField]
        private float _debugLogInterval = 1f;

        private float _nextDebugLogTime;
        private bool _isRunning;

        #endregion
    }
}