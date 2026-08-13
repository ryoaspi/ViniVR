using UnityEngine;

public class BottleChildActivationTrigger : MonoBehaviour
{
    #region Publics

    public enum ActivationStep
    {
        StepOne,
        StepTwo
    }

    #endregion


    #region API Unity

    private void OnTriggerEnter(Collider other)
    {
        Transform root = other.attachedRigidbody != null
            ? other.attachedRigidbody.transform
            : other.transform.root;

        Transform bottle = FindChildRecursive(root, "Bottle");

        if (bottle == null)
            return;

        switch (_activationStep)
        {
            case ActivationStep.StepOne:
                ActivateObjects(bottle, _stepOneObjectNames);
                break;

            case ActivationStep.StepTwo:
                ActivateObjects(bottle, _stepTwoObjectNames);
                break;
        }
    }

    #endregion


    #region Main Methods

    private void ActivateObjects(
        Transform bottle,
        string[] objectNames)
    {
        if (objectNames == null || objectNames.Length == 0)
            return;

        foreach (string objectName in objectNames)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                continue;

            Transform target =
                FindChildRecursive(
                    bottle,
                    objectName);

            if (target == null)
            {
                Debug.LogWarning(
                    $"[{nameof(BottleChildActivationTrigger)}] " +
                    $"GameObject '{objectName}' introuvable dans '{bottle.name}'.",
                    bottle);

                continue;
            }

            target.gameObject.SetActive(true);
        }
    }

    private Transform FindChildRecursive(
        Transform parent,
        string childName)
    {
        if (parent.name == childName)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result =
                FindChildRecursive(
                    child,
                    childName);

            if (result != null)
                return result;
        }

        return null;
    }

    #endregion


    #region Private and Protected

    [Header("Activation Step")]
    [SerializeField] private ActivationStep _activationStep;

    [Header("Step One")]
    [SerializeField] private string[] _stepOneObjectNames;

    [Header("Step Two")]
    [SerializeField] private string[] _stepTwoObjectNames;

    #endregion
}