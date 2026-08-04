using UnityEngine;

namespace Machine.Runtime
{
    public class LiquidLevelVisualizer : MonoBehaviour
    {
        #region Publics

        public float m_currentLevel => _currentLevel;

        #endregion


        #region API Unity

        private void Awake()
        {
            ValidateReferences();
            CacheInitialTransform();
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void Start()
        {
            RefreshFromContainer();
        }

        private void OnDisable()
        {
            UnregisterEvents();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            _minimumHeight = Mathf.Max(
                0.001f,
                _minimumHeight);

            _maximumHeight = Mathf.Max(
                _minimumHeight,
                _maximumHeight);

            if (_liquidContainer == null)
            {
                _liquidContainer =
                    GetComponentInParent<LiquidContainer>();
            }
        }

#endif

        #endregion


        #region Utils (méthodes publics)

        /// <summary>
        /// Définit directement le niveau visuel avec une valeur normalisée
        /// comprise entre 0 et 1.
        /// </summary>
        /// <param name="normalizedLevel">
        /// Niveau normalisé :
        /// 0 correspond à un conteneur vide ;
        /// 1 correspond à un conteneur plein.
        /// </param>
        public void SetLevel(float normalizedLevel)
        {
            ApplyLevel(
                Mathf.Clamp01(normalizedLevel));
        }

        /// <summary>
        /// Synchronise immédiatement le visuel avec le volume actuel
        /// du LiquidContainer.
        /// </summary>
        public void RefreshFromContainer()
        {
            if (_liquidContainer == null)
            {
                return;
            }

            SetLevel(
                _liquidContainer.m_normalizedVolume);
        }

        #endregion


        #region Main Methods (méthodes private)

        private void RegisterEvents()
        {
            if (_liquidContainer == null)
            {
                return;
            }

            _liquidContainer.m_onVolumeChanged.AddListener(
                SetLevel);
        }

        private void UnregisterEvents()
        {
            if (_liquidContainer == null)
            {
                return;
            }

            _liquidContainer.m_onVolumeChanged.RemoveListener(
                SetLevel);
        }

        private void ApplyLevel(float normalizedLevel)
        {
            if (_liquidVisual == null)
            {
                return;
            }

            _currentLevel = normalizedLevel;

            float currentHeight = Mathf.Lerp(
                _minimumHeight,
                _maximumHeight,
                _currentLevel);

            Vector3 newScale = _initialLocalScale;
            newScale.y = currentHeight;

            _liquidVisual.localScale = newScale;

            Vector3 newPosition = _initialLocalPosition;

            newPosition.y =
                _bottomLocalPosition +
                currentHeight * 0.5f;

            _liquidVisual.localPosition = newPosition;

            UpdateVisualVisibility();
        }

        private void UpdateVisualVisibility()
        {
            if (!_hideWhenEmpty || _liquidVisual == null)
            {
                return;
            }

            bool shouldBeVisible =
                _currentLevel > _emptyVisibilityTolerance;

            if (_liquidVisual.gameObject.activeSelf ==
                shouldBeVisible)
            {
                return;
            }

            _liquidVisual.gameObject.SetActive(
                shouldBeVisible);
        }

        private void CacheInitialTransform()
        {
            if (_liquidVisual == null)
            {
                return;
            }

            _initialLocalScale =
                _liquidVisual.localScale;

            _initialLocalPosition =
                _liquidVisual.localPosition;

            _bottomLocalPosition =
                _initialLocalPosition.y -
                _initialLocalScale.y * 0.5f;
        }

        private void ValidateReferences()
        {
            if (_liquidContainer == null)
            {
                _liquidContainer =
                    GetComponentInParent<LiquidContainer>();
            }

            if (_liquidContainer == null)
            {
                Debug.LogError(
                    $"[{nameof(LiquidLevelVisualizer)}] " +
                    $"Aucun {nameof(LiquidContainer)} n'est assigné.",
                    this);
            }

            if (_liquidVisual == null)
            {
                Debug.LogError(
                    $"[{nameof(LiquidLevelVisualizer)}] " +
                    "Le Transform représentant le liquide est manquant.",
                    this);
            }
        }

        #endregion


        #region Private and Protected

        [Header("Références")]
        [Tooltip(
            "Conteneur dont le volume doit être représenté.")]
        [SerializeField]
        private LiquidContainer _liquidContainer;

        [Tooltip(
            "Objet visuel qui sera redimensionné verticalement.")]
        [SerializeField]
        private Transform _liquidVisual;

        [Header("Dimensions")]
        [Min(0.001f)]
        [Tooltip(
            "Hauteur locale du visuel lorsque le conteneur est vide.")]
        [SerializeField]
        private float _minimumHeight = 0.001f;

        [Min(0.001f)]
        [Tooltip(
            "Hauteur locale du visuel lorsque le conteneur est plein.")]
        [SerializeField]
        private float _maximumHeight = 1f;

        [Header("Visibilité")]
        [Tooltip(
            "Masque complètement le visuel lorsque le conteneur est vide.")]
        [SerializeField]
        private bool _hideWhenEmpty = true;

        [Range(0f, 0.1f)]
        [Tooltip(
            "Tolérance en dessous de laquelle le niveau est considéré vide.")]
        [SerializeField]
        private float _emptyVisibilityTolerance = 0.001f;

        private Vector3 _initialLocalScale;
        private Vector3 _initialLocalPosition;

        private float _bottomLocalPosition;
        private float _currentLevel;

        #endregion
    }
}