using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TestPushButton : MonoBehaviour
{
    [Header("Paramètres de pression")]
    public Transform buttonVisual;
    public float pressDistance = 0.015f;
    public float speed = 10f;
    
    private Vector3 initialLocalPos;
    private bool isPressed = false;
    private XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        if (buttonVisual != null)
        {
            initialLocalPos = buttonVisual.localPosition;
        }
    }

    void OnEnable()
    {
        interactable.hoverEntered.AddListener(StartPress);
        interactable.hoverExited.AddListener(ReleasePress);
    }

    void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(StartPress);
        interactable.hoverExited.RemoveListener(ReleasePress);
    }

    private void StartPress(HoverEnterEventArgs args)
    {
        isPressed = true;
        Debug.Log("StartPress");
    }

    private void ReleasePress(HoverExitEventArgs args)
    {
        isPressed = false;
        Debug.Log("ReleasePress");
    }

    void Update()
    {
        if (buttonVisual)
        {
            Vector3 targetPos = isPressed ? initialLocalPos - new Vector3(0, pressDistance, 0) : initialLocalPos;
            buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, targetPos, Time.deltaTime * speed);
        }
    }
}