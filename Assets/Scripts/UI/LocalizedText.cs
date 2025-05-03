using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;
    private TMP_Text text;

    private void Start()
    {
        text = GetComponent<TMP_Text>();
        LocalizationManager.Instance.Register(this);
    }

    public void UpdateText()
    {
        if (text == null)
        {
            text = GetComponent<TMP_Text>();
        }        
        text.text = LocalizationManager.Instance.Get(key);
    }

    private void OnEnable()
    {
        UpdateText();
    }

    private void OnDestroy()
    {
        LocalizationManager.Instance.Unregister(this);
    }
}