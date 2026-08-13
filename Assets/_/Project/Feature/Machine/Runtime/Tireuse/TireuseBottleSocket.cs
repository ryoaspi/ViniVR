using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Machine.Runtime
{
    public class TireuseBottleSocket : MonoBehaviour
    {
        #region Publics

        #endregion


        #region API Unity

        private void Awake()
        {
            _socketInteractor = GetComponent<XRSocketInteractor>();
        }

        private void OnEnable()
        {
            if (_socketInteractor == null)
                return;

            _socketInteractor.selectEntered.AddListener(OnBottleEntered);
            _socketInteractor.selectExited.AddListener(OnBottleExited);
        }

        private void OnDisable()
        {
            if (_socketInteractor == null)
                return;

            _socketInteractor.selectEntered.RemoveListener(OnBottleEntered);
            _socketInteractor.selectExited.RemoveListener(OnBottleExited);
        }

        #endregion


        #region Utils

        #endregion


        #region Main Methods

        private void OnBottleEntered(SelectEnterEventArgs args)
        {
            if (_tireuse == null)
                return;

            GameObject bottle =
                args.interactableObject.transform.gameObject;
            
            Debug.Log(
                $"[TireuseBottleSocket] Entrée slot {_socketIndex} : {bottle.name}",
                bottle);

            _tireuse.SetBottle(
                _socketIndex,
                bottle);    
        }

        private void OnBottleExited(SelectExitEventArgs args)
        {
            if (_tireuse == null)
                return;
            
            Debug.Log(
                $"[TireuseBottleSocket] Sortie slot {_socketIndex}.",
                this);

            _tireuse.ClearBottle(_socketIndex);
        }

        #endregion


        #region Private and Protected

        [Header("Tireuse")]
        [SerializeField]
        private TireuseTriggerButton _tireuse;

        [Header("Socket")]
        [Tooltip("0 = gauche, 1 = milieu, 2 = droite.")]
        [Range(0, 2)]
        [SerializeField]
        private int _socketIndex;

        private XRSocketInteractor _socketInteractor;
        

        #endregion
    }
}