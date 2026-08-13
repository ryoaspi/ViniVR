using UnityEngine;

public class MeshMaterialRemoveTrigger : MonoBehaviour
{
    #region API Unity

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered)
            return;
        
        MeshFilter meshFilter = other.GetComponentInChildren<MeshFilter>();
        MeshRenderer meshRenderer = other.GetComponentInChildren<MeshRenderer>();

        if (meshFilter == null || meshRenderer == null)
            return;

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
            return;

        Material[] newMaterials = new Material[currentMaterials.Length - 1];

        newMaterials[0] = currentMaterials[0];

        for (int i = 2; i < currentMaterials.Length; i++)
        {
            newMaterials[i - 1] = currentMaterials[i];
        }

        meshRenderer.materials = newMaterials;
    }

    #endregion


    #region Private and Protected
    
    [Header("Visual Change")]
    [SerializeField] private Mesh _newMesh;

    private bool _hasTriggered;

    #endregion
}