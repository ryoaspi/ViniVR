using UnityEngine;

namespace Machine.Runtime
{
    public class DegorgeuseTriggerButton : MonoBehaviour
    {
        public void TriggerFilling()
        {
            _animator.SetTrigger("isFilling");
            _pistonAudioSource.Play();
        }
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _pistonAudioSource;

    }
}
