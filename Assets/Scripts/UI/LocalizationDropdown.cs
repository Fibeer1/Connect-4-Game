using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizationDropdown : MonoBehaviour
{
    private TMP_Dropdown languageDropdown;

    private void Start()
    {
        SetUpLanguageDropdown();
    }

    private void SetUpLanguageDropdown()
    {
        languageDropdown = GetComponent<TMP_Dropdown>();
        languageDropdown.ClearOptions();

        string[] availableLanguages = System.Enum.GetNames(typeof(LocalizationManager.Language));
        languageDropdown.AddOptions(new List<string>(availableLanguages));

        languageDropdown.value = (int)LocalizationManager.Instance.currentLanguage;
    }

    //This method gets called when the user selects an option in the dropdown
    public void OnDropdownValueChanged()
    {
        LocalizationManager.Language selectedLanguage = (LocalizationManager.Language)languageDropdown.value;
        LocalizationManager.Instance.currentLanguage = selectedLanguage;
        LocalizationManager.Instance.LoadLocalizedText(selectedLanguage);
        LocalizationManager.Instance.UpdateAllTexts();
    }
}
