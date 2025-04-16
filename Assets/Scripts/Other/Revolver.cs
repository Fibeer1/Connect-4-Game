using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Revolver : MonoBehaviour
{
    [SerializeField] private GameObject shootVFX;
    [SerializeField] private Transform shootPos;
    [SerializeField] private Vector3 heldOffset;
    [SerializeField] private Quaternion heldRotation;
    [SerializeField] private float heldRotationSpeed = 10f;
    private Vector3 putDownPosition;
    private Quaternion putDownRotation;

    private Animator animator;
    private Player player;

    public int bulletCount = 6;
    public int defaultBulletCount = 6;

    private const string stationaryAnim = "Stationary";
    private const string idleAnim = "Held";
    private const string shootAnim = "Shoot";
    private const string emptyAnim = "NoAmmo";

    private bool isPickedUp = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
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

        if (Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            Vector3 targetDir = hit.point - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * heldRotationSpeed);
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //Using crossfade here so that the animation can be played multiple times
            animator.CrossFadeInFixedTime(shootAnim, 0.15f);
            string targetAnim = bulletCount > 0 ? shootAnim : emptyAnim;
            animator.CrossFadeInFixedTime(targetAnim, 0.15f);
        }
    }

    public void PickUp()
    {
        StartCoroutine(MoveAnimation(player.transform.position + player.transform.rotation * heldOffset, heldRotation * player.transform.rotation, true));
    }

    public void PutDown()
    {
        isPickedUp = false;
        StartCoroutine(MoveAnimation(putDownPosition, putDownRotation, false));
    }

    public void Shoot()
    {
        bulletCount--;
        CameraShake.Shake(0.4f, 0.015f);
        Instantiate(shootVFX, shootPos.position, shootPos.rotation);
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

        string animToPlay = pickedUp ? idleAnim : stationaryAnim;
        isPickedUp = pickedUp;
        animator.Play(animToPlay);
    }
}
