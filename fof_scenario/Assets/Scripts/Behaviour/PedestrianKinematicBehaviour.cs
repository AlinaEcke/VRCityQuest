using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FoF/PedestrianKinematicBehaviour")]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavigationAgentComponent))]
[RequireComponent(typeof(SteeringAgentComponent))]
public class PedestrianKinematicBehaviour : MonoBehaviour
{
    #region Internal Data Structure
    public const int Stay = 0;
    public const int Walk = 1;
    #endregion

    #region Protected Fields
    public float speed;
    public float vSpeed;
    public float hSpeed;
    public Vector3 localVelocity;
    public Animator animator;
    #endregion

    #region Protected Fields
    protected float lookForwardDistance = 5.0f;
    protected Vector3 velocity = Vector3.zero;
    protected Vector3 targetPosition = Vector3.zero;
    protected CharacterController controller;
    protected TimeManager timeManager = null;
    protected NavigationAgentComponent navigation = null;
    protected SteeringAgentComponent steering = null;
    protected Token currentToken = null;
    protected Token prevToken = null;
    protected List<Token> nextCrosses = new List<Token>();
    protected Token lastCross = null;
    protected Token lastTarget = null;
    protected GameObject player = null;
    protected bool enable = true;
    protected FiniteStateMachine fsm;
    protected bool towardsPlayer = true;
    protected float hSpeedMult = 40f;
    #endregion

    #region Get / Set

    public float Speed
    {
        get { return speed; }
    }

    #endregion

    #region Unity Callbacks
    void Start()
    {
        controller = GetComponent<CharacterController>();
        GameObject coreManagers = GameObject.FindGameObjectWithTag("CoreManagers");
        timeManager = coreManagers.GetComponent<TimeManager>();
        steering = GetComponent<SteeringAgentComponent>();
        navigation = GetComponent<NavigationAgentComponent>();
        fsm = GetComponent<FiniteStateMachine>();
        targetPosition = transform.position;

        GameplayManager gameplay = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GameplayManager>();
        player = gameplay.Character;

        enable = true;

        towardsPlayer = true;
        if (Random.Range(0, 100000) % 100 > 70)
            towardsPlayer = false;

        InitTarget();
        //InitTargetOnToken();
    }
	
