using System.Collections;
using UnityEngine;

public class LightHandler : MonoBehaviour
{
    public static LightHandler instance;
    [SerializeField] private Light roomLight;
    [SerializeField] private float flickerFrequencyMin = 0.025f;
    [SerializeField] private float flickerFrequencyMax = 0.1f;

    private float cachedIntensity;

    private float randomMinIntensity;

    private AudioSource audioSource;

    private Coroutine flickerCoroutine;

    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        cachedIntensity = roomLight.intensity;
    }

    private IEnumerator OnFlicker(int flickerCycles, bool toggleAfterFlicker, float freqMin = -1, float freqMax = -1)
    {
        //Option to enter custom frequencies
        //Gives more control
        float frequencyMin = freqMin != -1 ? freqMin : flickerFrequencyMin;
        float frequencyMax = freqMax != -1 ? freqMax : flickerFrequencyMax;
        if (roomLight.intensity > 0)
        {
            cachedIntensity = roomLight.intensity;
            randomMinIntensity = cachedIntensity * 0.75f;
        }
        float currentFlickerFrequency = Random.Range(flickerFrequencyMin, flickerFrequencyMax);
        for (int i = 0; i < flickerCycles; i++)
        {
            //Each time the lights flicker, the duration they stay on/off is random
            currentFlickerFrequency = FlickerCycle(false, frequencyMin, frequencyMax);
            yield return new WaitForSeconds(currentFlickerFrequency);

            currentFlickerFrequency = FlickerCycle(true, frequencyMin, frequencyMax);
            yield return new WaitForSeconds(currentFlickerFrequency);
        }
        roomLight.intensity = toggleAfterFlicker ? cachedIntensity : 0;
    }

    private float FlickerCycle(bool shouldActivate, float freqMin, float freqMax)
    {
        float randomIntensity = Random.Range(randomMinIntensity, cachedIntensity);
        roomLight.intensity = shouldActivate ? randomIntensity : 0;
        audioSource.Play();
        return Random.Range(freqMin, freqMax);
    }

    public static void FlickerLights(int flickerCycles, bool toggleAfterFlicker, float freqMin = -1, float freqMax = -1)
    {
        if (instance.flickerCoroutine != null)
        {
            //Make sure there's only 1 instance of the flicker coroutine active
            instance.StopCoroutine(instance.flickerCoroutine);
        }
        instance.flickerCoroutine = instance.StartCoroutine(instance.OnFlicker(flickerCycles, toggleAfterFlicker, freqMin, freqMax));
    }

    public static void ToggleLights(bool shouldToggle)
    {
        instance.roomLight.intensity = shouldToggle ? instance.cachedIntensity : 0;
    }
}
