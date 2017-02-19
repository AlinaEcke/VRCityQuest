using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using FoF.Utils;
using SBS.XML;
using System.Xml;
using System.Xml.Schema;

[AddComponentMenu("FoF/Preloader")]
public class Preloader : MonoBehaviour
{
    public GameObject coreManagers = null;
    public GameObject gameManagers = null;
    public bool useBalanceBoard;
    public bool useExternalXML;
    public bool useOVR;
    public GameplayManager.CharacterGender gender;

    protected bool xmlError = false;
    protected bool envXMLParsed = false;
    protected bool checkSessionFlag = false;
    protected bool environmentGenerated = false;
    protected bool environmentDestroyed = false;
    protected bool targetPlaced = false;
    protected bool bbConnected = false;
    protected bool obstaclesXMLParsed = false;
    protected bool obstaclesPlaced = false;
    protected bool obstaclesDestroyed = false;
    protected bool staticObjectsCreated = false;
    protected bool texturesXMLParsed = false;
    protected bool texturesLoaded = false;
    protected bool buildingAssetsLoaded = false;
    protected bool gameplayXMLParsed = false;
    protected bool gameplayGenXMLParsed = false;
    protected bool gameplayReset = false;
    protected bool firstFrame = true;
    protected bool soundXMLParsed = false;
    protected string version = "0.136";
    protected string xmlErrorMessage = string.Empty;
    protected CQ_Interface _interface;

    #region Unity Callbacks
    void Start()
    {
        GameObject.DontDestroyOnLoad(gameObject);
        GameObject.DontDestroyOnLoad(coreManagers.transform.parent != null ? coreManagers.transform.parent.gameObject : coreManagers.gameObject);
        GameObject.DontDestroyOnLoad(gameManagers.transform.parent != null ? gameManagers.transform.parent.gameObject : gameManagers.gameObject);

        _interface = GameObject.FindGameObjectWithTag("Interface").GetComponent<CQ_Interface>();

        EnvironmentManager envManager = gameManagers.GetComponent<EnvironmentManager>();
        envManager.progressSignals.Register(gameObject, "OnEnvManagerProgress", 0.0f);

        BalanceBoardManager bbManager = coreManagers.GetComponent<BalanceBoardManager>();
        bbManager.progressSignals.Register(gameObject, "OnBBManagerProgress", 0.0f);

        ObstaclesManager staticManager = gameManagers.GetComponent<ObstaclesManager>();
        staticManager.progressSignals.Register(gameObject, "OnObstacleManagerProgress", 0.0f);

        ExternalTexturesManager texturesManager = gameManagers.GetComponent<ExternalTexturesManager>();
        texturesManager.progressSignals.Register(gameObject, "OnExternalTexturesManagerProgress", 0.0f);

        BuildingAssetsManager assetsManager = gameManagers.GetComponent<BuildingAssetsManager>();
        assetsManager.progressSignals.Register(gameObject, "OnBuildingAssetsManagerProgress", 0.0f);

        GameplayManager gameplayManager = gameManagers.GetComponent<GameplayManager>();
        gameplayManager.progressSignals.Register(gameObject, "OnGameplayManagerProgress", 0.0f);

        GeneralSoundsManager soundManager = gameManagers.GetComponent<GeneralSoundsManager>();
        soundManager.progressSignals.Register(gameObject, "OnSoundManagerProgress", 0.0f);

        TrackingManager.Instance.Version = version;
    }

    void Update()
    {
        if (firstFrame)
        {
#if UNITY_EDITOR
            Caching.CleanCache();
#endif
            Application.targetFrameRate = 50;
            this.StartCoroutine(this.OnFirstFrame());
            firstFrame = false;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(800, 5, 100, 20), "Ver: " + version + ((Debug.isDebugBuild) ? "d" : ""));
    }

    void OnApplicationQuit()
    {
        TrackingManager.Instance.StopTracking();
    }
    #endregion

    #region Signal Callbacks
    void OnEnvManagerProgress(object param)
    {
        EnvironmentManager.ProgressSignal signal = (EnvironmentManager.ProgressSignal)param;
        switch (signal.type)
        {
            case EnvironmentManager.ProgressSignalType.OnEnvironmentXmlParsed:
                envXMLParsed = true;
                break;
            case EnvironmentManager.ProgressSignalType.OnXmlError:
                xmlError = true;
                break;
            case EnvironmentManager.ProgressSignalType.OnEnvironmentGenerated:
                environmentGenerated = true;
                break;
            case EnvironmentManager.ProgressSignalType.OnEnvironmentDestroyed:
                environmentDestroyed = true;
                break;
            case EnvironmentManager.ProgressSignalType.OnTargetPlaced:
                targetPlaced = true;
                break;
            default:
                break;
        }
    }

