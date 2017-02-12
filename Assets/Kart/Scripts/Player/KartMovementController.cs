using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KartMovementController : MonoBehaviour
{
    [SerializeField]
    private Camera _kartCamera;
    [SerializeField]
    private float _turnSmoothing = 15;
    [SerializeField]
    private float _maxVelocity = 10f;
    [SerializeField]
    private float _accelerationRate = 0.2f;

    private Rigidbody _rigidBody;
    private float _velocity = 0f;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // TODO: move this into a centralized input controller?
        float horizontalInput = Input.GetAxis(InputStatics.LEFT_STICK_HORIZONTAL);
        float verticalInput = Input.GetAxis(InputStatics.LEFT_STICK_VERTICAL);

        UpdateRotation(horizontalInput, verticalInput);

        UpdatePosition(Input.GetKey(KeyCode.Joystick1Button0));
    }

    private void UpdateRotation(float horizontalInput, float verticalInput)
    {
        if (horizontalInput != 0 || verticalInput != 0)
        {
            Vector3 targetDirection;

            //Scale the forward and right vectors by their corresponding input to get the weighted impact that the camera vectors will have onb character orientation
            //Then add the two vectors together to get an accurate target direction based on the camera
            targetDirection = verticalInput * _kartCamera.transform.forward + horizontalInput * _kartCamera.transform.right;
            //Zero out the target because we don;t want our character to be angled based on the camera
            targetDirection.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            Quaternion newRotation = Quaternion.Lerp(GetComponent<Rigidbody>().rotation, targetRotation, _turnSmoothing * Time.deltaTime);
            _rigidBody.MoveRotation(newRotation);
        }
    }

    private void UpdatePosition(bool isAccelerating)
    {
        if (isAccelerating)
        {
            _rigidBody.AddForce(transform.forward.normalized * _accelerationRate);
        }
    }
}
