using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BottleParticleAnimation : MonoBehaviour
{
    #region API Unity

    private void Awake()
    {
        if (_bottleSocket == null)
        {
            Debug.LogError(
                $"[{nameof(BottleParticleAnimation)}] Aucun XRSocketInteractor assigné.",
                this);
        }
    }

    #endregion


    #region Utils

    /// <summary>
    /// Active et joue le ParticleSystem de la bouteille
    /// actuellement présente dans le socket.
    /// Prévu pour être appelé par un Animation Event.
    /// </summary>
    public void StartParticle()
    {
        ParticleSystem particleSystem = GetBottleParticleSystem();

        if (particleSystem == null)
            return;

        particleSystem.gameObject.SetActive(true);
        particleSystem.Play();
    }

    /// <summary>
    /// Arrête puis désactive le ParticleSystem.
    /// Prévu pour être appelé par un Animation Event.
    /// </summary>
    public void StopParticle()
    {
        ParticleSystem particleSystem = GetBottleParticleSystem();

        if (particleSystem == null)
            return;

        particleSystem.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear);

        particleSystem.gameObject.SetActive(false);
    }

    #endregion


    #region Main Methods

    private ParticleSystem GetBottleParticleSystem()
    {
        if (_bottleSocket == null)
            return null;

        if (!_bottleSocket.hasSelection)
            return null;

        Transform bottleRoot =
            _bottleSocket.firstInteractableSelected.transform;

        Transform particleTransform =
            FindChildRecursive(
                bottleRoot,
                _particleObjectName);

        if (particleTransform == null)
        {
            Debug.LogWarning(
                $"[{nameof(BottleParticleAnimation)}] " +
                $"'{_particleObjectName}' introuvable dans " +
                $"'{bottleRoot.name}'.",
                bottleRoot);

            return null;
        }

        ParticleSystem particleSystem =
            particleTransform.GetComponent<ParticleSystem>();

        if (particleSystem == null)
        {
            Debug.LogWarning(
                $"[{nameof(BottleParticleAnimation)}] " +
                $"Aucun ParticleSystem sur '{particleTransform.name}'.",
                particleTransform);

            return null;
        }

        return particleSystem;
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

    [Header("Bottle Socket")]
    [SerializeField] private XRSocketInteractor _bottleSocket;

    [Header("Particle")]
    [SerializeField]
    private string _particleObjectName = "Particle_System_Pshiiit";

    #endregion
}