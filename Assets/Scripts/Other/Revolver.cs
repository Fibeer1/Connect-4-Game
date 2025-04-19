using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Revolver : MonoBehaviour
{
    [SerializeField] private GameObject shootVFX;
    [SerializeField] private GameObject metalVFX;
    [SerializeField] private Transform shootPos;
    [SerializeField] private Vector3 heldOffset;
    [SerializeField] private Quaternion heldRotation;
    [SerializeField] private float heldRotationSpeed = 10f;
    private Vector3 putDownPosition;
    private Quaternion putDownRotation;

    private Coroutine moveCoroutine;

    private Animator animator;
    private Player player;
    private AudioSource audioSource;

    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip putDownClip;

    private RaycastHit hit;

    public int bulletCount = 6;
    public int defaultBulletCount = 6;

    private const string stationaryAnim = "Stationary";
    private const string idleAnim = "Held";
    private const string shootAnim = "Shoot";
    private const string emptyAnim = "NoAmmo";

    public bool isPickedUp = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        player = FindFirstObjectByType<Player>();
        
    }

    private void Start()
    {
        putDownPosition = transform.position;
        putDownRotation = transform.rotation;
        animator.Play(stationaryAnim);
    }

    private void Update()
    {
        HandleShooting();
    }

    private void HandleShooting()
    {
        if (!isPickedUp)
        {
            return;
        }

        Ray ray = player.cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100))
        {
            Vector3 targetDir = hit.point - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * heldRotationSpeed);
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //Using crossfade here so that the animation can be played multiple times
            string targetAnim = bulletCount > 0 ? shootAnim : emptyAnim;
            animator.Play(targetAnim, 0, 0);
        }
    }

    public void PickUp()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveAnimation(player.transform.position + 
            player.transform.rotation * heldOffset, heldRotation * player.transform.rotation, true));
    }

    public void PutDown()
    {
        isPickedUp = false;
        audioSource.PlayOneShot(putDownClip);
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveAnimation(putDownPosition, putDownRotation, false));
    }

    public void ClickSound()
    {
        audioSource.PlayOneShot(clickClip);
    }

    public void Shoot()
    {
        bulletCount--;
        CameraShake.Shake(0.4f, 0.015f);
        Instantiate(shootVFX, shootPos.position, shootPos.rotation);
        audioSource.PlayOneShot(clickClip);
        audioSource.PlayOneShot(shootClip);       
        if (hit.transform != null)
        {          
            if (hit.transform.tag == "Metal")
            {
                //Wall/vent has been hit
                Instantiate(metalVFX, hit.point, Quaternion.LookRotation(hit.normal));
            }

            VentMonster hitMonster = hit.transform.GetComponent<VentMonster>();
            if (hitMonster != null)
            {
                //Vent monster has been hit
                hitMonster.OnMonsterHit();
            }            
        }
    }

    private IEnumerator MoveAnimation(Vector3 targetPosition, Quaternion targetRotation, bool pickedUp)
    {
        float elapsedTime = 0;
        float duration = 0.75f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, elapsedTime / duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, elapsedTime / duration);
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f &&
                Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {                           
                break;
            }
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        string animToPlay = pickedUp ? idleAnim : stationaryAnim;
        isPickedUp = pickedUp;
        animator.Play(animToPlay);
        moveCoroutine = null;
    }
}
