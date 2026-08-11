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
        
        public void FillAllBottles()
        {
            for (int i = 0; i < _bottles.Length; i++)
            {
                if (_bottles[i] == null)
                    continue;

                _bottles[i].FillBottle();
            }
        }
        
        public void SetBottle(int index, GameObject bottle)
        {
            if (!IsValidBottleIndex(index))
                return;

            if (bottle == null)
            {
                _bottles[index] = null;
                return;
            }

            _bottles[index] =
                bottle.GetComponentInChildren<BottleLiquidFill>();

            if (_bottles[index] == null)
            {
                Debug.LogWarning(
                    $"[TireuseTriggerButton] Aucun BottleLiquidFill trouvé sur {bottle.name}.",
                    bottle);
            }
        }
        
        public void ClearBottle(int index)
        {
            if (!IsValidBottleIndex(index))
                return;

            _bottles[index] = null;
        }
        

        public void TireuseCapsuling()
        {
            _animator.SetTrigger("isCapsuling");
            _pistonsAudioSource.Play();
        }
        
        private bool IsValidBottleIndex(int index)
        {
            if (index >= 0 && index < _bottles.Length)
                return true;

            Debug.LogWarning(
                $"[TireuseTriggerButton] Index bouteille invalide : {index}.",
                this);

            return false;
        }

        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _pistonsAudioSource;
        [SerializeField] private AudioSource _doorAudioSource;
        [SerializeField] private AudioClip _doorOpeningClip;
        [SerializeField] private AudioClip _doorClosingClip;
        private BottleLiquidFill[] _bottles = new BottleLiquidFill[3];
        private bool _isDoorOpen = false;
    }
}
