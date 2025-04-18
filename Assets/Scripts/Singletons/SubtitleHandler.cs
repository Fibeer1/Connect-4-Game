using TMPro;
using UnityEngine;
using System.Collections;
using System;

public class SubtitleHandler : MonoBehaviour
{
    public static SubtitleHandler instance;
    [SerializeField] private GameObject subtitleTextPrefab;
    [SerializeField] private RectTransform textStartPoint;
    private int currentTextsNum;
    private Color opaqueColor = new Color(1, 1, 1, 1);
    private Color transparentColor = new Color(1, 1, 1, 0);

    private void Awake()
    {
        instance = this;
    }

    private IEnumerator OnSubtitleShow(string text, float duration)
    {
        Color startColor = transparentColor;
        Color endColor = opaqueColor;
        Vector2 offset = new Vector2(0, 50 * currentTextsNum);
        TextMeshProUGUI textInstance = Instantiate(subtitleTextPrefab, 
            offset, Quaternion.identity, textStartPoint).GetComponent<TextMeshProUGUI>();

        textInstance.rectTransform.anchoredPosition = offset;

        textInstance.text = text;
        currentTextsNum++;
        float elapsedTime = 0;
        float fadeDuration = 0.25f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textInstance.color = Color.Lerp(startColor, endColor, elapsedTime / fadeDuration);
            yield return null;
        }
        yield return new WaitForSeconds(duration);
        startColor = opaqueColor;
        endColor = transparentColor;
        elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textInstance.color = Color.Lerp(startColor, endColor, elapsedTime / fadeDuration);
            yield return null;
        }
        Destroy(textInstance.gameObject);
        currentTextsNum--;
    }

    public static void ShowSubtitle(string subtitleText, float duration)
    {
        instance.StartCoroutine(instance.OnSubtitleShow(subtitleText, duration));
    }
}
