using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Functional Parameters")]
    public bool canLookAround = true;
    public bool canInteract = true;
    [Header("General Parameters")]


    [Header("Camera Parameters")]
    [SerializeField] private Camera cam;
    private Transform camParent;
    private Vector3 initialParentPosition;
    [SerializeField] private float swayAmplitude = 0.05f;
    [SerializeField] private float swaySpeed = 1f;
    private float xRotation;
    private float yRotation;
    [SerializeField, Range(0.1f, 10)] public float lookSpeedX = 2f;
    [SerializeField, Range(0.1f, 10)] public float lookSpeedY = 2f;
    [SerializeField, Range(1, 180)] private float upLookLimit = 80f;
    [SerializeField, Range(1, 180)] private float downLookLimit = 80f;
    [SerializeField, Range(1, 180)] private float rightLookLimit = 80f;
    [SerializeField, Range(1, 180)] private float leftLookLimit = 80f;

    [Header("Interaction Parameters")]
    public Interactable currentInteractable;
    [SerializeField] private float interactionDistance = 1.5f;
    public LayerMask interactableLayerMask;

    [Header("Other Parameters")]
    public int placeholder; //Delete if I need to serialize any parameters that could be classified as "other"

    void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        camParent = cam.transform.parent;
        initialParentPosition = camParent.localPosition;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleCameraMovements();
        HandleInteraction();
        HandleCameraSway();
    }

    private void HandleCameraMovements()
    {
        if (!canLookAround)
        {
            return;
        }
        xRotation -= Input.GetAxis("Mouse Y") * lookSpeedY;
        yRotation += Input.GetAxis("Mouse X") * lookSpeedX;
        xRotation = Mathf.Clamp(xRotation, -upLookLimit, downLookLimit);
        yRotation = Mathf.Clamp(yRotation, -leftLookLimit, rightLookLimit);
        cam.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
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
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactionDistance, interactableLayerMask))
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
}
