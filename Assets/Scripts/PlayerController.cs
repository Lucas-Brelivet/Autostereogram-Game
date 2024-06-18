using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class PlayerController : MonoBehaviour
{
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

    private bool playerIsOnGround = true;
    private Vector2 movementInputVector = Vector2.zero;

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

    /// <summary>
    /// calculates the angle in degrees between a vector and the up direction
    /// </summary>
    private float AngleToUpDegrees(Vector3 vector)
    {
        return Mathf.Acos(Vector3.Dot(vector, Vector3.up)) * 180 / Mathf.PI;
    }

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


}
