using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Machine.Runtime
{
    public class PressSnapRequirement : MonoBehaviour
    {
        #region Publics

        [Header("Référence XR")]
        [SerializeField] private XRSocketInteractor _socketInteractor;

        [Header("Événements")]
        public UnityEvent m_onSocketOccupied;
        public UnityEvent m_onSocketEmptied;

        public bool m_isOccupied => _isOccupied;

        #endregion


        #region API Unity

        private void Awake()
        {
            FindSocketInteractor();
            RefreshSocketState();
        }

        private void OnEnable()
        {
            RegisterSocketEvents();
            RefreshSocketState();
        }

        private void OnDisable()
        {
            UnregisterSocketEvents();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (_socketInteractor == null)
            {
                _socketInteractor = GetComponent<XRSocketInteractor>();
            }
        }

#endif

        #endregion


        #region Utils

        public void RefreshSocketState()
        {
            if (_socketInteractor == null)
            {
                _isOccupied = false;
                return;
            }

            _isOccupied = _socketInteractor.hasSelection;
        }

        #endregion


        #region Main Methods

        private void FindSocketInteractor()
        {
            if (_socketInteractor != null)
            {
                return;
            }

            _socketInteractor = GetComponent<XRSocketInteractor>();

            if (_socketInteractor == null)
            {
                Debug.LogError(
                    $"[{nameof(PressSnapRequirement)}] " +
                    "Aucun XRSocketInteractor n'est assigné ou présent sur cet objet.",
                    this);
            }
        }

        private void RegisterSocketEvents()
        {
            if (_socketInteractor == null)
            {
                return;
            }

            _socketInteractor.selectEntered.AddListener(OnObjectEnteredSocket);
            _socketInteractor.selectExited.AddListener(OnObjectExitedSocket);
        }

        private void UnregisterSocketEvents()
        {
            if (_socketInteractor == null)
            {
                return;
            }

            _socketInteractor.selectEntered.RemoveListener(OnObjectEnteredSocket);
            _socketInteractor.selectExited.RemoveListener(OnObjectExitedSocket);
        }

        private void OnObjectEnteredSocket(SelectEnterEventArgs eventArgs)
        {
            if (_isOccupied)
            {
                return;
            }

            _isOccupied = true;

            m_onSocketOccupied?.Invoke();
        }

        private void OnObjectExitedSocket(SelectExitEventArgs eventArgs)
        {
            if (_socketInteractor != null &&
                _socketInteractor.hasSelection)
            {
                return;
            }

            _isOccupied = false;

            m_onSocketEmptied?.Invoke();
        }

        #endregion


        #region Private and Protected

        private bool _isOccupied;

        #endregion
    }
}