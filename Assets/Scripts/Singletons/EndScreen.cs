using TMPro;
using UnityEngine;
using System.Collections;

public class EndScreen : MonoBehaviour
{
    public static EndScreen Instance;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI endScreenText;

    private const string winText = "You beat the Wraith!";
    private const string lossText = "You lost.";

    private void Awake() => Instance = this;

    private IEnumerator OnEndGame(bool hasWon, float fadeDuration, float fadeDelay)
    {
        Fader.Fade(true, fadeDuration, fadeDelay);
        yield return new WaitForSeconds(fadeDuration + fadeDelay);
        endScreenText.text = hasWon ? winText : lossText;
        endScreen.SetActive(true);

        //Gradually reduce the volume of the audio listener
        yield return new WaitForSeconds(3);
        while (AudioListener.volume > 0)
        {
            AudioListener.volume -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    public static void EndGame(bool hasWon, float fadeDuration = 1f, float fadeDelay = 0) =>
        Instance.StartCoroutine(Instance.OnEndGame(hasWon, fadeDuration, fadeDelay));
}
