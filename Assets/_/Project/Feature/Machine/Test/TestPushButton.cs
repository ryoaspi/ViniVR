using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class TestPushButton : MonoBehaviour
{
    #region Publics

    [Header("Références")]
    public Transform m_buttonVisual;

    [Header("Paramètres de pression")]
    [Min(0f)]
    public float m_pressDistance = 0.015f;

    [Min(0f)]
    public float m_movementSpeed = 10f;

    [Min(0.01f)]
    public float m_pressDuration = 0.15f;

    [Header("Événement")]
    public UnityEvent m_onPressed;

    #endregion

    #region API Unity

    private void Awake()
    {
        _interactable = GetComponent<XRBaseInteractable>();

        if (m_buttonVisual == null)
        {
            Debug.LogError(
                "[TestPushButton] Aucun visuel de bouton n'est assigné.",
                this);

            enabled = false;
            return;
        }

        _initialLocalPosition = m_buttonVisual.localPosition;
    }

    private void OnEnable()
    {
        if (_interactable == null)
            return;

        _interactable.selectEntered.AddListener(HandleSelectEntered);
    }

    private void OnDisable()
    {
        if (_interactable != null)
            _interactable.selectEntered.RemoveListener(HandleSelectEntered);

        CancelInvoke(nameof(ReleaseButton));

        _isPressed = false;
        _canPress = true;
    }

    private void Update()
    {
        UpdateButtonVisual();
    }

    #endregion

    #region Utils (méthodes publics)

    public void PressButton()
    {
        if (!_canPress)
            return;

        _canPress = false;
        _isPressed = true;

        m_onPressed?.Invoke();

        CancelInvoke(nameof(ReleaseButton));
        Invoke(nameof(ReleaseButton), m_pressDuration);
    }

    public void ResetButton()
    {
        CancelInvoke(nameof(ReleaseButton));

        _isPressed = false;
        _canPress = true;

        if (m_buttonVisual != null)
            m_buttonVisual.localPosition = _initialLocalPosition;
    }

    #endregion

    #region Main Methods (méthodes private)

    private void HandleSelectEntered(SelectEnterEventArgs args)
    {
        PressButton();
    }

    private void ReleaseButton()
    {
        _isPressed = false;
        _canPress = true;
        
    }

    private void UpdateButtonVisual()
    {
        if (!m_buttonVisual)
            return;

        Vector3 pressedPosition =
            _initialLocalPosition + Vector3.down * m_pressDistance;

        Vector3 targetPosition = _isPressed
            ? pressedPosition
            : _initialLocalPosition;

        m_buttonVisual.localPosition = Vector3.Lerp(
            m_buttonVisual.localPosition,
            targetPosition,
            Time.deltaTime * m_movementSpeed);
    }

    #endregion

    #region Private and Protected

    private Vector3 _initialLocalPosition;

    private bool _isPressed;
    private bool _canPress = true;

    private XRBaseInteractable _interactable;

    #endregion
}