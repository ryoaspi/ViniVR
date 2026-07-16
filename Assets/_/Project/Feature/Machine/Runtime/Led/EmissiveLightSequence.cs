using UnityEngine;
using UnityEngine.Events;

public class EmissiveLightSequence : MonoBehaviour
{
    [Header("Voyants")]
    [SerializeField] private Renderer orangeRenderer;
    [SerializeField] private Renderer greenRenderer;

    [Header("Couleurs")]
    [ColorUsage(true, true)]
    [SerializeField] private Color orangeColor = new Color(1f, 0.25f, 0f);

    [ColorUsage(true, true)]
    [SerializeField] private Color greenColor = Color.green;

    [Header("Intensités")]
    [Min(0f)]
    [SerializeField] private float orangeIntensity = 5f;

    [Min(0f)]
    [SerializeField] private float greenIntensity = 5f;

    [Header("Clignotement orange")]
    [Min(1)]
    [SerializeField] private int orangeBlinkCount = 3;

    [Min(0.01f)]
    [SerializeField] private float orangeOnDuration = 0.3f;

    [Min(0.01f)]
    [SerializeField] private float orangeOffDuration = 0.3f;

    [Header("Voyant vert")]
    [Min(0f)]
    [SerializeField] private float greenDuration = 2f;

    [Tooltip("Le voyant vert reste allumé après la fin de la séquence.")]
    [SerializeField] private bool keepGreenOn = false;

    [Header("Événements")]
    public UnityEvent OnSequenceStarted;
    public UnityEvent OnOrangeBlinkFinished;
    public UnityEvent OnSequenceFinished;

    private static readonly int EmissionColorID =
        Shader.PropertyToID("_EmissionColor");

    private MaterialPropertyBlock _orangePropertyBlock;
    private MaterialPropertyBlock _greenPropertyBlock;

    private SequenceState _currentState = SequenceState.Inactive;

    private float _timer;
    private int _completedBlinks;
    private bool _isRunning;

    private enum SequenceState
    {
        Inactive,
        OrangeOn,
        OrangeOff,
        GreenOn
    }

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

            case SequenceState.GreenOn:
                CompleteSequence();
                break;
        }
    }

    /// <summary>
    /// Lance la séquence complète :
    /// orange clignotant, puis vert fixe.
    /// </summary>
    public void StartSequence()
    {
        if ( _isRunning)
            return;

        _isRunning = true;
        _completedBlinks = 0;

        SetGreenEmission(false);
        StartOrangeOn();

        OnSequenceStarted?.Invoke();
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
    /// Permet de recommencer la séquence, même si elle est déjà active.
    /// </summary>
    public void RestartSequence()
    {
        StopSequence();
        StartSequence();
    }

    public void ToggleSequence()
    {
        if (_isRunning)
        {
            StopSequence();
        }
        else
        {
            StartSequence();
        }
    }

    private void StartOrangeOn()
    {
        _currentState = SequenceState.OrangeOn;
        _timer = orangeOnDuration;

        SetOrangeEmission(true);
    }

    private void StartOrangeOff()
    {
        _currentState = SequenceState.OrangeOff;
        _timer = orangeOffDuration;

        SetOrangeEmission(false);
        _completedBlinks++;
    }

    private void CompleteOrangeOff()
    {
        if (_completedBlinks >= orangeBlinkCount)
        {
            OnOrangeBlinkFinished?.Invoke();
            StartGreenOn();
            return;
        }

        StartOrangeOn();
    }

    private void StartGreenOn()
    {
        _currentState = SequenceState.GreenOn;
        _timer = greenDuration;

        SetOrangeEmission(false);
        SetGreenEmission(true);

        if (keepGreenOn)
        {
            _isRunning = false;
            _currentState = SequenceState.Inactive;

            OnSequenceFinished?.Invoke();
        }
    }

    private void CompleteSequence()
    {
        SetGreenEmission(false);

        _isRunning = false;
        _currentState = SequenceState.Inactive;

        OnSequenceFinished?.Invoke();
    }

    private void TurnOffAllLights()
    {
        SetOrangeEmission(false);
        SetGreenEmission(false);
    }

    private void SetOrangeEmission(bool isEnabled)
    {
        SetEmission(
            orangeRenderer,
            _orangePropertyBlock,
            orangeColor,
            orangeIntensity,
            isEnabled
        );
    }

    private void SetGreenEmission(bool isEnabled)
    {
        SetEmission(
            greenRenderer,
            _greenPropertyBlock,
            greenColor,
            greenIntensity,
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

        propertyBlock.SetColor(EmissionColorID, finalColor);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }
}