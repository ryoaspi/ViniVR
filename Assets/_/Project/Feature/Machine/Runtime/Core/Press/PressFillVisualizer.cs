using UnityEngine;

namespace Machine.Runtime
{
    public class PressFillVisualizer : MonoBehaviour
    {
        #region Publics

        public float m_currentFillProgress => _currentFillProgress;
        public bool m_isChangingLevel => _isChangingLevel;

        #endregion


        #region API Unity

        private void Awake()
        {
            ValidateReferences();
            CacheInitialValues();
            ApplyFillProgress(_startFillProgress);
        }

        private void Update()
        {
            if (!_isChangingLevel)
            {
                return;
            }

            UpdateFillLevel();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            _fillDuration = Mathf.Max(0.01f, _fillDuration);
            _drainDuration = Mathf.Max(0.01f, _drainDuration);

            _minimumHeight = Mathf.Max(0.001f, _minimumHeight);
            _maximumHeight = Mathf.Max(_minimumHeight, _maximumHeight);

            _startFillProgress = Mathf.Clamp01(_startFillProgress);
        }

#endif

        #endregion


        #region Utils

        /// <summary>
        /// Lance le remplissage jusqu'au niveau maximal.
        /// À connecter au début de la rotation de la presse.
        /// </summary>
        public void BeginFilling()
        {
            StartLevelChange(
                1f,
                _fillDuration);
        }

        /// <summary>
        /// Lance la vidange jusqu'au niveau minimal.
        /// À connecter au démarrage de la pompe.
        /// </summary>
        public void BeginDraining()
        {
            StartLevelChange(
                0f,
                _drainDuration);
        }

        /// <summary>
        /// Termine immédiatement le remplissage.
        /// </summary>
        public void CompleteFilling()
        {
            StopLevelChangeAt(1f);
        }

        /// <summary>
        /// Termine immédiatement la vidange.
        /// </summary>
        public void CompleteDraining()
        {
            StopLevelChangeAt(0f);
        }

        /// <summary>
        /// Met en pause le changement de niveau.
        /// Le niveau actuel est conservé.
        /// </summary>
        public void PauseLevelChange()
        {
            _isChangingLevel = false;
        }

        /// <summary>
        /// Replace immédiatement le niveau à zéro.
        /// </summary>
        public void ResetFill()
        {
            StopLevelChangeAt(0f);
        }

        /// <summary>
        /// Replace immédiatement le niveau au maximum.
        /// </summary>
        public void SetFull()
        {
            StopLevelChangeAt(1f);
        }

        /// <summary>
        /// Définit directement un niveau entre 0 et 1.
        /// </summary>
        public void SetFillProgress(float progress)
        {
            StopLevelChangeAt(
                Mathf.Clamp01(progress));
        }

        #endregion


        #region Main Methods

        private void StartLevelChange(
            float targetProgress,
            float duration)
        {
            if (_fillVisual == null)
            {
                Debug.LogError(
                    $"[{nameof(PressFillVisualizer)}] " +
                    "Le visuel de remplissage est manquant.",
                    this);

                return;
            }

            _startProgress = _currentFillProgress;
            _targetProgress = Mathf.Clamp01(targetProgress);
            _currentChangeDuration = Mathf.Max(0.01f, duration);
            _elapsedTime = 0f;

            if (Mathf.Approximately(
                    _startProgress,
                    _targetProgress))
            {
                _isChangingLevel = false;
                return;
            }

            _isChangingLevel = true;
        }

        private void UpdateFillLevel()
        {
            _elapsedTime += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(
                _elapsedTime / _currentChangeDuration);

            float progress = Mathf.Lerp(
                _startProgress,
                _targetProgress,
                normalizedTime);

            ApplyFillProgress(progress);

            if (normalizedTime < 1f)
            {
                return;
            }

            _isChangingLevel = false;
        }

        private void StopLevelChangeAt(float progress)
        {
            _isChangingLevel = false;
            _elapsedTime = 0f;

            ApplyFillProgress(
                Mathf.Clamp01(progress));
        }

        private void ApplyFillProgress(float progress)
        {
            if (_fillVisual == null)
            {
                return;
            }

            _currentFillProgress = Mathf.Clamp01(progress);

            float currentHeight = Mathf.Lerp(
                _minimumHeight,
                _maximumHeight,
                _currentFillProgress);

            Vector3 newScale = _initialLocalScale;
            newScale.y = currentHeight;

            _fillVisual.localScale = newScale;

            Vector3 newPosition = _initialLocalPosition;

            newPosition.y =
                _bottomLocalPosition +
                currentHeight * 0.5f;

            _fillVisual.localPosition = newPosition;
        }

        private void CacheInitialValues()
        {
            if (_fillVisual == null)
            {
                return;
            }

            _initialLocalScale = _fillVisual.localScale;
            _initialLocalPosition = _fillVisual.localPosition;

            _bottomLocalPosition =
                _initialLocalPosition.y -
                _initialLocalScale.y * 0.5f;
        }

        private void ValidateReferences()
        {
            if (_fillVisual != null)
            {
                return;
            }

            Debug.LogError(
                $"[{nameof(PressFillVisualizer)}] " +
                "La référence du visuel de niveau est manquante.",
                this);
        }

        #endregion


        #region Private and Protected

        [Header("Références")]
        [SerializeField] private Transform _fillVisual;

        [Header("Durées")]
        [Min(0.01f)]
        [SerializeField] private float _fillDuration = 6f;

        [Min(0.01f)]
        [SerializeField] private float _drainDuration = 5f;

        [Header("Niveau")]
        [Range(0f, 1f)]
        [SerializeField] private float _startFillProgress;

        [Min(0.001f)]
        [SerializeField] private float _minimumHeight = 0.001f;

        [Min(0.001f)]
        [SerializeField] private float _maximumHeight = 0.5f;

        private Vector3 _initialLocalScale;
        private Vector3 _initialLocalPosition;

        private float _bottomLocalPosition;

        private float _currentFillProgress;
        private float _startProgress;
        private float _targetProgress;

        private float _elapsedTime;
        private float _currentChangeDuration;

        private bool _isChangingLevel;

        #endregion
    }
}