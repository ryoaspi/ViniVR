using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Machine.Runtime
{
    [RequireComponent(typeof(XRSocketInteractor))]
    public class HoseSocketOrientation : MonoBehaviour
    {
        #region API Unity

        private void Awake()
        {
            _socketInteractor =
                GetComponent<XRSocketInteractor>();

            if (_attachTransform == null)
            {
                _attachTransform =
                    _socketInteractor.attachTransform;
            }

            if (_attachTransform != null)
            {
                _initialLocalRotation =
                    _attachTransform.localRotation;
            }
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


        #region Main Methods

        private void HandleSelectEntered(
            SelectEnterEventArgs eventArgs)
        {
            LiquidHoseEnd hoseEnd =
                FindHoseEnd(
                    eventArgs.interactableObject.transform);

            if (hoseEnd == null ||
                _attachTransform == null)
            {
                return;
            }

            Quaternion orientationOffset =
                Quaternion.Euler(
                    hoseEnd.m_socketRotation);

            _attachTransform.localRotation =
                _initialLocalRotation *
                orientationOffset;
        }

        private void HandleSelectExited(
            SelectExitEventArgs eventArgs)
        {
            if (_attachTransform == null)
            {
                return;
            }

            _attachTransform.localRotation =
                _initialLocalRotation;
        }

        private LiquidHoseEnd FindHoseEnd(
            Transform interactableTransform)
        {
            if (interactableTransform == null)
            {
                return null;
            }

            if (interactableTransform.TryGetComponent(
                    out LiquidHoseEnd hoseEnd))
            {
                return hoseEnd;
            }

            hoseEnd =
                interactableTransform
                    .GetComponentInParent<LiquidHoseEnd>();

            if (hoseEnd != null)
            {
                return hoseEnd;
            }

            return interactableTransform
                .GetComponentInChildren<LiquidHoseEnd>(true);
        }

        #endregion


        #region Private and Protected

        [Header("Références")]
        [SerializeField]
        private Transform _attachTransform;

        private XRSocketInteractor _socketInteractor;
        private Quaternion _initialLocalRotation;

        #endregion
    }
}