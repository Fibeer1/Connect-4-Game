using TMPro;
using UnityEngine;
using System.Collections;

public class EndScreen : MonoBehaviour
{
    public static EndScreen Instance;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI endScreenText;

    private const string winText = "You won!";
    private const string lossText = "You lost.";

    private void Awake() => Instance = this;

    private IEnumerator OnEndGame(bool hasWon, float fadeDuration, float fadeDelay)
    {
        Fader.Fade(true, fadeDuration, fadeDelay);
        yield return new WaitForSeconds(1);
        endScreenText.text = hasWon ? winText : lossText;
        endScreen.SetActive(true);

        //Unlock the cursor and gradually reduce the volume of the audio listener
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        yield return new WaitForSeconds(1);
        while (AudioListener.volume > 0)
        {
            AudioListener.volume -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    public static void EndGame(bool hasWon, float fadeDuration = 0.5f, float fadeDelay = 0) =>
        Instance.StartCoroutine(Instance.OnEndGame(hasWon, fadeDuration, fadeDelay));
}