    void OnBBManagerProgress(object param)
    {
        BalanceBoardManager.ProgressSignal signal = (BalanceBoardManager.ProgressSignal)param;
        switch (signal.type)
        {
            case BalanceBoardManager.ProgressSignalType.OnBalanceBoardConnected:
                bbConnected = true;
                break;
            default:
                break;
        }
    }

    void OnObstacleManagerProgress(object param)
    {
        ObstaclesManager.ProgressSignal signal = (ObstaclesManager.ProgressSignal)param;
        switch (signal.type)
        {
            case ObstaclesManager.ProgressSignalType.OnObstacleXmlParsed:
                obstaclesXMLParsed = true;
                break;
            case ObstaclesManager.ProgressSignalType.OnXmlError:
                xmlError = true;
                break;
            case ObstaclesManager.ProgressSignalType.OnObstaclePlaced:
                obstaclesPlaced = true;
                break;
            case ObstaclesManager.ProgressSignalType.OnObstacleDestroyed:
                obstaclesDestroyed = true;
                break;
            default:
                break;
        }
    }

    void OnExternalTexturesManagerProgress(object param)
    {
        ExternalTexturesManager.ProgressSignal signal = (ExternalTexturesManager.ProgressSignal)param;
        switch (signal.type)
        {
            case ExternalTexturesManager.ProgressSignalType.OnTexturesXmlParsed:
                texturesXMLParsed = true;
                break;
            case ExternalTexturesManager.ProgressSignalType.OnXmlError:
                xmlError = true;
                break;
            case ExternalTexturesManager.ProgressSignalType.OnTexturesLoaded:
                texturesLoaded = true;
                break;
            default:
                break;
        }
    }

    void OnBuildingAssetsManagerProgress(object param)
    {
        BuildingAssetsManager.ProgressSignal signal = (BuildingAssetsManager.ProgressSignal)param;
        switch (signal.type)
        {
            case BuildingAssetsManager.ProgressSignalType.OnBuidingAssetsLoaded:
                buildingAssetsLoaded = true;
                break;
            default:
                break;
        }
    }

    void OnSoundManagerProgress(object param)
    {
        GeneralSoundsManager.ProgressSignal signal = (GeneralSoundsManager.ProgressSignal)param;
        switch (signal.type)
        {
            case GeneralSoundsManager.ProgressSignalType.OnSoundXmlParsed:
                soundXMLParsed = true;
                break;
            case GeneralSoundsManager.ProgressSignalType.OnXmlError:
                xmlError = true;
                break;
            default:
                break;
        }
    }

    void OnGameplayManagerProgress(object param)
    {
        GameplayManager.ProgressSignal signal = (GameplayManager.ProgressSignal)param;
        switch (signal.type)
        {
            case GameplayManager.ProgressSignalType.OnGameplayXmlParsed:
                gameplayXMLParsed = true;
                break;
            case GameplayManager.ProgressSignalType.OnGameplayGeneralXmlParsed:
                gameplayGenXMLParsed = true;
                break;
            case GameplayManager.ProgressSignalType.OnXmlError:
                xmlError = true;
                xmlErrorMessage = signal.param.ToString();
                break;
            case GameplayManager.ProgressSignalType.OnGameplayReset:
                gameplayReset = true;
                break;
            default:
                break;
        }
    }
    #endregion

    #region Public Members
    protected bool loaded = false;
    public void SetLoaded(bool flag)
    {
        loaded = flag;
    }

    public void LoadLevel(int level)
    {
        if (!loaded)
            this.StartCoroutine(this.LoadEnvironment(level));

        loaded = true;
    }

    public void UnloadLevel()
    {
        if (loaded)
            this.StartCoroutine(this.UnloadEnvironment());

        loaded = false;
    }

    public void CheckSession(string xmlFile)
    {
        loaded = true;
        this.StartCoroutine(this.CheckSessionXML(xmlFile));
    }
    #endregion

