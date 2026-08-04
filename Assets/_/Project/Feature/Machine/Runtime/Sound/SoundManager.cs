using UnityEngine;
using UnityEngine.UIElements;

namespace Machine.Runtime
{
    public class SoundManager : MonoBehaviour
    {
        #region Public
        #endregion
        
        #region Unity API
        #endregion

        
        #region Main Methods

        private void Start()
        {
            foreach (UIDocument uiDocument in _uiDocuments)
            {
                uiDocument.rootVisualElement.Query<Button>().ForEach(button => button.clicked += PlayButtonClick);
            }
        }
        #endregion

        
        #region Utils
        private void PlayButtonClick()
        {
            _audioSource.PlayOneShot(_buttonClickSound);
        }
        #endregion

        
        #region Private
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _buttonClickSound;
        [SerializeField] private UIDocument[] _uiDocuments;
        #endregion
    }
}
