using UnityEngine;

namespace Machine.Runtime
{
    public class LiquidConnectionPoint : MonoBehaviour
    {
        #region Publics

        public LiquidContainer m_container =>
            _liquidContainer;

        public string m_containerName =>
            _liquidContainer != null
                ? _liquidContainer.m_containerName
                : "Aucun conteneur";

        public LiquidContainerType m_containerType =>
            _liquidContainer != null
                ? _liquidContainer.m_containerType
                : LiquidContainerType.Other;

        public bool m_hasContainer =>
            _liquidContainer != null;

        #endregion


        #region API Unity

        private void Awake()
        {
            ValidateReferences();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (_liquidContainer == null)
            {
                _liquidContainer =
                    GetComponentInParent<LiquidContainer>();
            }
        }

#endif

        #endregion


        #region Main Methods (méthodes private)

        private void ValidateReferences()
        {
            if (_liquidContainer != null)
            {
                return;
            }

            _liquidContainer =
                GetComponentInParent<LiquidContainer>();

            if (_liquidContainer != null)
            {
                return;
            }
            
        }

        #endregion


        #region Private and Protected

        [Header("Conteneur associé")]
        [SerializeField]
        private LiquidContainer _liquidContainer;

        #endregion
    }
}
