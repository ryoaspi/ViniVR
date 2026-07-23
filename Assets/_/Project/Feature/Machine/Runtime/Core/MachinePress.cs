using System.Collections.Generic;
using TheFoundation.Runtime;
using UnityEngine;

namespace Machine.Runtime
{
    public class MachinePress : FBehaviour
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

            Info($"{grapeObject.name} has entered the machine press.");

            if (_destroyed)
            {
                grapeObject.SetActive(false);

                Info($"{grapeObject.name} has been destroyed because the machine cycle is finished.");
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

            Info($"{grapeObject.name} has exited the machine press.");
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

                Info($"{grapeObject.name} has been destroyed.");
            }

            _grapeObjects.Clear();

            Info("All grape objects currently inside the machine press have been destroyed.");
        }

        public void RestartGrappe()
        {
            _destroyed = false;
            _grapeObjects.Clear();

            Info("The machine press is ready to receive new grape objects.");
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