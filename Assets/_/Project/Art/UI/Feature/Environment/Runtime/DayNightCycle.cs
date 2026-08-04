using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class DayNightCycle : MonoBehaviour
{
    #region Publics

    [Header("Référence principale")]
    [Tooltip("Directional Light utilisée comme soleil.")]
    public Light m_sun;

    [Header("Configuration du cycle")]
    [Tooltip("Durée complète d'une journée en secondes réelles.")]
    [Min(1f)]
    public float m_dayDuration = 600f;

    [Tooltip("Heure au lancement de la scène.")]
    [Range(0f, 24f)]
    public float m_startHour = 8f;

    [Tooltip("Lance automatiquement le cycle au démarrage.")]
    public bool m_playOnStart = true;

    [Header("Orientation du soleil")]
    [Tooltip("Orientation horizontale de la trajectoire du soleil.")]
    [Range(0f, 360f)]
    public float m_sunYaw = 170f;

    [Header("Éclairage solaire")]
    [Min(0f)]
    public float m_maxSunIntensity = 1.2f;

    [Tooltip("Couleur du soleil selon l'heure normalisée : minuit, matin, midi, soir, minuit.")]
    public Gradient m_sunColor;

    [Header("Éclairage ambiant")]
    [Tooltip("Couleur ambiante selon l'heure.")]
    public Gradient m_ambientColor;

    [Range(0f, 8f)]
    public float m_maxAmbientIntensity = 1f;

    [Min(0f)]
    public float m_minAmbientIntensity = 0.08f;

    [Header("Brouillard")]
    public bool m_controlFog = true;

    [Tooltip("Couleur du brouillard selon l'heure.")]
    public Gradient m_fogColor;

    [Header("Optimisation VR")]
    [Tooltip("Temps entre deux mises à jour visuelles. 0,1 seconde équivaut à 10 mises à jour par seconde.")]
    [Range(0.02f, 1f)]
    public float m_visualUpdateInterval = 0.1f;

    public float m_currentHour => _normalizedTime * 24f;

    public bool m_isRunning => _isRunning;

    #endregion


    #region API Unity

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        RenderSettings.sun = m_sun;
        RenderSettings.ambientMode = AmbientMode.Flat;

        _normalizedTime = Mathf.Repeat(m_startHour / 24f, 1f);
        _isRunning = m_playOnStart;

        ApplyEnvironment();
    }

    private void Update()
    {
        if (!_isRunning)
            return;

        UpdateTime();

        _visualUpdateTimer += Time.deltaTime;

        if (_visualUpdateTimer < m_visualUpdateInterval)
            return;

        _visualUpdateTimer = 0f;
        ApplyEnvironment();
    }

    private void OnValidate()
    {
        m_dayDuration = Mathf.Max(1f, m_dayDuration);
        m_visualUpdateInterval = Mathf.Max(0.02f, m_visualUpdateInterval);
        m_maxSunIntensity = Mathf.Max(0f, m_maxSunIntensity);
        m_maxAmbientIntensity = Mathf.Max(0f, m_maxAmbientIntensity);
        m_minAmbientIntensity = Mathf.Max(0f, m_minAmbientIntensity);
    }

    #endregion


    #region Utils

    public void SetTime(float hour)
    {
        _normalizedTime = Mathf.Repeat(hour / 24f, 1f);
        ApplyEnvironment();
    }

    public void SetCycleRunning(bool isRunning)
    {
        _isRunning = isRunning;
    }

    public void ToggleCycle()
    {
        _isRunning = !_isRunning;
    }

    #endregion


    #region Main Methods

    private void UpdateTime()
    {
        float normalizedSpeed = 1f / m_dayDuration;

        _normalizedTime += Time.deltaTime * normalizedSpeed;
        _normalizedTime = Mathf.Repeat(_normalizedTime, 1f);
    }

    private void ApplyEnvironment()
    {
        ApplySunRotation();
        ApplySunLighting();
        ApplyAmbientLighting();
        ApplyFog();
    }

    private void ApplySunRotation()
    {
        float sunPitch = (_normalizedTime * 360f) - 90f;

        m_sun.transform.rotation = Quaternion.Euler(
            sunPitch,
            m_sunYaw,
            0f);
    }

    private void ApplySunLighting()
    {
        float sunHeight = Vector3.Dot(
            -m_sun.transform.forward,
            Vector3.up);

        float daylightFactor = Mathf.Clamp01(sunHeight);

        m_sun.intensity = daylightFactor * m_maxSunIntensity;
        m_sun.color = m_sunColor.Evaluate(_normalizedTime);
        m_sun.enabled = daylightFactor > 0.001f;
    }

    private void ApplyAmbientLighting()
    {
        float sunHeight = Vector3.Dot(
            -m_sun.transform.forward,
            Vector3.up);

        float daylightFactor = Mathf.Clamp01(sunHeight);

        RenderSettings.ambientLight =
            m_ambientColor.Evaluate(_normalizedTime);

        RenderSettings.ambientIntensity = Mathf.Lerp(
            m_minAmbientIntensity,
            m_maxAmbientIntensity,
            daylightFactor);
    }

    private void ApplyFog()
    {
        if (!m_controlFog)
            return;

        RenderSettings.fogColor =
            m_fogColor.Evaluate(_normalizedTime);
    }

    private bool ValidateReferences()
    {
        if (m_sun == null)
        {
            Debug.LogError(
                "[DayNightCycle] Aucun soleil n'est assigné.",
                this);

            return false;
        }

        if (m_sun.type != LightType.Directional)
        {
            Debug.LogError(
                "[DayNightCycle] La lumière assignée doit être une Directional Light.",
                m_sun);

            return false;
        }

        return true;
    }

    #endregion


    #region Private and Protected

    private float _normalizedTime;
    private float _visualUpdateTimer;
    private bool _isRunning;

    #endregion
}