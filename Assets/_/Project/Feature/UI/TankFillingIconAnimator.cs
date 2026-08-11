using UnityEngine;
using UnityEngine.UIElements;

public class TankFillingIconAnimator : MonoBehaviour
{
    #region API Unity

    private void Start()
    {
        ShowThermostatScreen();
    }

    #endregion


    #region Public Methods

    public void OnVolumeChanged(float normalizedVolume)
    {
        normalizedVolume = Mathf.Clamp01(normalizedVolume);

        _thermostatScreen.SetActive(false);
        _fillingScreen.SetActive(true);

        if (_uiDocument.rootVisualElement == null)
        {
            return;
        }
        
        _fillingIcon = _uiDocument.rootVisualElement.Q<VisualElement>("filling-icon");

        if (_fillingIcon == null)
        {
            Debug.LogError("TankFillingIconAnimator : l'élément " + "'filling-icon' est introuvable.");
            return;
        }

        int fillingLevel = Mathf.Clamp(
            Mathf.FloorToInt(
                normalizedVolume * _fillingClasses.Length
            ),
            0,
            _fillingClasses.Length - 1
        );

        foreach (string fillingClass in _fillingClasses)
        {
            _fillingIcon.RemoveFromClassList(fillingClass);
        }

        _fillingIcon.AddToClassList(
            _fillingClasses[fillingLevel]
        );
    }

    public void ShowThermostatScreen()
    {
        _thermostatScreen.SetActive(true);
        _fillingScreen.SetActive(false);
    }

    #endregion


    #region Private

    [Header("Interface")]
    [SerializeField] private UIDocument _uiDocument;

    [SerializeField] private GameObject _thermostatScreen;

    [SerializeField] private GameObject _fillingScreen;

    private VisualElement _fillingIcon;

    private readonly string[] _fillingClasses =
    {
        "filling-level-0",
        "filling-level-1",
        "filling-level-2",
        "filling-level-3",
        "filling-level-4"
    };

    #endregion
}

/*using UnityEngine;
using UnityEngine.UIElements;

public class TankFillingIconAnimator : MonoBehaviour
{
    #region Public
    #endregion
    
    #region Main Methods
    private void Start()
    {
        _fillingIcon = _uiDocument.rootVisualElement.Q<VisualElement>("filling-icon");
        _fillingScreen.SetActive(false);
        _thermostatScreen.SetActive(true);
    }
    
    private void Update()
    {
        if (!_isAnimating)
        {
            return;
        }

        if (Time.time < _nextIconChangeTime)
        {
            return;
        }

        _fillingIcon.RemoveFromClassList(
            _fillingClasses[_currentFillingLevel]
        );

        _currentFillingLevel++;

        if (_currentFillingLevel >= _fillingClasses.Length)
        {
            _currentFillingLevel = 0;
        }

        _fillingIcon.AddToClassList(
            _fillingClasses[_currentFillingLevel]
        );

        _nextIconChangeTime = Time.time + 1f;
    }
    #endregion
    
    #region Utils
    public void StartFillingUIAnimation()
    {
        _thermostatScreen.SetActive(false);
        _fillingScreen.SetActive(true);
        
        _fillingIcon = _uiDocument.rootVisualElement.Q<VisualElement>("filling-icon");
        
        _isAnimating = true;
        _currentFillingLevel = 0; 
        
        foreach (string fillingClass in _fillingClasses)
        {
            _fillingIcon.RemoveFromClassList(fillingClass);
        }

        _fillingIcon.AddToClassList(_fillingClasses[_currentFillingLevel]);
        
        _nextIconChangeTime = Time.time + 1f;
    }
    
    public void StopUIFillingAnimation()
    {
        _isAnimating = false;
        _thermostatScreen.SetActive(true);
        _fillingScreen.SetActive(false);
    }
    
    public void OnValveValueChanged(float value)
    {
        bool valveIsFullyOpen = value >= 0.99f;

        if (valveIsFullyOpen)
        {
            if (!_isAnimating)
            {
                StartFillingUIAnimation();
            }
        }
        else
        {
            StopUIFillingAnimation();
        }
    }
    #endregion
    
     #region Private
     [SerializeField] private UIDocument _uiDocument;
     [SerializeField] private GameObject _thermostatScreen;
     [SerializeField] private GameObject _fillingScreen;
     private VisualElement _fillingIcon;
     private readonly string[] _fillingClasses =
     {
         "filling-level-0",
         "filling-level-1",
         "filling-level-2",
         "filling-level-3",
         "filling-level-4"
     };
     private int _currentFillingLevel = 0;
     private float _nextIconChangeTime;
     private bool _isAnimating = false;
     #endregion
    
}*/