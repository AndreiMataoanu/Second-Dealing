using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class MoneyParticle : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float fadeTime = 1f;

    private TextMeshPro textMesh;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        textColor = textMesh.color;
    }

    void Update()
    {
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        float alphaChange = (1.0f / fadeTime) * Time.deltaTime;
        textColor.a -= alphaChange;
        textMesh.color = textColor;

        if(textColor.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
