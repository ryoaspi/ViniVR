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

        wishedTemperatureValue = uiDocumentReference.rootVisualElement.Q<Label>("wished-temperature-value");

        tankStartButton = uiDocumentReference.rootVisualElement.Q<Button>("tank-start-button");

        currentTemperatureValue = uiDocumentReference.rootVisualElement.Q<Label>("Temperature_value");

        thermoregulationStatusIcon = uiDocumentReference.rootVisualElement.Q<Image>("thermoregulation-status-icon");

        thermoregulationStatusText = uiDocumentReference.rootVisualElement.Q<Label>("thermoregulation-status-text");

        wishedTemperatureSlider.lowValue = 10f;
        wishedTemperatureSlider.highValue = 32f;

        wishedTemperatureSlider.SetValueWithoutNotify(18f);
        wishedTemperatureValue.text = "18°C";

        currentTemperatureValue.text = $"{currentTemperature:0.#}°C";

        targetTemperature = currentTemperature;
        UpdateThermoregulationStatus();

        tankStartButton.clicked += StartThermoregulation;

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
        UpdateThermoregulationStatus();
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
        UpdateThermoregulationStatus();
        
        isThermoregulating = true;
        
        tankStartButton.SetEnabled(false);
        wishedTemperatureSlider.SetEnabled(false);
        
        //Randomize between 2 and 4 seconds each 0.5° step
        nextTemperatureChangeTime = Time.time + Random.Range(2f, 5f);
    }
    
    private void UpdateThermoregulationStatus()
    {
        if (currentTemperature < targetTemperature)
        {
            thermoregulationStatusText.text = "Heating";
            thermoregulationStatusIcon.vectorImage = heatingIcon;
        } 
        else if (currentTemperature > targetTemperature)
        {
            thermoregulationStatusText.text = "Cooling";
            thermoregulationStatusIcon.vectorImage = coolingIcon;
        }
        else
        {
            thermoregulationStatusText.text = "Stable";
            thermoregulationStatusIcon.vectorImage = stableIcon;    
        }
    }
    #endregion
    
    #region Private
    [SerializeField] private UIDocument uiDocumentReference;
    [SerializeField] private float currentTemperature = 18.5f;
    private Image thermoregulationStatusIcon;
    private Label thermoregulationStatusText;
    [SerializeField] private VectorImage heatingIcon;
    [SerializeField] private VectorImage coolingIcon;
    [SerializeField] private VectorImage stableIcon;
    private float targetTemperature;
    private bool isThermoregulating = false;
    private float nextTemperatureChangeTime;
    private Slider wishedTemperatureSlider;
    private Label wishedTemperatureValue;
    private Button tankStartButton;
    private Label currentTemperatureValue;
    
    #endregion
}