using System;
using UnityEngine;

namespace Machine.Runtime
{
    public class TireuseTriggerButton : MonoBehaviour
    {
        public void TriggerFilling()
        {
            _animator.SetTrigger("isFilling");
        }

        [SerializeField] private Animator _animator;
    }
}
