using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS.Core;
using FoF.Utils;
using SBS.XML;
using System.Xml;
using SBS.Math;

[AddComponentMenu("FoF/GameplayManager")]
public class GameplayManager : MonoBehaviour
{
    #region Internal Data Structure
    public enum ProgressSignalType
    {
        OnGameplayXmlParsed = 0,
        OnXmlError,
        OnGameplayCharacterChoosen,
        OnGameplayReset,
        OnGameplayGeneralXmlParsed
    }

    public struct ProgressSignal
    {
        public ProgressSignalType type;
        public object param;

        public ProgressSignal(ProgressSignalType type)
        {
            this.type = type;
            this.param = null;
        }

        public ProgressSignal(ProgressSignalType type, object param)
        {
            this.type = type;
            this.param = param;
        }
    }

    public enum CharacterGender
    {
        Male = 0,
        Female
    }

    public enum TargetType
    {
        Bakery = 1,
        PostOffice,
        Jewelry,
        Bank,
        Greengrocer,
        Minimarket,
        Cinema,
        ToyShop,
        Caffe,
        Pharmacy
    }

    public enum GameplayType
    {
        Navigation = 0,
        ObstacleAvoidance
    }

    public class Target
    {
        public TargetType type;
        public GameObject gameObject;
    }

    public class BonusCross
    {
        public GameObject bonus;
        public EnvironmentManager.Road cross;
    }

    public const int Void = 0;
    public const int Briefing = 1;
    public const int Searching = 2;
    public const int Found = 3;
    public const int EndLevel = 4;
    public const int BriefingLearning = 5;
    public const int SearchingLearning = 6;
    public const int FoundLearning = 7;
    public const int EndLearning = 8;
    public const int ObstBriefing = 9;
    public const int ObstPlaying = 10;
    public const int ObstEnd = 11;
    #endregion

    #region Public Static Fields
    //public static int LevelCount = 3;
    public static bool HasDoneTutorial = false;
    #endregion

    #region Public Members
    public SignalSender progressSignals;
    public GameObject characterMale;
    public GameObject characterFemale;
    public GameObject characterOVR;
    public List<Target> targetList = new List<Target>();
    public List<int> activeTargetIndexes = new List<int>();
    public List<int> targetNumByLevel = new List<int>();
    public Target tutorialTarget = null;
    public List<bool> showOnMapByLevel = new List<bool>();
    public List<bool> showMapByLevel = new List<bool>();
    public bool learningPhase = true;
    public bool mapOnLearning = true;
    public bool targetOnLearning = true;
    public bool learningPhaseDone = false;
    public List<GameObject> bonusSourceList;
    public Transform bonusParent;


    public Dictionary<string, string> debugProperties = new Dictionary<string, string>();
    #endregion

    #region Public Get/Set
    public float GameplayTime
    {
        get
        {
            return gameplayTime;
        }
    }

    public int Level {
        get
        {
            if (level == 0 && HasDoneTutorial)
                level = 1;
            return level;
        }
        set { level = value; }
    }

    public GameObject Character
    {
        get { return character; }
    }

    public int Score
    {
        get { return score; }
        set { score = value; }
    }

    public int BonusCount
    {
        get { return bonusCounter; }
        set { bonusCounter = value; }
    }

    public int TotalBonusTaken
    {
        get { return totalBonusTaken; }
        set { totalBonusTaken = value; }
    }

    public Target CurrentTarget
    { get { return currentTarget; } }

    public CharacterGender Gender
    { get { return gender; } }

    public float ActualDistance
    {
        get
        {
            return actualDistance;
        }
        set
        {
            actualDistance = value;
        }
    }

    public bool LearningPhaseDone
    {
        get
        {
            return learningPhaseDone || !learningPhase;
        }
        set
        {
            learningPhaseDone = value;
        }
    }

    public GameplayType Gameplay
    {
        get
        {
            return gameplayType;
        }
        set
        {
            gameplayType = value;
        }
    }
    
    public int PedestrianHitCounterForTarget
    {
        get { return pedestrianHitCounterForTarget; }
        set { pedestrianHitCounterForTarget = value; }
    }


    public int PedestrianHitCounter
    {
        get { return pedestrianHitCounter; }
        set { pedestrianHitCounter = value; }
    }

    public int TotalPedestrianHitCounter
    {
        get { return totalPedestrianHitCounter; }
        set { totalPedestrianHitCounter = value; }
    }

    public int ObstaclePassedCounterForTarget
    {
        get
        {
            return obstaclePassedCounterForTarget;
        }
        set
        {
            obstaclePassedCounterForTarget = value;
        }
    }

    public int ObstaclePassedCounter
    {
        get
        {
            return obstaclePassedCounter;
        }
        set
        {
            obstaclePassedCounter = value;
        }
    }

    public int ObstacleHitCounterForTarget
    {
        get
        {
            return obstacleHitCounterForTarget;
        }
        set
        {
            obstacleHitCounterForTarget = value;
        }
    }

    public int ObstacleHitCounter
    {
        get
        {
            return obstacleHitCounter;
        }
        set
        {
            obstacleHitCounter = value;
        }
    }

    public int TotalObstaclePassedCounter
    {
        get
        {
            return totalObstaclePassedCounter;
        }
        set
        {
            totalObstaclePassedCounter = value;
        }
    }

    public int TotalObstacleHitCounter
    {
        get
        {
            return totalObstacleHitCounter;
        }
        set
        {
            totalObstacleHitCounter = value;
        }
    }

    public float TotalElapsedTime
    {
        get
        {
            return totalElapsedTime;
        }
        set
        {
            totalElapsedTime = value;
        }
    }

