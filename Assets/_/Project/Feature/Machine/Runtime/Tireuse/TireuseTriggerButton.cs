using UnityEngine;

namespace Machine.Runtime
{
    public class TireuseTriggerButton : MonoBehaviour
    {
        public void TriggerFilling()
        {
            _animator.SetTrigger("isFilling");
        }

        public void TireuseDoorOpening()
        {
            _animator.SetTrigger("isDoorOpening");
        }

        public void TireuseCapsuling()
        {
            _animator.SetTrigger("isCapsuling");
        }

        [SerializeField] private Animator _animator;
    }
}
