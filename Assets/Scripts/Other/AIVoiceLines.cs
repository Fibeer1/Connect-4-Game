using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;

public class AIVoiceLines : MonoBehaviour
{
    private const string intro1 = "You're awake. We are going to play a game of Connect 4.";
    private const string intro2 = "The rules are simple. We take turns placing tokens on the grid.";
    private const string intro3 = "The first one who connects 4 tokens in any direction wins the round.";
    private const string intro4 = "Whoever wins 3 rounds first wins the game.";
    private const string intro5 = "Take a look at the rulebook if anything is unclear.";
    private const string intro6 = "When you are ready, put a token in the grid to start the game.";
    public static string roundWonPlayer = "You win.";
    public static string roundWonAI = "I win.";
    public static string difficultySwitch = "I won't go so easy on you now.";
    public static string gameWonPlayer1 = "You win the game...";
    public static string gameWonPlayer2 = "...or do you?";
    public static string gameWonAI = "You lost.";
    public static string gameTie = "Tie. Nobody wins. Or maybe we both win?";
    public static string blockPlayer = "heh...";

    private Coroutine introSequence;

    private RoundManager roundManager;

    private AudioSource audioSource;
    [SerializeField] private AudioClip speechClip;
    private float speechFrequency = 0.1f;

    public static AIVoiceLines instance;

    private void Awake()
    {
        roundManager = FindFirstObjectByType<RoundManager>();
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        introSequence = StartCoroutine(IntroSequenceText());
    }

    private void Update()
    {
        if (roundManager.hasGameStarted && introSequence != null)
        {
            StopCoroutine(introSequence);
            introSequence = null;
        }
    }

    private IEnumerator IntroSequenceText()
    {
        string[] introLines = new string[] { intro1, intro2, intro3, intro4, intro5, intro6 };
        yield return new WaitForSeconds(0.75f);

        //Intro dialogue, can be skipped if the player puts a token on the grid
        for (int i = 0; i < introLines.Length; i++)
        {
            StartCoroutine(OnLineSaid(introLines[i], 4f));
            yield return new WaitForSeconds(4.75f);
        }

        introSequence = null;
    }

    private IEnumerator OnLineSaid(string line, float duration)
    {
        if (line == "")
        {
            yield break;
        }
        int lineLength = line.Length / 2;
        SubtitleHandler.ShowSubtitle(line, duration);

        for (int i = 0; i < lineLength; i++)
        {
            audioSource.PlayOneShot(speechClip);
            float updatedFrequency = speechFrequency;
            //Slow down the arrival of the next speech clip depending on the current symbol
            updatedFrequency *= Regex.IsMatch(line[i].ToString(), @"[.,!?;:'""()\[\]{}\-—]") ? 2 : line[i].ToString() == " " ? 1.5f : 1;
            yield return new WaitForSeconds(updatedFrequency);
        }
    }

    public static void SayLine(string line, float duration)
    {
        instance.StartCoroutine(instance.OnLineSaid(line, duration));
    }
}
