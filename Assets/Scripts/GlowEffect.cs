using UnityEngine;

public class GlowEffect : MonoBehaviour
{
    public Color glowColor = new Color(0f, 0.68f, 1f, 0.08f); // Plava boja #00AFFF s još manjom prozirnošću (alpha = 0.08)
    public float minIntensity = 0.03f;  // Još manji intenzitet
    public float maxIntensity = 0.1f;   // Još manji intenzitet
    public float pulseSpeed = 0.8f;  // Sporije pulsiranje

    private Material material;
    private float emissionIntensity;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
            material.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        if (material != null)
        {
            emissionIntensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PingPong(Time.time * pulseSpeed, 1f));
            Color emissionColor = glowColor * emissionIntensity;
            material.SetColor("_EmissionColor", emissionColor);
        }
    }
}
