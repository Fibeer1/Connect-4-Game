using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class VentMonster : MonoBehaviour
{
    public enum Action
    {
        Spawn,
        Wait,
        Attack,
        Flee
    }

    [SerializeField] private GameObject[] ventColliders;
    [SerializeField] private Transform[] ventDoors;
    [SerializeField] private Transform[] spawnPositions;
    [SerializeField] private Transform[] attackPositions;

    private int sideIndex;
    // 0 - West (left of the player)
    // 1 - East (right of the player)

    private bool[] ventsRotated = new bool[2];
    //Keeps track of the opened vents, cause they only need to be opened once

    public Action currentAction;
    private Coroutine moveCoroutine;

    [SerializeField] private float spawnTimer;
    [SerializeField] private float waitTimer;
    [SerializeField] private float spawnTimerMin = 20f;
    [SerializeField] private float spawnTimerMax = 40f;
    [SerializeField] private float waitTimerMin = 5f;
    [SerializeField] private float waitTimerMax = 10f;
    //spawnTimer is used when the monster is waiting to spawn
    //waitTimer is used when the monster prepares to attack the player

    [SerializeField] private float moveDuration = 2f;
    //The time it takes for the monster to reach the player
    private float footstepFrequency = 0.25f;

    [SerializeField] private float musicStingerThreshold = 0.75f;
    //The time at which the music stinger plays

    [SerializeField] private int hitsNeededToRetreat = 2; 
    //The number of bullets it takes for the monster to start retreating

    private AudioSource audioSource;
    [SerializeField] private AudioSource musicAS;
    [SerializeField] private AudioClip ventFootstepClip;
    [SerializeField] private AudioClip shriekClip;
    [SerializeField] private AudioClip growlClip;

    private RoundManager roundManager;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        roundManager = FindFirstObjectByType<RoundManager>();
    }

    private void Start()
    {
        PrepareToSpawn();
    }

    private void Update()
    {
        if (!roundManager.hasGameStarted || roundManager.isGameOver)
        {
            //Do not act if the game is not in progress
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }
            return;
        }
        HandleSpawnTimer();
        HandleWaiting();
    }

    private void HandleSpawnTimer()
    {
        if (currentAction != Action.Spawn)
        {
            return;
        }
        spawnTimer -= Time.deltaTime;
        if (spawnTimer < 0)
        {
            int ventIndex = Random.Range(0, 2);
            SpawnInVent(ventIndex);
        }
    }

    private void HandleWaiting()
    {
        if (currentAction != Action.Wait)
        {
            return;
        }
        waitTimer -= Time.deltaTime;
        if (waitTimer < 0)
        {
            //Disable the vent at the side the monster is at to allow the player to shoot it
            ventColliders[sideIndex].SetActive(false);
            StartCoroutine(RotateVentDoor(sideIndex));
            currentAction = Action.Attack;
            moveCoroutine = StartCoroutine(Move(transform.position, attackPositions[sideIndex].position, moveDuration));
        }
    }

    private IEnumerator RotateVentDoor(int doorIndex, float duration = 0.75f)
    {
        if (ventsRotated[doorIndex])
        {
            yield break;
        }
        float elapsedTime = 0;
        float targetAngle = doorIndex == 0 ? 90 : -90;
        Quaternion endRot = ventDoors[doorIndex].transform.localRotation * Quaternion.Euler(0, 0, targetAngle);
        while (elapsedTime < duration)
        {
            ventDoors[doorIndex].localRotation = 
                Quaternion.Slerp(ventDoors[doorIndex].localRotation, endRot, 
                elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            yield return null;
            if (Quaternion.Angle(ventDoors[doorIndex].localRotation, endRot) < 0.1f)
            {
                break;
            }
        }
        ventsRotated[doorIndex] = true;
        ventDoors[doorIndex].localRotation = endRot;
    }

    private IEnumerator Move(Vector3 fromPos, Vector3 toPos, float duration)
    {
        float elapsedTime = 0;
        float footstepDuration = footstepFrequency;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(fromPos, toPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            if (currentAction == Action.Attack && elapsedTime > musicStingerThreshold && !musicAS.isPlaying)
            {
                musicAS.Play();
            }

            footstepDuration -= Time.deltaTime;
            if (footstepDuration < 0)
            {
                audioSource.PlayOneShot(ventFootstepClip);
                footstepDuration = footstepFrequency;
            }
            yield return null;
        }
        
        if (currentAction == Action.Attack)
        {
            //The monster has reached the player         
            OnMonsterAttack();
        }
        else if (currentAction == Action.Flee)
        {
            //The monster has retreated
            PrepareToSpawn();
        }
    }

    public void Deactivate()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        gameObject.SetActive(false);
    }

    private void OnMonsterAttack()
    {
        AnimationSequencer.PlayerDeathSequence(sideIndex);
        Deactivate();
    }

    public void PrepareToSpawn()
    {
        spawnTimer = Random.Range(spawnTimerMin, spawnTimerMax);
        currentAction = Action.Spawn;
    }

    private void SpawnInVent(int ventIndex)
    {
        sideIndex = ventIndex;
        transform.position = spawnPositions[sideIndex].position;
        waitTimer = Random.Range(waitTimerMin, waitTimerMax);
        currentAction = Action.Wait;
        audioSource.PlayOneShot(growlClip);
        hitsNeededToRetreat = 2;
    }

    public void OnMonsterHit()
    {
        if (currentAction != Action.Attack)
        {
            return;
        }
        hitsNeededToRetreat--;
        if (hitsNeededToRetreat > 0)
        {
            return;
        }
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        CameraShake.Shake(1f, 0.015f);
        ventColliders[sideIndex].SetActive(true);
        currentAction = Action.Flee;
        audioSource.PlayOneShot(shriekClip);
        moveCoroutine = StartCoroutine(Move(transform.position, spawnPositions[sideIndex].position, moveDuration));
    }
}
