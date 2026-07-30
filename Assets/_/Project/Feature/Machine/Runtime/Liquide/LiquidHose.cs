using UnityEngine;

namespace Machine.Runtime
{
    public class LiquidHose : MonoBehaviour
    {
        #region Publics

        public LiquidHoseEnd m_endA => _endA;
        public LiquidHoseEnd m_endB => _endB;

        #endregion


        #region API Unity

        private void Awake()
        {
            ValidateReferences();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            TryFindHoseEnds();
        }

#endif

        #endregion


        #region Utils

        /// <summary>
        /// Retourne l'autre extrémité du tuyau.
        /// </summary>
        public LiquidHoseEnd GetOppositeEnd(LiquidHoseEnd currentEnd)
        {
            if (currentEnd == _endA)
            {
                return _endB;
            }

            if (currentEnd == _endB)
            {
                return _endA;
            }

            Debug.LogWarning(
                $"[{nameof(LiquidHose)}] L'extrémité demandée " +
                $"n'appartient pas au tuyau {name}.",
                this);

            return null;
        }

        #endregion


        #region Main Methods

        private void TryFindHoseEnds()
        {
            LiquidHoseEnd[] hoseEnds =
                GetComponentsInChildren<LiquidHoseEnd>(true);

            if (hoseEnds.Length < 2)
            {
                return;
            }

            if (_endA == null)
            {
                _endA = hoseEnds[0];
            }

            if (_endB == null)
            {
                _endB = hoseEnds[1];
            }
        }

        private void ValidateReferences()
        {
            TryFindHoseEnds();

            if (_endA == null || _endB == null)
            {
                Debug.LogError(
                    $"[{nameof(LiquidHose)}] Le tuyau {name} " +
                    $"doit posséder exactement deux extrémités assignées.",
                    this);

                return;
            }

            if (_endA == _endB)
            {
                Debug.LogError(
                    $"[{nameof(LiquidHose)}] Les deux références du tuyau " +
                    $"pointent vers la même extrémité.",
                    this);
            }
        }

        #endregion


        #region Private and Protected

        [Header("Extrémités du tuyau")]
        [SerializeField] private LiquidHoseEnd _endA;
        [SerializeField] private LiquidHoseEnd _endB;

        #endregion
    }
}