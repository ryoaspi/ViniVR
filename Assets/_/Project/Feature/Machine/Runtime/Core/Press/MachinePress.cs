using System.Collections.Generic;
using UnityEngine;

namespace Machine.Runtime
{
    public class MachinePress : MonoBehaviour
    {
        #region Publics

        #endregion


        #region API Unity

        private void OnTriggerEnter(Collider other)
        {
            if (!IsLayerIncluded(other.gameObject.layer))
            {
                return;
            }

            GameObject grapeObject = GetGrapeObject(other);
            

            if (_destroyed)
            {
                grapeObject.SetActive(false);
                
                return;
            }
            
            if (!_grapeObjects.Contains(grapeObject))
            {
                _grapeObjects.Add(grapeObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsLayerIncluded(other.gameObject.layer))
            {
                return;
            }

            GameObject grapeObject = GetGrapeObject(other);

            _grapeObjects.Remove(grapeObject);
            
        }

        #endregion


        #region Utils

        public void DestroyGrappe()
        {
            _destroyed = true;

            for (int i = _grapeObjects.Count - 1; i >= 0; i--)
            {
                GameObject grapeObject = _grapeObjects[i];

                if (grapeObject == null)
                {
                    continue;
                }

                grapeObject.SetActive(false);
                
            }

            _grapeObjects.Clear();
            
        }

        public void RestartGrappe()
        {
            _destroyed = false;
            _grapeObjects.Clear();
            
        }

        #endregion


        #region Main Methods

        private bool IsLayerIncluded(int layer)
        {
            return (_layerMask.value & (1 << layer)) != 0;
        }

        private GameObject GetGrapeObject(Collider grapeCollider)
        {
            if (grapeCollider.attachedRigidbody != null)
            {
                return grapeCollider.attachedRigidbody.gameObject;
            }

            return grapeCollider.gameObject;
        }

        #endregion


        #region Private and Protected

        [SerializeField]
        private LayerMask _layerMask;

        private readonly List<GameObject> _grapeObjects = new();

        private bool _destroyed;

        #endregion
    }
}