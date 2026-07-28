using UnityEngine;
using UnityEngine.UIElements;

public class ThermostatUI : MonoBehaviour
{
    [SerializeField] private Label valueLabel;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        Debug.Log("Root est null ? " + (root == null));
        Debug.Log("Nombre d'enfants du root : " + (root != null ? root.childCount.ToString() : "N/A"));

        var slider = root.Q<Slider>("wished-temperature-slider");
        valueLabel = root.Q<Label>("temperature-value-label");

        Debug.Log("Slider trouvé ? " + (slider != null));
        Debug.Log("Label trouvé ? " + (valueLabel != null));

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