using UnityEngine;

public class RemplirBac : MonoBehaviour
{
    public GameObject objetLiquide; // Le cube violet (peut être caché maintenant)
    public GameObject surfaceJus;   // NOUVEAU : Le plan avec le shader Water
    public ParticleSystem systemeJus;

    [Header("Réglages")]
    public float vitesseRemplissage = 0.5f;
    public float hauteurMax = 1.0f;
    public float pourcentageArretParticules = 0.9f;

    private float niveauActuel = 0f;
    private bool particulesArretees = false;

    void Update()
    {
        if (niveauActuel < hauteurMax)
        {
            niveauActuel += vitesseRemplissage * Time.deltaTime;

            // 1. Gérer le cube (peut servir de collision ou être caché)
            if (objetLiquide != null)
            {
                Vector3 nouvelleEchelle = objetLiquide.transform.localScale;
                nouvelleEchelle.y = niveauActuel;
                objetLiquide.transform.localScale = nouvelleEchelle;
                
                // Remonter le cube
                Vector3 nouvellePos = objetLiquide.transform.position;
                nouvellePos.y += (vitesseRemplissage * Time.deltaTime) / 2f;
                objetLiquide.transform.position = nouvellePos;
		// ... votre code existant qui bouge le cube ...

// AJOUT SÉCURISÉ : Recaler la surface exactement sur le sommet du cube
if (surfaceJus != null && objetLiquide != null)
{
    // Calcul simple : Position du cube + (Hauteur du cube / 2)
    float hauteurActuelle = objetLiquide.transform.localScale.y; 
    Vector3 posCube = objetLiquide.transform.position;
    
    Vector3 newPos = surfaceJus.transform.position;
    newPos.y = posCube.y + (hauteurActuelle / 2f);
    surfaceJus.transform.position = newPos;
}   
            }

                                    // 2. Gérer la Surface (Le plan Water Shader)
            // On suppose que surfaceJus est ENFANT de objetLiquide
            if (surfaceJus != null && objetLiquide != null)
            {
                // La hauteur du sommet par rapport au centre du parent est : ScaleY / 2
                float hauteurSommetLocal = objetLiquide.transform.localScale.y / 2f;
                
                // On place le plan exactement à ce sommet local
                // On garde les X et Z actuels (souvent 0) et on force le Y
                surfaceJus.transform.localPosition = new Vector3(0f, hauteurSommetLocal, 0f);
            }   
        }

        // Arrêt des particules
        float pourcentageRempli = niveauActuel / hauteurMax;
        if (pourcentageRempli >= pourcentageArretParticules && !particulesArretees && systemeJus != null)
        {
            systemeJus.Stop();
            particulesArretees = true;
        }
    }
}   