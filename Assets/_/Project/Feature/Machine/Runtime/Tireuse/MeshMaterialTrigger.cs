using UnityEngine;

public class MeshMaterialTrigger : MonoBehaviour
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

        ApplyVisualChange(meshFilter, meshRenderer);

        _hasTriggered = true;
    }

    #endregion


    #region Main Methods

    private void ApplyVisualChange(MeshFilter meshFilter, MeshRenderer meshRenderer)
    {
        if (_newMesh != null)
            meshFilter.sharedMesh = _newMesh;

        if (_additionalMaterial == null)
            return;

        Material[] currentMaterials = meshRenderer.materials;

        Material[] newMaterials =
            new Material[currentMaterials.Length + 1];

        for (int i = 0; i < currentMaterials.Length; i++)
        {
            newMaterials[i] = currentMaterials[i];
        }

        newMaterials[^1] = _additionalMaterial;

        meshRenderer.materials = newMaterials;
        
        _audioSource.Play();
    }

    #endregion


    #region Private and Protected

    [Header("Visual Change")]
    [SerializeField] private Mesh _newMesh;
    [SerializeField] private Material _additionalMaterial;
    [SerializeField] private AudioSource _audioSource;

    private bool _hasTriggered;

    #endregion
}