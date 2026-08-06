using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Machine.Runtime
{
    [RequireComponent(typeof(XRSocketInteractor))]
    [RequireComponent(typeof(LiquidConnectionPoint))]
    public class LiquidSocketEvents : MonoBehaviour
    {
        #region API Unity

        private void Awake()
        {
            _socketInteractor =
                GetComponent<XRSocketInteractor>();

            _connectionPoint =
                GetComponent<LiquidConnectionPoint>();

            ValidateReferences();
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
        }

        #endregion


        #region Main Methods (méthodes private)

        private void HandleSelectEntered(
            SelectEnterEventArgs eventArgs)
        {
            LiquidHoseEnd hoseEnd = FindHoseEnd(
                eventArgs.interactableObject.transform);

            if (hoseEnd == null)
            {
                Debug.LogWarning(
                    $"[{nameof(LiquidSocketEvents)}] " +
                    $"L'objet placé dans le socket {name} " +
                    $"ne possède aucun {nameof(LiquidHoseEnd)}.",
                    this);

                return;
            }

            hoseEnd.ConnectTo(_connectionPoint);
        }

        private void HandleSelectExited(
            SelectExitEventArgs eventArgs)
        {
            LiquidHoseEnd hoseEnd = FindHoseEnd(
                eventArgs.interactableObject.transform);

            if (hoseEnd == null)
            {
                return;
            }

            if (hoseEnd.m_connectionPoint != _connectionPoint)
            {
                return;
            }

            hoseEnd.Disconnect();
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

        private void ValidateReferences()
        {
            if (_socketInteractor == null)
            {
                Debug.LogError(
                    $"[{nameof(LiquidSocketEvents)}] " +
                    $"{nameof(XRSocketInteractor)} manquant.",
                    this);
            }

            if (_connectionPoint == null)
            {
                Debug.LogError(
                    $"[{nameof(LiquidSocketEvents)}] " +
                    $"{nameof(LiquidConnectionPoint)} manquant.",
                    this);
            }
        }

        #endregion


        #region Private and Protected

        private XRSocketInteractor _socketInteractor;
        private LiquidConnectionPoint _connectionPoint;

        #endregion
    }
}