using UnityEngine;

public class Glow_scena2 : MonoBehaviour
{
    [Header("Glow Colors")]
    public Color glowColorA = new Color(0f, 0.68f, 1f, 0.01f); // Plava, još prozirnija
    public Color glowColorB = new Color(0.6f, 0f, 1f, 0.01f);  // Ljubičasta, još prozirnija

    [Header("Intensity & Pulse")]
    public float minIntensity = 0.005f;  // gotovo nevidljiv
    public float maxIntensity = 0.025f;  // vrlo slab sjaj
    public float pulseSpeed = 0.5f;      // sporo pulsiranje
    public float colorChangeSpeed = 0.3f; // spora promjena boja

    private Material material;
    private float emissionIntensity;
    private Light glowLight;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.sharedMaterial; // VAŽNO: koristi sharedMaterial!
            material.EnableKeyword("_EMISSION");
        }

        // Pronađi child light ako postoji
        glowLight = GetComponentInChildren<Light>();
    }

    void Update()
    {
        if (material != null)
        {
            // Pulsiranje intenziteta
            emissionIntensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PingPong(Time.time * pulseSpeed, 1f));

            // Promjena boje
            Color currentGlowColor = Color.Lerp(glowColorA, glowColorB, Mathf.PingPong(Time.time * colorChangeSpeed, 1f));

            // Primjena na emission
            Color emissionColor = currentGlowColor * emissionIntensity;
            material.SetColor("_EmissionColor", emissionColor);

            // Ako postoji Point Light, mijenjaj i njegovu boju
            if (glowLight != null)
            {
                glowLight.color = currentGlowColor;
            }
        }
    }
}