    public float TotalLevelElapsedTime
    {
        get
        {
            return totalLevelElapsedTime;
        }
    }

    public float LapElapsedTime
    {
        get
        {
            return lapElapsedTime;
        }
    }

    public List<Target> TargetList
    {
        get { return targetList; }
    }

    public Stack<Target> TargetStack
    {
        get { return targetStack; }
    }

    public Stack<int> TargetIndexStack
    {
        get { return targetIndexStack; }
        set { targetIndexStack = value; }
    }

    public Stack<int> GateIndexStack
    {
        get { return gateIndexStack; }
        set { gateIndexStack = value; }
    }

    #endregion

    #region Protected Members
    protected int score = 0;
    protected int bonusCounter = 0;
    protected int totalBonusTaken = 0;
    protected int currentTargetScore = 10000;
    protected int level = 1;
    protected int currentParsedLevel = 1;
    protected bool currentShowTarget = false;
    protected int levelCount = 0;
    protected string configText = string.Empty;
    protected CharacterGender gender = CharacterGender.Male;
    protected CQ_Interface interfaces;
    protected GameObject cameraObj;
    protected GameObject character;
    protected Stack<Target> targetStack = new Stack<Target>();
    protected Stack<int> targetIndexStack = new Stack<int>();
    protected Stack<int> gateStack = new Stack<int>();
    protected Stack<int> gateIndexStack = new Stack<int>();
    protected Target currentTarget = null;
    protected FiniteStateMachine fsm;
    protected Preloader preloader;
	protected float characterStrafeFactor = 1.0f;
	public float characterSpeedFactor;
	public float characterRotationSpeedFactor;
    protected float characterAlpha = 1.0f;
    protected BalanceBoardManager bbManager = null;
    protected EnvironmentManager envManager = null;
    protected float manhattanTarget = 0;
    protected float actualDistance = 0;
    public GameplayType gameplayType = GameplayType.Navigation;

    protected int targetCounter = 0;
    protected List<float> startGameplayTime = new List<float>(); //120.0f;
    protected float gameplayTime = 0.0f;

    //protected float targetTimer = -1.0f;
    protected int pedestrianHitCounter = 0;
    protected int totalPedestrianHitCounter = 0;
    protected int obstaclePassedCounter = 0;
    protected int obstacleHitCounter = 0;
    protected int totalObstaclePassedCounter = 0;
    protected int totalObstacleHitCounter = 0;
    protected int obstacleHitCounterForTarget = 0;
    protected int obstaclePassedCounterForTarget = 0;
    protected int pedestrianHitCounterForTarget = 0;

    protected Token currentToken = null;
    protected Dictionary<Token, BonusCross> token2bonusCross;

    protected float totalTimer = -1.0f;
    protected float lapTimer = -1.0f;
    
    protected float totalElapsedTime = 0.0f;
    protected float totalLevelElapsedTime = 0.0f;
    protected float lapElapsedTime = 0.0f;
    #endregion

    #region Unity Callback
    void Start()
    {
        fsm = GetComponent<FiniteStateMachine>();
        cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        interfaces = GameObject.FindGameObjectWithTag("Interface").GetComponent<CQ_Interface>();
        preloader = GameObject.FindGameObjectWithTag("Preloader").GetComponent<Preloader>();
        bbManager = GameObject.FindGameObjectWithTag("CoreManagers").GetComponent<BalanceBoardManager>();
        envManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<EnvironmentManager>();

        gameplayTime = 120.0f;// startGameplayTime[level];
    }
    #endregion

