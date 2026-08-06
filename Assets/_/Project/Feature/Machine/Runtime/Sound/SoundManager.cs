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
        public void PlayButtonClick()
        {
            _audioSource.PlayOneShot(_buttonClickSound);
        }
        
        public void PlayStartButtonPresseClick()
        {
            _audioSource.PlayOneShot(_startButtonPresseSound);
        }
        #endregion

        
        #region Private
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _buttonClickSound;
        [SerializeField] private AudioClip _startButtonPresseSound;
        [SerializeField] private UIDocument[] _uiDocuments;
        #endregion
    }
}
