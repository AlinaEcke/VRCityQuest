using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS.Math;

[AddComponentMenu("FoF/PlayerKinematicBehaviour")]
[RequireComponent(typeof(CharacterController))]
public class PlayerKinematicBehaviour : MonoBehaviour
{
    #region Internal Data Structure
    public const int Stay = 0;
    public const int Walk = 1;
    public const int Choose = 2;
    public const int WalkFast = 3;
    public const int TurnBack = 4;

    public const bool TurnBackEnabled = true;
    #endregion

    #region Protected Fields
    //protected float canTurnBackTimer = -1.0f;
    //protected OVRCameraController CameraController = null;
    protected bool isRed = false;
    protected bool placed = false;
    protected bool inputEnabled = false;
    protected bool isWalkingSpeed2 = false;
    protected bool[] directionsEnabled;
    protected bool[] availableDirections = { false, false, false };
    protected int turnCounter = 0;
    protected int lastTurnState = Stay;
    protected float rotationTimer = -1.0f;
    protected float rotationDuration = 0.0f;
    protected float angleTarget = 0.0f;
    protected float verticalSpeed = 0.0f;
    protected float horizontalInput = 0.0f;
    protected float speed = 1.2f;
    protected float redBlinkTimer = -1.0f;
    protected float alpha = 0.7f;
    protected float inputDownTimer = -1.0f;
    protected float actualHorizontal = 0.0f;
    protected Vector3 rotationTarget = Vector3.zero;
    protected Vector3 gfxDirection = Vector3.forward;
    protected Vector3 velocity = Vector3.zero;
    protected TokensManager.TokenHit currentTokenHit = null;
    protected TokensManager.TokenHit prevTokenHit = null;
    protected GameObject coreManagers;
    protected GameObject gameManagers;
    protected CharacterController controller;
    protected BalanceBoardManager bbManager;
    protected TimeManager timeManager;
    protected EnvironmentManager envManager;
    protected FiniteStateMachine fsm;
    protected List<Material> materialList = new List<Material>();
    protected Transform characterRoot;
    protected GameObject cameraObj;
    public CQ_Interface interfaces;
    protected List<int> wallIndexes = new List<int>();
    protected GameplayManager gameplay = null;
    protected Preloader preloader = null;

    protected float speedFactor = 1.0f;
    protected float strafeFactor = 1.0f;
    protected float lastHorizontal = 0.0f;
	protected float rotationSpeed;

    protected int forwardIndex;
    protected int rightIndex;
    protected int leftIndex;

    protected Vector3 lastPosition = Vector3.zero;

    protected float redTimer = -1.0f;

    //TEST
    protected string tokenName = "";
    protected float longitudinal = 0.0f;
    protected float trasversal = 0.0f;
    protected GameObject floor = null;
    //TEST

    protected float lastHorizontalInput = 0.0f;
    protected string lastLeanDirection = string.Empty;

    protected float turnBackTimer = -1.0f;
    #endregion

    #region Public Get/Set
    public float VerticalSpeed { get { return verticalSpeed; } set { verticalSpeed = value; } }
    public bool IsRotating { get { return rotationTimer > 0.0f; } }
    public Vector3 Velocity { get { return velocity; } }
    public int KinematicState { get { return fsm.State; } set { fsm.State = value; } }
    public bool Collided { get { return isRed; } set { isRed = value; }}
    public bool InputEnabled { get { return inputEnabled; } }
    public TokensManager.TokenHit CurrentTokenHit { get { return currentTokenHit; } }
    #endregion

