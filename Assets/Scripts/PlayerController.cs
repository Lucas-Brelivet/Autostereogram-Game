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
    private new Rigidbody rigidbody;

    [SerializeField]
    private float movementSpeed = 1.5f;

    [SerializeField]
    private float cameraRotationSensitivity = 1;

    [SerializeField]
    private float jumpImpulseSpeed = 2;

    [SerializeField][Tooltip("Max slope angle that the player can stand on in degrees")]
    private float maxSlopeAngle = 45;

    [SerializeField]
    private AutostereogramGenerator autostereogramGenerator;
    
    [SerializeField]
    private Camera normalCamera;

    [SerializeField]
    public PlayerInput input;

    #endregion

    #region Non-serialized private fields
    private bool playerIsOnGround = true;
    private Vector2 movementInputVector = Vector2.zero;
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
        rigidbody.velocity = transform.TransformVector(new Vector3(movementInputVector.x, 0, movementInputVector.y).normalized * movementSpeed + Vector3.up * rigidbody.velocity.y);

        playerIsOnGround = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach(ContactPoint contact in collision.contacts)
        {
            if(AngleToUpDegrees(contact.normal) < 90 - maxSlopeAngle)
            {
                playerIsOnGround = true;
                break;
            } 
        }
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
    /// <summary>
    /// calculates the angle in degrees between a vector and the up direction
    /// </summary>
    private float AngleToUpDegrees(Vector3 vector)
    {
        return Mathf.Acos(Vector3.Dot(vector, Vector3.up)) * 180 / Mathf.PI;
    }
    #endregion

    #region Input fuctions
    public void OnMove(InputAction.CallbackContext context)
    {
        if(context.performed || context.canceled)
        {
            movementInputVector = context.ReadValue<Vector2>();
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
        if(context.performed && playerIsOnGround)
        {
            rigidbody.velocity += Vector3.up * jumpImpulseSpeed;
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

    public void StartMouseInteraction()
    {
        input.actions.FindActionMap("MouseInteraction").Enable();
    }
    #endregion

}
