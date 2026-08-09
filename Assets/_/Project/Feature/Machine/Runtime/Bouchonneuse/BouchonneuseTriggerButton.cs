using UnityEngine;

namespace Machine.Runtime
{
    public class BouchonneuseTriggerButton : MonoBehaviour
    {
        public void TriggerFilling()
        {
            _animator.SetTrigger("isFilling");
        }

        [SerializeField] private Animator _animator;
    }
}
