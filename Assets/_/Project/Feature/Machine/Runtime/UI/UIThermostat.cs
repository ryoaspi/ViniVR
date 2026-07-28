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
        
        wishedTemperatureSlider.SetValueWithoutNotify(18f);
        wishedTemperatureValue.text = "18°C";
        
        wishedTemperatureSlider.RegisterValueChangedCallback(OnWishedTemperatureChanged);
    }

    void Update()
    {
        
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
    #endregion
    
    #region Private
    [SerializeField] private UIDocument uiDocumentReference;
    private Slider wishedTemperatureSlider;
    private Label wishedTemperatureValue;
    #endregion
}