using System;
using UnityEngine;

namespace Machine.Runtime
{
    public class TireuseTriggerButton : MonoBehaviour
    {
        private void FixedUpdate()
        {
            if (_animator.GetBool("isAnimated") == true)
            {
                _timer -= Time.deltaTime;
                _animator.SetBool("isAnimated", false);
            };
        }

        public void booleanTireuse()
        {
            _animator.SetBool("isAnimated", true);
        }
        
        #region Private
        [SerializeField] private Animator _animator;
        [SerializeField] private float _timer = 4f;

        #endregion
    }
}
