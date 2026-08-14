using UnityEngine;

public class CylindreScripts : MonoBehaviour
{
    [Header("Paramètres de montée")]
    [SerializeField] private float hauteur = 2f;
    [SerializeField] private float vitesse = 1f;
    [SerializeField] private GameObject _bonusBouteille;

    private Vector3 positionDepart;
    private Vector3 positionCible;
    private bool enMontee = false;

    private void Awake()
    {
        positionDepart = transform.position;
        positionCible = positionDepart + Vector3.up * hauteur;
        _bonusBouteille.SetActive(false);
    }

    private void OnEnable()
    {
        // Recommence la montée à chaque activation
        transform.position = positionDepart;
        enMontee = true;
    }

    private void Update()
    {
        if (!enMontee)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            positionCible,
            vitesse * Time.deltaTime
        );

        // Arrivé à la hauteur cible
        if (transform.position == positionCible)
        {
            enMontee = false;
            _bonusBouteille.SetActive(true);
        }
    }
}
