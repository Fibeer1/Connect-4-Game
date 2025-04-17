using UnityEngine;

public class Connect4Token : MonoBehaviour
{
    //Since the tokens use physics-based movement and collision to move to their designated cell,
    //If too many tokens stack on top of each other, they start overlapping with each other because of gravity continuously pushing them down
    //This script is used to prevent that, by stopping physics simulation when detecting another token

    private Rigidbody rb;
    [SerializeField] private LayerMask landingMask;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private float snapOffset = 0.05f;
    public bool hasLanded = false;

    private AudioSource audioSource;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (hasLanded)
        {
            return;
        }
        Vector3 checkPos = transform.position + Vector3.down * snapOffset;
        Collider[] collidedTokens = Physics.OverlapSphere(checkPos, checkRadius, landingMask);
        foreach (Collider token in collidedTokens)
        {
            if (token.gameObject != gameObject) //Ignore self
            {
                OnLand();
                break;
            }
        }        
    }

    private void OnLand()
    {
        audioSource.Play();
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        hasLanded = true;
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmo which helps me determine where the sphere is and how big it is
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * snapOffset, checkRadius);
    }
}
