using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;

public class AIVoiceLines : MonoBehaviour
{
    private const string intro1 = "ai_intro1";
    private const string intro2 = "ai_intro2";
    private const string intro3 = "ai_intro3";
    private const string intro4 = "ai_intro4";
    private const string intro5 = "ai_intro5";
    private const string intro6 = "ai_intro6";
    public static string roundWonPlayer = "ai_round_won_player";
    public static string roundWonAI = "ai_round_won_ai";
    public static string difficultySwitch = "ai_difficulty_switch";
    public static string gameWonPlayer1 = "ai_game_won_player1";
    public static string gameWonPlayer2 = "ai_game_won_player2";
    public static string gameWonAI = "ai_game_won_ai";
    public static string gameTie = "ai_game_tie";
    public static string blockPlayer = "ai_block";

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
        string localizedLine = LocalizationManager.Instance.Get(line);
        int lineLength = localizedLine.Length / 2;
        SubtitleHandler.ShowSubtitle(localizedLine, duration);

        for (int i = 0; i < lineLength; i++)
        {
            audioSource.PlayOneShot(speechClip);
            float updatedFrequency = speechFrequency;
            //Slow down the arrival of the next speech clip depending on the current symbol
            updatedFrequency *= Regex.IsMatch(localizedLine[i].ToString(), @"[.,!?;:'""()\[\]{}\-—]") ? 2 : localizedLine[i].ToString() == " " ? 1.5f : 1;
            yield return new WaitForSeconds(updatedFrequency);
        }
    }

    public static void SayLine(string line, float duration)
    {
        instance.StartCoroutine(instance.OnLineSaid(line, duration));
    }
}
