using UnityEngine;
using UnityEngine.Events;

public class EmissiveLightSequence : MonoBehaviour
{
    #region Publics

    [Header("Événements")]
    public UnityEvent m_onSequenceStarted;
    public UnityEvent m_onOrangeBlinkFinished;
    public UnityEvent m_onSequenceFinished;

    #endregion


    #region API Unity

    private void Awake()
    {
        _orangePropertyBlock = new MaterialPropertyBlock();
        _greenPropertyBlock = new MaterialPropertyBlock();

        TurnOffAllLights();
    }

    private void Update()
    {
        if (!_isRunning)
            return;

        _timer -= Time.deltaTime;

        if (_timer > 0f)
            return;

        switch (_currentState)
        {
            case SequenceState.OrangeOn:
                StartOrangeOff();
                break;

            case SequenceState.OrangeOff:
                CompleteOrangeOff();
                break;

            case SequenceState.GreenAndOrangeOn:
                CompleteSequence();
                break;
        }
    }

    #endregion


    #region Utils (méthodes publics)

    /// <summary>
    /// Lance la séquence complète :
    /// orange clignotant, puis orange et vert fixes.
    /// </summary>
    public void StartSequence()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _completedBlinks = 0;

        SetGreenEmission(false);
        StartOrangeOn();

        m_onSequenceStarted?.Invoke();
    }

    /// <summary>
    /// Arrête immédiatement la séquence et éteint les voyants.
    /// </summary>
    public void StopSequence()
    {
        _isRunning = false;
        _currentState = SequenceState.Inactive;
        _timer = 0f;
        _completedBlinks = 0;

        TurnOffAllLights();
    }

    /// <summary>
    /// Redémarre la séquence depuis le début.
    /// </summary>
    public void RestartSequence()
    {
        StopSequence();
        StartSequence();
    }

    /// <summary>
    /// Active ou arrête la séquence selon son état actuel.
    /// </summary>
    public void ToggleSequence()
    {
        if (_isRunning)
        {
            StopSequence();
            return;
        }

        StartSequence();
    }

    #endregion


    #region Main Methods (méthodes private)

    private void StartOrangeOn()
    {
        _currentState = SequenceState.OrangeOn;
        _timer = _orangeOnDuration;

        SetOrangeEmission(true);
    }

    private void StartOrangeOff()
    {
        _currentState = SequenceState.OrangeOff;
        _timer = _orangeOffDuration;

        SetOrangeEmission(false);
        _completedBlinks++;
    }

    private void CompleteOrangeOff()
    {
        if (_completedBlinks >= _orangeBlinkCount)
        {
            m_onOrangeBlinkFinished?.Invoke();
            StartGreenAndOrangeOn();
            return;
        }

        StartOrangeOn();
    }

    private void StartGreenAndOrangeOn()
    {
        _currentState = SequenceState.GreenAndOrangeOn;
        _timer = _greenDuration;

        // Les deux voyants restent allumés pendant la même durée.
        SetOrangeEmission(true);
        SetGreenEmission(true);

        if (!_keepGreenOn)
            return;

        // Lorsque cette option est active, les deux voyants restent allumés.
        _isRunning = false;
        _currentState = SequenceState.Inactive;
        _timer = 0f;

        m_onSequenceFinished?.Invoke();
    }

    private void CompleteSequence()
    {
        SetOrangeEmission(false);
        SetGreenEmission(false);

        _isRunning = false;
        _currentState = SequenceState.Inactive;
        _timer = 0f;

        m_onSequenceFinished?.Invoke();
    }

    private void TurnOffAllLights()
    {
        SetOrangeEmission(false);
        SetGreenEmission(false);
    }

    private void SetOrangeEmission(bool isEnabled)
    {
        SetEmission(
            _orangeRenderer,
            _orangePropertyBlock,
            _orangeColor,
            _orangeIntensity,
            isEnabled
        );
    }

    private void SetGreenEmission(bool isEnabled)
    {
        SetEmission(
            _greenRenderer,
            _greenPropertyBlock,
            _greenColor,
            _greenIntensity,
            isEnabled
        );
    }

    private void SetEmission(
        Renderer targetRenderer,
        MaterialPropertyBlock propertyBlock,
        Color emissionColor,
        float intensity,
        bool isEnabled)
    {
        if (!targetRenderer)
            return;

        targetRenderer.GetPropertyBlock(propertyBlock);

        Color finalColor = isEnabled
            ? emissionColor * intensity
            : Color.black;

        propertyBlock.SetColor(_emissionColorId, finalColor);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }

    #endregion


    #region Private and Protected

    private enum SequenceState
    {
        Inactive,
        OrangeOn,
        OrangeOff,
        GreenAndOrangeOn
    }

    [Header("Voyants")]
    [SerializeField] private Renderer _orangeRenderer;
    [SerializeField] private Renderer _greenRenderer;

    [Header("Couleurs")]
    [ColorUsage(true, true)]
    [SerializeField] private Color _orangeColor = new(1f, 0.25f, 0f);

    [ColorUsage(true, true)]
    [SerializeField] private Color _greenColor = Color.green;

    [Header("Intensités")]
    [Min(0f)]
    [SerializeField] private float _orangeIntensity = 5f;

    [Min(0f)]
    [SerializeField] private float _greenIntensity = 5f;

    [Header("Clignotement orange")]
    [Min(1)]
    [SerializeField] private int _orangeBlinkCount = 3;

    [Min(0.01f)]
    [SerializeField] private float _orangeOnDuration = 0.3f;

    [Min(0.01f)]
    [SerializeField] private float _orangeOffDuration = 0.3f;

    [Header("Voyants orange et vert")]
    [Min(0f)]
    [SerializeField] private float _greenDuration = 2f;

    [Tooltip("Les voyants orange et vert restent allumés après la séquence.")]
    [SerializeField] private bool _keepGreenOn;

    private static readonly int _emissionColorId =
        Shader.PropertyToID("_EmissionColor");

    private MaterialPropertyBlock _orangePropertyBlock;
    private MaterialPropertyBlock _greenPropertyBlock;

    private SequenceState _currentState = SequenceState.Inactive;

    private float _timer;
    private int _completedBlinks;
    private bool _isRunning;

    #endregion
}