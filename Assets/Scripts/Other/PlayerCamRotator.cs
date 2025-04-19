using System;
using System.Collections;
using UnityEngine;

public class PlayerCamRotator : MonoBehaviour
{
    public enum Rotation
    {
        Left,
        Center,
        Right
    }

    public event Action<Rotation> OnPlayerTurn;

    [SerializeField] private Quaternion[] staticRotations;
    //X values are for the camera, Y values are for the player
    // 0 - Facing left vent
    // 1 - Facing grid
    // 2 - Facing right vent

    private Rotation currentRotation;
    public Coroutine currentRotationSequence;
    private Player player;
    [SerializeField] private Revolver revolver;

    private AudioSource audioSource;

    [SerializeField] private AudioClip[] shuffleClips;

    private void Awake()
    {
        player = GetComponent<Player>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        currentRotationSequence = StartCoroutine(ChangeRotation(Rotation.Center, 0.1f));
    }

    public void Turn(int directionIndex)
    {
        // 0 - Left
        // 1 - Right

        if (!player.enabled || !player.canInteract || 
            (directionIndex == 0 && currentRotation == Rotation.Left) ||
            (directionIndex == 1 && currentRotation == Rotation.Right) ||
            currentRotationSequence != null)
        {
            return;
        }
        Rotation targetRotation;
        targetRotation = directionIndex == 0 ? 
            (currentRotation == Rotation.Right ? Rotation.Center : Rotation.Left) : //Left
            (currentRotation == Rotation.Left ? Rotation.Center : Rotation.Right); //Right

        OnPlayerTurn.Invoke(targetRotation);

        int clipRNG = UnityEngine.Random.Range(0, shuffleClips.Length);
        audioSource.PlayOneShot(shuffleClips[clipRNG]);
        currentRotationSequence = StartCoroutine(ChangeRotation(targetRotation));


        if (targetRotation == Rotation.Center)
        {
            revolver.PutDown();
        }
    }

    public void ForceReturnToCenterRotation()
    {
        if (currentRotationSequence != null)
        {
            StopCoroutine(currentRotationSequence);
        }        
        currentRotationSequence = StartCoroutine(ChangeRotation(Rotation.Center));
    }

    private IEnumerator ChangeRotation(Rotation targetRotation, float duration = 0.75f)
    {
        float elapsedTime = 0;
        Quaternion targetQuat = 
            targetRotation == Rotation.Left ? staticRotations[0] : 
            targetRotation == Rotation.Center ? staticRotations[1] : 
            staticRotations[2];

        Quaternion camRotation = player.cam.transform.localRotation;
        Quaternion targetCamRotation = Quaternion.Euler(targetQuat.eulerAngles.x, 0, targetQuat.eulerAngles.z);

        Quaternion playerRotation = player.transform.rotation;
        Quaternion targetPlayerRotation = Quaternion.Euler(0, targetQuat.eulerAngles.y, 0);

        while (elapsedTime < duration)
        {
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetPlayerRotation, elapsedTime / duration);
            player.cam.transform.localRotation = Quaternion.Slerp(player.cam.transform.localRotation, targetCamRotation, elapsedTime / duration);
            yield return null;
            elapsedTime += Time.deltaTime;

            if (Quaternion.Angle(player.transform.rotation, targetPlayerRotation) < 0.1f &&
            Quaternion.Angle(player.cam.transform.localRotation, targetCamRotation) < 0.1f)
            {
                break;
            }
        }
        if (targetRotation != Rotation.Center)
        {
            revolver.PickUp();
        }
        currentRotation = targetRotation;
        player.transform.rotation = targetPlayerRotation;
        player.cam.transform.localRotation = targetCamRotation;
        currentRotationSequence = null;
    }
}
