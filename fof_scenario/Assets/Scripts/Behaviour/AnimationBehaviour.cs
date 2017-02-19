using System;
using UnityEngine;
using System.Collections;
using SBS.Math;

[AddComponentMenu("FoF/AnimationBehaviour")]
[RequireComponent(typeof(CharacterController))]
public class AnimationBehaviour : MonoBehaviour
{
    #region Internal Data Structure
    public const int Stay = 0;
    public const int Walk = 1;
    public const int Choose = 2;
    #endregion

    #region Public Properties

    public Animation CharacterAnimation
    {
        get { return characterAnimation; }
    }

    #endregion

    #region Protected Fields
    protected float walkSpeed = 1.6f;
    protected float walkFastSpeed = 2.0f;
    protected float speedFactor = 1.0f;
    protected Animation characterAnimation;

    protected bool isPlayingStay = false;

    public bool IsPlayingStay
    {
        get { return characterAnimation.IsPlaying("stay"); }
    }
    #endregion

    #region Unity Callbacks
    void Start()
    {
        Transform child = transform.GetChild(0).GetChild(0);
        characterAnimation = child.GetComponent<Animation>();
    }
    #endregion

    #region FSM States
    void OnStayEnter()
    {
        if (null != characterAnimation)
            characterAnimation.CrossFade("stay");
        GameObject.FindGameObjectWithTag("Interface").SendMessage("UpdateWalkingSpeed", -1);
    }
    void OnStayExec()
    {
    }
    void OnStayExit()
    {
    }

    void OnWalkEnter()
    {
        characterAnimation["walk"].speed = walkSpeed * speedFactor;
        characterAnimation.CrossFade("walk");
        GameObject.FindGameObjectWithTag("Interface").SendMessage("UpdateWalkingSpeed", 1);
    }
    void OnWalkExec()
    {
    }
    void OnWalkExit()
    {
    }

    void OnWalkFastEnter()
    {
        characterAnimation["walk"].speed = walkFastSpeed * speedFactor;
        characterAnimation.CrossFade("walk");
        GameObject.FindGameObjectWithTag("Interface").SendMessage("UpdateWalkingSpeed", 2);
    }
    void OnWalkFastExec()
    {
    }
    void OnWalkFastExit()
    {
    }

    void OnChooseEnter()
    {
        characterAnimation.CrossFade("stay");
        GameObject.FindGameObjectWithTag("Interface").SendMessage("UpdateWalkingSpeed", -1);
    }
    void OnChooseExec()
    {
    }
    void OnChooseExit()
    {
    }
    #endregion

    #region Public Members
    public void Init(float speedFactor)
    {
        this.speedFactor = speedFactor;
    }
    #endregion
}