    void Update()
    {
        if (!enable)
            return;

        //UpdateTargetOnToken();
        UpdateTarget();
        UpdateGfx();
        velocity = steering.m_wantedVelocity;

        controller.SimpleMove(velocity);
        speed = controller.velocity.magnitude;

        localVelocity = transform.InverseTransformDirection(controller.velocity);
        hSpeed = localVelocity.x;
        vSpeed = localVelocity.z;
        if (null != animator)
        {
            if (fsm.State == Stay)
            {
                vSpeed = 0f;
            }
            else
            {
                vSpeed = Mathf.Clamp(vSpeed, 0.33f, 1.0f);
                animator.SetFloat("VSpeed", vSpeed / steering.m_maxSpeed);
                //animator.SetFloat("HSpeed", hSpeed * hSpeedMult);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //if (collider == player.collider)
        //    Debug.LogWarning("OnTriggerEnter");
    }
    #endregion

    #region New Bahaviour
    protected const float TargetDistance = 5.0f;
    protected const float TargetRespawnDistance = 3.0f;
    protected Vector3 targetDirection = Vector3.zero;

    protected void InitTarget()
    {
        TokensManager.TokenHit hit;
        bool hitFlag = TokensManager.Instance.GetToken(transform.position, out hit);
        if (hitFlag)
        {
            currentToken = hit.token;
            lastCross = currentToken;
            prevToken = currentToken;

            Vector3 localPedPos = currentToken.transform.InverseTransformPoint(transform.position);

            int[] idOrder = { 0, 1, 2, 3 };
            float[] distId = { 0.0f, 0.0f, 0.0f, 0.0f };

            float lenght = currentToken.lengthOrRadius;
            float width = currentToken.width;
            if (currentToken.transform.rotation.y > 0)
            {
                lenght = currentToken.width;
                width = currentToken.lengthOrRadius;
            }

            distId[0] = (lenght * 0.5f) - localPedPos.z;
            distId[1] = (width * 0.5f) - localPedPos.x;
            distId[2] = (lenght * 0.5f) + localPedPos.z;
            distId[3] = (width * 0.5f) + localPedPos.x;

            for (int i = 0; i < distId.Length; i++)
            {
                for (int j = i + 1; j < distId.Length; j++)
                {
                    if (distId[i] > distId[j])
                    {
                        float tmp = distId[j];
                        distId[j] = distId[i];
                        distId[i] = tmp;

                        int tmp2 = idOrder[j];
                        idOrder[j] = idOrder[i];
                        idOrder[i] = tmp2;
                    }
                }
            }

            int id = 0;
            for (int i = 0; i < distId.Length; i++)
            {
                if (null != currentToken.links[idOrder[i]])
                {
                    id = idOrder[i];
                    break;
                }
            }

            lastTarget = currentToken.links[id];
            targetDirection = lastTarget.transform.forward;
            if (id > 1)
                targetDirection = -lastTarget.transform.forward;

            targetPosition = transform.position + (targetDirection * TargetDistance);
            navigation.MoveToPosition(targetPosition, 1.0f);
        }
    }

    protected void UpdateTarget()
    {
        if ((transform.position - targetPosition).magnitude < TargetRespawnDistance)
        {
            targetPosition = targetPosition + (targetDirection * TargetDistance);

            TokensManager.TokenHit hit;
            bool hitFlag = TokensManager.Instance.GetToken(targetPosition, out hit);
            if (hitFlag)
            {
                currentToken = hit.token;
                if (currentToken.type == Token.TokenType.Cross)
                {
                    int linkIndex = 0;
                    float manhattan = float.MaxValue;
                    if (towardsPlayer)
                        manhattan = float.MinValue;
                    for (int i = 0; i < currentToken.links.Length; i++)
                    {
                        Token tk = currentToken.links[i];
                        if (null != tk && tk != lastTarget)
                        {
                            Vector3 targetPos = tk.transform.position;
                            Vector3 playerPos = player.transform.position;
                            float localManhattan = Mathf.Abs(targetPos.x - playerPos.x) + Mathf.Abs(targetPos.z - playerPos.z);
                            bool distCheck = localManhattan < manhattan;
                            if (towardsPlayer)
                                distCheck = localManhattan > manhattan;
                            if (distCheck)
                            {
                                manhattan = localManhattan;
                                linkIndex = i;
                                lastTarget = tk;
                            }
                        }
                    }

                    Token next = currentToken.links[linkIndex];                        
                    targetDirection = next.transform.forward;
                    float trasversal = Random.Range(-0.8f, 0.8f);
                    float longitudinal = 0.1f;
                    if (linkIndex > 1)
                    {
                        targetDirection = -next.transform.forward;
                        longitudinal = 0.9f;
                    }

                    SBS.Math.SBSVector3 pos;
                    SBS.Math.SBSVector3 tang;
                    next.TokenToWorld(longitudinal, trasversal, out pos, out tang);

                    targetPosition = pos;
                }
            }

            navigation.MoveToPosition(targetPosition, 1.0f);
        }
    }
    #endregion

    #region Protected
    protected void FillNextCrosses(Token current)
    {        
        foreach (Token link in current.links)
        {
            if (null != link)
            {
                if (link.type == Token.TokenType.Cross)
                {
                    if (link != lastCross && link != lastTarget)
                    {
                        nextCrosses.Add(link);
                    }
                }
                else
                {
                    FillNextCrosses(link);
                }
            }
        }
    }

    protected void InitTargetOnToken()
    {
        List<Token> availableCrosses = new List<Token>();
        TokensManager.TokenHit hit;

        bool hitFlag = TokensManager.Instance.GetToken(transform.position, out hit);
        if (hitFlag)
        {
            currentToken = hit.token;
            lastCross = currentToken;
            prevToken = currentToken;
            nextCrosses.Clear();
            FillNextCrosses(currentToken);

            Vector3 localPedPos = currentToken.transform.InverseTransformDirection(transform.position);
            localPedPos.Scale(new Vector3(currentToken.width, 1.0f, currentToken.lengthOrRadius));

            int id = 0;
            if (localPedPos.x > localPedPos.z)
            {
                if (-localPedPos.x > localPedPos.z)
                    id = 2;
                else
                    id = 1;
            }
            else
            {
                if (-localPedPos.x > localPedPos.z)
                    id = 3;
                else
                    id = 0;
            }

            if (currentToken.links[id] == null)
            {
                int newId = id + 1;
                if (newId > 3)
                    newId = 0;

                if (currentToken.links[newId] == null)
                {
                    id--;
                    if (id < 0)
                        id = 3;
                }
                else
                {
                    id++;
                    if (id > 3)
                        id = 0;
                }
            }

            lastTarget = currentToken.links[id];
            targetPosition = lastTarget.transform.position;
            navigation.MoveToPosition(targetPosition, 1.0f);
        }
    }

    protected float stuckTimer = -1.0f;
    protected bool changeTarget = false;
    protected void UpdateTargetOnToken()
    {        
        TokensManager.TokenHit hit;
        Token tmpToken = null;

        bool hitFlag = TokensManager.Instance.GetToken(transform.position, out hit);
        if (hitFlag)
        {
            tmpToken = hit.token;

            if (tmpToken != currentToken)
            {
                if (tmpToken.type == Token.TokenType.Rect)
                {
                    changeTarget = true;
                }
                else
                {
                    lastCross = tmpToken;
                }
            }
            else
            {
                if (tmpToken.type == Token.TokenType.Rect)
                {
                    if (Mathf.Abs(hit.longitudinal - 0.5f) < 0.05f && changeTarget)
                    {
                        ChangeTarget();
                    }
                }
                else if (tmpToken == lastTarget)
                {
                    ChangeTarget();
                }
            }

            currentToken = tmpToken;
        }

        if (velocity.magnitude < 0.5f)
        {
            if (stuckTimer < 0)
                stuckTimer = TimeManager.Instance.MasterSource.TotalTime;
        }
        else
        {
            stuckTimer = TimeManager.Instance.MasterSource.TotalTime;
        }

        if (TimeManager.Instance.MasterSource.TotalTime - stuckTimer > 1.0f)
            ChangeTarget();
    }

    protected void ChangeTarget()
    {
        changeTarget = false;

        nextCrosses.Clear();
        FillNextCrosses(lastTarget);

        float manhattan = float.MaxValue;
        int closest = 0;
        for (int i = 0; i < nextCrosses.Count; i++)
        {
            Vector3 targetPos = nextCrosses[i].transform.position;
            Vector3 playerPos = player.transform.position;
            float localManhattan = Mathf.Abs(targetPos.x - playerPos.x) + Mathf.Abs(targetPos.z - playerPos.z);
            if (localManhattan < manhattan)
            {
                closest = i;
                manhattan = localManhattan;
            }
        }

        if (Random.Range(0, 100) > 50)
        {
            closest++;
            if (closest == nextCrosses.Count)
                closest = 0;
        }

        lastTarget = nextCrosses[closest];
        targetPosition = lastTarget.transform.position;
        navigation.MoveToPosition(targetPosition, 1.0f);
    }

    protected void MoveTargetOnToken()
    {
        bool tokenChanged = false;
        TokensManager.TokenHit hit;
        Token tmpToken = null;

        bool hitFlag = TokensManager.Instance.GetToken(transform.position, out hit);
        if (hitFlag)
        {
            tmpToken = hit.token;

            if (tmpToken != currentToken)
            {
                prevToken = currentToken;
                tokenChanged = true;
            }
            currentToken = tmpToken;

            if (tokenChanged)
            {
                StartCoroutine(ChooseNewTarget());
            }
        }
    }

    IEnumerator ChooseNewTarget()
    {
        yield return new WaitForSeconds(1.0f);
        List<Token> nextList = new List<Token>();
        nextList.AddRange(currentToken.links);
        nextList.RemoveAll(item => item == null);

        if (null != prevToken)
            nextList.Remove(prevToken);

        int randomIdx = 0;
        if (nextList.Count > 1)
            randomIdx = Random.Range(0, 10000) % nextList.Count;

        Token nextTargetToken = nextList[randomIdx];
        targetPosition = nextTargetToken.transform.position;
        float manhattan = Mathf.Abs(targetPosition.x - transform.position.x) + Mathf.Abs(targetPosition.z - transform.position.z);
        navigation.MoveToPosition(targetPosition, 1.0f);
    }

    protected void UpdateGfx()
    {
        transform.LookAt(transform.position + velocity * 10.0f);
    }
    #endregion

    #region Messages
    void ObstacleTriggerEnter(string obstacleName)
    {
        //Debug.Log("@PedestrianKinematicBehaviour: ObstacleTriggerEnter");
    }

    void ObstacleTriggerExit()
    {
        //ping
        //Debug.Log("@PedestrianKinematicBehaviour: ObstacleTriggerExit");
    }

    void ResetConfiguration()
    {
        enable = false;
    }
    #endregion

    private void OnPathRequestFailed()
    {
        StartCoroutine(ChooseNewTarget());
        SendMessageUpwards("OnNavigationRequestFailed", SendMessageOptions.DontRequireReceiver);
    }

    private void OnSteeringRequestSucceeded()
    {
        SendMessageUpwards("OnNavigationRequestSucceeded", SendMessageOptions.DontRequireReceiver);
        controller.SimpleMove(transform.TransformDirection(velocity));
    }

    #region FSMStates
    #region Stay
    protected float stayTimer = -1.0f;
    void OnStayEnter()
    {
        //Debug.Log("PEDESTRIAN KINEMATIC STAY ENTER: " + gameObject.name);
        //velocity = 0.0f;
        stayTimer = TimeManager.Instance.MasterSource.TotalTime;
    }

    void OnStayExec()
    {
        if (TimeManager.Instance.MasterSource.TotalTime - stayTimer > 3.0f)
            ChangeTarget();

        if (speed > 0.05f)
            fsm.State = Walk;
    }

    void OnStayExit()
    { }
    #endregion

    #region Walk
    void OnWalkEnter()
    { }
    float speedCtrl = 5;
    void OnWalkExec()
    {
        if (speed <= 0.05f)
            fsm.State = Stay;
    }

    void OnWalkExit()
    { }
    #endregion
    #endregion

}
