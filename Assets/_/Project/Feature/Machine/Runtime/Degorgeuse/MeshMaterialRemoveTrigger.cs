using UnityEngine;

public class MeshMaterialRemoveTrigger : MonoBehaviour
{
    #region API Unity

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered)
            return;

        Transform root = other.attachedRigidbody != null
            ? other.attachedRigidbody.transform
            : other.transform.root;

        Transform bottle = FindChildRecursive(root, "Bottle");

        if (bottle == null)
        {
            Debug.LogWarning(
                $"[{nameof(MeshMaterialRemoveTrigger)}] Enfant 'Bottle' introuvable.",
                other);

            return;
        }

        MeshFilter meshFilter = bottle.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = bottle.GetComponent<MeshRenderer>();

        if (meshFilter == null || meshRenderer == null)
        {
            Debug.LogWarning(
                $"[{nameof(MeshMaterialRemoveTrigger)}] " +
                $"MeshFilter ou MeshRenderer introuvable sur '{bottle.name}'.",
                bottle);

            return;
        }

        ChangeMeshAndRemoveSecondMaterial(meshFilter, meshRenderer);

        _hasTriggered = true;
    }

    #endregion


    #region Main Methods

    private void ChangeMeshAndRemoveSecondMaterial(
        MeshFilter meshFilter,
        MeshRenderer meshRenderer)
    {
        if (_newMesh != null)
            meshFilter.sharedMesh = _newMesh;

        Material[] currentMaterials = meshRenderer.materials;

        if (currentMaterials.Length < 2)
        {
            Debug.LogWarning(
                $"[{nameof(MeshMaterialRemoveTrigger)}] " +
                "Le MeshRenderer possède moins de deux Materials.",
                meshRenderer);

            return;
        }

        Material[] newMaterials =
            new Material[currentMaterials.Length - 1];

        newMaterials[0] = currentMaterials[0];

        for (int i = 2; i < currentMaterials.Length; i++)
        {
            newMaterials[i - 1] = currentMaterials[i];
        }

        meshRenderer.materials = newMaterials;
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
                FindChildRecursive(child, childName);

            if (result != null)
                return result;
        }

        return null;
    }

    #endregion


    #region Private and Protected

    [Header("Visual Change")]
    [SerializeField] private Mesh _newMesh;

    private bool _hasTriggered;

    #endregion
}