    #region Protected Members
    protected void ParseDebug(XmlNode node)
    {
        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode child = node.ChildNodes[i];
            switch (child.Name)
            {
                case ("useBalanceBoard"):
                    useBalanceBoard = (child.Attributes["active"].Value.ToLower().Equals("true"));
                    TrackingManager.Instance.BBEnabled = useBalanceBoard;
                    break;
                case ("useOVR"):
                    //useOVR = (child.Attributes["active"].Value.ToLower().Equals("true"));
                    //TrackingManager.Instance.OVREnabled = useOVR;
                    break;
            }
        }
    }

    protected void ParseDebug(XMLNode node)
    {
        foreach (XMLNode child in node.children)
        {
            switch (child.tagName)
            {
                case ("useBalanceBoard"):
                    useBalanceBoard = (child.GetAttributeAsString("active").ToLower().Equals("true"));
                    break;
            }
        }
    }

    protected void ValidationCallBack(object sender, ValidationEventArgs e)
    {
        switch (e.Severity)
        {
            case XmlSeverityType.Error:
                Debug.LogWarning("ERROR: " + e.Message);
                break;
            case XmlSeverityType.Warning:
                Debug.LogWarning("WARNING: " + e.Message);
                break;
        }
    }
    #endregion

    #region Coroutines
    IEnumerator OnFirstFrame()
    {
        try
        {
            string confPath = "AppData/default.xml";
            string configText = System.IO.File.ReadAllText(@confPath);

            xmlErrorMessage = "default.xml: ";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(configText);
            xmlDoc.Schemas.Add(null, @"AppData/xsd/scene.xsd");
            xmlDoc.Validate(ValidationCallBack);
            gameManagers.SendMessage("LoadConfiguration", configText);

            string generalPath = "Configurations/general.xml";
            string generalText = System.IO.File.ReadAllText(generalPath);

            xmlErrorMessage = "general.xml: ";
            XmlDocument generalXmlDoc = new XmlDocument();
            generalXmlDoc.LoadXml(generalText);
            generalXmlDoc.Schemas.Add(null, @"AppData/xsd/general.xsd");
            generalXmlDoc.Validate(ValidationCallBack);

            XmlNode root = generalXmlDoc.FirstChild;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode child = root.ChildNodes[i];
                switch (child.Name)
                {
                    case ("debug"):
                        ParseDebug(child);
                        break;
                }
            }

            gameManagers.SendMessage("LoadGeneralConf", generalText);
        }
        catch (Exception e)
        {
            xmlError = true;
            xmlErrorMessage += e.Message;
        }

        /*XMLReader reader = new XMLReader();
        XMLNode rootNode = reader.read(generalText).children[0] as XMLNode;
        foreach (XMLNode xmlNode in rootNode.children)
        {
            switch (xmlNode.tagName)
            {
                case ("debug"):
                    ParseDebug(xmlNode);
                    break;
            }
        }*/

        yield return this.StartCoroutine(this.FirstPreload());
    }

    IEnumerator FirstPreload()
    {
        _interface.SendMessage("StartInterface");

        if (xmlError)
        {
            _interface.gameObject.SendMessage("XMLError", xmlErrorMessage);
            //_interface.SetSplashMessage(xmlErrorMessage);
            Debug.LogWarning(xmlErrorMessage);
        }
        else
        {
            while (!gameplayGenXMLParsed && !soundXMLParsed && useExternalXML)
                yield return new WaitForEndOfFrame();

            InternalConsole.Instance.Log("Checking Balance Board (use = " + useBalanceBoard + ")");
            float bbTimer = TimeManager.Instance.MasterSource.TotalTime;
            while (!bbConnected && useBalanceBoard)
            {
                if (bbTimer > 0 && TimeManager.Instance.MasterSource.TotalTime - bbTimer > 5.0f)
                {
                    bbTimer = -1.0f;
                    InternalConsole.Instance.Log("Balance Board is not connected");
                    _interface.SetSplashMessage("Balance Board is not connected");
                }
                yield return new WaitForEndOfFrame();
            }

            _interface.SendMessage("InterfaceCanProcede");
        }
    }

    IEnumerator PreloadAll()
    {
        //_interface.SendMessage("StartInterface");

        if (xmlError)
        {
            _interface.SendMessage("XMLError", xmlErrorMessage);
            //_interface.SetSplashMessage(xmlErrorMessage);
            Debug.LogWarning(xmlErrorMessage);
        }
        else
        {
            while (!envXMLParsed && !obstaclesXMLParsed && !texturesXMLParsed && !gameplayXMLParsed && !gameplayGenXMLParsed && !soundXMLParsed && useExternalXML)
                yield return new WaitForEndOfFrame();

            InternalConsole.Instance.Log("Checking Balance Board (use = " + useBalanceBoard + ")");
            float bbTimer = TimeManager.Instance.MasterSource.TotalTime;
            while (!bbConnected && useBalanceBoard)
            {
                if (bbTimer > 0 && TimeManager.Instance.MasterSource.TotalTime - bbTimer > 5.0f)
                {
                    bbTimer = -1.0f;
                    InternalConsole.Instance.Log("Balance Board is not connected");
                    _interface.SetSplashMessage("Balance Board is not connected");
                }
                yield return new WaitForEndOfFrame();
            }

            InternalConsole.Instance.Log("PreloadAll");

            while (!texturesLoaded && !buildingAssetsLoaded)
                yield return new WaitForEndOfFrame();

            _interface.SendMessage("ConfigurationReady");
        }

        Debug.Log("PreloadAll");
    }

    float loadingStartTime = 0.0f;
    IEnumerator LoadEnvironment(int level)
    {
        loadingStartTime = TimeManager.Instance.MasterSource.TotalTime;
        _interface.SetLoadingPerc(10);
        gameManagers.SendMessage("SetupConfiguration", level);

        while (!environmentGenerated)
            yield return new WaitForEndOfFrame();

        _interface.SetLoadingPerc(50);
        InternalConsole.Instance.Log("OnEnvironmentGenerated");

        EnvironmentManager em = gameManagers.GetComponent<EnvironmentManager>();

        gameManagers.SendMessage("PlaceObstacles", level);

        while (!obstaclesPlaced && !targetPlaced)
            yield return new WaitForEndOfFrame();

        int tutPhase = 0;
        if (level == 0)
            tutPhase = 1;
        gameManagers.GetComponent<GameplayManager>().InitializeCharacter(tutPhase);

        if (level > 0 && gameManagers.GetComponent<GameplayManager>().LearningPhaseDone)
            gameManagers.SendMessage("InitializeSimplePath");

        _interface.SetLoadingPerc(90);

        environmentDestroyed = false;
        obstaclesDestroyed = false;
        gameplayReset = false;

        //WaitFakeLoading
        float elapsedTime = TimeManager.Instance.MasterSource.TotalTime - loadingStartTime;
        float waitTime = Mathf.Max(0.0f, 2.0f - elapsedTime);
        yield return new WaitForSeconds(waitTime);
        _interface.SetLoadingPerc(100);
        yield return new WaitForSeconds(0.5f);

        gameManagers.SendMessage("OnEnvironmentReady", em.Level);
    }

    IEnumerator UnloadEnvironment()
    {
        LevelRoot.Instance.BroadcastMessage("ResetConfiguration", SendMessageOptions.DontRequireReceiver);
        gameManagers.SendMessage("ResetConfiguration");

        while (!environmentDestroyed || !obstaclesDestroyed || !gameplayReset)
        {
            Debug.Log("environmentDestroyed: " + environmentDestroyed + ", obstaclesDestroyed: " + obstaclesDestroyed + ", gameplayReset: " + gameplayReset);
            yield return new WaitForEndOfFrame();
        }

        checkSessionFlag = false;
        environmentGenerated = false;
        obstaclesPlaced = false;
        targetPlaced = false;

        InternalConsole.Instance.Log("OnEnvironmentDestroyed");

        _interface.SendMessage("GoToNextInterfaceState");
    }

    IEnumerator CheckSessionXML(string xmlFile)
    {
        try
        {
            xmlError = false;
            string confPath = xmlFile;
            string configText = System.IO.File.ReadAllText(@confPath);

            xmlErrorMessage = "session.xml: ";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(configText);
            xmlDoc.Schemas.Add(null, @"AppData/xsd/scene.xsd");
            xmlDoc.Validate(ValidationCallBack);
            gameManagers.SendMessage("LoadConfiguration", configText);

            XmlNode root = xmlDoc.FirstChild;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode child = root.ChildNodes[i];
                switch (child.Name)
                {
                    case ("debug"):
                        ParseDebug(child);
                        break;
                }
            }
        }
        catch (Exception e)
        {
            xmlError = true;
            xmlErrorMessage += e.Message;
        }

        yield return this.StartCoroutine(this.PreloadAll());
    }
    #endregion
}