    #region Messages
    void BonusCollected(int bonusScore)
    {
        Score += bonusScore;
        bonusCounter++;
        totalBonusTaken++;
        interfaces.UpdateBonusInterface();

        interfaces.SendMessage("InitFlyier", bonusScore.ToString());

        TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "gem_collected", bonusScore.ToString());
    }

    void ObstacleAvoided(int bonusScore)
    {
        Score += bonusScore;
        interfaces.SendMessage("InitFlyier", bonusScore.ToString());
        interfaces.SendMessage("UpdateScore", score);
    }

    void CurrentToken(Token token)
    {
        currentToken = token;
    }

    void EnterOnCross()
    {
        Debug.Log("EnterOnCross");
        if (null == currentToken)
            return;

        if (Gameplay == GameplayType.ObstacleAvoidance)
        {
            BonusCross bc = token2bonusCross[currentToken];
            bc.bonus.SendMessage("ColorCross", bc.cross.gameObject);
            bc.bonus.SendMessage("Collect");
        }
    }

    void SetCharacterGender(CharacterGender g)
    {
        gender = g;
    }

    void CharacterCollided()
    {
        interfaces.SendMessage("InitFlyier", "-2");
        if (score > 0)
        {
            score = Mathf.Max(0, score - 2);
            interfaces.SendMessage("UpdateScore", score);
        }
        /*interfaces.SendMessage("InitFlyier", "-1000");
        if (currentTargetScore > 2000)
            currentTargetScore -= 1000;*/
    }

    void LevelCompleted()
    {
        fsm.State = EndLevel;
    }

    void LoadConfiguration(string configText)
    {
        this.StartCoroutine(this.LoadConfigXml(configText));
    }

    void LoadGeneralConf(string configText)
    {
        this.StartCoroutine(this.LoadGenConfigXml(configText));
    }

    void OnEnvironmentReady(int level)
    {
        this.level = level;

        interfaces.SendMessage("LoadNextInterfaceState");

        if (gameplayType == GameplayType.Navigation)
        {
            if (level > 0)
            {
                if (learningPhase && !learningPhaseDone)
                {
                    //PrepareTargetList(true);
                    StartCoroutine(StartLearningPhase());
                }
                else
                {
                    //PrepareTargetList(false);
                    StartCoroutine(StartGameplay());
                }
            }
        }
        else if (gameplayType == GameplayType.ObstacleAvoidance)
        {
            if (level > 0)
            {
                InitializeBonuses();
                StartCoroutine(StartObstacleAvoidance());
            }

            gameplayTime = startGameplayTime[level - 1];
        }
        //gameplayTime = 20.0f;

        totalTimer = -1.0f;
        lapTimer = -1.0f;
    }

    void InitializeBonuses()
    {
        token2bonusCross = new Dictionary<Token, BonusCross>();

        for (int i = 0; i < envManager.Crosses.Count; i++)
        {
            int rndIndex = Random.Range(0, 100000) % bonusSourceList.Count;
            Vector3 position = envManager.Crosses[i].position + Vector3.up * 1.0f;
            GameObject bonus = GameObject.Instantiate(bonusSourceList[rndIndex], position, Quaternion.identity) as GameObject;
            bonus.transform.parent = bonusParent;

            BonusCross bc = new BonusCross();
            bc.bonus = bonus;
            bc.cross = envManager.Crosses[i];
            token2bonusCross.Add(envManager.Crosses[i].token, bc);
        }
    }

    void RemoveBonuses()
    {
        for (int i = 0; i < bonusParent.childCount; i++)
        {
            GameObject bonus = bonusParent.transform.GetChild(i).gameObject;
            Destroy(bonus);
        }
    }

    void ResetNewSession()
    {
        activeTargetIndexes.Clear();
    }

    void ResetConfiguration()
    {
        targetList.Clear();
        RemoveBonuses();
        fsm.State = Void;
        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnGameplayReset));
    }
    #endregion

    #region Coroutines
    IEnumerator LoadConfigXml(string configText)
    {
        try
        {
            showOnMapByLevel.Clear();
            targetNumByLevel.Clear();
            showMapByLevel.Clear();
            levelCount = 0;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(configText);

            XmlNodeList levelNodeList = xmlDoc.GetElementsByTagName("level");
            for (int i = 0; i < levelNodeList.Count; i++)
            {
                levelCount++;
                if (levelNodeList[i].Attributes["showtargetonmap"] != null)
                    showOnMapByLevel.Add(levelNodeList[i].Attributes["showtargetonmap"].Value.ToLower().Equals("true"));
                else
                    showOnMapByLevel.Add(true);

                if (levelNodeList[i].Attributes["showmap"] != null)
                    showMapByLevel.Add(levelNodeList[i].Attributes["showmap"].Value.ToLower().Equals("true"));
                else
                    showMapByLevel.Add(true);
            }

            XmlNodeList obstTimers = xmlDoc.GetElementsByTagName("obstacletimer");
            startGameplayTime.Clear();
            for (int i = 0; i < obstTimers.Count; i++)
            {
                if (obstTimers[i].Attributes["seconds"] != null)
                {
                    Debug.Log("---> " + float.Parse(obstTimers[i].Attributes["seconds"].Value));
                    startGameplayTime.Add(float.Parse(obstTimers[i].Attributes["seconds"].Value));
                }
                else
                {
                    Debug.Log("---> DEFAULT");
                    startGameplayTime.Add(100.0f);
                }
            }

            XmlNodeList gameplayNodeList = xmlDoc.GetElementsByTagName("gameplay");
            XmlNodeList children = gameplayNodeList[0].ChildNodes;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].Name.Equals("targets"))
                    targetNumByLevel.Add(int.Parse(children[i].Attributes["number"].Value));
            }

            XmlNodeList learningNodeList = xmlDoc.GetElementsByTagName("learningphase");
            for (int i = 0; i < learningNodeList.Count; i++)
            {
                if (null != learningNodeList[i].Attributes["enabled"])
                    learningPhase = learningNodeList[i].Attributes["enabled"].Value.ToLower().Equals("true");

                if (null != learningNodeList[i].Attributes["showtargetonmap"])
                    targetOnLearning = learningNodeList[i].Attributes["showtargetonmap"].Value.ToLower().Equals("true");

                if (null != learningNodeList[i].Attributes["showmap"])
                    mapOnLearning = learningNodeList[i].Attributes["showmap"].Value.ToLower().Equals("true");
            }

            progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnGameplayXmlParsed));
        }
        catch (XmlException e)
        {
            progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnXmlError, e.Message));
        }

        yield return new WaitForEndOfFrame();
        
        /*XMLReader reader = new XMLReader();
        XMLNode rootNode = reader.read(configText).children[0] as XMLNode;

        foreach (XMLNode xmlNode in rootNode.children)
        {
            switch (xmlNode.tagName)
            {
                case ("gameplay"):
                    ParseGameplay(xmlNode);
                    break;
                case ("level"):
                    levelCount++;
                    currentParsedLevel = xmlNode.GetAttributeAsInt("id");
                    showOnMapByLevel.Add(xmlNode.GetAttributeAsString("showtargetonmap").ToLower().Equals("true"));
                    break;
            }
        }

        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnGameplayXmlParsed));
        yield return new WaitForEndOfFrame();*/
    }

    IEnumerator LoadGenConfigXml(string genConfigText)
    {
        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(genConfigText);

            XmlNode root = xmlDoc.FirstChild;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode child = root.ChildNodes[i];
                switch (child.Name)
                {
                    case ("character"):
                        ParseCharacter(child);
                        break;
                }
            }
            progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnGameplayGeneralXmlParsed));
        }
        catch (XmlException e)
        {
            progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnXmlError, e.Message));
        }
        yield return new WaitForEndOfFrame();

        /*XMLReader reader = new XMLReader();
        XMLNode rootNode = reader.read(genConfigText).children[0] as XMLNode;

        foreach (XMLNode xmlNode in rootNode.children)
        {
            switch (xmlNode.tagName)
            {
                case ("character"):
                    ParseCharacter(xmlNode);
                    break;
                case ("debug"):
                    ParseDebug(xmlNode);
                    break;
            }
        }

        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnGameplayGeneralXmlParsed));

        yield return new WaitForEndOfFrame();*/
    }

    IEnumerator StartGameplay()
    {
        targetCounter = 0;
        TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "start", "level_" + level);
        yield return new WaitForSeconds(2.5f);
        fsm.State = Briefing;
    }

    IEnumerator StartLearningPhase()
    {
        targetCounter = 0;
        TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "start", "learning_phase");
        yield return new WaitForSeconds(2.5f);
        fsm.State = BriefingLearning;
    }

    IEnumerator StartObstacleAvoidance()
    {
        targetCounter = 0;
        TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "start", "level_" + level);
        yield return new WaitForSeconds(2.5f);
        fsm.State = ObstBriefing;
    }
    #endregion

    #region Parsing Functions
    protected void ParseDebug(XMLNode node)
    {
        foreach (XMLNode child in node.children)
        {
            switch (child.tagName)
            {
                case ("targetCheckOnMap"):
                    debugProperties.Add("targetCheckOnMap", child.GetAttributeAsString("active").ToLower());
                    break;
            }
        }
    }

    protected void ParseLevel(XMLNode node)
    {
        foreach (XMLNode child in node.children)
        {
            switch (child.tagName)
            {
                case ("gameplay"):
                    ParseGameplay(child);
                    break;
            }
        }
    }

    protected void ParseGameplay(XMLNode node)
    {
        foreach (XMLNode child in node.children)
        {
            if (child.tagName.Equals("targets"))
            {
                targetNumByLevel.Add(child.GetAttributeAsInt("number"));
            }
        }
    }

    protected void ParseCharacter(XmlNode node)
    {
        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode child = node.ChildNodes[i];
            switch (child.Name)
            {
                case ("strafespeedfactor"):
                    characterStrafeFactor = float.Parse(child.Attributes["value"].Value);
                    break;
                case ("speedfactor"):
                    characterSpeedFactor = float.Parse(child.Attributes["value"].Value);
                    break;
                case ("alpha"):
                    characterAlpha = float.Parse(child.Attributes["value"].Value);
                    break;
            }
        }
    }

    protected void ParseCharacter(XMLNode node)
    {
        foreach (XMLNode child in node.children)
        {
            switch (child.tagName)
            {
                case ("strafespeedfactor"):
                    characterStrafeFactor = child.GetAttributeAsFloat("value");
                    break;
                case ("speedfactor"):
                    characterSpeedFactor = child.GetAttributeAsFloat("value");
                    break;
                case ("alpha"):
                    characterAlpha = child.GetAttributeAsFloat("value");
                    break;
            }
        }
    }
    #endregion

    #region Public Members
    public int GetLevelCount()
    {
        return levelCount;
    }
    public void SetLevelCount(int value)
    {
        levelCount = value;
    }

    public bool GetShowMap()
    {
        if (level > 0 && showMapByLevel.Count >= level)
        {
            if (learningPhase && !learningPhaseDone)
                return mapOnLearning;

            return showMapByLevel[level - 1];
        }

        return false;
    }

    public bool GetShowTarget()
    {
        if (level > 0 && showOnMapByLevel.Count >= level)
        {
            if (learningPhase && !learningPhaseDone)
                return mapOnLearning && targetOnLearning;

            return showMapByLevel[level - 1] && showOnMapByLevel[level - 1];
        }

        return false;
    }

    public int GetTargetByLevel(int level)
    {
        if (level > 0)
            return targetNumByLevel[0];

        return 0;
    }

    public void InitializeCharacter(int tutorialPhase)
    {
        if (preloader.useOVR)
        {
            character = characterOVR;
            characterOVR.SetActiveRecursively(true);
            characterFemale.SetActiveRecursively(false);
            characterMale.SetActiveRecursively(false);
        }
        else
        {
            if (gender == CharacterGender.Male)
            {
                character = characterMale;
                characterMale.SetActiveRecursively(true);
                characterFemale.SetActiveRecursively(false);
                if (null != characterOVR)
                    characterOVR.SetActiveRecursively(false);
                //GameObject.Destroy(characterFemale);
            }
            else
            {
                character = characterFemale;
                characterFemale.SetActiveRecursively(true);
                characterMale.SetActiveRecursively(false);
                if (null != characterOVR)
                    characterOVR.SetActiveRecursively(false);
                //GameObject.Destroy(characterMale);
            }
        }

		character.GetComponent<PlayerKinematicBehaviour>().Init(characterSpeedFactor, characterStrafeFactor, characterRotationSpeedFactor, characterAlpha);
        if (learningPhase && !learningPhaseDone)
            character.GetComponent<PlayerKinematicBehaviour>().PlaceCharacter(tutorialPhase, 0);
        else
            character.GetComponent<PlayerKinematicBehaviour>().PlaceCharacter(tutorialPhase, -1);
        character.GetComponent<PlayerKinematicBehaviour>().VerticalSpeed = 0.0f;

        LevelRoot.Instance.BroadcastMessage("SetInputEnabled", false);
    }
    #endregion

    #region Protected Members
    public void FillTargetStacks()
    {
        targetStack.Clear();
        int totalTarget = GetTargetByLevel(level);

        string levelindexes = "";
        for (int i = 0; i < levelCount; i++)
        {   
            levelindexes = "level: " + i + " indexes: ";
            for (int c = 0; c < totalTarget; c++)
                levelindexes += " " + DataContainer.Instance.levelsTargets[i, c];
            
            Debug.Log("§§§ " + levelindexes);
        }

            //Debug.Log("######## targetList.Count = " + targetList.Count + "  totalTarget : " + totalTarget + " level: " + level); 
        for (int i = 0; i < totalTarget; i++)
        {
            //Debug.Log("DataContainer.Instance.levelsTargets[level - 1,"+i+"] = " + DataContainer.Instance.levelsTargets[level - 1, i]);
            int index = 0;
            for (int c = 0; c < targetList.Count; c++)
            {
                if ((int)targetList[c].type == DataContainer.Instance.levelsTargets[level - 1, i])
                    index = c;
            }

            targetStack.Push(targetList[index]);
        }

    }

    protected void PrepareTargetList(bool isLearning)
    {
        targetStack.Clear();
        gateStack.Clear();

        List<int> targetIndexes = new List<int>();
        for (int i = 0; i < targetList.Count; i++)
            targetIndexes.Add(i);

        int totalTarget = Mathf.Min(targetIndexes.Count, GetTargetByLevel(level));

        if (activeTargetIndexes.Count == 0)
        {
            string dbg = "activeTargetIndexes: ";
            for (int i = 0; i < totalTarget; i++)
            {
                int index = Random.Range(0, 10000) % targetIndexes.Count;
                activeTargetIndexes.Add(targetIndexes[index]);
                dbg += targetIndexes[index] + ", ";
                targetIndexes.RemoveAt(index);
            }
            //Debug.LogWarning(dbg);
        }

        targetIndexes.Clear();
        for (int i = 0; i < totalTarget; i++)
            targetIndexes.Add(activeTargetIndexes[i]);

        for (int i = 0; i < totalTarget; i++)
        {
            int index = Random.Range(0, 10000) % targetIndexes.Count;
            targetStack.Push(targetList[targetIndexes[index]]);
            TrackingManager.Instance.AddTargetPos(targetList[targetIndexes[index]].gameObject.transform.position);
            targetIndexes.RemoveAt(index);
        }

        if (isLearning)
        {
            //for (int i = 0; i < 3; i++)
            //    targetStack.Push(targetList[lastIndex]);

            for (int i = 0; i < targetStack.Count; i++)
            {
                int gate = i % 4; // Random.Range(0, 100000) % 4;
                //if (targetStack.Count - i <= 4)
                //    gate = targetStack.Count - i - 1;
                gateStack.Push(gate);
            }

            gateStack.Pop();
        }

        /*
        int[] gates = gateStack.ToArray();
        string gStr = "";
        for (int i = 0; i < gates.Length; i++)
            gStr += gates[i] + ", ";

        Target[] targets = targetStack.ToArray();
        string tStr = "";
        for (int i = 0; i < targets.Length; i++)
            tStr += targets[i].type + ", ";

        Debug.LogWarning("GATES: " + gStr);
        Debug.LogWarning("TARGETS: " + tStr);
        */

        interfaces.SendMessage("UpdateTotalTarget", totalTarget);
    }

    public void PrepareTargetGate(bool isLearning)
    {
        //Debug.Log("################################################################# targetList.Count = " + targetList.Count);
        //targetStack.Clear();
        targetIndexStack.Clear();
        gateStack.Clear();
        gateIndexStack.Clear();

        List<int> targetIndexes = new List<int>();
        for (int i = 0; i < targetList.Count; i++)
            targetIndexes.Add(i);

        int totalTarget = Mathf.Min(targetIndexes.Count, GetTargetByLevel(level));

        //if (activeTargetIndexes.Count == 0)
        //{
        //    for (int i = 0; i < totalTarget; i++)
        //    {
        //        int index = Random.Range(0, 10000) % targetIndexes.Count;
        //        activeTargetIndexes.Add(targetIndexes[index]);
        //        targetIndexes.RemoveAt(index);
        //    }
        //}

        //targetIndexes.Clear();
        //for (int i = 0; i < totalTarget; i++)
        //    targetIndexes.Add(activeTargetIndexes[i]);

        //List<int> currentTargetIndexes;
		List<int> selectedTargetIndexes = new List<int>();
		List<int> currentTargetIndexes = new List<int>();
		for (int c = 0; c < targetIndexes.Count; c++)
			currentTargetIndexes.Add(targetIndexes[c]);

		for (int i = 0; i < totalTarget; i++)
		{
			int index = Random.Range(0, 10000) % currentTargetIndexes.Count;
			selectedTargetIndexes.Add((int)targetList[currentTargetIndexes[index]].type);
			currentTargetIndexes.RemoveAt(index);
		}

        for (int l = 0; l < levelCount; l++)
        {
            string dbg = "LEVEL " + (l+1) + " - ";
            currentTargetIndexes = new List<int>();
			for (int c = 0; c < selectedTargetIndexes.Count; c++)
				currentTargetIndexes.Add(selectedTargetIndexes[c]);
            
            for (int i = 0; i < totalTarget; i++)
            {
                int index = Random.Range(0, 10000) % currentTargetIndexes.Count;
                targetIndexStack.Push(currentTargetIndexes[index]);
                dbg += currentTargetIndexes[index] + ", ";
                for (int k = 0; k < targetList.Count; k++)
                {
                    if (currentTargetIndexes[index] == (int)targetList[k].type)
                    {
                        TrackingManager.Instance.AddTargetPos(targetList[k].gameObject.transform.position);
                        break;
                    }
                }
                currentTargetIndexes.RemoveAt(index);
            }
            currentTargetIndexes.Clear();
            //Debug.Log(dbg);
        }

        if (isLearning)
        {
            //for (int i = 0; i < 3; i++)
            //    targetStack.Push(targetList[lastIndex]);

            for (int i = 0; i < targetStack.Count; i++)
            {
                int gate = i % 4; // Random.Range(0, 100000) % 4;
                //if (targetStack.Count - i <= 4)
                //    gate = targetStack.Count - i - 1;
                gateStack.Push(gate);
                gateIndexStack.Push(gate);
            }

            //gateStack.Pop();
        }
        //interfaces.SendMessage("UpdateTotalTarget", totalTarget);
    }
    #endregion

    #region FSM States
    void OnVoidStateEnter() { }
    void OnVoidStateExec() { }
    void OnVoidStateExit() { }

    protected float briefingTimer = -1.0f;
    void OnBriefingStateEnter()
    {
        if (targetStack.Count > 0)
        {
            currentTarget = targetStack.Pop();

            Debug.Log("CERCA IL TARGET: " + currentTarget.type.ToString() + ", count: " + targetStack.Count + ", currentTarget.gameObject: " + currentTarget.gameObject);

            if (currentTarget.gameObject != null)
            {
                Vector3 targetPos = currentTarget.gameObject.transform.position;
                interfaces.ShowTargetOnMap(targetPos);
                interfaces.ViewInstructions("Find the", currentTarget.type.ToString(), false, false);
                interfaces.SendMessage("UpdateTargetSigns", currentTarget.type.ToString());
                briefingTimer = TimeManager.Instance.MasterSource.TotalTime;

                Vector3 playerPos = character.transform.position;
                manhattanTarget = Mathf.Abs(targetPos.x - playerPos.x) + Mathf.Abs(targetPos.z - playerPos.z);
                actualDistance = 0;
                LevelRoot.Instance.BroadcastMessage("SetInputEnabled", true);

                if (totalTimer < 0)
                    totalTimer = TimeManager.Instance.MasterSource.TotalTime;

                lapTimer = TimeManager.Instance.MasterSource.TotalTime;

                TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, currentTarget.type.ToString(), character.transform.position, "search", "target_" + (++targetCounter));
            }
        }
        else
        {
            Debug.LogWarning("EMPTY TARGET STACK");
        }
    }
    void OnBriefingStateExec()
    {
        float currentTime = TimeManager.Instance.MasterSource.TotalTime;

        if (briefingTimer > 0 && currentTime - briefingTimer > 4.0f)
        {
            briefingTimer = -1.0f;
            fsm.State = Searching;
        }
    }
    void OnBriefingStateExit()
    {
        interfaces.HideInstructions();
        interfaces.SendMessage("SetTargetSignsVisibility", true);
    }

    protected float stayTimer = -1.0f;
    void OnSearchingStateEnter()
    {
        Debug.Log("STO CERCANDO IL TARGET: " + currentTarget.type);
    }
    void OnSearchingStateExec()
    {
        float currentTime = TimeManager.Instance.MasterSource.TotalTime;

        if (character.GetComponent<FiniteStateMachine>().State == PlayerKinematicBehaviour.Stay)
        {
            if (stayTimer < 0.0f)
                stayTimer = currentTime;
        }
        else
        {
            stayTimer = -1.0f;
        }

        float elapsedTime = currentTime - stayTimer;
        if (stayTimer > 0.0f && elapsedTime > 1.0f)
        {
            Transform playerTr = character.transform;
            if (currentTarget.gameObject != null)
            {
                Transform targetTr = currentTarget.gameObject.transform;
                if ((playerTr.position - targetTr.position).magnitude < 12.0f)
                {
                    Vector3 localPlayerPos = targetTr.InverseTransformPoint(playerTr.position);
                    bool proximityCheck = (localPlayerPos.x > -1.0f) && (localPlayerPos.x < 6.0f);
                    if (proximityCheck)
                    {
                        fsm.State = Found;
                    }
                }
            }
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K))
            fsm.State = EndLevel;
