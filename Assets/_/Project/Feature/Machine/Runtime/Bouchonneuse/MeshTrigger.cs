using UnityEngine;

public class MeshTrigger : MonoBehaviour
{
    #region API Unity

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered)
            return;

        MeshFilter meshFilter = other.GetComponentInChildren<MeshFilter>();

        if (meshFilter == null)
            return;

        ApplyVisualChange(meshFilter);

        _hasTriggered = true;
    }

    #endregion


    #region Main Methods

    private void ApplyVisualChange(MeshFilter meshFilter)
    {
        if (_newMesh != null)
            meshFilter.sharedMesh = _newMesh;
        
        _audioSource.Play();
    }

    #endregion


    #region Private and Protected

    [Header("Visual Change")]
    [SerializeField] private Mesh _newMesh;
    [SerializeField] private AudioSource _audioSource;

    private bool _hasTriggered;

    #endregion
}