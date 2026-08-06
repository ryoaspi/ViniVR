using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Machine.Runtime
{
    [RequireComponent(typeof(XRSocketInteractor))]
    public class ConditionalSocketRotation : MonoBehaviour
    {
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

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (_attachTransform == null)
            {
                XRSocketInteractor socketInteractor =
                    GetComponent<XRSocketInteractor>();

                if (socketInteractor != null)
                {
                    _attachTransform =
                        socketInteractor.attachTransform;
                }
            }
        }

#endif

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

            if (hoseEnd == _hoseBaseEnd)
            {
                _attachTransform.localEulerAngles =
                    _baseEndRotation;

                return;
            }

            if (hoseEnd == _hoseFinalEnd)
            {
                _attachTransform.localEulerAngles =
                    _finalEndRotation;
            }
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

        [SerializeField]
        private LiquidHoseEnd _hoseBaseEnd;

        [SerializeField]
        private LiquidHoseEnd _hoseFinalEnd;

        [Header("Rotations locales")]
        [SerializeField]
        private Vector3 _baseEndRotation;

        [SerializeField]
        private Vector3 _finalEndRotation =
            new Vector3(0f, 180f, 0f);

        private XRSocketInteractor _socketInteractor;

        #endregion
    }
}