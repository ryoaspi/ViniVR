using UnityEngine;
using UnityEngine.Events;

namespace Machine.Runtime
{
    public class LiquidHoseEnd : MonoBehaviour
    {
        #region Publics

        [Header("Événements")]
        public UnityEvent m_onConnected;
        public UnityEvent m_onDisconnected;

        public LiquidConnectionPoint m_connectionPoint =>
            _connectionPoint;

        public LiquidContainer m_connectedContainer =>
            _connectionPoint != null
                ? _connectionPoint.m_container
                : null;

        public bool m_isConnected =>
            _connectionPoint != null;

        #endregion


        #region Utils (méthodes publics)

        public void ConnectTo(
            LiquidConnectionPoint connectionPoint)
        {
            if (connectionPoint == null)
            {
                Debug.LogWarning(
                    $"[{nameof(LiquidHoseEnd)}] " +
                    "Tentative de connexion avec un point nul.",
                    this);

                return;
            }

            if (_connectionPoint == connectionPoint)
            {
                return;
            }

            _connectionPoint = connectionPoint;

            Debug.Log(
                $"[{nameof(LiquidHoseEnd)}] " +
                $"{name} connecté à " +
                $"{_connectionPoint.m_containerName}.",
                this);

            m_onConnected?.Invoke();
        }

        public void Disconnect()
        {
            if (_connectionPoint == null)
            {
                return;
            }

            Debug.Log(
                $"[{nameof(LiquidHoseEnd)}] " +
                $"{name} déconnecté de " +
                $"{_connectionPoint.m_containerName}.",
                this);

            _connectionPoint = null;

            m_onDisconnected?.Invoke();
        }

        #endregion


        #region Private and Protected

        private LiquidConnectionPoint _connectionPoint;

        #endregion
    }
}
