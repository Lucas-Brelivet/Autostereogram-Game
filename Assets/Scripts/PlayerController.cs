using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class PlayerController : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private Transform head;

    [SerializeField]
    private CharacterController characterController;

    [SerializeField]
    private float walkingSpeed = 1.5f;

    [SerializeField]
    private float runningSpeed = 4f;

    [SerializeField]
    private float accelerationOnGround = 3f;

    [SerializeField]
    private float decelerationOnGround = 6f;

    [SerializeField]
    private float accelerationInAir = 1.5f;

    [SerializeField]
    private float decelerationInAir = 3f;

    [SerializeField]
    private float cameraRotationSensitivity = 1;

    [SerializeField]
    private float jumpHeight = 1;

    [SerializeField]
    private AutostereogramGenerator autostereogramGenerator;
    
    [SerializeField]
    private Camera normalCamera;

    [SerializeField]
    public PlayerInput input;

    [SerializeField]
    [Range(0,1)]
    private float movementInputDeadZone = 0.1f;

    #endregion

    #region Non-serialized private fields
    //Velocity in object space;
    private Vector3 horizontalVelocity;
    private Vector3 verticalVelocity;
    private bool sprinting = false;
    private Vector3 movementInputVector = Vector3.zero;
    private Pose initialHeadLocalPose;
    private List<Interactable> interactables = new List<Interactable>();
    private Interactable currentInteractable;
    private Clickable currentClickable;

    #endregion

    #region Unity functions
    private void Start()
    {
        initialHeadLocalPose = new Pose(head.localPosition, head.localRotation);

        input.actions.FindActionMap("Display").Enable();
        input.actions.FindActionMap("Interactions").Enable();
    }

    private void FixedUpdate()
    {
        UpdateHorizontalVelocity();
        ApplyGravity();
        characterController.Move(transform.TransformVector((horizontalVelocity + verticalVelocity) * Time.fixedDeltaTime));
    }


    private void OnTriggerEnter(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if(interactable)
        {
            interactables.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if(interactable)
        {
            interactables.Remove(interactable);
        }
    }

    #endregion

    #region Helper functions
    private void UpdateHorizontalVelocity()
    {
        float horizontalSpeed = horizontalVelocity.magnitude;
        float currentTargetSpeed = sprinting ? runningSpeed : walkingSpeed;
        float accelerationMultiplier = sprinting ? runningSpeed / walkingSpeed : 1;

        if (movementInputVector.magnitude < movementInputDeadZone || horizontalSpeed > currentTargetSpeed)
        {
            float decelerationValue = characterController.isGrounded ? decelerationOnGround : decelerationInAir;
            float deltaSpeed = decelerationValue * accelerationMultiplier * Time.fixedDeltaTime;
            
            horizontalVelocity = deltaSpeed >= horizontalSpeed ? Vector3.zero : horizontalVelocity * (1 - deltaSpeed/horizontalSpeed);
        }
        else
        {
            float accelerationValue = characterController.isGrounded ? accelerationOnGround : accelerationInAir;
            float deltaSpeed = accelerationValue * accelerationMultiplier * Time.fixedDeltaTime;

            horizontalVelocity += deltaSpeed * movementInputVector;
            horizontalSpeed = horizontalVelocity.magnitude;
            if(horizontalSpeed > currentTargetSpeed)
            {
                horizontalVelocity *= currentTargetSpeed / horizontalSpeed;
            }
        }
    }

    private void ApplyGravity()
    {
        if(!characterController.isGrounded)
        {
            verticalVelocity += Physics.gravity * Time.fixedDeltaTime;
        }
    }

    #endregion

    #region Input fuctions
    public void OnMove(InputAction.CallbackContext context)
    {
        if(context.performed || context.canceled)
        {
            Vector2 inputVector = context.ReadValue<Vector2>();
            movementInputVector = new Vector3(inputVector.x, 0, inputVector.y);
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            Vector2 mouseDelta = context.ReadValue<Vector2>();
            float yAngle = mouseDelta.x * cameraRotationSensitivity;
            transform.Rotate(Vector3.up, yAngle, Space.World);

            float xAngle = -mouseDelta.y * cameraRotationSensitivity;
            float newHeadAngle = head.localRotation.eulerAngles.x + xAngle;
            newHeadAngle = (newHeadAngle + 180) % 360 - 180;
            if(newHeadAngle >= 90)
            {
                head.localRotation = Quaternion.AngleAxis(90, Vector3.right);
            }
            else if(newHeadAngle <= -90)
            {
                head.localRotation = Quaternion.AngleAxis(-90, Vector3.right);
            }
            else
            {
                head.localRotation = Quaternion.AngleAxis(newHeadAngle, Vector3.right);
            }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed && characterController.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * Physics.gravity.magnitude) * Vector3.up;
        }
    }

    public void OnToggleStereo(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            bool isStereoActive = autostereogramGenerator.gameObject.activeSelf;
            autostereogramGenerator.SetActive(!isStereoActive);
            normalCamera.gameObject.SetActive(isStereoActive);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(currentInteractable)
        {
            currentInteractable.EndInteraction(this);
            currentInteractable = null;
        }
        else if(interactables.Count > 0)
        {
            currentInteractable = interactables[interactables.Count-1];
            currentInteractable.Interact(this);
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            Vector2 mousePosition = input.actions.FindAction("MousePosition").ReadValue<Vector2>();

            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit = new RaycastHit();

            if(Physics.Raycast(ray, out hit))
            {
                currentClickable = hit.collider.GetComponent<Clickable>();
                if(currentClickable)
                {
                    currentClickable.ClickDown();
                }
            }
        }
        else if(context.canceled && currentClickable)
        {
            currentClickable.ClickUp();
        }
        
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            sprinting = true;
        }
        if(context.canceled)
        {
            sprinting = false;
        }
    }
    #endregion

    #region Public methods

    public void SetHeadPose(Pose pose)
    {
        head.position = pose.position;
        head.rotation = pose.rotation;
    }

    public void ReinitializeHeadPose()
    {
        head.localPosition = initialHeadLocalPose.position;
        head.localRotation = initialHeadLocalPose.rotation;
    }

    #endregion

}
