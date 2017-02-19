using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Obstacle : MonoBehaviour
{
    #region Public Fields
    public bool soundEnabled = true;
    public bool isAnimated;
    public bool isActive;
    public bool hasChild;
    public int soundVolume;
    public float stopTrasversal;
    public Token currentToken;
    public string name;
    #endregion

    #region Protected Fields
    protected bool activated = false;
    protected float activationDistance = 6.0f;
    protected float alpha = 1.0f;
    protected float speed = 1.0f;
    protected List<Animation> obstacleAnimation = new List<Animation>();
    protected ObstaclesManager obstacleManager = null;
    protected GameplayManager gameplayManager = null;
    protected GeneralSoundsManager generalSounds = null;
    protected GameObject animChild = null;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GameplayManager>();
        generalSounds = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GeneralSoundsManager>();
        obstacleManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<ObstaclesManager>();

        if (hasChild)
        {
            for (int i = 0; i < transform.GetChildCount(); i++)
            {
                Transform obs = transform.GetChild(i);
                if (isAnimated)
                {
                    Transform root = obs.GetChild(0);
                    Transform child = root.GetChild(0);
                    ObstacleChild comp = child.gameObject.AddComponent<ObstacleChild>();
                    comp.name = name;
                    child.gameObject.AddComponent<FootprintComponent>();
                    if (isAnimated)
                        obstacleAnimation.Add(root.GetComponent<Animation>());
                }
                else
                {
                    ObstacleChild comp = obs.gameObject.AddComponent<ObstacleChild>();
                    comp.name = name;
                    obs.gameObject.AddComponent<FootprintComponent>();
                }
            }
        }
        else
        {
            if (isAnimated)
            {
                Transform root = transform.GetChild(0);
                obstacleAnimation.Add(root.GetComponent<Animation>());
                Transform animatedChild = root.GetChild(0);
                ObstacleChild comp = animatedChild.gameObject.AddComponent<ObstacleChild>();
                comp.name = name;
                animatedChild.gameObject.AddComponent<FootprintComponent>();
                animChild = animatedChild.gameObject;

                Transform childTr = animChild.transform;
                initialPos = new Vector3(childTr.position.x, childTr.position.y, childTr.position.z);
                initialRot = new Quaternion(childTr.rotation.x, childTr.rotation.y, childTr.rotation.z, childTr.rotation.w);
            }
            else
            {
                gameObject.AddComponent<FootprintComponent>();
            }
        }

        SetActivationDistance();
    }

    private bool playerHit = false;
    private bool isLastPassingPlayer = false;
    private bool isPassingPlayer = false;
    void Update()
    {
        string target = "";
        if (gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation)
        {
            if (null != gameplayManager.CurrentTarget)
                target = gameplayManager.CurrentTarget.type.ToString();
        }
        
        //chack passing player
        if (null != gameplayManager.Character)
        {
            Vector3 playerPos = gameplayManager.Character.transform.position;
            //if ((playerPos - transform.position).magnitude < 9.0f)
            //if (currentToken == gameplayManager.Character.GetComponent<PlayerKinematicBehaviour>().getto
            if (gameplayManager.Character.GetComponent<PlayerKinematicBehaviour>().CurrentTokenHit != null)
            {
                if (gameplayManager.Character.GetComponent<PlayerKinematicBehaviour>().CurrentTokenHit.token == currentToken)
                {
                    Vector3 localCharacterPosition = transform.InverseTransformPoint(playerPos);
                    isPassingPlayer = (Mathf.Abs(localCharacterPosition.x) < 1.0f);
                    if (isPassingPlayer)
                    {
                        if (isLastPassingPlayer != isPassingPlayer)
                            isLastPassingPlayer = isPassingPlayer;

                        if (!playerHit)
                            playerHit = gameplayManager.Character.GetComponent<PlayerKinematicBehaviour>().Collided;
                    }
                    else
                    {
                        if (isLastPassingPlayer != isPassingPlayer)
                        {
                            if (!playerHit)
                            {
                                gameplayManager.ObstaclePassedCounterForTarget++;
                                gameplayManager.ObstaclePassedCounter++;
                                gameplayManager.TotalObstaclePassedCounter++;

                                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, target, playerPos, "passed_obs", name);
                                gameplayManager.SendMessage("ObstacleAvoided", 1);

                                generalSounds.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.obst_Passed);
                            }
                            playerHit = false;
                            isLastPassingPlayer = isPassingPlayer;
                        }
                    }
                }
            }
        }

        if (isAnimated)
        {
            if (isActive)
            {
                if (null != gameplayManager.Character)
                {
                    Vector3 playerPos = gameplayManager.Character.transform.position;
                    Vector3 playerFw = gameplayManager.Character.transform.forward;
                    Vector3 localCharacterPosition = transform.InverseTransformPoint(playerPos);
                    if (Mathf.Abs(localCharacterPosition.x) < activationDistance) // && localCharacterPosition.z > -1 && localCharacterPosition.z < 10)
                    {
                        bool start = false;
                        if (gameplayManager.Level == 0)
                            start = (localCharacterPosition.z < 1 && localCharacterPosition.z > -10);
                        else
                            start = (localCharacterPosition.z > -1 && localCharacterPosition.z < 10);

                        if (start)
                        {
                            isActive = false;
                            //StartAnimation();
                            if (null != animChild)
                            {
                                StartMoveTo(playerPos + playerFw * Random.Range(2.0f, 4.0f));
                                if (null != TrackingManager.Instance)
                                    TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, target, playerPos, "move_obs", name);
                            }
                        }
                    }
                }
            }

            UpdateMoveTo();

            /*if (obstacleAnimation.Count > 0)
            {
                foreach (Animation anim in obstacleAnimation)
                {
                    if (anim.isPlaying)
                    {
                        float longitudinal, trasversal;
                        currentToken.WorldToToken(anim.transform.GetChild(0).position, out longitudinal, out trasversal);

                        if (trasversal <= stopTrasversal)
                            StopAnimation();
                    }
                }
            }*/
        }
    }
    #endregion

    #region Protected Members
    protected float movingTimer = -1.0f;
    protected float movingDuration = 0.0f;
    protected Vector3 initialPos = Vector3.zero;
    protected Quaternion initialRot = Quaternion.identity;
    protected Vector3 direction = Vector3.zero;
    protected float distance = 0.0f;
    protected float targetRotAngle = 0.0f;
    protected float damp = 0.0f;

    protected void StartMoveTo(Vector3 targetPos)
    {
        SendMessageUpwards("PlayObstacleSound", obstacleManager.GetSoundVolume(gameplayManager.Level) * 0.01f, SendMessageOptions.DontRequireReceiver);
        movingTimer = TimeManager.Instance.MasterSource.TotalTime;
        float mult = 1.0f - Mathf.Min(1.0f, ((initialPos - targetPos).magnitude * 0.1f)) * 0.5f;
        movingDuration = Random.Range(2.0f, 5.0f) * mult;
        Transform childTr = animChild.transform;
        initialPos = new Vector3(childTr.position.x, childTr.position.y, childTr.position.z);
        initialRot = new Quaternion(childTr.rotation.x, childTr.rotation.y, childTr.rotation.z, childTr.rotation.w);
        distance = Mathf.Min((targetPos - initialPos).magnitude, ObstaclesManager.ObstacleOccupation * 0.5f);
        direction = (targetPos - initialPos);
        direction.y = 0.0f;
        direction.Normalize();
        targetRotAngle = Random.Range(-90.0f, 90.0f);
        damp = 0.0f;

        if (gameplayManager.Level == 0)
        {
            distance = ObstaclesManager.ObstacleOccupation * 0.5f;
            direction = Vector3.right;
            movingDuration = 4.0f;
        }
    }

    protected void StopMove()
    {
        SendMessageUpwards("StopObstacleSound", SendMessageOptions.DontRequireReceiver);
        movingTimer = -1.0f;
    }

    protected void UpdateMoveTo()
    {
        if (movingTimer > 0.0f)
        {
            float currTime = TimeManager.Instance.MasterSource.TotalTime;
            float deltaTime = currTime - movingTimer;
            float timefactor = deltaTime / movingDuration;
            if (timefactor > 0.75f)
            {
                damp += 0.04f;
                if (damp > 0.5f)
                    damp = 0.5f;
                movingDuration += TimeManager.Instance.MasterSource.DeltaTime * damp;
            }

            if (timefactor >= 1.0f)
            {
                timefactor = 1.0f;
                StopMove();
            }
            Vector3 currentPosition = initialPos + direction * distance * timefactor;
            animChild.transform.position = currentPosition;
            float angleStep = targetRotAngle / movingDuration;
            if (animChild.gameObject.name.Contains("rolling"))
            {
                Vector3 right = Vector3.Cross(direction, Vector3.up);
                float rotDamp = Mathf.Max(5.0f - (damp * 2.5f), 0.0f);
                animChild.transform.RotateAroundLocal(animChild.transform.InverseTransformDirection(right), -rotDamp * TimeManager.Instance.MasterSource.DeltaTime / movingDuration);
            }
            else
            {
                animChild.transform.RotateAroundLocal(Vector3.up, angleStep * TimeManager.Instance.MasterSource.DeltaTime * 0.01f);
            }

            if (gameplayManager.Level == 0)
            {
                Vector3 playerPos = gameplayManager.Character.transform.position;
                Vector3 childPos = animChild.transform.position;
                Vector3 localCharacterPosition = transform.InverseTransformPoint(playerPos);
                Vector3 localChildPosition = transform.InverseTransformPoint(childPos);
                if (Mathf.Abs(localCharacterPosition.z - localChildPosition.z) < 0.5f)
                {
                    timefactor = 1.0f;
                    StopMove();
                }
            }
        }
    }

    protected void SetActivationDistance()
    {
        float rangeLimit = ObstaclesManager.ObstacleOccupation;
        activationDistance = Random.Range(rangeLimit * 0.5f, rangeLimit);
        if (gameplayManager.Level == 0)
            activationDistance = 6.5f;
    }

    protected void StartAnimation()
    {
        if (obstacleAnimation.Count > 0)
        {
            foreach (Animation anim in obstacleAnimation)
            {
                if (!anim.isPlaying)
                {
                    anim["move"].speed = speed;
                    anim.Play();
                }
            }
        }
    }

    protected void StopAnimation()
    {
        if (obstacleAnimation.Count > 0)
        {
            foreach (Animation anim in obstacleAnimation)
                anim.Stop();
        }
    }

    protected void Reset()
    {
        if (obstacleAnimation.Count > 0)
        {
            SetActivationDistance();
            animChild.transform.position = initialPos;
            animChild.transform.rotation = initialRot;
            /*foreach (Animation anim in obstacleAnimation)
            {
                anim.Play();
                anim["move"].time = 0.0f;
                anim["move"].enabled = true;
                anim.Sample();
                anim["move"].enabled = false;
            }*/
        }
    }

    protected void BlendShaders(Transform tr)
    {
        if (null != tr.GetComponent<Renderer>())
        {
            foreach (Material mat in tr.GetComponent<Renderer>().materials)
            {
                mat.color = new Color(1.0f, 1.0f, 1.0f, alpha);
            }
        }

        for (int i = 0; i < tr.childCount; i++)
            BlendShaders(tr.GetChild(i));
    }
    #endregion

    #region Messages
    void ForceActivate()
    {
        activated = true;
        isActive = true;
    }

    void Activate()
    {
        if (!activated)
        {
            activated = true;
            isActive = true;
        }
    }

    void Deactivate()
    {
        if (activated)
        {
            Reset();
            isActive = false;
            activated = false;
        }
    }

    void OnObstTriggerEnter(Collider other)
    {
        OnTriggerEnter(other);
    }

    void OnObstTriggerExit(Collider other)
    {
        OnTriggerExit(other);
    }

    void OnTriggerEnter(Collider other)
    {
        other.gameObject.SendMessage("ObstacleTriggerEnter", name, SendMessageOptions.DontRequireReceiver);
        if (/*soundEnabled && */other.name.Contains("Character"))
        {
            PlayerKinematicBehaviour pk = other.gameObject.GetComponent<PlayerKinematicBehaviour>();
            if (pk.InputEnabled)
                generalSounds.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.static_obstacle);
        }
    }

    void OnTriggerExit(Collider other)
    {
        other.gameObject.SendMessage("ObstacleTriggerExit", SendMessageOptions.DontRequireReceiver);
    }
    #endregion

    #region Public Members
    public void SetAlpha(float value)
    {
        alpha = value;
        BlendShaders(transform);
    }

    public void SetSpeed(float value)
    {
        speed = value;
    }
    #endregion
}
