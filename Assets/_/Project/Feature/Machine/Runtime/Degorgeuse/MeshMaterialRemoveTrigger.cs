using UnityEngine;

public class MeshMaterialRemoveTrigger : MonoBehaviour
{
    #region API Unity

    private void OnTriggerEnter(Collider other)
    {
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

        ChangeMeshAndRemoveSecondMaterial(
            meshFilter,
            meshRenderer);

        PlayParticle(root);
        PlaySound();
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

    private void PlayParticle(Transform root)
    {
        Transform particleObject =
            FindChildRecursive(root, "Particle_System_Pshiiit");

        if (particleObject == null)
        {
            Debug.LogWarning(
                $"[{nameof(MeshMaterialRemoveTrigger)}] " +
                "Particle_System_Pshiiit introuvable.",
                root);

            return;
        }

        particleObject.gameObject.SetActive(true);

        ParticleSystem particleSystem =
            particleObject.GetComponent<ParticleSystem>();

        if (particleSystem == null)
        {
            Debug.LogWarning(
                $"[{nameof(MeshMaterialRemoveTrigger)}] " +
                "Aucun ParticleSystem trouvé sur Particle_System_Pshiiit.",
                particleObject);

            return;
        }

        particleSystem.Play();
    }

    private void PlaySound()
    {
        if (_audioSource == null)
            return;

        _audioSource.Play();
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

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;

    #endregion
}