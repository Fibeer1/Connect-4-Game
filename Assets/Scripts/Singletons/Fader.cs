using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    [SerializeField] private bool shouldPlayAtStart = false;
    public static Fader Instance;
    [SerializeField] private Image blackScreen;
    private Color opaqueColor = new Color(0, 0, 0, 1);
    private Color transparentColor = new Color(0, 0, 0, 0);

    private void Awake()
    {
        Instance = this;
        if (shouldPlayAtStart)
        {
            StartCoroutine(OnFade(false, 0.5f, 0.5f));
        }
    }

    private IEnumerator OnFade(bool fadeIn, float duration, float delay)
    {
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = fadeIn ? transparentColor : opaqueColor;
        Color startColor = fadeIn ? transparentColor : opaqueColor;
        Color endColor = fadeIn ? opaqueColor : transparentColor;
        yield return new WaitForSeconds(delay);
        float elapsedTime = 0;
        float fadeDuration = duration;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            blackScreen.color = Color.Lerp(startColor, endColor, elapsedTime / fadeDuration);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        if (!fadeIn)
        {
            blackScreen.gameObject.SetActive(false);
        }
    }

    public static void Fade(bool fadeIn, float duration, float delay)
    {
        Instance.StartCoroutine(Instance.OnFade(fadeIn, duration, delay));
    }    
}
