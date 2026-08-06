using UnityEngine;

namespace Machine.Runtime
{
    public class PressAnimationEventRelay : MonoBehaviour
    {
        #region Utils

        public void NotifyDoorsClosed()
        {
            if (!ValidateController())
            {
                return;
            }

            _pressMachineController.NotifyDoorsClosed();
        }

        public void NotifyPressRotationCompleted()
        {
            if (!ValidateController())
            {
                return;
            }

            _pressMachineController.NotifyPressRotationCompleted();
        }

        public void NotifyDoorsOpened()
        {
            if (!ValidateController())
            {
                return;
            }

            _pressMachineController.NotifyDoorsOpened();
        }
        
        public void NotifyDoorAnimationEndpoint()
        {
            if (!ValidateController())
            {
                return;
            }

            _pressMachineController.NotifyDoorAnimationEndpoint();
        }
        
                
        public void EnableParticles()
        {
            if (_particles != null)
                _particles.SetActive(true);
        }

        public void DisableParticles()
        {
            if (_particles != null)
                _particles.SetActive(false);
        }

        #endregion


        #region Main Methods

        private bool ValidateController()
        {
            if (_pressMachineController != null)
            {
                return true;
            }

            Debug.LogError(
                $"[{nameof(PressAnimationEventRelay)}] " +
                "La référence PressMachineController est manquante.",
                this);

            return false;
        }
        
        #endregion


        #region Private and Protected

        [SerializeField] private PressMachineController _pressMachineController;
        [SerializeField] private GameObject _particles;

        #endregion
    }
}