#endif
    }
    void OnSearchingStateExit()
    {
    }

    protected float foundTimer = -1.0f;
    void OnFoundStateEnter()
    {
        lapElapsedTime = TimeManager.Instance.MasterSource.TotalTime - lapTimer;
        totalElapsedTime += lapElapsedTime;
        totalLevelElapsedTime += lapElapsedTime;

        Debug.Log("TROVATO IL TARGET: " + currentTarget.type + " lapElapsedTime: " + lapElapsedTime);
        foundTimer = TimeManager.Instance.MasterSource.TotalTime;
        int starNum = 3;
        actualDistance = Mathf.Max(actualDistance, manhattanTarget);
        float diffFactor = actualDistance / manhattanTarget;
        if (diffFactor > 2.2f)
            starNum = 1;
        else if (diffFactor > 1.3f)
            starNum = 2;
        Debug.Log("Actual: " + actualDistance + ", manhattan: " + manhattanTarget);
        interfaces.ViewInstructions("Well done!", "empty", true, true, starNum);

        interfaces.SendMessage("UpdateTargets", 1);
        interfaces.SendMessage("SetTargetSignsVisibility", false);
        score += 20;
        interfaces.SendMessage("UpdateScore", score);

        //float elapsedTime = TimeManager.Instance.MasterSource.TotalTime - targetTimer;
       
        if (level > 0 && LearningPhaseDone)
        {
            interfaces.SendMessage("InitFlyier", "20");
            TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, currentTarget.type.ToString(), character.transform.position, "found", "target_" + targetCounter);
            TrackingManager.Instance.LogSummaryRow(currentTarget.type.ToString(), actualDistance, manhattanTarget, lapElapsedTime, starNum, obstaclePassedCounterForTarget, obstacleHitCounterForTarget, pedestrianHitCounterForTarget, level);
        }
    }
    void OnFoundStateExec()
    {
        float currentTime = TimeManager.Instance.MasterSource.TotalTime;

        if (foundTimer > 0 && currentTime - foundTimer > 4.0f)
        {
            foundTimer = -1.0f;

            if (targetStack.Count > 0)
                fsm.State = Briefing;
            else
                fsm.State = EndLevel;
        }
    }
    void OnFoundStateExit()
    {
        obstacleHitCounterForTarget = 0;
        obstaclePassedCounterForTarget = 0;
        pedestrianHitCounterForTarget = 0;
        //obstacleHitCounter = 0;
        //obstaclePassedCounter = 0;
        //pedestrianHitCounter = 0;

        interfaces.HideInstructions();
        currentTargetScore = 10000;
    }

    void OnEndLevelStateEnter()
    {
        TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, "", character.transform.position, "end", "level_" + level);
        LevelRoot.Instance.BroadcastMessage("SetInputEnabled", false);
        LevelRoot.Instance.BroadcastMessage("LevelFinished", false);
        Debug.Log("FINITO LIVELLO!");
        string message = "Level " + level + " Completed!";

        if (level != 0)
            interfaces.ShowResultsPopup(message, 0);

        StartCoroutine(FinalSequence());
        level++;
    }
    void OnEndLevelStateExec()
    {
    }
    void OnEndLevelStateExit()
    {
        totalLevelElapsedTime = 0.0f;
        interfaces.HideInstructions();

        obstaclePassedCounter = 0;
        obstacleHitCounter = 0;
        pedestrianHitCounter = 0;
    }

    IEnumerator FinalSequence()
    {
        float t = 4.0f;
        if (preloader.useOVR)
            t = 10.0f;
        yield return new WaitForSeconds(t);
        interfaces.StartFadeOut(0.5f,null);
        yield return new WaitForSeconds(1.0f);
        preloader.UnloadLevel();
    }

    void OnBriefingLearningEnter()
    {
        if (targetStack.Count > 0)
        {

            //character.GetComponent<PlayerKinematicBehaviour>().PlaceCharacter(0, gateStack.Pop());
            currentTarget = targetStack.Pop();

            Debug.Log("LEARNING... CERCA IL TARGET: " + currentTarget.type.ToString() + ", count: " + targetStack.Count);

            Vector3 targetPos = currentTarget.gameObject.transform.position;
            
            interfaces.ShowTargetOnMap(targetPos);

            interfaces.ViewInstructions("Find the", currentTarget.type.ToString(), false, false);
            interfaces.SendMessage("UpdateTargetSigns", currentTarget.type.ToString());
            briefingTimer = TimeManager.Instance.MasterSource.TotalTime;
            lapTimer = TimeManager.Instance.MasterSource.TotalTime;

            Vector3 playerPos = character.transform.position;
            manhattanTarget = Mathf.Abs(targetPos.x - playerPos.x) + Mathf.Abs(targetPos.z - playerPos.z);
            actualDistance = 0;
            LevelRoot.Instance.BroadcastMessage("SetInputEnabled", true);

            TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, currentTarget.type.ToString(), character.transform.position, "search", "target_" + (++targetCounter));
        }
        else
        {
            Debug.LogWarning("EMPTY TARGET STACK");
        }
    }
    void OnBriefingLearningExec()
    {
        float currentTime = TimeManager.Instance.MasterSource.TotalTime;

        if (briefingTimer > 0 && currentTime - briefingTimer > 4.0f)
        {
            briefingTimer = -1.0f;
            fsm.State = SearchingLearning;
        }
    }
    void OnBriefingLearningExit()
    {
        interfaces.HideInstructions();
        interfaces.SendMessage("SetTargetSignsVisibility", true);
    }

    //protected float stayTimer = -1.0f;
    void OnSearchingLearningEnter()
    {
        Debug.Log("STO CERCANDO IL TARGET: " + currentTarget.type);
    }
    void OnSearchingLearningExec()
    {
        float currentTime = TimeManager.Instance.MasterSource.TotalTime;

        if (character.GetComponent<FiniteStateMachine>().State == PlayerKinematicBehaviour.Stay)
        {
            if (stayTimer < 0.0f)
                stayTimer = currentTime;
        }
        else
        {
            stayTimer = -1.0f;
        }

        float elapsedTime = currentTime - stayTimer;
        if (stayTimer > 0.0f && elapsedTime > 1.0f)
        {
            Transform playerTr = character.transform;
            Transform targetTr = currentTarget.gameObject.transform;
            if ((playerTr.position - targetTr.position).magnitude < 12.0f)
            {
                Vector3 localPlayerPos = targetTr.InverseTransformPoint(playerTr.position);
                bool proximityCheck = (localPlayerPos.x > 0.0f) && (localPlayerPos.x < 5.0f);
                if (proximityCheck)
                {
                    fsm.State = FoundLearning;
                }
            }

        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K))
            fsm.State = EndLearning;
