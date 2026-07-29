using UnityEngine;
using UnityEngine.UIElements;

public class UIThermostat : MonoBehaviour
{
    
    #region Publics
    #endregion
    
    #region Unity API
    #endregion
    
    #region Main Methods
    void Start()
    {
        wishedTemperatureSlider = uiDocumentReference.rootVisualElement.Q<Slider>("wished-temperature-slider");
        wishedTemperatureSlider.lowValue = 10f;
        
        wishedTemperatureValue = uiDocumentReference.rootVisualElement.Q<Label>("wished-temperature-value");
        wishedTemperatureSlider.highValue = 32f;
        
        tankStartButton = uiDocumentReference.rootVisualElement.Q<Button>("tank-start-button");
        tankStartButton.clicked += StartThermoregulation;
        
        currentTemperatureValue = uiDocumentReference.rootVisualElement.Q<Label>("Temperature_value");
        currentTemperatureValue.text = $"{currentTemperature:0.#}°C";
        
        wishedTemperatureSlider.SetValueWithoutNotify(18f);
        wishedTemperatureValue.text = "18°C";
        
        wishedTemperatureSlider.RegisterValueChangedCallback(OnWishedTemperatureChanged);
    }

    void Update()
    {
        if (!isThermoregulating)
        {
            return;
        }
        
        if (Time.time < nextTemperatureChangeTime)
        {
            return;
        }
        
        currentTemperature = Mathf.MoveTowards(currentTemperature, targetTemperature, 0.5f);
        currentTemperatureValue.text = $"{currentTemperature:0.#}°C";
        nextTemperatureChangeTime = Time.time + Random.Range(2f, 5f);
        
        if (currentTemperature == targetTemperature)
        {
            isThermoregulating = false;
            tankStartButton.SetEnabled(true);
            wishedTemperatureSlider.SetEnabled(true);
        }
    }
    #endregion
    
    #region Utils
    private void OnWishedTemperatureChanged(ChangeEvent<float> evt)
    {
        //Force a round or .5 value each slider step
        float roundedValue = Mathf.Round(evt.newValue * 2f) / 2f;
        wishedTemperatureSlider.SetValueWithoutNotify(roundedValue);
        wishedTemperatureValue.text = $"{roundedValue:0.#}°C";
    }
    
    private void StartThermoregulation()
    {
        if (isThermoregulating)
        {
            return;
        }
        targetTemperature = wishedTemperatureSlider.value;
        isThermoregulating = true;
        tankStartButton.SetEnabled(false);
        wishedTemperatureSlider.SetEnabled(false);
        
        //Randomize between 2 and 4 seconds each 0.5° step
        nextTemperatureChangeTime = Time.time + Random.Range(2f, 5f);
    }
    #endregion
    
    #region Private
    [SerializeField] private UIDocument uiDocumentReference;
    [SerializeField] private float currentTemperature = 18.5f;
    private float targetTemperature;
    private bool isThermoregulating = false;
    private float nextTemperatureChangeTime;
    private Slider wishedTemperatureSlider;
    private Label wishedTemperatureValue;
    private Button tankStartButton;
    private Label currentTemperatureValue;
    
    #endregion
}