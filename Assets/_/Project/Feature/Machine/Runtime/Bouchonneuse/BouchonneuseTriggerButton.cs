using UnityEngine;

namespace Machine.Runtime
{
    public class BouchonneuseTriggerButton : MonoBehaviour
    {
        public void TriggerFilling(string parameterName)
        {
            _animator.SetTrigger(parameterName);
        }

        [SerializeField] private Animator _animator;
        
    }
}
