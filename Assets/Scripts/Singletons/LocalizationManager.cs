using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class LocalizationManager : MonoBehaviour
{
    public enum Language
    {
        English,
        Bulgarian
    }

    public static LocalizationManager Instance { get; private set; }

    public Language currentLanguage = Language.English;

    private Dictionary<string, string> localizedText;

    private List<LocalizedText> localizedTexts = new List<LocalizedText>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLocalizedText(currentLanguage);
    }

    public void LoadLocalizedText(Language language)
    {
        string langCode = language.ToString().ToLower(); //e.g., "english" → "english.json"
        TextAsset jsonFile = Resources.Load<TextAsset>($"Localization/{langCode}");

        if (jsonFile != null)
        {
            string json = jsonFile.text;
            LocalizationData localizationData = JsonUtility.FromJson<LocalizationData>(json);

            // If deserialization fails, log an error
            if (localizationData == null || localizationData.items == null)
            {
                Debug.LogError("Failed to deserialize JSON into LocalizationData.");
                return;
            }

            localizedText = localizationData.ToDictionary();
        }
        else
        {
            Debug.LogError("Localization file not found: " + langCode);
        }
    }

    public void Register(LocalizedText text)
    {
        if (!localizedTexts.Contains(text))
        {
            localizedTexts.Add(text);
        }
    }

    public void Unregister(LocalizedText text)
    {
        localizedTexts.Remove(text);
    }

    public void UpdateAllTexts()
    {
        foreach (var text in localizedTexts)
        {
            if (text == null)
            {
                Unregister(text);
                continue;
            }
            text.UpdateText();
        }           
    }

    public string Get(string key)
    {
        return localizedText != null && localizedText.TryGetValue(key, out var value)
            ? value
            : $"#{key}";
    }
}

[System.Serializable]
public class LocalizationItem
{
    public string key;
    public string value;
}

[System.Serializable]
public class LocalizationData
{
    public LocalizationItem[] items;

    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        foreach (var item in items)
            dict[item.key] = item.value;
        return dict;
    }
}