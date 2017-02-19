using UnityEngine;
using System.Collections;
using SBS.Core;

public class TutorialManager : MonoBehaviour 
{
    #region Public members 
    
    public bool newSequence = false;
    #endregion

    #region public Enums

    public enum TutorialStates
    {
        Instructions,
        Instructions2,
        FindTarget,
        GoStraight,
        ReturnToCenter,
        Message,
        MoveRight,
        MoveLeft,
        Stop,
        AvoidObstacleRight,
        AvoidObstacleLeft,
        AvoidMovingObstacle,
        TurnRight,
        TurnLeft,
        Procede,
        FinishOk,
        SpeedUp,
        SlowDown,
        StopTarget,
        StartAgain,
        TurnBack
    }
    #endregion

    #region Protected members
    //protected TutorialStates[] Step1List = { TutorialStates.Instructions, TutorialStates.GoStraight, TutorialStates.SpeedUp, TutorialStates.SlowDown, TutorialStates.Stop, TutorialStates.TurnBack, TutorialStates.FinishOk };
    //protected TutorialStates[] Step2List = { TutorialStates.GoStraight, TutorialStates.AvoidObstacleLeft, TutorialStates.AvoidObstacleRight, TutorialStates.FinishOk };
    //protected TutorialStates[] Step3List = { TutorialStates.Instructions2, TutorialStates.GoStraight, TutorialStates.TurnRight, TutorialStates.TurnLeft, TutorialStates.GoStraight, TutorialStates.StopTarget, TutorialStates.FinishOk };

    protected string[] Step1MessagesMan = { "Follow the instructions...", "Let's see how to walk", "To start walking, lean forward once", "Return back to centre, he will keep walking", "To stop, lean backward once and return centre", "Well done! Let's try again" };
    protected string[] Step1MessagesWoman = { "Follow the instructions...", "Let's see how to walk", "To start walking, lean forward once", "Return back to centre, she will keep walking", "To stop, lean backward once and return centre", "Well done! Let's try again" };
    protected string[] Step2MessagesMan = { "Remember to always return to centre as soon as he starts or stops walking", "Lean forward to start walking and return to centre", "Lean bakward once to stop", "Great! Let's move on" };
    protected string[] Step2MessagesWoman = { "Remember to always return to centre as soon as she starts or stops walking", "Lean forward to start walking and return to centre", "Lean bakward once to stop", "Great! Let's move on" };
    protected string[] Step3Messages = { "Now we will see how to walk faster", "To start walking, lean forward once and return to centre", "Lean forward again to walk faster and return back to centre", "Lean backward once to slow down", "Lean backward again to stop", "Great! Let's move on" };
    protected string[] Step4Messages = { "When you are stopped, you can turn around to face the other way. Let's see how", "To turn around, lean backward (and return) to centre", "Great! Let's try again", "Lean backward and return to centre", "Great! Let's move on" };
    protected string[] Step5Messages = { "Find the", "Lean forward to walk", "Lean to turn to the right", "Lean to turn to the left", "Lean forward to walk", "Stop in front of the BAKERY", "Well Done! Let's move on" };
    protected string[] Step6Messages = { "Lean forward to walk", "Avoid the obstacle", "Avoid the obstacle", "Well Done! The Tutorial is finished." };



    protected TutorialStates[] Step1List = { TutorialStates.Instructions, TutorialStates.Message, TutorialStates.GoStraight, TutorialStates.ReturnToCenter, TutorialStates.Stop, TutorialStates.FinishOk };
    protected TutorialStates[] Step2List = { TutorialStates.Message, TutorialStates.GoStraight, TutorialStates.Stop, TutorialStates.FinishOk };
    protected TutorialStates[] Step3List = { TutorialStates.Message, TutorialStates.GoStraight, TutorialStates.SpeedUp, TutorialStates.SlowDown, TutorialStates.Stop, TutorialStates.FinishOk };
    protected TutorialStates[] Step4List = { TutorialStates.Message, TutorialStates.TurnBack, TutorialStates.Message, TutorialStates.TurnBack, TutorialStates.FinishOk };
    protected TutorialStates[] Step5List = { TutorialStates.Instructions2, TutorialStates.GoStraight, TutorialStates.TurnRight, TutorialStates.TurnLeft, TutorialStates.GoStraight, TutorialStates.StopTarget, TutorialStates.FinishOk };
    protected TutorialStates[] Step6List = { TutorialStates.GoStraight, TutorialStates.AvoidObstacleLeft, TutorialStates.AvoidObstacleRight, TutorialStates.FinishOk };

    protected int tutPhase = 1;

    protected TutorialFSM fsm = null;
    protected CQ_Interface _interface;
    protected GameplayManager gameplayManager;
    protected TutorialStates[] currentTutorialStatesSequence;
    protected string[] currentTutorialSequenceMessages;
    [SerializeField]
    protected int currentStep = 0;

    protected bool stepFailed = false;
    protected bool readyForNextStep = false;
    protected GameObject characterRef = null;

    protected bool started = false;

    #endregion

    #region public Properties
    public TutorialStates[] CurrentSequence
    {
        get { return currentTutorialStatesSequence; }
    }

    public float Longitudinal
    {
        get { return Long; }
    }

    public float Trasversal
    {
        get { return Trasv; }
    }

    public TokensManager.TokenHit CurrentToken
    {
        get { return currentToken; }
    }

    public GameObject character
    {
        get { return characterRef.gameObject; }
    }
    #endregion

    #region protected Classes

    protected class TutorialFSM : FSM.Object<TutorialFSM, TutorialStates>
    {        
        #region protected Members
        protected TutorialStates currentState;
        protected TutorialManager tutorialManager;
        protected GameplayManager gameplayManager;
        protected CQ_Interface _interface;
        protected BalanceBoardManager bbManager;
        protected GeneralSoundsManager genSoundsManager;
        protected bool statePassed = false;
        protected bool lastStateFailed = false;
        protected bool stateResetted = false;
        protected Vector3 lastCharPosition;
        protected Vector3 lastCharVelocity;
        protected PlayerKinematicBehaviour playerKinem;
        protected int kinematicState = 0;
        protected float resetTimer = -1.0f;
        //protected 
        #endregion

        #region Functions

