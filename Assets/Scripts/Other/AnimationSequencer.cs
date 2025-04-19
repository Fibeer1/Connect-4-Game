using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimationSequencer : MonoBehaviour
{
    public static AnimationSequencer instance;
    [SerializeField] private AudioSource normalAS;
    [SerializeField] private AudioSource muffledAS;

    [Header("Round Change Variables")]
    [SerializeField] private AudioClip whooshClip;
    [SerializeField] private AudioClip bangingClip;

    [Header("Player Win Variables")]
    [SerializeField] private GameObject wraithFaceLight;
    [SerializeField] private AudioSource ambience1;
    [SerializeField] private AudioSource ambience2;

    [Header("Player Death Variables")]
    [SerializeField] private Transform wraithJumpscarePosition;
    [SerializeField] private AudioClip jumpscareStartClip;
    [SerializeField] private AudioClip jumpscareEndClip;
    [SerializeField] private AudioClip neckSnapClip;

    private RoundManager roundManager;
    private Player player;
    private PlayerCamRotator playerCamScript;
    private Wraith wraith;
    private AmbientSoundManager ambientSoundManager;
    [SerializeField] private Animator wraithAnimator;
    [SerializeField] private Animator playerCamAnimator;
    private Coroutine currentSequence;

    private const string wraithLookUpAnim = "Wraith|DeadLookUp";
    private const string wraithJumpscareAnim = "Wraith|Jumpscare";
    private const string wraithCamJumpscareAnim = "PlayerCam|CamJumpscare";
    

    private void Awake()
    {
        instance = this;
        roundManager = FindFirstObjectByType<RoundManager>();
        player = FindFirstObjectByType<Player>();
        wraith = FindFirstObjectByType<Wraith>();
        ambientSoundManager = FindFirstObjectByType<AmbientSoundManager>();
        playerCamScript = FindFirstObjectByType<PlayerCamRotator>();
        wraithFaceLight.SetActive(false);
    }
    
    private IEnumerator OnRoundChange(int winnerIndex)
    {
        LightHandler.FlickerLights(3, false);
        yield return new WaitForSeconds(0.5f);
        roundManager.UpdateScores(winnerIndex);
        normalAS.PlayOneShot(whooshClip);
        yield return new WaitForSeconds(0.5f);
        muffledAS.PlayOneShot(bangingClip);
        yield return new WaitForSeconds(3f);
        LightHandler.FlickerLights(3, true);
        roundManager.BeginNewRound();
        currentSequence = null;
    }

    private IEnumerator OnPlayerWin()
    {
        wraith.shouldAct = false;
        ambientSoundManager.shouldPlaySound = false;
        LightHandler.ToggleLights(true);
        player.DisablePlayer();
        playerCamScript.ForceReturnToCenterRotation();
        float ambientSoundsVolume = 1;
        float elapsedTime = 0;
        float ambiencefadeDuration = 3;
        yield return new WaitForSeconds(2);
        while (elapsedTime < ambiencefadeDuration)
        {
            ambientSoundsVolume = Mathf.Lerp(ambientSoundsVolume, 0, elapsedTime / ambiencefadeDuration);
            ambience1.volume = ambientSoundsVolume;
            ambience2.volume = ambientSoundsVolume;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        AIVoiceLines.SayLine(AIVoiceLines.gameWonPlayer2, 3);
        yield return new WaitForSeconds(3);
        wraithAnimator.Play(wraithLookUpAnim, 0, 0);
        yield return new WaitForSeconds(2);
        LightHandler.FlickerLights(5, false, 0.05f, 0.075f);
        wraithFaceLight.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        wraithFaceLight.SetActive(false);
        yield return new WaitForSeconds(1);
        EndScreen.EndGame(true);
    }

    private IEnumerator OnPlayerDeath()
    {
        wraith.shouldAct = false;
        ambientSoundManager.shouldPlaySound = false;
        LightHandler.FlickerLights(5, false);
        player.DisablePlayer();
        playerCamScript.ForceReturnToCenterRotation();
        yield return new WaitForSeconds(2);
        wraithAnimator.transform.position = wraithJumpscarePosition.position;
        wraithAnimator.transform.rotation = wraithJumpscarePosition.rotation;
        wraithAnimator.Play(wraithJumpscareAnim, 0, 0);
        player.cam.transform.SetParent(playerCamAnimator.transform, true);
        playerCamAnimator.Play(wraithCamJumpscareAnim, 0, 0);
        yield return new WaitForSeconds(0.25f);
        LightHandler.FlickerLights(3, true);
    }

    public void JumpscareStartSound()
    {
        //This method and the others below it will be called as animation events
        //This one plays when the Wraith grabs the player
        normalAS.PlayOneShot(jumpscareStartClip);
    }

    public void JumpscareEndSound()
    {
        //Plays when the Wraith snaps the player's neck
        normalAS.Stop();
        normalAS.PlayOneShot(jumpscareEndClip);
        normalAS.PlayOneShot(neckSnapClip);
    }

    public void DeathEndGame()
    {
        //Plays a bit after the player's neck gets snapped
        EndScreen.EndGame(false, 0, 0.1f);
    }

    public static void RoundChangeSequence(int winnerIndex)
    {
        if (instance.currentSequence != null)
        {
            return;
        }
        instance.currentSequence = instance.StartCoroutine(instance.OnRoundChange(winnerIndex));
    }

    public static void PlayerWinSequence()
    {
        if (instance.currentSequence != null)
        {
            //Make sure the win animation actually plays
            instance.StopCoroutine(instance.currentSequence);
        }
        instance.currentSequence = instance.StartCoroutine(instance.OnPlayerWin());        
    }

    public static void PlayerDeathSequence()
    {
        if (instance.currentSequence != null)
        {
            //Make sure the death animation actually plays
            instance.StopCoroutine(instance.currentSequence);
        }
        instance.currentSequence = instance.StartCoroutine(instance.OnPlayerDeath());
    }
}
