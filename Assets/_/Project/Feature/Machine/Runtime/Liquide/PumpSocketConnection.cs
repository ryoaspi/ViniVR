using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Machine.Runtime
{
    [RequireComponent(typeof(XRSocketInteractor))]
    public class PumpSocketConnection : MonoBehaviour
    {
        #region Publics

        public LiquidHoseEnd m_connectedHoseEnd =>
            _connectedHoseEnd;

        public bool m_isConnected =>
            _connectedHoseEnd != null;

        #endregion


        #region API Unity

        private void Awake()
        {
            _socketInteractor =
                GetComponent<XRSocketInteractor>();
        }

        private void OnEnable()
        {
            if (_socketInteractor == null)
            {
                return;
            }

            _socketInteractor.selectEntered.AddListener(
                HandleSelectEntered);

            _socketInteractor.selectExited.AddListener(
                HandleSelectExited);
        }

        private void OnDisable()
        {
            if (_socketInteractor == null)
            {
                return;
            }

            _socketInteractor.selectEntered.RemoveListener(
                HandleSelectEntered);

            _socketInteractor.selectExited.RemoveListener(
                HandleSelectExited);

            _connectedHoseEnd = null;
        }

        #endregion


        #region Main Methods

        private void HandleSelectEntered(
            SelectEnterEventArgs eventArgs)
        {
            LiquidHoseEnd hoseEnd =
                FindHoseEnd(eventArgs.interactableObject.transform);

            if (hoseEnd == null)
            {
                Debug.LogWarning(
                    $"[{nameof(PumpSocketConnection)}] " +
                    $"L'objet placé dans le socket {name} " +
                    $"ne possède aucun {nameof(LiquidHoseEnd)}.",
                    this);

                return;
            }

            _connectedHoseEnd = hoseEnd;
            
        }

        private void HandleSelectExited(
            SelectExitEventArgs eventArgs)
        {
            LiquidHoseEnd hoseEnd =
                FindHoseEnd(eventArgs.interactableObject.transform);

            if (hoseEnd == null)
            {
                return;
            }

            if (_connectedHoseEnd != hoseEnd)
            {
                return;
            }

            _connectedHoseEnd = null;
        }

        private LiquidHoseEnd FindHoseEnd(
            Transform interactableTransform)
        {
            if (interactableTransform == null)
            {
                return null;
            }

            LiquidHoseEnd hoseEnd =
                interactableTransform.GetComponent<LiquidHoseEnd>();

            if (hoseEnd != null)
            {
                return hoseEnd;
            }

            hoseEnd =
                interactableTransform.GetComponentInParent<LiquidHoseEnd>();

            if (hoseEnd != null)
            {
                return hoseEnd;
            }

            return interactableTransform
                .GetComponentInChildren<LiquidHoseEnd>();
        }

        #endregion


        #region Private and Protected

        private XRSocketInteractor _socketInteractor;
        private LiquidHoseEnd _connectedHoseEnd;

        #endregion
    }
}