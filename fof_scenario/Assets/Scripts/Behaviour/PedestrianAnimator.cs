using UnityEngine;
using System.Collections;

public class PedestrianAnimator : MonoBehaviour
{  
    #region Protected Members
    protected Animation pedestrianAnimation;
    protected PedestrianKinematicBehaviour pedKinematic;
    protected float walkSpeed = 0.8f;
    #endregion

    #region Public Properties
    public Animation PedestrianAnimation
    {
        get { return pedestrianAnimation; }
    }

    public float speedFactor = 1.0f;
    #endregion

    #region FSMStates
    #region Stay
    void OnStayEnter()
    {
        if (null != pedestrianAnimation)
            pedestrianAnimation.CrossFade("stay");
    }

    void OnStayExec()
    { }

    void OnStayExit()
    { }
    #endregion

    #region Walk
    void OnWalkEnter()
    {
        pedestrianAnimation["walk"].speed = walkSpeed * speedFactor;
        pedestrianAnimation.CrossFade("walk");
        
    }

    void OnWalkExec()
    {
        if (walkSpeed != pedKinematic.Speed)
        {
            walkSpeed = pedKinematic.Speed;
            pedestrianAnimation["walk"].speed = pedKinematic.Speed * speedFactor;
        }
    }

    void OnWalkExit()
    { }
    #endregion
    #endregion

    #region Unity Callbacks
    void Start()
    {
        Transform child = transform.GetChild(0);
        pedestrianAnimation = child.GetComponent<Animation>();
        pedKinematic = gameObject.GetComponent<PedestrianKinematicBehaviour>();
    }

    #endregion
}