        void ResetState(CQ_Interface.TutorialPages interfaceState)
        {
            if (lastStateFailed)
            {
                _interface.StartFadeOut(0.3f, () =>
                {
                    _interface.GoToTutorialState(interfaceState, tutorialManager.currentTutorialSequenceMessages[tutorialManager.currentStep]);
                    tutorialManager.character.transform.position = lastCharPosition;
                    playerKinem.KinematicState = 0;
                    playerKinem.gameObject.transform.forward = lastForward;
                    _interface.StartFadeIn(0.4f, () => { lastCharPosition = tutorialManager.character.transform.position; });
                });

                stateResetted = true;
                resetTimer = TimeManager.Instance.MasterSource.TotalTime;
            }
            else
            {
                lastCharPosition = tutorialManager.character.transform.position;
                _interface.GoToTutorialState(interfaceState, tutorialManager.currentTutorialSequenceMessages[tutorialManager.currentStep]);
            }

            kinematicState = playerKinem.KinematicState;
            lastForward = playerKinem.gameObject.transform.forward;

            lastStateFailed = false;
            statePassed = false;
        }

        void CheckFirstState()
        {
            if (tutorialManager.newSequence)
            {
                playerKinem.KinematicState = 0;
                tutorialManager.newSequence = false;
            }
        }

        #endregion

        #region Ctor
        public TutorialFSM(TutorialManager _tutorialManager, GameplayManager _gameplayManager) : base()
        {
            FSM.Object<TutorialFSM, TutorialStates>.Function voidFunc = (a, b) => { };
            this.AddState(TutorialManager.TutorialStates.Instructions, OnInstructionEnter, OnInstructionExec, OnInstructionExit);
            this.AddState(TutorialManager.TutorialStates.Instructions2, OnInstruction2Enter, OnInstruction2Exec, OnInstruction2Exit);
            this.AddState(TutorialManager.TutorialStates.Message, OnMessageEnter, OnMessageExec, OnMessageExit);
            this.AddState(TutorialManager.TutorialStates.GoStraight, OnGoStraightEnter, OnGoStraightExec, OnGoStraightExit);
            this.AddState(TutorialManager.TutorialStates.ReturnToCenter, OnReturnToCenterEnter, OnReturnToCenterExec, OnReturnToCenterExit);
            this.AddState(TutorialManager.TutorialStates.Stop, OnStopEnter, OnStopExec, OnStopExit);
            this.AddState(TutorialManager.TutorialStates.TurnRight, OnTurnRightEnter, OnTurnRightExec, OnTurnRightExit);
            this.AddState(TutorialManager.TutorialStates.TurnLeft, OnTurnLeftEnter, OnTurnLeftExec, OnTurnLeftExit);
            this.AddState(TutorialManager.TutorialStates.AvoidObstacleLeft, OnAvoidObstacleLeftEnter, OnAvoidObstacleLeftExec, OnAvoidObstacleLeftExit);
            this.AddState(TutorialManager.TutorialStates.AvoidObstacleRight, OnAvoidObstacleRightEnter, OnAvoidObstacleRightExec, OnAvoidObstacleRightExit);
            //this.AddState(TutorialManager.TutorialStates.AvoidMovingObstacle, OnAvoidMovingObstacleEnter, OnAvoidMovingObstacleExec, OnAvoidMovingObstacleExit);
            this.AddState(TutorialManager.TutorialStates.Procede, OnProcedeEnter, OnProcedeExec, OnProcedeExit);
            this.AddState(TutorialManager.TutorialStates.FinishOk, OnFinishOkEnter, OnFinishOkExec, OnFinishOkExit);
            this.AddState(TutorialManager.TutorialStates.SpeedUp, OnSpeedUpEnter, OnSpeedUpExec, OnSpeedUpExit);
            this.AddState(TutorialManager.TutorialStates.SlowDown, OnSlowDownEnter, OnSlowDownExec, OnSlowDownExit);
            this.AddState(TutorialManager.TutorialStates.StopTarget, OnStopTargetEnter, OnStopTargetExec, OnStopTargetExit);
            this.AddState(TutorialManager.TutorialStates.StartAgain, OnStartAgainEnter, OnStartAgainExec, OnStartAgainExit);
            this.AddState(TutorialManager.TutorialStates.TurnBack, OnTurnBackEnter, OnTurnBackExec, OnTurnBackExit);

            bbManager = GameObject.FindGameObjectWithTag("CoreManagers").GetComponent<BalanceBoardManager>();
            _interface = GameObject.FindGameObjectWithTag("Interface").GetComponent<CQ_Interface>();
            genSoundsManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GeneralSoundsManager>();
            gameplayManager = _gameplayManager;
            tutorialManager = _tutorialManager;
            playerKinem = tutorialManager.character.GetComponent<PlayerKinematicBehaviour>();
        }
        #endregion

        #region States
        #region Instructions State
        protected float startTutorialTimer = -1.0f;
        protected bool introViewed = false;
        void OnInstructionEnter(TutorialFSM self, float time)
        {
            playerKinem.SendMessage("SetInputEnabled", false);
            Debug.Log("--- TUTORIAL MANAGER: INSTRUCTIONS STATE ---");
            _interface.GoToTutorialState(CQ_Interface.TutorialPages.startTutorial, tutorialManager.currentTutorialSequenceMessages[tutorialManager.currentStep]);
            startTutorialTimer = TimeManager.Instance.MasterSource.TotalTime;

            CheckFirstState();
            introViewed = true;
        }

        void OnInstructionExec(TutorialFSM self, float time)
        {
            float now = TimeManager.Instance.MasterSource.TotalTime;

            if (introViewed && startTutorialTimer > 0.0f && now - startTutorialTimer > 3.0f)
                tutorialManager.GoToNextStep(true);
        }

        void OnInstructionExit(TutorialFSM self, float time)
        {
            startTutorialTimer = -1.0f;
            _interface.ShowHideTutorial(true);
            _interface.HideInstructions();
            tutorialManager.ItsACrossToken = false;
            playerKinem.SendMessage("SetInputEnabled", true);
        }
        #endregion

        #region Instructions2 State
        void OnInstruction2Enter(TutorialFSM self, float time)
        {
            playerKinem.SendMessage("SetInputEnabled", false);
            Debug.Log("--- TUTORIAL MANAGER: INSTRUCTIONS STATE ---");
            _interface.ViewInstructions("Find the", "Bakery", false, false);
            startTutorialTimer = TimeManager.Instance.MasterSource.TotalTime;

            CheckFirstState();
        }

        void OnInstruction2Exec(TutorialFSM self, float time)
        {
            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (startTutorialTimer > 0.0f && now - startTutorialTimer > 3.0f)
                tutorialManager.GoToNextStep(true);
        }

