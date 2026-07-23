using UnityEngine;

public class FollowGrip : MonoBehaviour
{
    public Transform grabbedBaseTarget; // Glissez l'objet Saisie_Base ici
    public Transform grabbedEndTarget;  // Glissez l'objet Saisie_End ici

    public Transform boneHoseBase;      // Glissez l'os HOSE_BASE ici
    public Transform boneHoseEnd;       // Glissez l'os HOSE_END ici

    void LateUpdate()
    {
        // Force l'os de départ à suivre le point de saisie de la base
        boneHoseBase.position = grabbedBaseTarget.position;
        boneHoseBase.rotation = grabbedBaseTarget.rotation;

        // Force l'os de fin à suivre le point de saisie de l'extrémité
        boneHoseEnd.position = grabbedEndTarget.position;
        boneHoseEnd.rotation = grabbedEndTarget.rotation;
    }
}