using UnityEngine;
using UnityEngine.UIElements;

public class ThermostatUI : MonoBehaviour
{
    private Label valueLabel;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        var slider = root.Q<Slider>("wished-temperature-slider");
        valueLabel = root.Q<Label>("temperature-value-label");

        UpdateLabel(slider.value);

        slider.RegisterValueChangedCallback(evt =>
        {
            UpdateLabel(evt.newValue);
        });
    }

    private void UpdateLabel(float value)
    {
        valueLabel.text = value.ToString("F1");
    }
}