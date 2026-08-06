using UnityEngine;

namespace Machine.Runtime
{
    public class LiquidPumpController : MonoBehaviour
    {
        #region Publics

        public bool m_isRunning => _isRunning;

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

        #endregion


        #region Utils

        public void StartPump()
        {
            _isRunning = true;
        }

        public void StopPump()
        {
            _isRunning = false;
        }

        #endregion


        #region Main Methods

        private void TransferLiquid()
        {
            if (_inputSocket == null ||
                _outputSocket == null)
            {
                return;
            }

            if (!_inputSocket.m_isConnected ||
                !_outputSocket.m_isConnected)
            {
                return;
            }

            LiquidContainer source =
                _inputSocket
                    .m_connectedHoseEnd
                    .m_connectedContainer;

            LiquidContainer destination =
                _outputSocket
                    .m_connectedHoseEnd
                    .m_connectedContainer;

            if (source == null ||
                destination == null)
            {
                return;
            }

            float requestedAmount =
                _flowRatePerSecond *
                Time.deltaTime;

            float removedAmount =
                source.RemoveLiquid(
                    requestedAmount);

            if (removedAmount <= 0f)
            {
                return;
            }

            float addedAmount =
                destination.AddLiquid(
                    removedAmount);

            /*
             * Si la destination est pleine,
             * on remet le surplus dans la source.
             */
            if (addedAmount < removedAmount)
            {
                source.AddLiquid(
                    removedAmount - addedAmount);
            }
        }

        #endregion


        #region Private and Protected

        [Header("Sockets")]
        [SerializeField]
        private PumpSocketConnection _inputSocket;

        [SerializeField]
        private PumpSocketConnection _outputSocket;

        [Header("Débit")]
        [Min(0f)]
        [SerializeField]
        private float _flowRatePerSecond = 10f;

        private bool _isRunning;

        #endregion
    }
}