    #region Unity Callbacks
    void Start()
    {
        directionsEnabled = new bool[] { true, true, true, true };

        controller = GetComponent<CharacterController>();
        coreManagers = GameObject.FindGameObjectWithTag("CoreManagers");
        gameManagers = GameObject.FindGameObjectWithTag("GameManagers");
        bbManager = coreManagers.GetComponent<BalanceBoardManager>();
        timeManager = coreManagers.GetComponent<TimeManager>();
        gameplay = gameManagers.GetComponent<GameplayManager>();
        fsm = GetComponent<FiniteStateMachine>();
        preloader = GameObject.FindGameObjectWithTag("Preloader").GetComponent<Preloader>();

        characterRoot = transform.GetChild(0);

        cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        interfaces = GameObject.FindGameObjectWithTag("Interface").GetComponent<CQ_Interface>();


     /*   CameraController = gameObject.GetComponentInChildren<OVRCameraController>();   

        if (CameraController != null)
        {
            // Make sure to set the initial direction of the camera 
            // to match the game player direction
            CameraController.SetOrientationOffset(transform.rotation);
            CameraController.SetYRotation(0.0f);
        }*/
    }
	
    void Update()
    {
        UpdateOnToken();

        if (IsRotating)
        {
            UpdateRotation();
        }

        if (Input.GetKeyDown(KeyCode.R))
        { 
            if(interfaces.State != CQ_Interface.TutorialPage)
                ResetCharacter();
        }

        controller.SimpleMove(transform.TransformDirection(velocity));
    }

    /*void OnGUI()
    {
        GUI.Box(new Rect(10, 100, 200, 100), "TEST");
        GUI.Label(new Rect(20, 120, 150, 20), "TK: " + tokenName);
        GUI.Label(new Rect(20, 140, 150, 20), "LG: " + longitudinal.ToString());
        GUI.Label(new Rect(20, 160, 150, 20), "TR: " + trasversal.ToString());
    }*/
    #endregion

    #region Protected
    protected void BlendShaders(Transform tr)
    {
        if (null != tr.GetComponent<Renderer>())
        {
            foreach (Material mat in tr.GetComponent<Renderer>().materials)
            {
                materialList.Add(mat);
                mat.color = new Color(1.0f, 1.0f, 1.0f, alpha);
            }
        }

        for (int i = 0; i < tr.childCount; i++)
            BlendShaders(tr.GetChild(i));
    }

    protected void ChangeColor(bool red)
    {
        Color color = new Color(1.0f, 1.0f, 1.0f, alpha);
        if (red)
            color = new Color(1.0f, 0.0f, 0.0f, alpha);

        foreach (Material mat in materialList)
            mat.color = color;
    }

    protected Vector3 startDirection = Vector3.forward;
    protected void StartRotation(float angle)
    {
       // Debug.Log("StartRotation");
        rotationTimer = TimeManager.Instance.MasterSource.TotalTime;
        angleTarget = angle;
		rotationDuration = 0.0f;
        if (speed*speedFactor > 0.0f)
			rotationDuration = Random.Range(6.0f, 8.0f) / (speed*speedFactor);
        else
            Debug.LogWarning("speed: " + speed + ", rotation: " + rotationDuration);

        startDirection = transform.TransformDirection(Vector3.forward);

    }

    protected void UpdateRotation()
    {
        float currentTime = TimeManager.Instance.MasterSource.TotalTime;
        float elapsedTime = currentTime - rotationTimer;

		float timeFactor = 1.0f;
		if (rotationDuration > 0.0f)
        	timeFactor = elapsedTime / rotationDuration;
        
        if (timeFactor >= 1.0f)
        {
            timeFactor = 1.0f;
            StopRotation();
        }

        float angleFactor = angleTarget * timeFactor;

        Vector3 targetVector = Quaternion.AngleAxis(angleFactor, Vector3.up) * startDirection;
        transform.rotation = Quaternion.LookRotation(targetVector, Vector3.up);
       // CameraController.SetOrientationOffset(transform.rotation);
     //   CameraController.SetYRotation(transform.rotation.y);


    }

    protected void StopRotation()
    {
        rotationTimer = -1.0f;
        if (!isWalkingSpeed2)
        {
            verticalSpeed = 1.0f;
            speed = 1.10f;
#if UNITY_EDITOR    //CHEAT DENIS
            speed = 1.10f;
#endif
        }
        else
        {
            verticalSpeed = 1.0f;
            speed = 1.6f;
#if UNITY_EDITOR //cheat Denis
            speed = 1.6f;
#endif
        }
    }

