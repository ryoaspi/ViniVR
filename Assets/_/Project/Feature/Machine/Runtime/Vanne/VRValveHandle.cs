using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class VRValveHandle : MonoBehaviour
{
    #region Publics

    [Header("Événements")]
    public UnityEvent m_onValveOpened;
    public UnityEvent m_onValveClosed;
    public UnityEvent<float> m_onValveValueChanged;

    public bool m_isOpen => _currentAngle >= _openAngle - _stateTolerance;

    public bool m_isClosed => _currentAngle <= _closedAngle + _stateTolerance;

    public float m_normalizedValue =>
        Mathf.InverseLerp(_closedAngle, _openAngle, _currentAngle);

    #endregion


    #region API Unity

    private void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();

        if (!_rotationPivot)
            _rotationPivot = transform;

        _currentAngle = Mathf.Clamp(
            _startingAngle,
            _closedAngle,
            _openAngle
        );

        ApplyRotation();
        UpdateValveState(true);
    }

    private void OnEnable()
    {
        _interactable.selectEntered.AddListener(HandleSelectEntered);
        _interactable.selectExited.AddListener(HandleSelectExited);
    }

    private void OnDisable()
    {
        _interactable.selectEntered.RemoveListener(HandleSelectEntered);
        _interactable.selectExited.RemoveListener(HandleSelectExited);
    }

    private void Update()
    {
        if (!_isGrabbed || !_interactorTransform)
            return;

        UpdateValveRotation();
    }

    private void OnValidate()
    {
        if (_openAngle < _closedAngle)
            _openAngle = _closedAngle;

        _startingAngle = Mathf.Clamp(
            _startingAngle,
            _closedAngle,
            _openAngle
        );
    }

    #endregion


    #region Utils (méthodes publics)

    /// <summary>
    /// Place immédiatement la vanne en position ouverte.
    /// </summary>
    public void OpenValve()
    {
        SetValveAngle(_openAngle);
    }

    /// <summary>
    /// Place immédiatement la vanne en position fermée.
    /// </summary>
    public void CloseValve()
    {
        SetValveAngle(_closedAngle);
    }

    /// <summary>
    /// Définit l'ouverture avec une valeur comprise entre 0 et 1.
    /// </summary>
    public void SetValveValue(float normalizedValue)
    {
        float targetAngle = Mathf.Lerp(
            _closedAngle,
            _openAngle,
            Mathf.Clamp01(normalizedValue)
        );

        SetValveAngle(targetAngle);
    }

    /// <summary>
    /// Définit directement l'angle de la vanne.
    /// </summary>
    public void SetValveAngle(float angle)
    {
        _currentAngle = Mathf.Clamp(
            angle,
            _closedAngle,
            _openAngle
        );

        ApplyRotation();
        NotifyValueChanged();
        UpdateValveState(false);
    }

    #endregion


    #region Main Methods (méthodes private)

    private void HandleSelectEntered(SelectEnterEventArgs eventArgs)
    {
        _interactorTransform =
            eventArgs.interactorObject.GetAttachTransform(_interactable);

        if (!_interactorTransform)
            return;

        _isGrabbed = true;

        Vector3 rotationAxis = GetWorldRotationAxis();
        Vector3 directionToInteractor =
            _interactorTransform.position - _rotationPivot.position;

        _previousInteractorDirection =
            Vector3.ProjectOnPlane(directionToInteractor, rotationAxis);

        if (_previousInteractorDirection.sqrMagnitude <= 0.0001f)
        {
            _isGrabbed = false;
            _interactorTransform = null;
            return;
        }

        _previousInteractorDirection.Normalize();
    }

    private void HandleSelectExited(SelectExitEventArgs eventArgs)
    {
        _isGrabbed = false;
        _interactorTransform = null;

        SnapToNearestStateIfNecessary();
    }

    private void UpdateValveRotation()
    {
        Vector3 rotationAxis = GetWorldRotationAxis();

        Vector3 directionToInteractor =
            _interactorTransform.position - _rotationPivot.position;

        Vector3 currentInteractorDirection =
            Vector3.ProjectOnPlane(directionToInteractor, rotationAxis);

        if (currentInteractorDirection.sqrMagnitude <= 0.0001f)
            return;

        currentInteractorDirection.Normalize();

        float angleDelta = Vector3.SignedAngle(
            _previousInteractorDirection,
            currentInteractorDirection,
            rotationAxis
        );

        if (Mathf.Abs(angleDelta) < _minimumRotationDelta)
            return;

        _currentAngle = Mathf.Clamp(
            _currentAngle + angleDelta,
            _closedAngle,
            _openAngle
        );

        _previousInteractorDirection = currentInteractorDirection;

        ApplyRotation();
        NotifyValueChanged();
        UpdateValveState(false);
    }

    private void ApplyRotation()
    {
        Vector3 localEulerAngles = _initialLocalRotation.eulerAngles;

        switch (_rotationAxis)
        {
            case RotationAxis.X:
                localEulerAngles.x += _currentAngle;
                break;

            case RotationAxis.Y:
                localEulerAngles.y += _currentAngle;
                break;

            case RotationAxis.Z:
                localEulerAngles.z += _currentAngle;
                break;
        }

        _rotationPivot.localRotation =
            Quaternion.Euler(localEulerAngles);
    }

    private Vector3 GetWorldRotationAxis()
    {
        return _rotationAxis switch
        {
            RotationAxis.X => _rotationPivot.right,
            RotationAxis.Y => _rotationPivot.up,
            RotationAxis.Z => _rotationPivot.forward,
            _ => _rotationPivot.up
        };
    }

    private void SnapToNearestStateIfNecessary()
    {
        if (!_snapOnRelease)
            return;

        float distanceFromClosed =
            Mathf.Abs(_currentAngle - _closedAngle);

        float distanceFromOpen =
            Mathf.Abs(_currentAngle - _openAngle);

        if (distanceFromClosed <= distanceFromOpen)
        {
            SetValveAngle(_closedAngle);
            return;
        }

        SetValveAngle(_openAngle);
    }

    private void NotifyValueChanged()
    {
        m_onValveValueChanged?.Invoke(m_normalizedValue);
    }

    private void UpdateValveState(bool initializeOnly)
    {
        ValveState newState = ValveState.Intermediate;

        if (m_isClosed)
            newState = ValveState.Closed;
        else if (m_isOpen)
            newState = ValveState.Open;

        if (newState == _currentState)
            return;

        _currentState = newState;

        if (initializeOnly)
            return;

        switch (_currentState)
        {
            case ValveState.Open:
                m_onValveOpened?.Invoke();
                break;

            case ValveState.Closed:
                m_onValveClosed?.Invoke();
                break;
        }
    }

    #endregion


    #region Private and Protected

    private enum RotationAxis
    {
        X,
        Y,
        Z
    }

    private enum ValveState
    {
        Intermediate,
        Closed,
        Open
    }

    [Header("Références")]
    [Tooltip("Pivot placé exactement sur l'axe de rotation de la poignée.")]
    [SerializeField] private Transform _rotationPivot;

    [Header("Rotation")]
    [Tooltip("Axe local autour duquel la poignée doit tourner.")]
    [SerializeField] private RotationAxis _rotationAxis = RotationAxis.Y;

    [Tooltip("Angle correspondant à la position fermée.")]
    [SerializeField] private float _closedAngle;

    [Tooltip("Angle correspondant à la position ouverte.")]
    [SerializeField] private float _openAngle = 90f;

    [Tooltip("Position initiale de la vanne.")]
    [SerializeField] private float _startingAngle;

    [Header("Comportement")]
    [Tooltip("Replace la poignée sur ouverte ou fermée lorsqu'elle est lâchée.")]
    [SerializeField] private bool _snapOnRelease;

    [Min(0f)]
    [Tooltip("Tolérance utilisée pour considérer la vanne ouverte ou fermée.")]
    [SerializeField] private float _stateTolerance = 2f;

    [Min(0f)]
    [Tooltip("Ignore les mouvements trop faibles afin d'éviter les tremblements.")]
    [SerializeField] private float _minimumRotationDelta = 0.05f;

    private XRSimpleInteractable _interactable;
    private Transform _interactorTransform;

    private Vector3 _previousInteractorDirection;
    private Quaternion _initialLocalRotation;

    private ValveState _currentState = ValveState.Intermediate;

    private float _currentAngle;
    private bool _isGrabbed;

    #endregion
}