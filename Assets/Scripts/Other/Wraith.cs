using UnityEngine;

public class Wraith : MonoBehaviour
{
    //DESCRIPTION:
    //The Wraith will wait for the player to face a vent, at which point he will start
        //waiting. After he is done waiting, he will try to reach the player.
        //If the player turns towards the center during this time, he will go back to his "dead" pose.
        //If he does not turn in time, the Wraith will kill the player

    private enum Action
    {
        Dead, //When the Wraith is not waiting and the player is facing the center
        Wait, //When the player is not facing the center and the Wraith is waiting to start reaching towards him
        Reach //When the player is not facing the center and the Wraith is trying to reach him
    }

    private Action currentAction;

    private PlayerCamRotator camRotator;
    private RoundManager roundManager;
    private Animator animator;

    [SerializeField] private float timeToReach = 3f;
    //Time it takes for the Wraith to reach the player
    [SerializeField] private float timeToStartReaching = 3f;
    //Time it takes for the Wraith to start reaching towards the player

    [SerializeField] private float reachTimer;
    [SerializeField] private float startReachTimer;

    private const string deadAnim = "Wraith|Dead";
    private const string reachAnim = "Wraith|DeadReach";
    private const string reachBackAnim = "Wraith|DeadReachBack";

    public bool shouldAct = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        camRotator = FindFirstObjectByType<PlayerCamRotator>();
        roundManager = FindFirstObjectByType<RoundManager>();
    }

    private void Start()
    {
        camRotator.OnPlayerTurn += OnPlayerTurn;
        animator.Play(deadAnim, 0, 0);
    }

    private void Update()
    {
        if ((!roundManager.hasGameStarted || roundManager.isGameOver) || !shouldAct)
        {
            //Do not act if the game is not in progress
            return;
        }
        HandleWaitingToReach();
        HandleReaching();
    }

    private void HandleWaitingToReach()
    {
        if (currentAction != Action.Wait)
        {
            return;
        }
        startReachTimer -= Time.deltaTime;
        if (startReachTimer <= 0)
        {
            currentAction = Action.Reach;
            animator.Play(reachAnim, 0, 0);
        }
    }

    private void HandleReaching()
    {
        if (currentAction != Action.Reach)
        {
            return;
        }
        reachTimer -= Time.deltaTime;
        if (reachTimer <= 0)
        {
            shouldAct = false;
            AnimationSequencer.PlayerDeathSequence();
        }
    }

    private void OnPlayerTurn(PlayerCamRotator.Rotation rotation)
    {
        if (!shouldAct)
        {
            return;
        }
        if (rotation == PlayerCamRotator.Rotation.Center)
        {
            //The player is facing the center
            if (currentAction == Action.Reach)
            {
                animator.Play(reachBackAnim, 0, 0);
            }
            currentAction = Action.Dead;
        }
        else
        {
            //The player is facing a vent
            if (currentAction == Action.Dead)
            {
                startReachTimer = timeToStartReaching;
                reachTimer = timeToReach;
                currentAction = Action.Wait;
            }
        }
    }
}
