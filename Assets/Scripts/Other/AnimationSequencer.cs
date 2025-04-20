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
    [SerializeField] private Transform wraithLeftJumpscarePosition;
    [SerializeField] private Transform wraithRightJumpscarePosition;
    [SerializeField] private AudioClip jumpscareStartClip;
    [SerializeField] private AudioClip jumpscareEndClip;
    [SerializeField] private AudioClip neckSnapClip;

    private RoundManager roundManager;
    private Player player;
    private PlayerCamRotator playerCamScript;
    private Wraith wraith;
    private VentMonster ventMonster;
    private AmbientSoundManager ambientSoundManager;
    [SerializeField] private Animator wraithAnimator;
    [SerializeField] private Animator playerCamAnimator;
    private Coroutine currentSequence;
    private Coroutine currentEndSequence;

    private const string wraithLookUpAnim = "Wraith|DeadLookUp";
    private const string wraithJumpscareAnim = "Wraith|Jumpscare";
    private const string wraithCamJumpscareAnim = "PlayerCam|CamJumpscare";
    private const string wraithCamJumpscareRightAnim = "PlayerCam|CamJumpscareRight";
    private const string wraithCamJumpscareReachAnim = "PlayerCam|CamJumpscareReach";
    private const string wraithReachJumpscareAnim = "Wraith|JumpscareReach";

    private void Awake()
    {
        instance = this;
        roundManager = FindFirstObjectByType<RoundManager>();
        player = FindFirstObjectByType<Player>();
        wraith = FindFirstObjectByType<Wraith>();
        ambientSoundManager = FindFirstObjectByType<AmbientSoundManager>();
        playerCamScript = FindFirstObjectByType<PlayerCamRotator>();
        ventMonster = FindFirstObjectByType<VentMonster>();
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
        wraith.SetUpForDefaultJumpscare();
        SetUpForEndSequence();        
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

    private IEnumerator OnPlayerDeath(int sideIndex)
    {
        //The Wraith can appear either on the left or right, depending on the side index
        wraith.SetUpForDefaultJumpscare();
        SetUpForEndSequence();
        LightHandler.FlickerLights(5, false);
        yield return new WaitForSeconds(2);
        Vector3 wraithTargetPos = sideIndex == 0 ? wraithLeftJumpscarePosition.position : wraithRightJumpscarePosition.position;
        Quaternion wraithTargetRot = sideIndex == 0 ? wraithLeftJumpscarePosition.rotation : wraithRightJumpscarePosition.rotation;
        string camTargetAnim = sideIndex == 0 ? wraithCamJumpscareAnim : wraithCamJumpscareRightAnim;
        wraithAnimator.transform.SetPositionAndRotation(wraithTargetPos, wraithTargetRot);
        wraithAnimator.Play(wraithJumpscareAnim, 0, 0);
        player.cam.transform.SetParent(playerCamAnimator.transform, true);
        playerCamAnimator.Play(camTargetAnim, 0, 0);
        yield return new WaitForSeconds(0.25f);
        LightHandler.FlickerLights(3, true);
    }

    private IEnumerator OnPlayerReachDeath()
    {
        SetUpForEndSequence();
        yield return new WaitForSeconds(0.25f);
        wraithAnimator.Play(wraithReachJumpscareAnim, 0, 0);
        player.cam.transform.SetParent(playerCamAnimator.transform, true);
        playerCamAnimator.Play(wraithCamJumpscareReachAnim, 0, 0);
    }

    private void SetUpForEndSequence()
    {
        LightHandler.ToggleLights(true);
        ventMonster.Deactivate();
        wraith.shouldAct = false;
        ambientSoundManager.shouldPlaySound = false;
        player.DisablePlayer();
        playerCamScript.ForceReturnToCenterRotation();
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
        if (instance.currentEndSequence != null)
        {
            //Make sure end sequences do not get overwritten
            return;
        }
        instance.currentEndSequence = instance.StartCoroutine(instance.OnPlayerWin());        
    }

    public static void PlayerDeathSequence(int sideIndex = 0)
    {
        if (instance.currentSequence != null)
        {
            //Make sure the death animation actually plays
            instance.StopCoroutine(instance.currentSequence);
        }
        if (instance.currentEndSequence != null)
        {
            //Make sure end sequences do not get overwritten
            return;
        }
        instance.currentEndSequence = instance.StartCoroutine(instance.OnPlayerDeath(sideIndex));
    }

    public static void PlayerReachDeathSequence()
    {
        if (instance.currentSequence != null)
        {
            //Make sure the death animation actually plays
            instance.StopCoroutine(instance.currentSequence);
        }
        if (instance.currentEndSequence != null)
        {
            //Make sure end sequences do not get overwritten
            return;
        }
        instance.currentEndSequence = instance.StartCoroutine(instance.OnPlayerReachDeath());
    }
}
