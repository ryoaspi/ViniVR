using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Machine.Runtime
{
    public class HoseGrabPhysicsController : MonoBehaviour
    {
        #region API Unity

        private void Awake()
        {
            ValidateReferences();
            CacheRigidbodies();

            SetVisualState(true);
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void Update()
        {
            if (!_pendingReleaseEvaluation)
            {
                return;
            }

            if (Time.frameCount < _releaseEvaluationFrame)
            {
                return;
            }

            _pendingReleaseEvaluation = false;

            EvaluateCurrentState();
        }

        private void OnDisable()
        {
            UnregisterEvents();

            _manualGrabCount = 0;
            _pendingReleaseEvaluation = false;

            RestorePhysics();
            SetVisualState(true);
        }

        #endregion


        #region Main Methods

        private void RegisterEvents()
        {
            if (_baseGrabInteractable != null)
            {
                _baseGrabInteractable.selectEntered.AddListener(
                    HandleSelectEntered);

                _baseGrabInteractable.selectExited.AddListener(
                    HandleSelectExited);
            }

            if (_endGrabInteractable != null)
            {
                _endGrabInteractable.selectEntered.AddListener(
                    HandleSelectEntered);

                _endGrabInteractable.selectExited.AddListener(
                    HandleSelectExited);
            }
        }

        private void UnregisterEvents()
        {
            if (_baseGrabInteractable != null)
            {
                _baseGrabInteractable.selectEntered.RemoveListener(
                    HandleSelectEntered);

                _baseGrabInteractable.selectExited.RemoveListener(
                    HandleSelectExited);
            }

            if (_endGrabInteractable != null)
            {
                _endGrabInteractable.selectEntered.RemoveListener(
                    HandleSelectEntered);

                _endGrabInteractable.selectExited.RemoveListener(
                    HandleSelectExited);
            }
        }

        private void HandleSelectEntered(
            SelectEnterEventArgs eventArgs)
        {
            if (IsSocketInteractor(eventArgs.interactorObject))
            {
                /*
                 * Le tuyau vient d'être pris par un socket.
                 *
                 * On le rend visible, mais on conserve la physique
                 * du flexible figée afin que le snap reste stable.
                 */
                FreezePhysics();
                SetVisualState(true);

                return;
            }

            /*
             * Sélection provenant d'une main XR.
             */
            _manualGrabCount++;

            _pendingReleaseEvaluation = false;

            FreezePhysics();
            SetVisualState(false);
        }

        private void HandleSelectExited(
            SelectExitEventArgs eventArgs)
        {
            if (IsSocketInteractor(eventArgs.interactorObject))
            {
                ScheduleStateEvaluation();

                return;
            }

            _manualGrabCount =
                Mathf.Max(0, _manualGrabCount - 1);

            if (_manualGrabCount > 0)
            {
                return;
            }

            /*
             * On n'évalue pas immédiatement.
             *
             * XRI peut déclencher la sortie de la main avant que
             * le XRSocketInteractor soit complètement enregistré
             * comme nouvel interactor.
             */
            ScheduleStateEvaluation();
        }

        private void ScheduleStateEvaluation()
        {
            _pendingReleaseEvaluation = true;
            _releaseEvaluationFrame = Time.frameCount + 1;
        }

        private void EvaluateCurrentState()
        {
            if (_manualGrabCount > 0)
            {
                FreezePhysics();
                SetVisualState(false);

                return;
            }

            bool isSnapped =
                IsGrabbedBySocket(_baseGrabInteractable) ||
                IsGrabbedBySocket(_endGrabInteractable);

            if (isSnapped)
            {
                /*
                 * Une ou deux extrémités sont connectées.
                 *
                 * Version stable :
                 * le flexible reste figé une fois raccordé.
                 */
                FreezePhysics();
                SetVisualState(true);

                return;
            }

            /*
             * Plus aucune main et aucun socket.
             * Le tuyau redevient entièrement physique.
             */
            RestorePhysics();
            SetVisualState(true);
        }

        private void FreezePhysics()
        {
            if (_hoseRigidbodies == null)
            {
                return;
            }

            foreach (Rigidbody hoseRigidbody in _hoseRigidbodies)
            {
                if (hoseRigidbody == null)
                {
                    continue;
                }

                /*
                 * Important :
                 * Unity n'autorise pas la modification des vitesses
                 * lorsqu'un Rigidbody est déjà kinematic.
                 */
                if (!hoseRigidbody.isKinematic)
                {
                    hoseRigidbody.linearVelocity = Vector3.zero;
                    hoseRigidbody.angularVelocity = Vector3.zero;
                }

                hoseRigidbody.isKinematic = true;
            }
        }

        private void RestorePhysics()
        {
            if (_hoseRigidbodies == null ||
                _initialKinematicStates == null)
            {
                return;
            }

            for (int i = 0; i < _hoseRigidbodies.Length; i++)
            {
                Rigidbody hoseRigidbody =
                    _hoseRigidbodies[i];

                if (hoseRigidbody == null)
                {
                    continue;
                }

                hoseRigidbody.isKinematic =
                    _initialKinematicStates[i];
            }
        }

        private void SetVisualState(bool isVisible)
        {
            if (_hoseRenderer == null)
            {
                return;
            }

            _hoseRenderer.enabled = isVisible;
        }

        private void CacheRigidbodies()
        {
            if (_physicsRoot == null)
            {
                return;
            }

            Rigidbody[] detectedRigidbodies =
                _physicsRoot.GetComponentsInChildren<Rigidbody>(true);

            Rigidbody baseRigidbody =
                GetGrabRigidbody(_baseGrabInteractable);

            Rigidbody endRigidbody =
                GetGrabRigidbody(_endGrabInteractable);

            int validCount = 0;

            foreach (Rigidbody detectedRigidbody
                     in detectedRigidbodies)
            {
                if (detectedRigidbody == null)
                {
                    continue;
                }

                if (detectedRigidbody == baseRigidbody ||
                    detectedRigidbody == endRigidbody)
                {
                    continue;
                }

                validCount++;
            }

            _hoseRigidbodies =
                new Rigidbody[validCount];

            _initialKinematicStates =
                new bool[validCount];

            int index = 0;

            foreach (Rigidbody detectedRigidbody
                     in detectedRigidbodies)
            {
                if (detectedRigidbody == null)
                {
                    continue;
                }

                if (detectedRigidbody == baseRigidbody ||
                    detectedRigidbody == endRigidbody)
                {
                    continue;
                }

                _hoseRigidbodies[index] =
                    detectedRigidbody;

                _initialKinematicStates[index] =
                    detectedRigidbody.isKinematic;

                index++;
            }

            Debug.Log(
                $"[{nameof(HoseGrabPhysicsController)}] " +
                $"{_hoseRigidbodies.Length} Rigidbody du flexible détectés.",
                this);
        }

        private Rigidbody GetGrabRigidbody(
            XRGrabInteractable grabInteractable)
        {
            if (grabInteractable == null)
            {
                return null;
            }

            return grabInteractable.GetComponent<Rigidbody>();
        }

        private bool IsGrabbedBySocket(
            XRGrabInteractable grabInteractable)
        {
            if (grabInteractable == null)
            {
                return false;
            }

            foreach (IXRSelectInteractor interactor
                     in grabInteractable.interactorsSelecting)
            {
                if (interactor is XRSocketInteractor)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsSocketInteractor(
            IXRSelectInteractor interactor)
        {
            return interactor is XRSocketInteractor;
        }

        private void ValidateReferences()
        {
            if (_hoseRenderer == null)
            {
                Debug.LogError(
                    $"[{nameof(HoseGrabPhysicsController)}] " +
                    "SkinnedMeshRenderer du tuyau non assigné.",
                    this);
            }

            if (_physicsRoot == null)
            {
                Debug.LogError(
                    $"[{nameof(HoseGrabPhysicsController)}] " +
                    "Physics Root non assigné.",
                    this);
            }

            if (_baseGrabInteractable == null)
            {
                Debug.LogError(
                    $"[{nameof(HoseGrabPhysicsController)}] " +
                    "XRGrabInteractable de Saisie_Base non assigné.",
                    this);
            }

            if (_endGrabInteractable == null)
            {
                Debug.LogError(
                    $"[{nameof(HoseGrabPhysicsController)}] " +
                    "XRGrabInteractable de Saisie_End non assigné.",
                    this);
            }
        }

        #endregion


        #region Private and Protected

        [Header("Visuel")]
        [SerializeField]
        private SkinnedMeshRenderer _hoseRenderer;

        [Header("Physique")]
        [SerializeField]
        private Transform _physicsRoot;

        [Header("Extrémités interactives")]
        [SerializeField]
        private XRGrabInteractable _baseGrabInteractable;

        [SerializeField]
        private XRGrabInteractable _endGrabInteractable;

        private Rigidbody[] _hoseRigidbodies;
        private bool[] _initialKinematicStates;

        private int _manualGrabCount;

        private bool _pendingReleaseEvaluation;
        private int _releaseEvaluationFrame;

        #endregion
    }
}