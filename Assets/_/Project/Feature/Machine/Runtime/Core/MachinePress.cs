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
            if (other.gameObject.layer == LayerMask.NameToLayer(_layerMask.ToString()))
            {
                Info($"{other.gameObject.name} has been entered");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer(_layerMask.ToString()))
            {
                Info($"{other.gameObject.name} has been exited");
            }
        }

        #endregion
        
        
        #region Utils

        public bool DestroyGrappe()
        {
            _destroyed = true;
            return _destroyed;
        }
        
        #endregion
        
        
        #region Private And Protectde
        
        [SerializeField] private LayerMask _layerMask;
        
        private bool _destroyed = false;
        
        #endregion
    }
}