    protected void ComputeInput(out float horizontal, out float vertical, bool forceKeyDown)
    {
        horizontal = 0.0f;
        vertical = 0.0f;

        if (!inputEnabled)
            return;

        bool inputUp = bbManager.GetKeyDown(KeyCode.UpArrow) && directionsEnabled[0] && !TimeManager.Instance.MasterSource.IsPaused;
        bool inputDown = bbManager.GetKeyDown(KeyCode.DownArrow) && directionsEnabled[3] && !TimeManager.Instance.MasterSource.IsPaused;
        bool inputRight = bbManager.GetKey(KeyCode.RightArrow) && directionsEnabled[1] && !TimeManager.Instance.MasterSource.IsPaused;
        bool inputLeft = bbManager.GetKey(KeyCode.LeftArrow) && directionsEnabled[2] && !TimeManager.Instance.MasterSource.IsPaused;
        if (forceKeyDown)
        {
            inputRight = bbManager.GetKeyDown(KeyCode.RightArrow) && directionsEnabled[1];
            inputLeft = bbManager.GetKeyDown(KeyCode.LeftArrow) && directionsEnabled[2];
        }
        bool inputDownContinuous = bbManager.GetKey(KeyCode.DownArrow) && directionsEnabled[3] && (fsm.State == Walk || fsm.State == WalkFast);

		if (inputUp || Input.GetAxis("Vertical")> 0.75f)
            vertical = 1.0f;
		if (inputDown|| Input.GetAxis("Vertical")< -0.75f)
        {
            if(interfaces.State != CQ_Interface.TutorialPage)
                inputDownTimer = TimeManager.Instance.MasterSource.TotalTime;
            vertical = -1.0f;
        }
		if (inputRight || Input.GetAxis("Horizontal")> 0.75f)
            horizontal = 1.0f;
		if (inputLeft || Input.GetAxis("Horizontal")< -0.75f)
            horizontal = -1.0f;

        if (inputDownTimer > 0.0f)
        {
            if (TimeManager.Instance.MasterSource.TotalTime - inputDownTimer > 1.0f)
            {
                inputDownTimer = -1.0f;
                if (inputDownContinuous)
                    vertical = -1.0f;
            }
        }

        //tracking
        string target = "";
        if (null != gameplay.CurrentTarget)
            if (gameplay.Gameplay == GameplayManager.GameplayType.Navigation)
                target = gameplay.CurrentTarget.type.ToString();

        if (fsm.State == Walk || fsm.State == WalkFast)
        {
            if (lastHorizontalInput != horizontal)
            {
                if (horizontal == 0.0f)
                {
                    TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "stop lean", lastLeanDirection);
                    lastLeanDirection = string.Empty;
                }
                else if (horizontal < 0.0f)
                {
                    if (lastLeanDirection.Length > 0)
                        TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "stop lean", lastLeanDirection);

                    lastLeanDirection = "left";
                    TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "start lean", lastLeanDirection);
                }
                else if (horizontal > 0.0f)
                {
                    if (lastLeanDirection.Length > 0)
                        TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "stop lean", lastLeanDirection);

