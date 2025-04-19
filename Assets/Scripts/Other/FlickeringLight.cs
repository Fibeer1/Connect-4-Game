using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [SerializeField] private GameObject originObject;
    private Light lightSource;
    public float flickerSpeed = 25f;
    public float intensityMin = 0.2f;
    public float intensityMax = 1.2f;

    [SerializeField] private string shaderGraphShaderName = "Shader Graphs/TVStatic";
    [SerializeField] private string colorReference = "_StaticColor";

    private void Awake()
    {
        lightSource = GetComponent<Light>();

        if (originObject != null)
        {
            Renderer renderer = originObject.GetComponent<Renderer>();

            //Loop through the materials applied to the object
            foreach (Material material in renderer.materials)
            {
                //Check if the material is using the Shader Graph shader
                if (material.shader.name == shaderGraphShaderName)
                {
                    Color materialColor = material.GetColor(colorReference);
                    lightSource.color = materialColor;
                    break;
                }
            }
            
        }
        
    }

    private void Update()
    {
        if (lightSource == null)
        {
            return;
        }
        //Smooth randomness
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        float flicker = Mathf.Lerp(intensityMin, intensityMax, noise);
        lightSource.intensity = flicker;
    }
}
