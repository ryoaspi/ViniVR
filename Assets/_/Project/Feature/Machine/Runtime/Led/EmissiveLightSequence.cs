using UnityEngine;
using UnityEngine.Events;

namespace Machine.Runtime
{
    public class EmissiveLightSequence : MonoBehaviour
    {
        #region Publics

        [Header("Événements")]
        public UnityEvent m_onSequenceStarted;
        public UnityEvent m_onOrangeLightActivated;
        public UnityEvent m_onGreenLightActivated;
        public UnityEvent m_onLightsReady;
        public UnityEvent m_onLightsTurnedOff;

        public bool m_areLightsReady => _currentState == LightSequenceState.Ready;

        #endregion


        #region API Unity

        private void Awake()
        {
            InitializePropertyBlocks();
            TurnOffLights();
        }

        private void Update()
        {
            UpdateSequence();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            _orangeIntensity = Mathf.Max(0f, _orangeIntensity);
            _greenIntensity = Mathf.Max(0f, _greenIntensity);
            _delayBeforeGreenLight = Mathf.Max(0f, _delayBeforeGreenLight);
        }

#endif

        #endregion


        #region Utils

        public void StartSequence()
        {
            
            if (_currentState != LightSequenceState.Off)
            {
                return;
            }

            _elapsedTime = 0f;
            _hasLightsReadyEventBeenInvoked = false;
            _currentState = LightSequenceState.WaitingForGreen;

            SetOrangeLight(true);
            SetGreenLight(false);

            m_onSequenceStarted?.Invoke();
            m_onOrangeLightActivated?.Invoke();

            if (_delayBeforeGreenLight <= 0f)
            {
                ActivateGreenLight();
            }
        }

        public void TurnOffLights()
        {
            _elapsedTime = 0f;
            _hasLightsReadyEventBeenInvoked = false;
            _currentState = LightSequenceState.Off;

            SetOrangeLight(false);
            SetGreenLight(false);

            m_onLightsTurnedOff?.Invoke();
        }

        #endregion


        #region Main Methods

        private void UpdateSequence()
        {
            if (_currentState != LightSequenceState.WaitingForGreen)
            {
                return;
            }

            _elapsedTime += Time.deltaTime;

            if (_elapsedTime < _delayBeforeGreenLight)
            {
                return;
            }

            ActivateGreenLight();
        }

        private void ActivateGreenLight()
        {
            SetGreenLight(true);

            _currentState = LightSequenceState.Ready;

            m_onGreenLightActivated?.Invoke();

            NotifyLightsReady();
        }

        private void NotifyLightsReady()
        {
            if (_hasLightsReadyEventBeenInvoked)
            {
                return;
            }

            _hasLightsReadyEventBeenInvoked = true;

            m_onLightsReady?.Invoke();
        }

        private void InitializePropertyBlocks()
        {
            _orangePropertyBlock = new MaterialPropertyBlock();
            _greenPropertyBlock = new MaterialPropertyBlock();

            _emissionPropertyId = Shader.PropertyToID(_emissionPropertyName);
        }

        private void SetOrangeLight(bool isActive)
        {
            SetEmission(
                _orangeRenderer,
                _orangePropertyBlock,
                _orangeColor,
                _orangeIntensity,
                isActive);
        }

        private void SetGreenLight(bool isActive)
        {
            SetEmission(
                _greenRenderer,
                _greenPropertyBlock,
                _greenColor,
                _greenIntensity,
                isActive);
        }

        private void SetEmission(
            Renderer targetRenderer,
            MaterialPropertyBlock propertyBlock,
            Color emissionColor,
            float intensity,
            bool isActive)
        {
            if (!targetRenderer || propertyBlock == null)
            {
                return;
            }

            targetRenderer.GetPropertyBlock(propertyBlock);

            Color finalColor = isActive
                ? emissionColor * intensity
                : Color.black;

            propertyBlock.SetColor(_emissionPropertyId, finalColor);

            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        #endregion


        #region Private and Protected

        private enum LightSequenceState
        {
            Off,
            WaitingForGreen,
            Ready
        }
        
        [Header("Renderers")]
        [SerializeField] private Renderer _orangeRenderer;
        [SerializeField] private Renderer _greenRenderer;

        [Header("Couleurs HDR")]
        [ColorUsage(true, true)]
        [SerializeField] private Color _orangeColor = new(1f, 0.25f, 0f, 1f);

        [ColorUsage(true, true)]
        [SerializeField] private Color _greenColor = Color.green;

        [Header("Intensités")]
        [Min(0f)]
        [SerializeField] private float _orangeIntensity = 5f;

        [Min(0f)]
        [SerializeField] private float _greenIntensity = 5f;

        [Header("Temporisation")]
        [Min(0f)]
        [SerializeField] private float _delayBeforeGreenLight = 1f;

        [Header("Shader")]
        [SerializeField] private string _emissionPropertyName = "_EmissionColor";

        private LightSequenceState _currentState = LightSequenceState.Off;

        private MaterialPropertyBlock _orangePropertyBlock;
        private MaterialPropertyBlock _greenPropertyBlock;

        private int _emissionPropertyId;
        private float _elapsedTime;
        private bool _hasLightsReadyEventBeenInvoked;

        #endregion
    }
}