        void OnInstruction2Exit(TutorialFSM self, float time)
        {
            startTutorialTimer = -1.0f;
            _interface.ShowHideTutorial(true);
            _interface.HideInstructions();
            tutorialManager.ItsACrossToken = false;
            playerKinem.SendMessage("SetInputEnabled", true);
        }
        #endregion

        #region Message State
        protected float startMessageTimer = -1.0f;
        void OnMessageEnter(TutorialFSM self, float time)
        {
            playerKinem.SendMessage("SetInputEnabled", false);
            _interface.GoToTutorialState(CQ_Interface.TutorialPages.message, tutorialManager.currentTutorialSequenceMessages[tutorialManager.currentStep]);
            _interface.SetAnimatedMessage(tutorialManager.tutPhase == 2 ? "image" : "text");
            _interface.ShowHideTutorial(true);
            startMessageTimer = TimeManager.Instance.MasterSource.TotalTime;

            //SetMessageText();
            CheckFirstState();
        }

        void OnMessageExec(TutorialFSM self, float time)
        {
            float now = TimeManager.Instance.MasterSource.TotalTime;

            if (startMessageTimer > 0.0f && now - startMessageTimer > 8.0f)
                tutorialManager.GoToNextStep(true);
        }

        void OnMessageExit(TutorialFSM self, float time)
        {
            startMessageTimer = -1.0f;
            _interface.ShowHideTutorial(true);
            _interface.HideInstructions();
            tutorialManager.ItsACrossToken = false;
            playerKinem.SendMessage("SetInputEnabled", true);
        }
        #endregion

        #region GoStraight State
        protected float longitudinalControl = 0.0f;
        void OnGoStraightEnter(TutorialFSM self, float time)
        {
            lastCharPosition = tutorialManager.character.transform.position;
            _interface.GoToTutorialState(CQ_Interface.TutorialPages.shortStraight, tutorialManager.currentTutorialSequenceMessages[tutorialManager.currentStep], tutorialManager.tutPhase == 0);
            _interface.ShowHideTutorial(true);

            if (this.PrevState == TutorialStates.Message)
                longitudinalControl = tutorialManager.reverseToken ? 0.9f : 0.10f;
            else if(this.PrevState == TutorialStates.TurnLeft || this.PrevState == TutorialStates.TurnRight)
                longitudinalControl = tutorialManager.reverseToken ? 1.0f - 0.7f : 0.7f;
            else
                longitudinalControl = tutorialManager.reverseToken ? 1.0f - 0.1f : 0.1f;

            kinematicState = playerKinem.KinematicState;
            lastStateFailed = false;
            statePassed = false;

            playerKinem.SetDirectionsEnabled(true, false, false, false);
            CheckFirstState();
        }

        void OnGoStraightExec(TutorialFSM self, float time)
        {
            if (!playerKinem.InputEnabled)
                return;

            //_interface.UpdateFade();
            float now = TimeManager.Instance.MasterSource.TotalTime;

            if (stateResetted)
            {
                if (now - resetTimer > 1.0f)
                {
                    resetTimer = -1.0f;
                    stateResetted = false;
                    playerKinem.KinematicState = 0;
                    playerKinem.SendMessage("SetInputEnabled", true);
                }
            }
            else
            {
                if (tutorialManager.CurrentToken.token.type == Token.TokenType.Cross && !directionsChanged && playerKinem.KinematicState == 2)
                {
                    directionsChanged = true;
                    playerKinem.SetAvailableDirections(true, false, false);
                }

                if (statePassed)
                    playerKinem.SetDirectionsEnabled(false, false, false, false);

                if (!statePassed && bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Up))
                {
                    genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                    _interface.ShowHideTutorial(false);
                    //playerKinem.SetDirectionsEnabled(false, false, false, false);
                    statePassed = true;
                }

                if (this.PrevState != TutorialStates.Instructions2)
                {
                    bool nextStateReady = tutorialManager.reverseToken ? tutorialManager.Longitudinal < longitudinalControl : tutorialManager.Longitudinal > longitudinalControl;
                    if (statePassed && nextStateReady)
                    {
                        tutorialManager.GoToNextStep(true);
                    }


                    bool stateFailed = tutorialManager.reverseToken ? tutorialManager.Longitudinal < longitudinalControl * 0.5f : tutorialManager.Longitudinal > 1 - longitudinalControl * 0.5f;
                    if (stateFailed)
                        tutorialManager.GoToNextStep(false);
                }
                else
                {
                    if (statePassed && tutorialManager.Longitudinal > 0.95f && !tutorialManager.ItsACrossToken)
                        tutorialManager.GoToNextStep(true);
                }
            }
        }

        void OnGoStraightExit(TutorialFSM self, float time)
        {
            //_interface.ResetFade();
            resetTimer = -1.0f;
            tutorialManager.ItsACrossToken = false;
            directionsChanged = false;
        }

        #endregion

        #region ReturnToCenter State
        protected float startReturnToCenterTimer = -1.0f;
        protected float stopReturnToCenterTimer = -1.0f;
        void OnReturnToCenterEnter(TutorialFSM self, float time)
        {
            playerKinem.SendMessage("SetInputEnabled", false);
            _interface.ShowHideTutorial(true);

            _interface.GoToTutorialState(CQ_Interface.TutorialPages.returnToCenter, tutorialManager.currentTutorialSequenceMessages[tutorialManager.currentStep], tutorialManager.tutPhase == 0);
            startReturnToCenterTimer = TimeManager.Instance.MasterSource.TotalTime;
            stopReturnToCenterTimer = -1.0f;
            //SetMessageText();
            CheckFirstState();
        }

        void OnReturnToCenterExec(TutorialFSM self, float time)
        {
            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (startReturnToCenterTimer > 0.0f && now - startReturnToCenterTimer > 5.0f)
            {
                startReturnToCenterTimer = -1.0f;
                stopReturnToCenterTimer = now;
                _interface.ShowHideTutorial(false);
            }

            if (stopReturnToCenterTimer > 0.0f && now - stopReturnToCenterTimer > 1.5f)
            {
                stopReturnToCenterTimer = -1.0f;
                tutorialManager.GoToNextStep(true);
            }
        }

        void OnReturnToCenterExit(TutorialFSM self, float time)
        {
            startReturnToCenterTimer = -1.0f;
            _interface.ShowHideTutorial(true);
            //_interface.HideInstructions();
            tutorialManager.ItsACrossToken = false;
            playerKinem.SendMessage("SetInputEnabled", true);
        }
        #endregion