                    lastLeanDirection = "right";
                    TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "start lean", lastLeanDirection);
                }

                lastHorizontalInput = horizontal;
            }
        }
    }

    protected void UpdateVelocityByInput(float horizontal, float vertical)
    {
        Vector3 localVelocity = new Vector3(horizontal * 0.6f * strafeFactor, 0.0f, vertical);
        velocity = Vector3.Lerp(velocity, localVelocity * speed * speedFactor, 0.19f);
    }

    protected void UpdateRotationByInput(float horizontal)
    {
        float lSpeed = 2.2f;
        if (horizontal == 0.0f)
			lSpeed *= rotationSpeed;

        actualHorizontal = Mathf.Lerp(actualHorizontal, horizontal, TimeManager.Instance.MasterSource.DeltaTime * lSpeed);
        
        if (Mathf.Abs(horizontal - actualHorizontal) < 0.02f)
            actualHorizontal = horizontal;

        float rightValue = Mathf.Clamp(actualHorizontal, -0.8f, 0.8f);
        characterRoot.rotation = Quaternion.LookRotation(transform.forward + transform.right * rightValue, Vector3.up);
    }

    protected void UpdateOnToken()
    {
        if (!placed)
            return;

        bool isOnToken = TokensManager.Instance.GetToken(transform.position, out currentTokenHit);
        if (null == prevTokenHit)
            prevTokenHit = currentTokenHit;

        tokenName = "OUT!!";
        if (isOnToken)
        {
            tokenName = currentTokenHit.token.name;
            longitudinal = currentTokenHit.longitudinal;
            trasversal = currentTokenHit.trasversal;

            if (prevTokenHit.token != currentTokenHit.token)
            {
                gameManagers.SendMessage("CurrentToken", currentTokenHit.token);
                if (currentTokenHit.token.type == Token.TokenType.Cross)
                {
                    int prevIndex = 0;
                    for (int i = 0; i < currentTokenHit.token.links.Length; i++)
                    {
                        if (prevTokenHit.token == currentTokenHit.token.links[i])
                        {
                            prevIndex = i;
                            break;
                        }
                    }

                    prevIndex = (prevIndex == 0) ? 4 : prevIndex;

                    forwardIndex = (prevIndex + 2) % 4;
                    rightIndex = (prevIndex - 1) % 4;
                    leftIndex = (prevIndex + 1) % 4;

                    wallIndexes.Add(prevIndex);

                    availableDirections[0] = currentTokenHit.token.links[forwardIndex] != null;
                    if (availableDirections[0])
                        wallIndexes.Add(forwardIndex);

                    availableDirections[1] = currentTokenHit.token.links[rightIndex] != null;
                    if (availableDirections[1])
                        wallIndexes.Add(rightIndex);

                    availableDirections[2] = currentTokenHit.token.links[leftIndex] != null;
                    if (availableDirections[2])
                        wallIndexes.Add(leftIndex);

                    fsm.State = Choose;
                }
                else
                {
                    wallIndexes.Clear();
                    envManager.DisableWallsOnToken();
                    Debug.Log("Disable walls");
                }

                prevTokenHit = currentTokenHit;
            }
        }    
    }

    protected float ComputeLongDist(Vector3 position)
    {
        float distX = Mathf.Abs(lastPosition.x - position.x);
        float distZ = Mathf.Abs(lastPosition.z - position.z);
        return ((distX > distZ) ? distX : distZ);
    }
    #endregion

    #region Messages
    void OnPause()
    {
        LevelRoot.Instance.BroadcastMessage("SetInputEnabled", false);
    }

    void OnResume()
    {
        LevelRoot.Instance.BroadcastMessage("SetInputEnabled", true); 
    }

    void LevelFinished()
    {
        verticalSpeed = 0.0f;
        fsm.State = Stay;
    }

    void ObstacleTriggerEnter(string obstacleName)
    {
        if (!inputEnabled)
            return;

     //   Debug.Log("@PlayerKinematicBehaviour: ObstacleTriggerEnter: " + obstacleName);
        isRed = true;
        ChangeColor(isRed);
        gameManagers.GetComponent<GameplayManager>().SendMessage("CharacterCollided");
        redTimer = TimeManager.Instance.MasterSource.TotalTime;

        string target = "";
        if(gameplay.CurrentTarget != null)
           target =  (gameplay.Gameplay == GameplayManager.GameplayType.Navigation) ? gameplay.CurrentTarget.type.ToString() : "";

        if (obstacleName.Contains("Pedestrian"))
        {
            gameplay.PedestrianHitCounterForTarget++;
            gameplay.PedestrianHitCounter++;
            gameplay.TotalPedestrianHitCounter++;
            TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, target, transform.position, "hit_ped", obstacleName);
        }
        else
        {
            gameplay.ObstacleHitCounterForTarget++;
            gameplay.ObstacleHitCounter++;
            gameplay.TotalObstacleHitCounter++;
            TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, target, transform.position, "hit_obs", obstacleName);
        }

    }

    void ObstacleTriggerExit()
    {
        //if (!inputEnabled)
        //    return;

    //    Debug.Log("@PlayerKinematicBehaviour: ObstacleTriggerExit");
        isRed = false;
        ChangeColor(isRed);
        redTimer = -1.0f;
    }

    void SetInputEnabled(bool flag)
    {
      //  Debug.Log("@PlayerKinematicBehaviour: SetInputEnabled " + flag);
        inputEnabled = flag;
    }

    void ResetConfiguration()
    {
        placed = false;
        transform.position = new Vector3(0.0f, -4.0f, 0.0f);
        directionsEnabled = new bool[] { true, true, true, true };
        verticalSpeed = 0.0f;
        fsm.State = Stay;
    }
    #endregion

    #region FSM States
    void OnStayEnter()
    {
        verticalSpeed = 0.0f;

        if (gameplay != null)
            gameplay.ActualDistance += ComputeLongDist(transform.position);

        lastPosition = transform.position;

        //hack
        float time = TimeManager.Instance.MasterSource.TotalTime;
        turnBackTimer = time - 2.5f;
    }

    void OnStayExec()
    {
        if (!placed)
            return;

        float time = TimeManager.Instance.MasterSource.TotalTime;

        float horizontal, vertical;
        ComputeInput(out horizontal, out vertical, false);

        UpdateVelocityByInput(0.0f, verticalSpeed);
        UpdateRotationByInput(0.0f);

        string target = "";
        if (gameplay.Gameplay == GameplayManager.GameplayType.Navigation)
        {
            if (null != gameplay.CurrentTarget)
                target = gameplay.CurrentTarget.type.ToString();
        }
        

        if (vertical > 0.0f)
        {
            if (isWalkingSpeed2)
            {
                fsm.State = WalkFast;
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "forward", "speed_2");
            }
            else
            {
                fsm.State = Walk;
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "forward", "speed_1");
            }
        }
        else if (vertical < 0.0f && TurnBackEnabled && (time - turnBackTimer > 3.0f)) // && canTurnBackTimer < 0)
        {
            turnBackTimer = time;
            fsm.State = TurnBack;
            TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "backward", "180");
            //lastTurnState = Stay;
        }

        if (redTimer > 0)
        {
            if (TimeManager.Instance.MasterSource.TotalTime - redTimer > 3.0f)
            {
                redTimer = -1.0f;
                ChangeColor(false);
            }
        }
    }

    void OnStayExit()
    {
    }

    void OnWalkEnter()
    {
        if (rotationTimer < 0)
        {
            turnCounter = 0;
            verticalSpeed = 1.0f;
            speed = 1.10f;
#if UNITY_EDITOR    //CHEAT DENIS
            speed = 1.10f;
#endif
            isWalkingSpeed2 = false;
            lastTurnState = Stay;
        }
        else
        {
            turnCounter = 0;
            verticalSpeed = 1f;
            speed = 0.7f;
            isWalkingSpeed2 = false;
            lastTurnState = Stay;
        
        }



    }

    void OnWalkExec()
    {
        float horizontal, vertical;
        ComputeInput(out horizontal, out vertical, false);

        if (IsRotating || currentTokenHit.token.type == Token.TokenType.Cross)
            vertical = 0.0f;

        if (horizontal != lastHorizontal)
            cameraObj.SendMessage("SetLookRight", horizontal, SendMessageOptions.DontRequireReceiver);
         
        lastHorizontal = horizontal;
        UpdateVelocityByInput(horizontal, verticalSpeed);
        if (!preloader.useOVR)
            UpdateRotationByInput(horizontal);

        string target = "";
        if (null != gameplay.CurrentTarget)
            target = (gameplay.Gameplay == GameplayManager.GameplayType.Navigation) ? gameplay.CurrentTarget.type.ToString() : "";

        if (vertical > 0.0f)
        {
            fsm.State = WalkFast;
            TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "forward", "speed_2");
        }
        else if (vertical < 0.0f)
        {
            fsm.State = Stay;
            TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "backward", "stop");
        }
    }
    void OnWalkExit()
    {
    }

    void OnWalkFastEnter()
    {
        if (rotationTimer < 0)
        {
            verticalSpeed = 1.0f;
            speed = 1.6f;
            isWalkingSpeed2 = true;
#if UNITY_EDITOR //cheat Denis
            speed = 1.6f;
#endif
        }
        else 
        {
            verticalSpeed = 1f;
            speed = 1f;
            isWalkingSpeed2 = true;
        }

    }
    void OnWalkFastExec()
    {
        float horizontal, vertical;
        ComputeInput(out horizontal, out vertical, false);

        if (IsRotating || currentTokenHit.token.type == Token.TokenType.Cross)
            vertical = 0.0f;


        if (horizontal != lastHorizontal)
            cameraObj.SendMessage("SetLookRight", horizontal, SendMessageOptions.DontRequireReceiver);

        lastHorizontal = horizontal;
        UpdateVelocityByInput(horizontal, verticalSpeed);
        if (!preloader.useOVR)
            UpdateRotationByInput(horizontal);

        if (vertical < 0.0f)
        {
            fsm.State = Walk;
            string target = "";
            if (null != gameplay.CurrentTarget)
                target = (gameplay.Gameplay == GameplayManager.GameplayType.Navigation) ? gameplay.CurrentTarget.type.ToString() : "";
            TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "backward", "speed_1");
        }
    }
    void OnWalkFastExit()
    {
    }

    bool enableWalls = true;
    void OnChooseEnter()
    {
        //gameObject.GetComponentInChildren<OVRGui>().
        verticalSpeed = 0.0f;
        UpdateVelocityByInput(0.0f, verticalSpeed);
        interfaces.ShowDirectionArrows(availableDirections[0], availableDirections[1], availableDirections[2]);

        gameplay.ActualDistance += ComputeLongDist(transform.position);
        lastPosition = transform.position;
        enableWalls = true;

        //Debug.Log("@@@@@@@@@@@@@@@@@@@" + gameplay);
        //Debug.Log(gameplay.CurrentTarget);
        //Debug.Log(gameplay.CurrentTarget.type);
         
        string target = "";
        if (gameplay.CurrentTarget != null)
            target = (gameplay.Gameplay == GameplayManager.GameplayType.Navigation) ? gameplay.CurrentTarget.type.ToString() : "";
        
        TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "stop", "intersection");

        //hack
        float time = TimeManager.Instance.MasterSource.TotalTime;
        turnBackTimer = time - 2.5f;
    }
    void OnChooseExec()
    {
        float time = TimeManager.Instance.MasterSource.TotalTime;

        float horizontal, vertical;
        ComputeInput(out horizontal, out vertical, true);

        UpdateVelocityByInput(0.0f, verticalSpeed);
        UpdateRotationByInput(0.0f);

        string target = "";
        if (null != gameplay.CurrentTarget)
            target = (gameplay.Gameplay == GameplayManager.GameplayType.Navigation) ? gameplay.CurrentTarget.type.ToString() : "";

        if (vertical > 0 && availableDirections[0])
        {
            wallIndexes.Remove(forwardIndex);
            if (isWalkingSpeed2)
                fsm.State = WalkFast;
            else
                fsm.State = Walk;

            TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "turn", "straight");
            gameplay.SendMessage("EnterOnCross");
            Debug.Log("UP");
            return;
        }
        if (horizontal > 0 && availableDirections[1])
        {
            StartRotation(90.0f);
            wallIndexes.Remove(rightIndex);
            if (isWalkingSpeed2)
                fsm.State = WalkFast;
            else
                fsm.State = Walk;

            TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "turn", "right");
            gameplay.SendMessage("EnterOnCross");
            Debug.Log("RIGHT");
            return;
        }
        if (horizontal < 0 && availableDirections[2])
        {
            StartRotation(-90.0f);
            wallIndexes.Remove(leftIndex);
            if (isWalkingSpeed2)
                fsm.State = WalkFast;
            else
                fsm.State = Walk;

            TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "turn", "left");
            gameplay.SendMessage("EnterOnCross");
            Debug.Log("LEFT");
            return;
        }
        
        if (vertical < 0.0f && TurnBackEnabled && (time - turnBackTimer > 3.0f))
        {
            turnBackTimer = time;
            enableWalls = false;
            fsm.State = TurnBack;
            lastTurnState = Choose;
            TrackingManager.Instance.LogEntry(TrackingManager.Command.Action, target, transform.position, "backward", "180");
            Debug.Log("DOWN");
        }
    }
    void OnChooseExit()
    {
      //  Debug.Log("OnChooseExit ");
        interfaces.HideDirectionArrows();
        if (enableWalls)
        {
            envManager.EnableWallOnToken(currentTokenHit.token, wallIndexes);
            Debug.Log("Enabled walls: " + wallIndexes);
        }
    }

    void OnTurnBackEnter()
    {
        Debug.Log("turnCounter " + turnCounter);
        turnCounter++;
        StartRotation(180.0f);
    }

    void OnTurnBackExec()
    {
        if (!IsRotating)
        {
            if (turnCounter % 2 == 0)
            {
                fsm.State = lastTurnState;
                Debug.Log("first " + lastTurnState + " turnCounter " + turnCounter);
            }
            else {
                Debug.Log("second " + lastTurnState);
                fsm.State = Stay;
            }
               
        }
    }

    void OnTurnBackExit()
    {
        //canTurnBackTimer = TimeManager.Instance.MasterSource.TotalTime;
    }
    #endregion

    #region Public Members
	public void Init(float speedFactor, float strafeFactor, float rotationSpeedFactor, float alpha)
    {
        this.alpha = alpha;
        BlendShaders(transform);

		this.speedFactor = speedFactor;
		this.rotationSpeed = rotationSpeedFactor;
        this.strafeFactor = strafeFactor;

        AnimationBehaviour anim = GetComponent<AnimationBehaviour>();
        if (null != anim)
            anim.Init(speedFactor);
    }

    public void PlaceCharacter(int tutPhase, int gate)
    {
        if (preloader.useOVR)
            gate = 0;

        envManager = gameManagers.GetComponent<EnvironmentManager>();
        SBSVector3 pos, tang;
        envManager.GetAvailableStartPosition(tutPhase, gate, out pos, out tang);
        transform.position = pos + (SBSVector3.up * 0.01f);
        transform.LookAt(pos + tang * 10.0f);
        placed = true;

        cameraObj.SendMessage("SetupCamera", gameObject, SendMessageOptions.DontRequireReceiver);
      /*  if (CameraController != null)
        {
            CameraController.SetOrientationOffset(transform.rotation);
            CameraController.SetYRotation(0.0f);
        }*/

        lastPosition = transform.position;
    }

    public void ResetCharacter()
    {
        if (CurrentTokenHit.token.type != Token.TokenType.Cross)
        {
            SBSVector3 pos, tang;
            CurrentTokenHit.token.TokenToWorld(0.1f, 0.0f, out pos, out tang);
            transform.position = pos + (SBSVector3.up * 0.01f);
            transform.LookAt(pos + tang * 10.0f);
            placed = true;
            lastPosition = transform.position;
            fsm.State = Stay;
        }
    }

    public void SetAvailableDirections(bool forward, bool right, bool left)
    {
        availableDirections[0] = forward;
        availableDirections[1] = right;
        availableDirections[2] = left;
    }

    public void SetDirectionsEnabled(bool forward, bool right, bool left, bool back)
    {
        directionsEnabled[0] = forward;
        directionsEnabled[1] = right;
        directionsEnabled[2] = left;
        directionsEnabled[3] = back;
    }

    bool[] lastDirections = new bool[4];
    public void DisableAllDirections()
    {
        lastDirections = directionsEnabled;
        SetDirectionsEnabled(false, false, false, false);
    }

    public void EnableLastDirectionsConfiguration()
    {
        SetDirectionsEnabled(lastDirections[0], lastDirections[1], lastDirections[2], lastDirections[3]);
    }
    #endregion
}
