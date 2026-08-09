using UnityEngine;

namespace Machine.Runtime
{
    public class DegorgeuseTriggerButton : MonoBehaviour
    {
        public void TriggerFilling()
        {
            _animator.SetTrigger("isFilling");
        }
        

        [SerializeField] private Animator _animator;
    }
}
