using UnityEngine;

public class AmbientSoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource[] ambientSources;
    [SerializeField] private AudioClip[] ambientSounds;
    [SerializeField] private float soundTimer;
    [SerializeField] private float soundTimerMin = 30f;
    [SerializeField] private float soundTimerMax = 50f;

    public bool shouldPlaySound = true;

    private void Start()
    {
        soundTimer = Random.Range(soundTimerMin, soundTimerMax);
    }

    private void Update()
    {
        if (!shouldPlaySound)
        {
            return;
        }
        soundTimer -= Time.deltaTime;
        if (soundTimer <= 0)
        {
            PlaySound();
        }
    }

    private void PlaySound()
    {
        //Get a random source and clip and play them
        int sourceRNG = Random.Range(0, ambientSources.Length);
        int soundRNG = Random.Range(0, ambientSounds.Length);
        ambientSources[sourceRNG].PlayOneShot(ambientSounds[soundRNG]);
        soundTimer = Random.Range(soundTimerMin, soundTimerMax);
    }
}
