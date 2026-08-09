using UnityEngine;

namespace Machine.Runtime
{
    public class TireuseTriggerButton : MonoBehaviour
    {
        public void TriggerFilling()
        {
            _animator.SetTrigger("isFilling");
            _pistonsAudioSource.Play();
        }

        public void ToggleDoor()
        {
            if (_isDoorOpen)
            {
                _animator.SetTrigger("isDoorClosing");
                _doorAudioSource.PlayOneShot(_doorClosingClip);
                _isDoorOpen = false;
            }
            else
            {
                _animator.SetTrigger("isDoorOpening");
                _doorAudioSource.PlayOneShot(_doorOpeningClip);
                _isDoorOpen = true;
            }
        } 

        public void TireuseCapsuling()
        {
            _animator.SetTrigger("isCapsuling");
            _pistonsAudioSource.Play();
        }

        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _pistonsAudioSource;
        [SerializeField] private AudioSource _doorAudioSource;
        [SerializeField] private AudioClip _doorOpeningClip;
        [SerializeField] private AudioClip _doorClosingClip;
        private bool _isDoorOpen = false;
    }
}
