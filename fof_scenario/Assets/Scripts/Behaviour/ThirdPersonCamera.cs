using UnityEngine;
using System.Collections;

[AddComponentMenu("FoF/ThirdPersonCamera")]
public class ThirdPersonCamera : MonoBehaviour
{
    #region Public Fields
    public float distance = 20.0f;
    public float height = 5.0f;
    public float heightDamping = 2.0f;
    public float lookAtHeight = 0.0f;
    public float lookAtForward = 0.0f;
    public float rotationSnapTime = 0.3F;
    public float distanceSnapTime;
    public float distanceMultiplier;
    public float rotationStrafeFactor = 0.5f;
    #endregion

    #region Protected Fields
    protected Vector3 lookAtVector;
    protected float usedDistance;
    protected float wantedRotationAngle;
    protected float wantedHeight;
    protected float currentRotationAngle;
    protected float currentHeight;
    protected Quaternion currentRotation;
    protected Vector3 wantedPosition;
    protected float yVelocity = 0.0F;
    protected float zVelocity = 0.0F;
    protected Transform target;
    protected CharacterController controller;
    protected bool isActive = false;

    protected bool isThird = true;
    protected Transform characterTranform;
    protected float wantedLookAtRight = 0.0f;
    protected float lookAtRight = 0.0f;
    #endregion

    #region Messages
    void SetupCamera(GameObject character)
    {
        characterTranform = character.transform;
        target = characterTranform;
        controller = character.GetComponent<CharacterController>();
        isActive = true;
    }

    void SetLookRight(float value)
    {
        wantedLookAtRight = value * rotationStrafeFactor;
    }

    void SwitchCamera()
    {
        isThird = !isThird;
        if (isThird)
        {
            distance = 2.43f;
            lookAtHeight = 1.3f;
        }
        else
        {
            distance = 0.0f;
            lookAtHeight = 2.0f;
        }
    }
    #endregion

    #region Unity Callbacks
    void Start()
    {
        //RenderSettings.fog = false;
    }

    void LateUpdate()
    {
        if (isActive)
        {
            float deltaTime = TimeManager.Instance.MasterSource.DeltaTime;

            if (Input.GetKeyDown(KeyCode.C))
                SwitchCamera();

            if (isThird)
                lookAtRight = 0.0f;
            else
                lookAtRight = Mathf.Lerp(lookAtRight, wantedLookAtRight, rotationSnapTime * deltaTime);

            lookAtVector = target.TransformDirection(new Vector3(lookAtRight, lookAtHeight, lookAtForward));
            wantedHeight = target.position.y + height;
            currentHeight = transform.position.y;

            wantedRotationAngle = target.eulerAngles.y;
            currentRotationAngle = transform.eulerAngles.y;

            float tmp = Mathf.SmoothDampAngle(currentRotationAngle, wantedRotationAngle, ref yVelocity, rotationSnapTime);
            if (tmp != float.NaN)
                currentRotationAngle = Mathf.SmoothDampAngle(currentRotationAngle, wantedRotationAngle, ref yVelocity, rotationSnapTime);

            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * deltaTime);

            wantedPosition = target.position;
            wantedPosition.y = currentHeight;

            usedDistance = Mathf.SmoothDampAngle(usedDistance, distance + (controller.velocity.magnitude * distanceMultiplier), ref zVelocity, distanceSnapTime);

            wantedPosition += Quaternion.Euler(0.0f, currentRotationAngle, 0.0f) * new Vector3(0.0f, 0.0f, -usedDistance);

            transform.position = wantedPosition;

            transform.LookAt(target.position + lookAtVector);
        }
    }
    #endregion
}