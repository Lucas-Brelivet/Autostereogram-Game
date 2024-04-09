using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private Transform head;

    [SerializeField]
    private float movementSpeed = 1.5f;

    [SerializeField]
    private float cameraRotationSensitivity = 1;

    private Vector3 velocity;

    private void Start()
    {

    }

    private void Update()
    {
        if(AttitudeSensor.current != null && !AttitudeSensor.current.enabled)
        {
            if(!AttitudeSensor.current.enabled)
            {
                InputSystem.EnableDevice(AttitudeSensor.current);
            }
            Debug.Log(AttitudeSensor.current.attitude.ReadValue());
        }
        transform.Translate(velocity * Time.deltaTime, Space.Self);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 inputVector = context.ReadValue<Vector2>();
        velocity = new Vector3(inputVector.x, 0, inputVector.y).normalized * movementSpeed;
    }

    public void OnLookDesktop(InputAction.CallbackContext context)
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

    public void OnLookHandHeld(InputAction.CallbackContext context)
    {
        Quaternion attitude = context.ReadValue<Quaternion>();
        Debug.Log(attitude);
        head.localRotation = attitude;
    }


}
