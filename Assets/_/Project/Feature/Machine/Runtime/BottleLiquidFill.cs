using UnityEngine;

public class BottleLiquidFill : MonoBehaviour
{
    #region Publics

    public float m_currentFill => _currentFill;

    #endregion


    #region API Unity

    private void Awake()
    {
        Initialize();
        SetFillImmediate(_startFill);
        SetFillingParticles(false);
    }

    private void Update()
    {
        if (!_isFilling)
            return;

        UpdateFill();
    }

    #endregion


    #region Utils

    /// <summary>
    /// Lance le remplissage jusqu'à la valeur cible configurée.
    /// </summary>
    public void FillBottle()
    {
        StartFill(_targetFill);
    }

    /// <summary>
    /// Définit immédiatement le niveau du liquide.
    /// </summary>
    public void SetFillImmediate(float value)
    {
        _currentFill = Mathf.Clamp01(value);
        _isFilling = false;

        ApplyFill();
        SetFillingParticles(false);
    }

    /// <summary>
    /// Vide immédiatement la bouteille.
    /// </summary>
    public void EmptyBottle()
    {
        SetFillImmediate(0f);
    }

    #endregion


    #region Main Methods

    private void Initialize()
    {
        if (_liquidRenderer == null)
        {
            Debug.LogError(
                $"[{nameof(BottleLiquidFill)}] Aucun Renderer assigné.",
                this);

            enabled = false;
            return;
        }

        Material material = _liquidRenderer.sharedMaterial;

        if (material == null)
        {
            Debug.LogError(
                $"[{nameof(BottleLiquidFill)}] Aucun Material trouvé.",
                this);

            enabled = false;
            return;
        }

        if (!material.HasProperty(_fillProperty))
        {
            Debug.LogError(
                $"[{nameof(BottleLiquidFill)}] " +
                $"La propriété shader '{_fillProperty}' est introuvable.",
                this);

            enabled = false;
        }
    }

    private void StartFill(float target)
    {
        target = Mathf.Clamp01(target);

        if (_fillDuration <= 0f)
        {
            SetFillImmediate(target);
            return;
        }

        _startValue = _currentFill;
        _targetValue = target;

        _fillTimer = 0f;
        _isFilling = true;

        SetFillingParticles(true);
    }

    private void UpdateFill()
    {
        _fillTimer += Time.deltaTime;

        float t = Mathf.Clamp01(
            _fillTimer / _fillDuration);

        _currentFill = Mathf.Lerp(
            _startValue,
            _targetValue,
            t);

        ApplyFill();

        if (t < 1f)
            return;

        _currentFill = _targetValue;
        _isFilling = false;

        ApplyFill();

        SetFillingParticles(false);
    }

    private void ApplyFill()
    {
        if (!_liquidRenderer)
            return;

        _liquidRenderer.material.SetFloat(
            _fillProperty,
            _currentFill);
    }

    private void SetFillingParticles(bool active)
    {
        if (!_remplissageParticule)
            return;

        _remplissageParticule.SetActive(active);
    }

    #endregion


    #region Private and Protected

    [Header("Liquid")]
    [SerializeField]
    private Renderer _liquidRenderer;

    [Header("Shader")]
    [Tooltip("Nom de la propriété qui contrôle le niveau du liquide.")]
    [SerializeField]
    private string _fillProperty = "_FillAmount";

    [Header("Fill")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _startFill = 0f;

    [Range(0f, 1f)]
    [SerializeField]
    private float _targetFill = 1f;

    [Tooltip("Durée du remplissage en secondes.")]
    [Min(0f)]
    [SerializeField]
    private float _fillDuration = 1f;

    [Header("Particules")]
    [SerializeField]
    private GameObject _remplissageParticule;

    private float _currentFill;
    private float _startValue;
    private float _targetValue;
    private float _fillTimer;

    private bool _isFilling;

    #endregion
}