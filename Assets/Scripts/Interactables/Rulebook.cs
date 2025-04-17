using System.Collections;
using UnityEngine;

public class Rulebook : Interactable
{
    private Vector3 stationaryPosition;
    private Quaternion stationaryRotation;
    [SerializeField] private Vector3 heldPosition;
    [SerializeField] private Quaternion heldRotation;

    [SerializeField] private Quaternion heldCamRotation;
    private Quaternion defaultCamRotation;

    private Animator animator;
    private Player player;
    private PlayerCamRotator playerRotator;

    private Coroutine openCloseMovement;

    private const string stationaryAnim = "Stationary";
    private const string openAnim = "Open";
    private const string closeAnim = "Close";

    private AudioSource audioSource;

    [SerializeField] private AudioClip putDownClip;

    private bool pickedUp = false;

    //Animations only change the rotation of the bones in the armature of the book
    //Positions are changed in code
    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        player = FindFirstObjectByType<Player>();
        playerRotator = player.GetComponent<PlayerCamRotator>();
    }

    private void Start()
    {
        animator.Play(stationaryAnim);
        stationaryPosition = transform.position;
        stationaryRotation = transform.rotation;
        defaultCamRotation = player.cam.transform.localRotation;
    }

    private void Update()
    {
        if (!pickedUp)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Mouse1) && openCloseMovement == null)
        {
            //RMB to close the book
            animator.Play(closeAnim, 0, 0);
            player.canInteract = true;
            audioSource.PlayOneShot(putDownClip);
            openCloseMovement = StartCoroutine(MoveBook(stationaryPosition, stationaryRotation, false));
            //Make sure the camera can only rotate from one script at a time
            playerRotator.currentRotationSequence = openCloseMovement;
        }
    }

    public override void Interact()
    {
        base.Interact();
        if (openCloseMovement == null)
        {
            player.canInteract = false;
            animator.Play(openAnim, 0, 0);
            openCloseMovement = StartCoroutine(MoveBook(heldPosition, heldRotation, true));
            playerRotator.currentRotationSequence = openCloseMovement;
        }
    }

    private IEnumerator MoveBook(Vector3 targetPosition, Quaternion targetRotation, bool shouldPickUp = false, float duration = 1.5f)
    {
        if (!shouldPickUp)
        {
            //Prematurely deactivate the text object before the animation happens,
                //if the player will not hold the book
            Pause.ToggleInteractionControls(false);
        }
        float elapsedTime = 0;
        Quaternion targetCamRotation = shouldPickUp ? heldCamRotation : defaultCamRotation;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Slerp(transform.position, targetPosition, elapsedTime / duration);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsedTime / duration);
            player.cam.transform.localRotation = Quaternion.Slerp(player.cam.transform.localRotation,
                    targetCamRotation, elapsedTime / duration);

            yield return null;
            elapsedTime += Time.deltaTime;

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f &&
                Quaternion.Angle(transform.rotation, targetRotation) < 0.01f &&
                Quaternion.Angle(player.cam.transform.localRotation, targetCamRotation) < 0.01f)
            {
                
                break;
            }
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
        player.cam.transform.localRotation = targetCamRotation;

        if (playerRotator.currentRotationSequence == openCloseMovement)
        {
            playerRotator.currentRotationSequence = null;
        }
        openCloseMovement = null;
        if (shouldPickUp)
        {
            //Activate the text object after the animation happens,
                //if the player is holding the book
            Pause.ToggleInteractionControls(true, "RMB - Close book");
        }
        pickedUp = shouldPickUp;       
    }
}