        #region Stop State
        float stopTimer = -1.0f;
        void OnStopEnter(TutorialFSM self, float time)
        {
            ResetState(CQ_Interface.TutorialPages.shortBack);
            //_interface.GoToTutorialState(CQ_Interface.TutorialPages.shortBack);
            _interface.ShowHideTutorial(true);
            playerKinem.SetDirectionsEnabled(false, false, false, true);
            CheckFirstState();
        }

        void OnStopExec(TutorialFSM self, float time)
        {
            _interface.UpdateFade();
            
            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (stateResetted)
            {
                if (now - resetTimer > 1.0f)
                {
                    resetTimer = 0.0f;
                    stateResetted = false;
                    playerKinem.KinematicState = 1;
                    playerKinem.SendMessage("SetInputEnabled", true);
                }
            }
            else
            {
                if (!playerKinem.InputEnabled)
                    return;

                if (statePassed)
                {
                    playerKinem.SetDirectionsEnabled(false, false, false, false);
                }

                if (!statePassed && bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Down))
                {
                    genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                    _interface.ShowHideTutorial(false);
                    statePassed = true;
                    stopTimer = now;
                }

                if (statePassed) 
                {
                    if (stopTimer > 0.0f && now - stopTimer > 1.0f)
                    {
                        stopTimer = -1.0f;
                        tutorialManager.GoToNextStep(true);
                    }
                }
                else
                {
                    bool stateFailed = tutorialManager.reverseToken ? tutorialManager.Longitudinal < 1 - 0.95f : tutorialManager.Longitudinal > 0.95f;
                    if (stateFailed)
                    {
                        lastStateFailed = true;
                        tutorialManager.GoToNextStep(false);
                    }
                }
            }
        }
        void OnStopExit(TutorialFSM self, float time)
        {
            stopTimer = -1.0f;
            tutorialManager.ItsACrossToken = false;
        }
        #endregion

        #region AvoidObstacleLeft State
        protected float currentLeftMove = 0.0f;
        protected float obstacleTimer = -1.0f;
        bool AvoidObstacleLeftKeypressed = false;

        void OnAvoidObstacleLeftEnter(TutorialFSM self, float time)
        {
            ResetState(CQ_Interface.TutorialPages.avoidObstacleLeftBis);
            //_interface.GoToTutorialState(CQ_Interface.TutorialPages.avoidObstacleLeftBis);
            _interface.ShowHideTutorial(true);
            //obstacleTimer = TimeManager.Instance.MasterSource.TotalTime;
            statePassed = false;
            kinematicState = playerKinem.KinematicState;
            currentLeftMove = tutorialManager.Trasversal;
            AvoidObstacleLeftKeypressed = false;
            playerKinem.SetDirectionsEnabled(false, false, true, false);
            CheckFirstState();
        }

        void OnAvoidObstacleLeftExec(TutorialFSM self, float time)
        {

            _interface.UpdateFade();

            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (stateResetted)
            {
                if (now - resetTimer > 1.0f)
                {
                    resetTimer = 0.0f;
                    stateResetted = false;
                    playerKinem.KinematicState = 1;
                    //obstacleTimer = TimeManager.Instance.MasterSource.TotalTime;
                    playerKinem.SendMessage("SetInputEnabled", true);
                }
            }
            else
            {
                if (!playerKinem.InputEnabled)
                    return;

                if (!AvoidObstacleLeftKeypressed && bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Left))
                    AvoidObstacleLeftKeypressed = true;

                bool itsOnLeft = false;// = tutorialManager.reverseToken ? tutorialManager.Trasversal > -0.35f : tutorialManager.Trasversal < -0.35f;
                if (currentLeftMove != tutorialManager.Trasversal)
                    itsOnLeft = true;

                if (playerKinem.Collided)
                {
                    tutorialManager.GoToNextStep(false);
                    lastStateFailed = true;
                }
                else
                {
                    //if (tutorialManager.Longitudinal > 0.26f)
                    if (tutorialManager.Longitudinal > 0.40f || tutorialManager.Trasversal < 0.0f)
                    {
                        playerKinem.SetDirectionsEnabled(false, false, false, false);
                        _interface.ShowHideTutorial(false);
                    }
                }


                //if (tutorialManager.Longitudinal > 0.43f)
                if (tutorialManager.Longitudinal > 0.50f)
                {
                    if (itsOnLeft)
                    {
                        genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                        tutorialManager.GoToNextStep(true);
                    }
                    else
                    {
                        tutorialManager.GoToNextStep(false);
                        lastStateFailed = true;
                    }
                }

                //if (obstacleTimer > 0.0f && now - obstacleTimer > 2.0f && !AvoidObstacleLeftKeypressed)
                //    _interface.GoToTutorialState(CQ_Interface.TutorialPages.avoidObstacleLeftBis);
            }
        }
        void OnAvoidObstacleLeftExit(TutorialFSM self, float time)
        {
            //_interface.ResetFade();
            obstacleTimer = -1.0f;
            tutorialManager.ItsACrossToken = false;
            currentLeftMove = 0.0f;
        }

        #endregion

        #region AvoidObstacleRight State
        protected float currentRightMove = 0.0f;
        void OnAvoidObstacleRightEnter(TutorialFSM self, float time)
        {
            ResetState(CQ_Interface.TutorialPages.avoidObstacleRightBis);
            //_interface.GoToTutorialState(CQ_Interface.TutorialPages.avoidObstacleRightBis);
            _interface.ShowHideTutorial(true);
            //obstacleTimer = TimeManager.Instance.MasterSource.TotalTime;
            statePassed = false;
            kinematicState = playerKinem.KinematicState;
            currentRightMove = tutorialManager.Trasversal;
            AvoidObstacleLeftKeypressed = false;
            playerKinem.SetDirectionsEnabled(false, true, false, false);
            CheckFirstState();

            GameObject.FindGameObjectWithTag("StaticObjects").BroadcastMessage("Reset", SendMessageOptions.DontRequireReceiver);
            GameObject.FindGameObjectWithTag("StaticObjects").BroadcastMessage("Activate", SendMessageOptions.DontRequireReceiver);
            //GameObject.FindGameObjectWithTag("StaticObjects").BroadcastMessage("Deactivate", SendMessageOptions.DontRequireReceiver);
            _interface.UpdateFade();
        }

        void OnAvoidObstacleRightExec(TutorialFSM self, float time)
        {

            _interface.UpdateFade();

            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (stateResetted)
            {
                if (now - resetTimer > 0.5f)
                {
                    resetTimer = 0.0f;
                    stateResetted = false;
                    playerKinem.KinematicState = 1;
                    //obstacleTimer = TimeManager.Instance.MasterSource.TotalTime;
                    playerKinem.SendMessage("SetInputEnabled", true);
                    GameObject.FindGameObjectWithTag("StaticObjects").BroadcastMessage("ForceActivate", SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                if (!playerKinem.InputEnabled)
                    return;

                if (!AvoidObstacleLeftKeypressed && bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Right))
                    AvoidObstacleLeftKeypressed = true;
               
                bool itsOnRight = false ;//tutorialManager.reverseToken ? tutorialManager.Trasversal < 0.35f : tutorialManager.Trasversal > 0.35f;
                if (currentRightMove != tutorialManager.Trasversal && tutorialManager.Trasversal >= 0.3f)
                    itsOnRight = true;

                //Debug.Log("tutorialManager.Trasversal " + tutorialManager.Trasversal);

                if (!itsOnRight)
                {
                    if (playerKinem.Collided)
                    {
                        tutorialManager.GoToNextStep(false);
                        lastStateFailed = true;
                    }
                }
                //else if (itsOnRight && tutorialManager.Longitudinal > 0.55f)
                else if (itsOnRight && tutorialManager.Longitudinal > 0.80f)
                    _interface.ShowHideTutorial(false);

                //if (tutorialManager.Longitudinal > 0.63f)
                if (tutorialManager.Longitudinal > 0.90f)
                {
                    if (itsOnRight)
                    {
                        genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                        _interface.ShowHideTutorial(false);
                        tutorialManager.GoToNextStep(true);
                    }
                    else
                    {
                        tutorialManager.GoToNextStep(false);
                        lastStateFailed = true;
                    }
                }

                //if (obstacleTimer > 0.0f && now - obstacleTimer > 2.0f && !AvoidObstacleLeftKeypressed)
                //    _interface.GoToTutorialState(CQ_Interface.TutorialPages.avoidObstacleRightBis);
            }
        }

        void OnAvoidObstacleRightExit(TutorialFSM self, float time)
        {
            //_interface.ResetFade();
            obstacleTimer = -1.0f;
            tutorialManager.ItsACrossToken = false;
        }
        #endregion

        #region TurnRight State
        Vector3 lastForward = new Vector3();
        bool turnRightPressed = false;
        protected AnimationBehaviour playerAnim;
        bool directionEnabled = false;

        void OnTurnRightEnter(TutorialFSM self, float time)
        {
            Debug.Log("TUTORIAL MANAGER - TURN RIGHT STATE");
            ResetState(CQ_Interface.TutorialPages.turnRight);
            //_interface.GoToTutorialState(CQ_Interface.TutorialPages.turnRight);
            _interface.ShowHideTutorial(false);
            statePassed = false;
            turnRightPressed = false;
            directionEnabled = false;
            playerKinem.SetDirectionsEnabled(false, false, false, false);
            CheckFirstState();

            playerAnim = playerKinem.gameObject.GetComponent<AnimationBehaviour>();
        }

        void OnTurnRightExec(TutorialFSM self, float time)
        {

            _interface.UpdateFade();
            
            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (stateResetted)
            {
                if (now - resetTimer > 1.0f)
                {
                    resetTimer = 0.0f;
                    stateResetted = false;
                    playerKinem.KinematicState = 0;
                    playerKinem.SendMessage("SetInputEnabled", true);
                }
            }
            else
            {
                if (!playerKinem.InputEnabled)
                    return;

                if (tutorialManager.CurrentToken.token.type == Token.TokenType.Cross && !directionsChanged && playerKinem.KinematicState == 2)
                {
                    //Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                    directionsChanged = true;
                    playerKinem.SetAvailableDirections(false, true, false);
                }

                if (!turnRightPressed && playerAnim.IsPlayingStay && !directionEnabled)
                {
                    playerKinem.SetDirectionsEnabled(false, true, false, false);
                    directionEnabled = true;
                    _interface.ShowHideTutorial(true);
                }

                if (turnRightPressed)
                    playerKinem.SetDirectionsEnabled(false, false, false, false);


                if (directionEnabled && !turnRightPressed && bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Right))   //3
                {
                    //playerKinem.SetDirectionsEnabled(false, true, false, false);
                    _interface.ShowHideTutorial(false);
                    turnRightPressed = true;
                    tutorialManager.ItsACrossToken = false;
                    //playerKinem.SetDirectionsEnabled(false, false, false, false);
                }

                bool tokenConditions = tutorialManager.ItsACrossToken && (tutorialManager.reverseToken && tutorialManager.Longitudinal > 0.5f || !tutorialManager.reverseToken && tutorialManager.Longitudinal < 0.5f);

                if (tokenConditions && turnRightPressed)
                {
                    genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                    _interface.ShowHideTutorial(false);
                    tutorialManager.GoToNextStep(true);
                }
            }
        }

        void OnTurnRightExit(TutorialFSM self, float time)
        {
            //_interface.ResetFade();
            turnRightPressed = false;
            tutorialManager.ItsACrossToken = false;
            directionsChanged = false;
        }
        #endregion

        #region TurnLeft State
        bool turnLeftKeyPressed = false;
        bool directionsChanged = false;
        void OnTurnLeftEnter(TutorialFSM self, float time)
        {
            ResetState(CQ_Interface.TutorialPages.turnLeft);
            //_interface.GoToTutorialState(CQ_Interface.TutorialPages.turnLeft);
            _interface.ShowHideTutorial(true);
            statePassed = false;
            kinematicState = playerKinem.KinematicState;
            //playerKinem.SetDirectionsEnabled(false, false, true, false);
            playerKinem.SetDirectionsEnabled(false, false, false, false);
            CheckFirstState();
            directionEnabled = false;
        }

        void OnTurnLeftExec(TutorialFSM self, float time)
        {
            _interface.UpdateFade();

            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (stateResetted)
            {
                if (now - resetTimer > 1.0f)
                {
                    resetTimer = 0.0f;
                    stateResetted = false;
                    playerKinem.KinematicState = 0;
                    playerKinem.SendMessage("SetInputEnabled", true);
                }
            }
            else
            {
                if (!playerKinem.InputEnabled)
                    return;

                if (tutorialManager.CurrentToken.token.type == Token.TokenType.Cross && !directionsChanged && playerKinem.KinematicState == 2)
                {
                    directionsChanged = true;
                    playerKinem.SetAvailableDirections(false, false, true);
                }

                if (!turnLeftKeyPressed && playerAnim.IsPlayingStay && !directionEnabled)
                {
                    playerKinem.SetDirectionsEnabled(false, false, true, false);
                    directionEnabled = true;
                    _interface.ShowHideTutorial(true);
                    Debug.Log("TURN RIGHT TUTORIAL SHOWN: 111");
                }

                if (turnLeftKeyPressed)
                    playerKinem.SetDirectionsEnabled(false, false, false, false);

                if (directionEnabled && !turnLeftKeyPressed && bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Left))   //3
                {
                    //playerKinem.SetDirectionsEnabled(false, false, true, false);
                    _interface.ShowHideTutorial(false);
                    turnLeftKeyPressed = true;
                    tutorialManager.ItsACrossToken = false;
                    //playerKinem.SetDirectionsEnabled(false, false, false, false);
                }

                bool tokenConditions = tutorialManager.ItsACrossToken && (tutorialManager.reverseToken && tutorialManager.Longitudinal > 0.5f || !tutorialManager.reverseToken && tutorialManager.Longitudinal < 0.5f);
                if (turnLeftKeyPressed && tokenConditions)
                {
                    genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                    _interface.ShowHideTutorial(false);
                    tutorialManager.GoToNextStep(true);
                }
            }
        }

        void OnTurnLeftExit(TutorialFSM self, float time)
        {
            //_interface.ResetFade();
            turnLeftKeyPressed = false;
            tutorialManager.ItsACrossToken = false;
            directionsChanged = false;
        }
        #endregion

        #region Procede State
        void OnProcedeEnter(TutorialFSM self, float time)
        {
            ResetState(CQ_Interface.TutorialPages.shortStraight);
            //_interface.GoToTutorialState(CQ_Interface.TutorialPages.shortStraight);
            _interface.ShowHideTutorial(true);
            statePassed = false;
            kinematicState = playerKinem.KinematicState;
            playerKinem.SetDirectionsEnabled(true, false, false, false);
            CheckFirstState();
        }
        void OnProcedeExec(TutorialFSM self, float time)
        {
            _interface.UpdateFade();


            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (stateResetted)
            {
                if (now - resetTimer > 1.0f)
                {
                    resetTimer = 0.0f;
                    stateResetted = false;
                    playerKinem.KinematicState = 0;
                    playerKinem.SendMessage("SetInputEnabled", true);
                }
            }
            else
            {
                if (!playerKinem.InputEnabled)
                    return;

                if (statePassed)
                    playerKinem.SetDirectionsEnabled(false, false, false, false);

                if (!statePassed && bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Up))
                {
                    genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                    _interface.ShowHideTutorial(false);
                    statePassed = true;
                    //playerKinem.SetDirectionsEnabled(false, false, false, false);
                }

                if (statePassed && tutorialManager.Longitudinal > 0.95f && !tutorialManager.ItsACrossToken)
                    tutorialManager.GoToNextStep(true);
            }
        }
        void OnProcedeExit(TutorialFSM self, float time)
        {
            //_interface.ResetFade();
            tutorialManager.ItsACrossToken = false;
        }
        #endregion

        #region FinishOk State
        float finishTimer = -1.0f;

        void OnFinishOkEnter(TutorialFSM self, float time)
        {
            _interface.ShowHideTutorial(false);
            //_interface.ViewInstructions("Well Done!", "empty", true, true);
            _interface.ViewInstructions(tutorialManager.currentTutorialSequenceMessages[tutorialManager.currentStep], "empty", true, true);
            finishTimer = TimeManager.Instance.MasterSource.TotalTime;
        }

        void OnFinishOkExec(TutorialFSM self, float time)
        {
            _interface.UpdateFade();


            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (stateResetted)
            {
                if (now - resetTimer > 1.0f)
                {
                    resetTimer = 0.0f;
                    stateResetted = false;
                    playerKinem.KinematicState = 0;
                    finishTimer = TimeManager.Instance.MasterSource.TotalTime;
                    playerKinem.SendMessage("SetInputEnabled", true);
                }
            }
            else
            {
                if (!playerKinem.InputEnabled)
                    return;

                if ((finishTimer > 0.0f && now - finishTimer >= 3.0f) || tutorialManager.Longitudinal > 0.95f)
                {
                    finishTimer = -1.0f;
                    tutorialManager.GoToNextStep(true);
                }
            }
        }

        void OnFinishOkExit(TutorialFSM self, float time)
        {
            //_interface.ResetFade();
            finishTimer = -1.0f;
            _interface.HideInstructions();
        }
        #endregion

        #region SpeedUp State
        void OnSpeedUpEnter(TutorialFSM self, float time)
        {
            ResetState(CQ_Interface.TutorialPages.speedUp);
            //_interface.GoToTutorialState(CQ_Interface.TutorialPages.speedUp);
            _interface.ShowHideTutorial(true);

            lastStateFailed = false;
            statePassed = false;
            playerKinem.SetDirectionsEnabled(true, false, false, false);
            CheckFirstState();
        }

        void OnSpeedUpExec(TutorialFSM self, float time)
        {
            _interface.UpdateFade();


            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (stateResetted)
            {
                if (now - resetTimer > 1.0f)
                {
                    resetTimer = 0.0f;
                    stateResetted = false;
                    playerKinem.KinematicState = 1;
                    playerKinem.SendMessage("SetInputEnabled", true);
                }
            }
            else
            {
                if (!playerKinem.InputEnabled)
                    return;

                if (statePassed)
                    playerKinem.SetDirectionsEnabled(false, false, false, false);

                if (!statePassed && bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Up))
                {
                    genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                    _interface.ShowHideTutorial(false);
                    statePassed = true;
                    //playerKinem.SetDirectionsEnabled(false, false, false, false);
                }

                //if (!statePassed)
                //{
                //    if (bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Down) ||
                //        bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Left) ||
                //        bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Right))
                //    {
                //        genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueKO);
                //        tutorialManager.GoToNextStep(false);
                //        lastStateFailed = true;
                //    }
                //}

                if (tutorialManager.Longitudinal > 0.5f)
                {
                    if (statePassed)
                    {
                        tutorialManager.GoToNextStep(true);
                    }
                    else
                    {
                        lastStateFailed = true;
                        tutorialManager.GoToNextStep(false);
                    }
                }
            }
        }

        void OnSpeedUpExit(TutorialFSM self, float time)
        {
            //_interface.ResetFade();
            tutorialManager.ItsACrossToken = false;
            statePassed = false;
        }
        #endregion

        #region SlowDown State
        void OnSlowDownEnter(TutorialFSM self, float time)
        {
            ResetState(CQ_Interface.TutorialPages.slowDown);
            //_interface.GoToTutorialState(CQ_Interface.TutorialPages.slowDown);
            _interface.ShowHideTutorial(true);
            playerKinem.SetDirectionsEnabled(false, false, false, true);
            CheckFirstState();
        }

        void OnSlowDownExec(TutorialFSM self, float time)
        {
            _interface.UpdateFade();


            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (stateResetted)
            {
                if (now - resetTimer > 1.0f)
                {
                    resetTimer = 0.0f;
                    stateResetted = false;
                    playerKinem.KinematicState = 3;
                    playerKinem.SendMessage("SetInputEnabled", true);
                }
            }
            else
            {
                if (!playerKinem.InputEnabled)
                    return;

                if (statePassed)
                    playerKinem.SetDirectionsEnabled(false, false, false, false);

                if (!statePassed && bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Down))
                {
                    genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                    _interface.ShowHideTutorial(false);
                    statePassed = true;
                    //playerKinem.SetDirectionsEnabled(false, false, false, false);
                }

                //if (!statePassed)
                //{
                //    if (bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Up) ||
                //        bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Left) ||
                //        bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Right))
                //    {
                //        genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueKO);
                //        tutorialManager.GoToNextStep(false);
                //        lastStateFailed = true;
                //    }
                //}

                if (tutorialManager.reverseToken ? tutorialManager.Longitudinal < 0.7f : tutorialManager.Longitudinal > 0.7f)
                {
                    if (statePassed)
                    {
                        tutorialManager.GoToNextStep(true);
                    }
                    else
                    {
                        lastStateFailed = true;
                        tutorialManager.GoToNextStep(false);
                    }
                }
            }
        }

        void OnSlowDownExit(TutorialFSM self, float time)
        {
            //_interface.ResetFade();
            tutorialManager.ItsACrossToken = false;
            statePassed = false;
        }
        #endregion

        #region StopTarget State
        bool stopTargetKeyPressed = false;
        void OnStopTargetEnter(TutorialFSM self, float time)
        {
            ResetState(CQ_Interface.TutorialPages.stopOnTarget);
            //_interface.GoToTutorialState(CQ_Interface.TutorialPages.stopOnTarget);
            _interface.ShowHideTutorial(true);
            playerKinem.SetDirectionsEnabled(false, false, false, true);
            CheckFirstState();
            stopTargetKeyPressed = false;
        }

        void OnStopTargetExec(TutorialFSM self, float time)
        {
            _interface.UpdateFade();

            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (stateResetted)
            {
                if (now - resetTimer > 1.0f)
                {
                    resetTimer = 0.0f;
                    stateResetted = false;
                    playerKinem.KinematicState = 1;
                    playerKinem.SendMessage("SetInputEnabled", true);
                }
            }
            else
            {
                if (!playerKinem.InputEnabled)
                    return;

                if (!stopTargetKeyPressed && bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Down))
                    stopTargetKeyPressed = true;

                Vector3 localPlayerPos = gameplayManager.tutorialTarget.gameObject.transform.InverseTransformPoint(playerKinem.gameObject.transform.position);
                bool proximityCheck = (localPlayerPos.x > 0.0f) && (localPlayerPos.x < 5.0f);

                bool longPassed = tutorialManager.reverseToken ? tutorialManager.Longitudinal < 0.05f : tutorialManager.Longitudinal > 0.95f;

                if (longPassed)
                {
                    genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueKO);
                    lastStateFailed = true;
                    tutorialManager.GoToNextStep(false);
                }

                if (stopTargetKeyPressed)
                {
                    if (proximityCheck)
                    {
                        genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                        _interface.ShowHideTutorial(false);
                        tutorialManager.GoToNextStep(true);
                    }
                    else
                    {
                        genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueKO);
                        lastStateFailed = true;
                        tutorialManager.GoToNextStep(false);
                    }
                    //playerKinem.SetDirectionsEnabled(false, false, false, false);
                    //super patch
                    tutorialManager.DisableDirections();
                }
            }
        }

        void OnStopTargetExit(TutorialFSM self, float time)
        {
            //_interface.ResetFade();
            tutorialManager.ItsACrossToken = false;
            stopTargetKeyPressed = false;
        }
        #endregion

        #region StartAgain State
        protected float startAgainTimer = 0.0f;

        void OnStartAgainEnter(TutorialFSM self, float time)
        {
            playerKinem.SendMessage("SetInputEnabled", false);
            _interface.GoToTutorialState(CQ_Interface.TutorialPages.startAgain, "Start again");
            startAgainTimer = TimeManager.Instance.MasterSource.TotalTime; 
            _interface.ShowHideTutorial(true);
            playerKinem.KinematicState = 0;
            playerKinem.DisableAllDirections();
        }

        void OnStartAgainExec(TutorialFSM self, float time)
        {
            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (startAgainTimer > 0 && now - startAgainTimer > 2.0f)
                this.State = PrevState;
        }

        void OnStartAgainExit(TutorialFSM self, float time)
        {
            //_interface.ResetFade();
            startAgainTimer = -1.0f;
            tutorialManager.ItsACrossToken = false;
            playerKinem.SendMessage("ObstacleTriggerExit");
            playerKinem.EnableLastDirectionsConfiguration();
        }

        #endregion

        #region TurnBack State
        protected float turnBackTimer = -1.0f;

        void OnTurnBackEnter(TutorialFSM self, float time)
        {
            ResetState(CQ_Interface.TutorialPages.retroFront);
            //_interface.GoToTutorialState(CQ_Interface.TutorialPages.shortBack);
            _interface.ShowHideTutorial(true);
            playerKinem.SetDirectionsEnabled(false, false, false, true);
            CheckFirstState();
        }

        void OnTurnBackExec(TutorialFSM self, float time)
        {
            _interface.UpdateFade();


            float now = TimeManager.Instance.MasterSource.TotalTime;
            if (stateResetted)
            {
                if (now - resetTimer > 1.0f)
                {
                    resetTimer = 0.0f;
                    stateResetted = false;
                    playerKinem.KinematicState = 1;
                    playerKinem.SendMessage("SetInputEnabled", true);
                }
            }
            else
            {
                if (!playerKinem.InputEnabled)
                    return;

                if (statePassed)
                {
                    playerKinem.SetDirectionsEnabled(false, false, false, false);

                }

                if (!statePassed && bbManager.GetDirectionOnce(BalanceBoardManager.Directions.Down))
                {
                    genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                    _interface.ShowHideTutorial(false);
                    statePassed = true;
                    turnBackTimer = now;
                }

                if (statePassed && turnBackTimer > 0 && now - turnBackTimer > 2.0f)
                {
                    turnBackTimer = -1.0f;
                    tutorialManager.GoToNextStep(true);
                }
            }
        }
        void OnTurnBackExit(TutorialFSM self, float time)
        {
            //_interface.ResetFade();
            turnBackTimer = -1.0f;
            tutorialManager.ItsACrossToken = false;
        }
        #endregion
        #endregion
    }

    public void DisableDirections()
    {
        StartCoroutine(DisableDirectionsCoroutine());
    }

    IEnumerator DisableDirectionsCoroutine()
    {
        //super patch
        yield return new WaitForSeconds(0.5f);
        character.GetComponent<PlayerKinematicBehaviour>().SetDirectionsEnabled(false, false, false, false);
    }
    #endregion

    #region Messages

    void StartTutorial()
    {
        Debug.Log("TUTORIAL MANAGER STARTED");
        characterRef = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GameplayManager>().Character;
        //_interface.SendMessage("SetPlayerIsAMan", !characterRef.gameObject.name.Contains("Female"));
        tutPhase = 1;
        currentStep = 0;
        currentTutorialStatesSequence = Step1List;
        currentTutorialSequenceMessages = gameplayManager.Gender== GameplayManager.CharacterGender.Male ?  Step1MessagesMan : Step1MessagesWoman;

        fsm = new TutorialFSM(this, gameplayManager);
        fsm.State = currentTutorialStatesSequence[currentStep];
        tutorialFinished = false;
        started = true;

        TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "start", "tutorial");
        TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase1", "start");
    }

    #endregion

    #region Functions

    /*void Fade()
    {
        _interface.InitializeFade();
        _interface.StartFadeOut(0.3f, () => _interface.StartFadeIn(0.5f, null));
    }*/

    void StartChangeSequence(TutorialStates[] stepsList, string[] stepMessagesList)
    {
        _interface.StartFadeOut(0.5f, () => { ChangeSequence(stepsList, stepMessagesList); }); 
    }

    void ChangeSequence(TutorialStates[] stepsList, string[] messagesList)
    {
        characterRef.gameObject.GetComponent<PlayerKinematicBehaviour>().PlaceCharacter(tutPhase, -1);
        currentTutorialStatesSequence = stepsList;
        currentTutorialSequenceMessages = messagesList;
        newSequence = true;
        currentStep = 0;
        fsm.State = currentTutorialStatesSequence[currentStep];
        _interface.StartFadeIn(0.5f, null);
    }

    void GoToNextStep(bool currentStepValid)
    {
        if (fsm.State == TutorialStates.FinishOk)
        {
            ++tutPhase;
            if (tutPhase == 2)
            {
                StartChangeSequence(Step2List, gameplayManager.Gender == GameplayManager.CharacterGender.Male ? Step2MessagesMan : Step2MessagesWoman);
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase1", "end");
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase2", "start");
            }
            else if (tutPhase == 3)
            {
                StartChangeSequence(Step3List, Step3Messages);
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase2", "end");
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase3", "start");
            }
            else if (tutPhase == 4)
            {
                StartChangeSequence(Step4List, Step4Messages);
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase3", "end");
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase4", "start");
            }
            else if (tutPhase == 5)
            {
                StartChangeSequence(Step5List, Step5Messages);
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase4", "end");
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase5", "start");
            }
            else if (tutPhase == 6)
            {
                StartChangeSequence(Step6List, Step6Messages);
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase5", "end");
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase6", "start");
            }
            else
            {
                tutorialFinished = true;
                gameplayManager.SendMessage("LevelCompleted");
                character.gameObject.GetComponent<PlayerKinematicBehaviour>().SetDirectionsEnabled(true, true, true, true);
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase6", "end");
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "end", "tutorial");
                return;
            }
        }
        else
        {
            if (currentStepValid)
            {
                fsm.State = currentTutorialStatesSequence[++currentStep];
            }
            else
            {
                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "phase" + tutPhase, "error");
                fsm.State = TutorialStates.StartAgain;
            }
        }
    }

    TokensManager.TokenHit currentToken = new TokensManager.TokenHit();

    void UpdateToken()
    {
        TokensManager.TokenHit hit;
        if (characterRef != null)
        {
            hitFlag = TokensManager.Instance.GetToken(characterRef.transform.position, out hit);

            if (hitFlag)
            {
                if (currentToken.token != hit.token)
                {
                    currentToken = hit;
                    ItsACrossToken = currentToken.token.type == Token.TokenType.Cross;
                    reverseToken = hit.longitudinal > 0.5f;
                }
                Long = hit.longitudinal;
                Trasv = hit.trasversal;
                CurrentTokenName = hit.token.name;
            }
            //Debug.Log("CurrentToken: " + CurrentToken + " Long: " + Long + " Trasv: " + Trasv);
        }
    }

    void ResetConfiguration()
    {
    }
    #endregion

    #region Unity Callbacks
    public bool hitFlag;
    public string CurrentTokenName;
    public float Long;
    public float Trasv;
    public bool reverseToken = false;
    public bool ItsACrossToken = false;
    protected bool tutorialFinished = false;

    void Start()
    {
        _interface = GameObject.FindGameObjectWithTag("Interface").GetComponent<CQ_Interface>();
        gameplayManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GameplayManager>();
    }

    void Update()
    {
        if (!started)
            return;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            GoToNextStep(true);
        }
#endif
        if (_interface.State == CQ_Interface.TutorialPage)
        {
            fsm.Update();
            //_interface.UpdateFade();
            if (!tutorialFinished)
                UpdateToken();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            tutorialFinished = true;
            gameplayManager.SendMessage("LevelCompleted");
            character.gameObject.GetComponent<PlayerKinematicBehaviour>().SetDirectionsEnabled(true, true, true, true);
        }
#endif
    }

    #endregion
}
