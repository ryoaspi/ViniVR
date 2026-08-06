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

            if (_attachTransform == null &&
                _socketInteractor != null)
            {
                _attachTransform =
                    _socketInteractor.attachTransform;
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
        }

        private void OnDisable()
        {
            if (_socketInteractor == null)
            {
                return;
            }

            _socketInteractor.selectEntered.RemoveListener(
                HandleSelectEntered);
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

            _attachTransform.localEulerAngles =
                hoseEnd.m_socketRotation;
        }

        private LiquidHoseEnd FindHoseEnd(
            Transform interactableTransform)
        {
            if (interactableTransform == null)
            {
                return null;
            }

            LiquidHoseEnd hoseEnd =
                interactableTransform
                    .GetComponent<LiquidHoseEnd>();

            if (hoseEnd != null)
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
                .GetComponentInChildren<LiquidHoseEnd>();
        }

        #endregion


        #region Private and Protected

        [Header("Références")]
        [SerializeField]
        private Transform _attachTransform;

        private XRSocketInteractor _socketInteractor;

        #endregion
    }
}