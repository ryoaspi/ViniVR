using UnityEngine;

namespace Machine.Runtime
{
    public class LiquidLevelVisualizer : MonoBehaviour
    {
        #region Publics

        public float m_normalizedLevel =>
            _liquidContainer != null
                ? _liquidContainer.m_normalizedVolume
                : 0f;

        public bool m_isFilling =>
            _isFilling;

        #endregion


        #region API Unity

        private void Awake()
        {
            InitializeVisual();
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void Start()
        {
            RefreshVisual();
        }

        private void Update()
        {
            UpdateFilling();
        }

        private void OnDisable()
        {
            UnregisterEvents();
            _isFilling = false;
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            _minimumHeight = Mathf.Max(
                0.0001f,
                _minimumHeight);

            _maximumHeight = Mathf.Max(
                _minimumHeight,
                _maximumHeight);

            _fillRatePerSecond = Mathf.Max(
                0f,
                _fillRatePerSecond);

            _emptyVisibilityTolerance = Mathf.Clamp01(
                _emptyVisibilityTolerance);
        }

#endif

        #endregion


        #region Utils (méthodes publics)

        public void BeginFilling()
        {
            if (_liquidContainer == null)
            {
                Debug.LogWarning(
                    $"[{nameof(LiquidLevelVisualizer)}] " +
                    "Impossible de démarrer le remplissage : " +
                    "aucun LiquidContainer n'est assigné.",
                    this);

                return;
            }

            if (_liquidContainer.m_isFull)
            {
                Debug.Log(
                    $"[{nameof(LiquidLevelVisualizer)}] " +
                    $"Le conteneur {_liquidContainer.m_containerName} " +
                    "est déjà plein.",
                    this);

                return;
            }

            _isFilling = true;
            
        }

        public void PauseLevel()
        {
            _isFilling = false;
        }

        public void RefreshVisual()
        {
            if (_liquidContainer == null)
            {
                Debug.LogWarning(
                    $"[{nameof(LiquidLevelVisualizer)}] " +
                    "Aucun LiquidContainer n'est assigné.",
                    this);

                return;
            }

            ApplyLiquidLevel(
                _liquidContainer.m_normalizedVolume);
        }

        public void SetLevel(float normalizedLevel)
        {
            if (_liquidContainer == null)
            {
                return;
            }

            float targetVolume =
                Mathf.Clamp01(normalizedLevel) *
                _liquidContainer.m_maximumVolume;

            _liquidContainer.SetVolume(targetVolume);
        }

        #endregion


        #region Main Methods (méthodes private)

        private void InitializeVisual()
        {
            if (_liquidVisual == null)
            {
                Debug.LogError(
                    $"[{nameof(LiquidLevelVisualizer)}] " +
                    "Aucun Transform Liquid Visual n'est assigné.",
                    this);

                enabled = false;
                return;
            }

            MeshFilter meshFilter =
                _liquidVisual.GetComponent<MeshFilter>();

            if (meshFilter == null ||
                meshFilter.sharedMesh == null)
            {
                Debug.LogError(
                    $"[{nameof(LiquidLevelVisualizer)}] " +
                    "Le Liquid Visual doit posséder un MeshFilter.",
                    _liquidVisual);

                enabled = false;
                return;
            }

            _meshHeight =
                meshFilter.sharedMesh.bounds.size.y;

            if (_meshHeight <= Mathf.Epsilon)
            {
                Debug.LogError(
                    $"[{nameof(LiquidLevelVisualizer)}] " +
                    "La hauteur du mesh est invalide.",
                    _liquidVisual);

                enabled = false;
                return;
            }

            _originalLocalPosition =
                _liquidVisual.localPosition;

            _bottomLocalPositionY =
                _originalLocalPosition.y -
                (_maximumHeight * 0.5f);

            _isInitialized = true;
        }

        private void RegisterEvents()
        {
            if (_liquidContainer == null)
            {
                return;
            }

            _liquidContainer.m_onVolumeChanged.AddListener(
                HandleVolumeChanged);

            _liquidContainer.m_onContainerFull.AddListener(
                HandleContainerFull);
        }

        private void UnregisterEvents()
        {
            if (_liquidContainer == null)
            {
                return;
            }

            _liquidContainer.m_onVolumeChanged.RemoveListener(
                HandleVolumeChanged);

            _liquidContainer.m_onContainerFull.RemoveListener(
                HandleContainerFull);
        }

        private void UpdateFilling()
        {
            if (!_isFilling ||
                _liquidContainer == null)
            {
                return;
            }

            if (_liquidContainer.m_isFull)
            {
                _isFilling = false;
                return;
            }

            float amountToAdd =
                _fillRatePerSecond *
                Time.deltaTime;

            _liquidContainer.AddLiquid(
                amountToAdd);
        }

        private void HandleVolumeChanged(
            float normalizedVolume)
        {
            ApplyLiquidLevel(normalizedVolume);
        }

        private void HandleContainerFull()
        {
            _isFilling = false;
            
        }

        private void ApplyLiquidLevel(
            float normalizedVolume)
        {
            if (!_isInitialized ||
                _liquidVisual == null)
            {
                return;
            }

            float clampedVolume =
                Mathf.Clamp01(normalizedVolume);

            float targetHeight = Mathf.Lerp(
                _minimumHeight,
                _maximumHeight,
                clampedVolume);

            float targetScaleY =
                targetHeight / _meshHeight;

            Vector3 targetScale =
                _liquidVisual.localScale;

            targetScale.y = targetScaleY;

            _liquidVisual.localScale =
                targetScale;

            Vector3 targetPosition =
                _originalLocalPosition;

            targetPosition.y =
                _bottomLocalPositionY +
                (targetHeight * 0.5f);

            _liquidVisual.localPosition =
                targetPosition;

            ApplyVisibility(clampedVolume);
        }

        private void ApplyVisibility(
            float normalizedVolume)
        {
            if (!_hideWhenEmpty)
            {
                _liquidVisual.gameObject.SetActive(true);
                return;
            }

            bool shouldBeVisible =
                normalizedVolume >
                _emptyVisibilityTolerance;

            _liquidVisual.gameObject.SetActive(
                shouldBeVisible);
        }

        #endregion


        #region Private and Protected

        [Header("Références")]
        [SerializeField]
        private LiquidContainer _liquidContainer;

        [SerializeField]
        private Transform _liquidVisual;

        [Header("Remplissage")]
        [Tooltip(
            "Quantité de liquide ajoutée chaque seconde.")]
        [Min(0f)]
        [SerializeField]
        private float _fillRatePerSecond = 10f;

        [Header("Dimensions locales")]
        [Tooltip(
            "Hauteur visuelle minimale du liquide.")]
        [Min(0.0001f)]
        [SerializeField]
        private float _minimumHeight = 0.001f;

        [Tooltip(
            "Hauteur visuelle du liquide lorsque la cuve est pleine.")]
        [Min(0.0001f)]
        [SerializeField]
        private float _maximumHeight = 2.6f;

        [Header("Visibilité")]
        [SerializeField]
        private bool _hideWhenEmpty = true;

        [Range(0f, 1f)]
        [SerializeField]
        private float _emptyVisibilityTolerance = 0.001f;

        private Vector3 _originalLocalPosition;

        private float _bottomLocalPositionY;
        private float _meshHeight = 1f;

        private bool _isInitialized;
        private bool _isFilling;

        #endregion
    }
}