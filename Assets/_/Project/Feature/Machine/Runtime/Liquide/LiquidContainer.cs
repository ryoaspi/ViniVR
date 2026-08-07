using UnityEngine;
using UnityEngine.Events;

namespace Machine.Runtime
{

    public class LiquidContainer : MonoBehaviour, ILiquidContainer
    {
        #region Publics

        [Header("Événements")]
        public UnityEvent<float> m_onVolumeChanged;
        public UnityEvent m_onContainerEmpty;
        public UnityEvent m_onContainerFull;

        public string m_containerName => _containerName;

        public LiquidContainerType m_containerType =>
            _containerType;

        public float m_currentVolume =>
            _currentVolume;

        public float m_maximumVolume =>
            _maximumVolume;

        public float m_normalizedVolume =>
            _maximumVolume <= 0f
                ? 0f
                : _currentVolume / _maximumVolume;

        public bool m_isEmpty =>
            _currentVolume <= _volumeTolerance;

        public bool m_isFull =>
            _currentVolume >=
            _maximumVolume - _volumeTolerance;

        #endregion


        #region API Unity

        private void Awake()
        {
            _currentVolume = Mathf.Clamp(
                _startingVolume,
                0f,
                _maximumVolume);

            NotifyVolumeChanged();
        }

    #if UNITY_EDITOR

        private void OnValidate()
        {
            _maximumVolume = Mathf.Max(
                0.01f,
                _maximumVolume);

            _startingVolume = Mathf.Clamp(
                _startingVolume,
                0f,
                _maximumVolume);

            _volumeTolerance = Mathf.Max(
                0f,
                _volumeTolerance);
        }

    #endif

        #endregion


        #region Utils (méthodes publics)

        public float RemoveLiquid(float requestedAmount)
        {
            if (requestedAmount <= 0f || m_isEmpty)
            {
                return 0f;
            }

            bool wasEmpty = m_isEmpty;

            float removedAmount = Mathf.Min(
                requestedAmount,
                _currentVolume);

            _currentVolume -= removedAmount;

            NotifyVolumeChanged();

            if (!wasEmpty && m_isEmpty)
            {
                m_onContainerEmpty?.Invoke();
            }

            return removedAmount;
        }

        public float AddLiquid(float requestedAmount)
        {
            if (requestedAmount <= 0f || m_isFull)
            {
                return 0f;
            }

            bool wasFull = m_isFull;

            float availableCapacity =
                _maximumVolume - _currentVolume;

            float addedAmount = Mathf.Min(
                requestedAmount,
                availableCapacity);

            _currentVolume += addedAmount;

            NotifyVolumeChanged();

            if (!wasFull && m_isFull)
            {
                m_onContainerFull?.Invoke();
            }

            return addedAmount;
        }

        public void SetVolume(float volume)
        {
            _currentVolume = Mathf.Clamp(
                volume,
                0f,
                _maximumVolume);

            NotifyVolumeChanged();
        }

        public void ChangedVolume()
        {
            Debug.Log($"volume: {_currentVolume}");
        }

        #endregion


        #region Main Methods (méthodes private)

        private void NotifyVolumeChanged()
        {
            m_onVolumeChanged?.Invoke(
                m_normalizedVolume);
        }

        #endregion


        #region Private and Protected

        [Header("Identification")]
        [SerializeField] private string _containerName =
            "Liquid Container";

        [SerializeField]
        private LiquidContainerType _containerType =
            LiquidContainerType.Other;

        [Header("Volume")]
        [Min(0.01f)]
        [SerializeField] private float _maximumVolume =
            100f;

        [Min(0f)]
        [SerializeField] private float _startingVolume;

        [Min(0f)]
        [SerializeField] private float _volumeTolerance =
            0.001f;

        private float _currentVolume;

        #endregion
    }
}