#endif
    }
    void OnSearchingLearningExit()
    {
    }

    protected int learningGateId = 0;
    protected float fadeTimer = -1.0f;
    void OnFoundLearningEnter()
    {
        lapElapsedTime = TimeManager.Instance.MasterSource.TotalTime - lapTimer;
        totalElapsedTime += lapElapsedTime;
        totalLevelElapsedTime += lapElapsedTime;

        Debug.Log("TROVATO IL TARGET: " + currentTarget.type);
        foundTimer = TimeManager.Instance.MasterSource.TotalTime;
        int starNum = 3;
        float diffFactor = actualDistance / manhattanTarget;
        if (diffFactor > 2.2f)
            starNum = 1;
        else if (diffFactor > 1.3f)
            starNum = 2;
        Debug.Log("Actual: " + actualDistance + ", manhattan: " + manhattanTarget + " - diffFactor: " + diffFactor);
        interfaces.ViewInstructions("Well done!", "empty", true, true, starNum);
        //interfaces.ShowResultsPopup("Well done!", starNum);
        interfaces.SendMessage("UpdateTargets", 1);
        interfaces.SendMessage("SetTargetSignsVisibility", false);
        //score += currentTargetScore;
        //interfaces.SendMessage("UpdateScore", score);

        TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, currentTarget.type.ToString(), character.transform.position, "found", "target_" + targetCounter);
        TrackingManager.Instance.LogSummaryRow(currentTarget.type.ToString(), actualDistance, manhattanTarget, lapElapsedTime, starNum, obstaclePassedCounter, obstacleHitCounter, pedestrianHitCounter, -1);
    }
    void OnFoundLearningExec()
    {
        float currentTime = TimeManager.Instance.MasterSource.TotalTime;

        if (foundTimer > 0 && currentTime - foundTimer > 4.0f)
        {
            foundTimer = -1.0f;
            fadeTimer = currentTime;
            interfaces.StartFadeOut(0.5f, null);

            /*if (targetStack.Count > 0)
                fsm.State = BriefingLearning;
            else
                fsm.State = EndLearning;*/
        }

        if (fadeTimer > 0 && currentTime - fadeTimer > 0.6f)
        {
            fadeTimer = -1.0f;
            //fsm.State = EndLearning;
            //return;
            if (targetStack.Count > 0)
            {
                int currGate = ++learningGateId % 4;
                /*if (gateStack.Count > 0)
                    currGate = gateStack.Pop();
                else
                    Debug.LogWarning("NO GATE TO BE POPPED");*/

                character.GetComponent<PlayerKinematicBehaviour>().PlaceCharacter(0, currGate);
                fsm.State = BriefingLearning;
            }
            else
            {
                fsm.State = EndLearning;
            }
        }
    }
    void OnFoundLearningExit()
    {
        interfaces.HideInstructions();
        currentTargetScore = 10000;
        interfaces.StartFadeIn(0.5f, null);
    }

    void OnEndLearningEnter()
    {
        Debug.Log("FINITO LEARNING!");
        interfaces.ViewInstructions("Learning Completed!", "empty", true, true, 0);
        StartCoroutine(FinalSequence());
        learningPhaseDone = true;

        TrackingManager.Instance.LogEntry(TrackingManager.Command.Event, currentTarget.type.ToString(), character.transform.position, "end", "learning_phase");
    }
    void OnEndLearningExec()
    {
    }
    void OnEndLearningExit()
    {
        interfaces.HideInstructions();
    }

    //Obstacle Avoidance Gameplay
    void OnBriefingObstAvoidEnter()
    {
        Debug.Log("OnBriefingObstAvoidEnter");
        interfaces.ViewInstructions("Collect as many gems as possible within given time", "empty", false, false, 0);
        briefingTimer = TimeManager.Instance.MasterSource.TotalTime;
    }
    void OnBriefingObstAvoidExec()
    {
        float currentTime = TimeManager.Instance.MasterSource.TotalTime;
        
        float t = 4.0f;
        if (preloader.useOVR)
            t = 10.0f;

        if (briefingTimer > 0 && currentTime - briefingTimer > t)
        {
            briefingTimer = -1.0f;
            fsm.State = ObstPlaying;
        }
    }
    void OnBriefingObstAvoidExit()
    {
        Debug.Log("OnBriefingObstAvoidExit");
        interfaces.HideInstructions();
        LevelRoot.Instance.BroadcastMessage("SetInputEnabled", true);
    }

    void OnObstAvoidEnter()
    {
        gameplayTime = startGameplayTime[level-1];
        interfaces.SetGameTimeVisibility(false);
    }

	bool AllGemsCollected () {

		int totalNumberOfGems = envManager.Crosses.Count;

		if (totalNumberOfGems - bonusCounter == 0) {

			return true;
		}

		return false;
	}

    void OnObstAvoidExec()
    {
       // gameplayTime -= TimeManager.Instance.MasterSource.DeltaTime;

        //if (gameplayTime <= 0.0f)
		if (AllGemsCollected ()) {

			fsm.State = ObstEnd;
		}

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K))
            fsm.State = ObstEnd;
#endif
    }
    void OnObstAvoidExit()
    {
    }

    void OnEndObstAvoidEnter()
    {
        LevelRoot.Instance.BroadcastMessage("SetInputEnabled", false);
        Character.GetComponent<PlayerKinematicBehaviour>().KinematicState = 0;
        string message = "Level " + level + " Completed!";
        if (level == 0)
            message = "Tutorial Completed!";
        interfaces.ShowResultsPopup(message, 0);

        TrackingManager.Instance.LogSummaryRow(BonusCount, startGameplayTime[level - 1], obstaclePassedCounter, obstacleHitCounter, pedestrianHitCounter, level);

        StartCoroutine(FinalSequence());
        level++;
    }
    void OnEndObstAvoidExec()
    {
    }
    void OnEndObstAvoidExit()
    {
        obstaclePassedCounter = 0;
        obstacleHitCounter = 0;
        pedestrianHitCounter = 0;
        interfaces.HideInstructions();
    }
    #endregion

}
