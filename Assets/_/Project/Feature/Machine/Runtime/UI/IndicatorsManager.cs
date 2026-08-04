using UnityEngine;
using UnityEngine.UIElements;

namespace Machine.Runtime
{
    public class IndicatorManager : MonoBehaviour
    {
        #region public
        #endregion
        
        #region Unity API
        void Start()
        {
            for (int i = 0; i < _indicators.Length; i++)
            {
                _indicators[i].SetActive(i == 0);
            }
            
            _introductionButton = _introductionUIDocument.rootVisualElement.Q<Button>("button_introduction");
            _introductionButton.clicked += GoToNextStep;
            
            _presseButton = _presseUIDocument.rootVisualElement.Q<Button>("button_press");
            _presseButton.clicked += GoToNextStep;
            
            _transfertButton = _transfertUIDocument.rootVisualElement.Q<Button>("button_transfert");
            _transfertButton.clicked += GoToNextStep;
            
            _debourbageButton = _debourbageUIDocument.rootVisualElement.Q<Button>("button_debourbage");
            _debourbageButton.clicked += GoToNextStep;
            
            _assemblageButton = _assemblageUIDocument.rootVisualElement.Q<Button>("button_assemblage");
            _assemblageButton.clicked += GoToNextStep;
            
            _fermentationButton = _fermentationUIDocument.rootVisualElement.Q<Button>("button_fermentation");
            _fermentationButton.clicked += GoToNextStep;
            
            _tirageButton = _tirageUIDocument.rootVisualElement.Q<Button>("button_tirage");
            _tirageButton.clicked += GoToNextStep;
            
            _remuageButton = _remuageUIDocument.rootVisualElement.Q<Button>("button_remuage");
            _remuageButton.clicked += GoToNextStep;
            
            _degorgementButton = _degorgementUIDocument.rootVisualElement.Q<Button>("button_degorgement");
            _degorgementButton.clicked += GoToNextStep;
            
            _bouchonnageButton = _bouchonnageUIDocument.rootVisualElement.Q<Button>("button_bouchonnage");
            _bouchonnageButton.clicked += GoToNextStep;
            
        }
        #endregion
        
        #region Main Methods
        public void GoToNextStep()
        {
            _introductionButton.SetEnabled(false);
            _presseButton.SetEnabled(false);
            _transfertButton.SetEnabled(false);
            _debourbageButton.SetEnabled(false);
            _assemblageButton.SetEnabled(false);
            _tirageButton.SetEnabled(false);
            _remuageButton.SetEnabled(false);
            _degorgementButton.SetEnabled(false);
            _bouchonnageButton.SetEnabled(false);
   
            _indicators[_currentStepIndex].SetActive(false);
            _currentStepIndex++;
            _indicators[_currentStepIndex].SetActive(true);
        }
        #endregion
        
        #region Utils
        #endregion
        
        #region private
        [SerializeField] private GameObject[] _indicators;
        [SerializeField] private UIDocument _introductionUIDocument;
        [SerializeField] private UIDocument _presseUIDocument;
        [SerializeField] private UIDocument _transfertUIDocument;
        [SerializeField] private UIDocument _debourbageUIDocument;
        [SerializeField] private UIDocument _assemblageUIDocument;
        [SerializeField] private UIDocument _fermentationUIDocument;
        [SerializeField] private UIDocument _tirageUIDocument;
        [SerializeField] private UIDocument _remuageUIDocument;
        [SerializeField] private UIDocument _degorgementUIDocument;
        [SerializeField] private UIDocument _bouchonnageUIDocument;
        private int _currentStepIndex = 0;
        private Button _introductionButton;
        private Button _presseButton;
        private Button _transfertButton;
        private Button _debourbageButton;
        private Button _assemblageButton;
        private Button _fermentationButton;
        private Button _tirageButton;
        private Button _remuageButton;
        private Button _degorgementButton;
        private Button _bouchonnageButton;
        #endregion
    }
}
