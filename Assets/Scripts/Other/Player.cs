using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Functional Parameters")]
    public bool canLookAround = true;
    public bool canInteract = true;

    [Header("Camera Parameters")]
    public Camera cam;
    private Transform camParent;
    private Vector3 initialParentPosition;
    [SerializeField] private float swayAmplitude = 0.05f;
    [SerializeField] private float swaySpeed = 1f;

    [Header("Interaction Parameters")]
    public Interactable currentInteractable;
    [SerializeField] private float interactionDistance = 1.5f;
    public LayerMask interactableLayerMask;

    [Header("Other Parameters")]
    private Pause pause;
    private Revolver revolver;
    private Rulebook book;

    private void Awake()
    {
        pause = FindFirstObjectByType<Pause>();
        revolver = FindFirstObjectByType<Revolver>();
        book = FindFirstObjectByType<Rulebook>();
        cam = GetComponentInChildren<Camera>();
        camParent = cam.transform.parent;
        initialParentPosition = camParent.localPosition;
    }

    private void Update()
    {
        HandleInteraction();
        HandleCameraSway();
    }

    private void HandleCameraSway()
    {
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmplitude;

        // Slight up/down + small rotation
        camParent.localPosition = initialParentPosition + new Vector3(0f, sway, 0f);
    }

    private void HandleInteraction()
    {
        if (!canInteract)
        {
            return;
        }
        //Click mechanics
        if (Input.GetKeyDown(KeyCode.Mouse0) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }

        //Hover mechanics
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayerMask))
        {
            Interactable interactable = hit.transform.GetComponent<Interactable>();
            if (interactable != null)
            {
                //Properly nullify the previous interactable
                if (interactable != currentInteractable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnHoverExit();
                    }
                    currentInteractable = interactable;
                    currentInteractable.OnHoverEnter();
                }
                return;
            }
        }
        // If no hit or not interactable
        if (currentInteractable != null)
        {
            currentInteractable.OnHoverExit();
            currentInteractable = null;
        }
        
    }

    public void DisablePlayer()
    {
        //Used for animation sequences
        //Prevents the player from acting and pausing the game
        pause.enabled = false;
        enabled = false;
        if (revolver.isPickedUp)
        {
            revolver.PutDown();
        }
        if (book.isPickedUp)
        {
            book.PutBookBack();
        }       
    }
}
