using UnityEngine;

public class LiquidWobble : MonoBehaviour
{
    Renderer rend;
    Vector3 lastPos;
    Vector3 lastRot;
    
    float wobbleAmountX;
    float wobbleAmountZ;
    float targetWobbleX;
    float targetWobbleZ;

    [Header("Réglages du Wobble")]
    public float MaxWobble = 0.2f;    // Inclinaison max du liquide
    public float WobbleSpeed = 5f;    // Vitesse de la vague
    public float Recovery = 2f;       // Temps avant de s'arrêter (viscosité)
    public float Sensibility = 0.05f; // Plus c'est bas, plus c'est fluide/doux

    void Start()
    {
        rend = GetComponent<Renderer>();
        lastPos = transform.position;
        lastRot = transform.eulerAngles;
    }

    void Update()
    {
        // 1. Calcul de la vitesse réelle de déplacement
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 velocity = (transform.position - lastPos) / dt;
        Vector3 angularVelocity = (transform.eulerAngles - lastRot) / dt;

        // Code de sécurité si la rotation fait un saut de 360° (problème d'Euler)
        if (angularVelocity.x > 180 / dt) angularVelocity.x -= 360 / dt;
        if (angularVelocity.z > 180 / dt) angularVelocity.z -= 360 / dt;

        // 2. Détermination de la cible brute (l'impact)
        targetWobbleX += (velocity.x * Sensibility) + (angularVelocity.z * Sensibility * 2f);
        targetWobbleZ += (velocity.z * Sensibility) + (angularVelocity.x * Sensibility * 2f);

        // 3. Limitation de l'impact maximum
        targetWobbleX = Mathf.Clamp(targetWobbleX, -MaxWobble, MaxWobble);
        targetWobbleZ = Mathf.Clamp(targetWobbleZ, -MaxWobble, MaxWobble);

        // 4. LISSAGE : Effet de vague qui diminue doucement (Lerp)
        targetWobbleX = Mathf.Lerp(targetWobbleX, 0, Time.deltaTime * Recovery);
        targetWobbleZ = Mathf.Lerp(targetWobbleZ, 0, Time.deltaTime * Recovery);

        // 5. Calcul de l'oscillation (le va-et-vient du liquide)
        wobbleAmountX = Mathf.Sin(Time.time * WobbleSpeed) * targetWobbleX;
        wobbleAmountZ = Mathf.Sin(Time.time * WobbleSpeed) * targetWobbleZ;

        // 6. Envoi fluide au shader
        rend.material.SetFloat("_WobbleX", wobbleAmountX);
        rend.material.SetFloat("_WobbleZ", wobbleAmountZ);

        // Sauvegarde pour la frame suivante
        lastPos = transform.position;
        lastRot = transform.eulerAngles;
    }
}
