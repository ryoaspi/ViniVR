using UnityEngine;

public class AnimRemous : MonoBehaviour
{
    [Header("Réglages des remous")]
    public float vitesseX = 0.1f; 
    public float vitesseY = 0.1f; 

    private Renderer monRendu;

    void Start()
    {
        monRendu = GetComponent<Renderer>();
    }

    void Update()
    {
        if (monRendu != null && monRendu.material != null)
        {
            float x = Time.time * vitesseX;
            float y = Time.time * vitesseY;
            monRendu.material.mainTextureOffset = new Vector2(x, y);
        }
    }
}   