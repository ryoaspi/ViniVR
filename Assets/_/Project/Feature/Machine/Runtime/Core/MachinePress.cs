using System;
using TheFoundation.Runtime;
using UnityEngine;

namespace Machine.Runtime
{
    public class MachinePress : FBehaviour
    {
        #region API Unity

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.GetMask(_layerMask.ToString()))
            {
                Info($"{other.gameObject.name} has been entered");
                if (_destroyed)
                {
                    other.gameObject.SetActive(false);
                    Info($"{other.gameObject.name} has been destroyed");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.GetMask(_layerMask.ToString()))
            {
                Info($"{other.gameObject.name} has been exited");
            }
        }

        #endregion
        
        
        #region Utils

        public void DestroyGrappe()
        {
            _destroyed = true;
        }

        public void RestartGrappe()
        {
            _destroyed = false;
        }
        
        #endregion
        
        
        #region Private And Protectde
        
        [SerializeField] private LayerMask _layerMask;
        
        private bool _destroyed;
        
        #endregion
    }
}
