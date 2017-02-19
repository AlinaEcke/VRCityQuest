using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS.Core;
using FoF.Utils;
using SBS.XML;
using System.Xml;
using SBS.Math;


[AddComponentMenu("FoF/EnvironmentManager")]
public class EnvironmentManager : MonoBehaviour
{
    #region Internal Data Structure
    public enum ProgressSignalType
    {
        OnEnvironmentXmlParsed = 0,
        OnXmlError,
        OnEnvironmentGenerated,
        OnEnvironmentDestroyed,
        OnTargetPlaced
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

    public class Road : System.IComparable<Road>
    {
        public Vector3 position = Vector3.zero;
        public Vector2 size = Vector2.zero;
        public float angleRotation = 0.0f;
        public GameObject gameObject;
        public Token token;
        public float manhattan = 0.0f;

        public int CompareTo(Road other)
        {
            return this.manhattan.CompareTo(other.manhattan);
        }
    }

    public class Building
    {
        public Vector3 position = Vector3.zero;
        public Vector2 size = Vector2.zero;
        public int floors = 0;
        public float angleRotation = 0.0f;
        public GameObject groundFloor = null;
        public List<GameObject> middleFloors = new List<GameObject>();
        public GameObject roof = null;
        public int textureIndex = -1;
        public int colorIndex = -1;
        public bool vaseActive = false;
        public bool pillarActive = false;
        public bool balconyActive = false;
        public List<Tuple<string, int>> targets = new List<Tuple<string, int>>();
    }
    #endregion

    #region Constants
    public const int TEXTURE_NUM = 4;
    public const int COLOR_NUM = 7;
    #endregion

    #region Public Members
    public SignalSender progressSignals;
    public Vector3 worldMin = Vector3.zero;
    public Vector3 worldMax = Vector3.zero;
    public GameObject[] pedestrianList;
    public Material daySkybox;
    public Material nightSkybox;
    public List<bool> isDayLight = new List<bool>();
    public List<int> totalPedestrians = new List<int>();
    #endregion

    #region Protected Members
    protected string configText = string.Empty;
    protected int level = 1;
    protected int currentParsedLevel = 1;
    //protected List<int> vRoadsList = new List<int>();
    //protected List<int> hRoadsList = new List<int>();
    //protected List<List<StringPerc>> roadsPercentageLists = new List<List<StringPerc>>();
    //protected List<List<StringPerc>> buildingsPercentageLists = new List<List<StringPerc>>();

    protected int vRoads = 0;
    protected int hRoads = 0;
    public int target = 0;
    protected List<StringPerc> roadsPercentage = new List<StringPerc>();
    protected List<StringPerc> buildingsPercentage = new List<StringPerc>();

    protected Transform tokenParent;
    protected Transform buildingParent;

    protected GameObject tokenAxA;
    protected GameObject tokenAxB;
    protected GameObject tokenAxC;
    protected GameObject tokenAxD;
    protected GameObject tokenBxB;
    protected GameObject tokenBxC;
    protected GameObject tokenBxD;
    protected GameObject tokenCxC;
    protected GameObject tokenCxD;
    protected GameObject tokenDxD;

    protected GameObject tokenAx1;
    protected GameObject tokenAx2;
    protected GameObject tokenAx3;
    protected GameObject tokenBx1;
    protected GameObject tokenBx2;
    protected GameObject tokenBx3;
    protected GameObject tokenCx1;
    protected GameObject tokenCx2;
    protected GameObject tokenCx3;
    protected GameObject tokenDx1;
    protected GameObject tokenDx2;
    protected GameObject tokenDx3;

    protected Dictionary<string, GameObject> buildingsSrc = new Dictionary<string,GameObject>();
    protected Dictionary<Token, List<GameObject>> tokenTargets = new Dictionary<Token, List<GameObject>>();
    protected Dictionary<GameObject, Building> targetBuilding = new Dictionary<GameObject, Building>();

    //protected List<Cross> crosses = new List<Cross>();
    protected List<Road> crosses = new List<Road>();
    protected List<Road> v_roads = new List<Road>();
    protected List<Road> h_roads = new List<Road>();
    protected List<Building> buildings = new List<Building>();
    protected List<Building> border_buildings = new List<Building>();
    protected List<int> floorList = new List<int>();

    protected float city_width = 0.0f;
    protected float city_height = 0.0f;

    protected GameObject gameManagers;

    protected List<Color> colorList = new List<Color>();
    protected Transform pedestrianParent;
    protected TokensManager.TokenHit currentTokenHit = null;

    protected GameObject pathGrid;
    protected GameObject pathManager;

    protected int hTowerCoord = 0;
    protected int vTowerCoord = 0;
    protected int roadTextureId = -1;

    protected GameplayManager gameplayManager = null;
    #endregion

    #region Public Get/Set
    public int Level
    {
        get { return level; }
        set { level = value; }
    }

    public int PedestrianNum
    {
        get
        {
            if (level == 0)
                return 0;
            return totalPedestrians[level - 1];
        }
    }

    public bool IsDaylight
    {
        get
        {
            if (level == 0 || !gameManagers.GetComponent<GameplayManager>().LearningPhaseDone)
                return true;

            //if (GameObject.FindGameObjectWithTag("Preloader").GetComponent<Preloader>().useOVR)
            //    return false;

            return isDayLight[level - 1];
        }
    }

    public List<Road> Crosses
    {
        get
        {
            return crosses;
        }
    }

    public List<Road> VerticalRoads
    {
        get
        {
            return v_roads;
        }
    }

    public List<Road> HorizontalRoads
    {
        get
        {
            return h_roads;
        }
    }

    public List<Building> Buildings
    {
        get
        {
            return buildings;
        }
    }

    public List<Building> BorderBuildings
    {
        get
        {
            return border_buildings;
        }
    }

    public List<int> FloorList
    {
        get
        {
            return floorList;
        }
        set
        {
            floorList = value;
        }
    }

    public int VTowerCoord
    {
        get
        {
            return vTowerCoord;
        }
        set
        {
            vTowerCoord = value;
        }
    }

    public int HTowerCoord
    {
        get
        {
            return hTowerCoord;
        }
        set
        {
            hTowerCoord = value;
        }
    }

    public int VRoads
    {
        get
        {
            return vRoads;
        }
        set
        {
            vRoads = value;
        }
    }

    public int HRoads
    {
        get
        {
            return hRoads;
        }
        set
        {
            hRoads = value;
        }
    }

    public float CityHeight
    {
        get
        {
            return city_height;
        }
        set
        {
            city_height = value;
        }
    }

    public float CityWidth
    {
        get
        {
            return city_width;
        }
        set
        {
            city_width = value;
        }
    }

    public int RoadTextureId
    {
        get
        {
            return roadTextureId;
        }
        set
        {
            roadTextureId = value;
        }
    }
    #endregion

    #region Unity Callback	
	void Start ()
    {
        tokenAxA = Resources.Load("Tokens/token_AxA") as GameObject;
        tokenAxB = Resources.Load("Tokens/token_AxB") as GameObject;
        tokenAxC = Resources.Load("Tokens/token_AxC") as GameObject;
        tokenAxD = Resources.Load("Tokens/token_AxD") as GameObject;
        tokenBxB = Resources.Load("Tokens/token_BxB") as GameObject;
        tokenBxC = Resources.Load("Tokens/token_BxC") as GameObject;
        tokenBxD = Resources.Load("Tokens/token_BxD") as GameObject;
        tokenCxC = Resources.Load("Tokens/token_CxC") as GameObject;
        tokenCxD = Resources.Load("Tokens/token_CxD") as GameObject;
        tokenDxD = Resources.Load("Tokens/token_DxD") as GameObject;

        tokenAx1 = Resources.Load("Tokens/token_Ax1") as GameObject;
        tokenAx2 = Resources.Load("Tokens/token_Ax2") as GameObject;
        tokenAx3 = Resources.Load("Tokens/token_Ax3") as GameObject;
        tokenBx1 = Resources.Load("Tokens/token_Bx1") as GameObject;
        tokenBx2 = Resources.Load("Tokens/token_Bx2") as GameObject;
        tokenBx3 = Resources.Load("Tokens/token_Bx3") as GameObject;
        tokenCx1 = Resources.Load("Tokens/token_Cx1") as GameObject;
        tokenCx2 = Resources.Load("Tokens/token_Cx2") as GameObject;
        tokenCx3 = Resources.Load("Tokens/token_Cx3") as GameObject;
        tokenDx1 = Resources.Load("Tokens/token_Dx1") as GameObject;
        tokenDx2 = Resources.Load("Tokens/token_Dx2") as GameObject;
        tokenDx3 = Resources.Load("Tokens/token_Dx3") as GameObject;

        buildingsSrc.Add("building_1x1", Resources.Load("Buildings/building_1x1") as GameObject);
        buildingsSrc.Add("building_1x2", Resources.Load("Buildings/building_1x2") as GameObject);
        buildingsSrc.Add("building_1x3", Resources.Load("Buildings/building_1x3") as GameObject);
        buildingsSrc.Add("building_2x2", Resources.Load("Buildings/building_2x2") as GameObject);
        buildingsSrc.Add("building_2x3", Resources.Load("Buildings/building_2x3") as GameObject);
        buildingsSrc.Add("building_3x3", Resources.Load("Buildings/building_3x3") as GameObject);

        buildingsSrc.Add("floor_1x1", Resources.Load("Buildings/floor_1x1") as GameObject);
        buildingsSrc.Add("floor_1x2", Resources.Load("Buildings/floor_1x2") as GameObject);
        buildingsSrc.Add("floor_1x3", Resources.Load("Buildings/floor_1x3") as GameObject);
        buildingsSrc.Add("floor_2x2", Resources.Load("Buildings/floor_2x2") as GameObject);
        buildingsSrc.Add("floor_2x3", Resources.Load("Buildings/floor_2x3") as GameObject);
        buildingsSrc.Add("floor_3x3", Resources.Load("Buildings/floor_3x3") as GameObject);

        buildingsSrc.Add("roof_1x1", Resources.Load("Buildings/roof_1x1") as GameObject);
        buildingsSrc.Add("roof_1x2", Resources.Load("Buildings/roof_1x2") as GameObject);
        buildingsSrc.Add("roof_1x3", Resources.Load("Buildings/roof_1x3") as GameObject);
        buildingsSrc.Add("roof_2x2", Resources.Load("Buildings/roof_2x2") as GameObject);
        buildingsSrc.Add("roof_2x3", Resources.Load("Buildings/roof_2x3") as GameObject);
        buildingsSrc.Add("roof_3x3", Resources.Load("Buildings/roof_3x3") as GameObject);

        buildingsSrc.Add("tower_building_1x1", Resources.Load("Buildings/tower_building_1x1") as GameObject);
        buildingsSrc.Add("tower_floor_1x1", Resources.Load("Buildings/tower_floor_1x1") as GameObject);
        buildingsSrc.Add("tower_roof_1x1", Resources.Load("Buildings/tower_roof_1x1") as GameObject);

        tokenParent = GameObject.FindGameObjectWithTag("Tokens").transform;
        buildingParent = GameObject.FindGameObjectWithTag("Buildings").transform;
        pedestrianParent = GameObject.FindGameObjectWithTag("Pedestrians").transform;
        gameManagers = GameObject.FindGameObjectWithTag("GameManagers");

        colorList.Add(new Color(1.0f, 1.0f, 1.0f));
        colorList.Add(new Color(0.784f, 0.784f, 0.784f));
        colorList.Add(new Color(0.545f, 0.431f, 0.305f));
        colorList.Add(new Color(0.494f, 0.545f, 0.325f));
        colorList.Add(new Color(1.0f, 0.686f, 0.686f));
        colorList.Add(new Color(0.588f, 0.8f, 0.8f));
        colorList.Add(new Color(0.622f, 0.372f, 0.372f));

        gameplayManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GameplayManager>();
    }
    #endregion

    #region Messages
    void LoadConfiguration(string configText)
    {
        this.StartCoroutine(this.LoadConfigXml(configText));
    }

    void SetupConfiguration(int level)
    {
        if (gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation)
            this.StartCoroutine(this.GenerateEnvironment(level));
        else
            this.StartCoroutine(this.CreateEnvironment());
    }

    void ResetConfiguration()
    {
        this.StartCoroutine(this.DestroyEnvironment());
    }

    void InitializeSimplePath()
    {
        float cellSize = 1.0f;
        Vector3 gridSize = worldMax - worldMin;
        Vector3 cellNum = gridSize / cellSize;

        pathGrid = new GameObject("PathGridWithObstacles");
        pathGrid.transform.position = worldMin;
        PathGridComponent pathGridComp = pathGrid.AddComponent<PathGridComponent>();
        pathGridComp.m_numberOfRows = Mathf.CeilToInt(cellNum.z);
        pathGridComp.m_numberOfColumns = Mathf.CeilToInt(cellNum.x);
        pathGridComp.m_cellSize = cellSize;
        pathGridComp.m_debugShow = false;
        pathGrid.SendMessage("Awake");
        ObstacleGridComponent obs = pathGrid.AddComponent<ObstacleGridComponent>();
        obs.m_rasterizeEveryFrame = true;
        obs.m_show = false;

        pathManager = new GameObject("PathManager");
        PathManagerComponent pathManagerComponent = pathManager.AddComponent<PathManagerComponent>();
        pathManagerComponent.m_pathTerrainComponent = pathGridComp;

        int numCells = Mathf.Max(pathGridComp.m_numberOfRows, pathGridComp.m_numberOfColumns);
        numCells *= numCells;
        pathManagerComponent.m_maxNumberOfNodesPerPlanner = numCells >> 2;
        pathManagerComponent.m_maxNumberOfPlanners = totalPedestrians[level - 1] + 1;
        pathManager.SendMessage("Awake");

        GeneratePedestrian(totalPedestrians[level - 1], pathManagerComponent);
    }

    protected void OnEnvironmentReady(int level)
    {
        GenerateCityMap();
    }
    #endregion

    #region Coroutines
    IEnumerator LoadConfigXml(string configText)
    {
        isDayLight.Clear();
        totalPedestrians.Clear();

        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(configText);

            XmlNode root = xmlDoc.FirstChild;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode child = root.ChildNodes[i];
                switch (child.Name)
                {
                    case ("city_grid"):
                        ParseCityGrid(child);
                        break;
                    case ("levels"):
                        ParseLevels(child);
                        break;
                }
            }
            progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnEnvironmentXmlParsed));
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
                case ("city_grid"):
                    ParseCityGrid(xmlNode);
                    break;
                case("level"):
                    currentParsedLevel = xmlNode.GetAttributeAsInt("id");
                    ParseLevel(xmlNode);
                    break;
            }
        }

        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnEnvironmentXmlParsed));

        yield return new WaitForEndOfFrame(); //this.StartCoroutine(this.GenerateEnvironment());*/
    }

    IEnumerator GenerateEnvironment(int level)
    {
        Debug.Log("*** GenerateEnvironment " + level);
        SetLevelConfigs(level);

        if (level == 0)
        {
            BuildCrossList();
            BuildRoadAndBuildingList();
            BuildFloorList();
        }
        else
        {
            DataContainer.LoadFromFile();
            DataContainer.Instance.UnloadDataStructures();
        }

        //GENERATE
        PlaceCityAssets();
        PlaceBuildingsFloors();
        BuildTokenGraph();

        SetIllumination();
        CombineBuidingMeshes();

        LevelRoot.Instance.worldBoundsMin = worldMin;
        LevelRoot.Instance.worldBoundsMax = worldMax;
        LevelRoot.Instance.Initialize();

        gameManagers.GetComponent<TokensManager>().InitializeTokensByLevelObjects();

        PlaceCityTargets();
        if (level == 0)
        {
            DefineCityTargets();
            gameManagers.GetComponent<ObstaclesManager>().DefineObstacles(level);

            /*if (level > 0)
            {
                DataContainer.Instance.LoadDataStructures();
                DataContainer.filename = "Sessions\test01.ses";
                DataContainer.SaveToFile();
            }*/
        }
        else
        {
            DataContainer.Instance.UnloadLevelData();
        }

        //gameplayManager.FillTargetStacks();

        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnEnvironmentGenerated));
        yield return new WaitForEndOfFrame();
    }

    public void StartCreateEnvironment()
    {
        this.StartCoroutine(CreateEnvironment());
    }

    IEnumerator CreateEnvironment()
    {
        Debug.Log("*** CreateEnvironment");
        
        if (gameplayManager.Gameplay == GameplayManager.GameplayType.ObstacleAvoidance)
            level = gameplayManager.Level;

        SetLevelConfigs(level);

        BuildCrossList();
        BuildRoadAndBuildingList();
        BuildFloorList();

        //GENERATE
        PlaceCityAssets();
        PlaceBuildingsFloors();
        BuildTokenGraph();

        SetIllumination();
        CombineBuidingMeshes();

        LevelRoot.Instance.worldBoundsMin = worldMin;
        LevelRoot.Instance.worldBoundsMax = worldMax;
        LevelRoot.Instance.Initialize();

        gameManagers.GetComponent<TokensManager>().InitializeTokensByLevelObjects();

        DefineCityTargets();
        gameManagers.GetComponent<ObstaclesManager>().DefineObstacles(level);

        PlaceCityTargets();
        gameplayManager.PrepareTargetGate(true);

        if (gameplayManager.Gameplay != GameplayManager.GameplayType.ObstacleAvoidance)
        {
            DataContainer.Instance.LoadDataStructures();
        }
        //DataContainer.SaveToFile();    


        if (gameplayManager.Gameplay == GameplayManager.GameplayType.ObstacleAvoidance)
            progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnEnvironmentGenerated));

        yield return new WaitForEndOfFrame();

        Preloader preloader = GameObject.FindGameObjectWithTag("Preloader").GetComponent<Preloader>();

        if (gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation)
            preloader.UnloadLevel();
    }

    IEnumerator DestroyEnvironment()
    {
        GameObject.Destroy(pathGrid);
        GameObject.Destroy(pathManager);

        for (int i = 0; i < pedestrianParent.childCount; i++)
            GameObject.Destroy(pedestrianParent.GetChild(i).gameObject);

        yield return this.StartCoroutine(this.DestroyLateEnvironment());
    }

    IEnumerator DestroyLateEnvironment()
    {
        for (int i = 0; i < buildingParent.childCount; i++)
            GameObject.Destroy(buildingParent.GetChild(i).gameObject);

        for (int i = 0; i < tokenParent.childCount; i++)
            GameObject.Destroy(tokenParent.GetChild(i).gameObject);

        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnEnvironmentDestroyed));
        yield return new WaitForEndOfFrame();
    }  
    #endregion

    #region Parsing Functions
    protected void ParseLevels(XmlNode node)
    {
        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode child = node.ChildNodes[i];
            if (child.Name.Equals("level"))
            {
                currentParsedLevel = int.Parse(child.Attributes["id"].Value);
                ParseLevel(child);
            }
        }
    }

    protected void ParseLevel(XmlNode node)
    {
        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode child = node.ChildNodes[i];
            switch (child.Name)
            {
                case ("pedestrians"):
                    totalPedestrians.Add(int.Parse(child.Attributes["number"].Value));
                    break;
                case ("graphics"):
                    ParseGraphics(child);
                    break;
            }
        }
    }

    protected void ParseGraphics(XmlNode node)
    {
        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode child = node.ChildNodes[i];
            switch (child.Name)
            {
                case ("illumination"):
                    isDayLight.Add(!child.Attributes["type"].Value.ToLower().Equals("night"));
                    break;
            }
        }
    }

    /*protected void ParseLevel(XMLNode node)
    {
        foreach (XMLNode child in node.children)
        {
            switch (child.tagName)
            {
                //case ("city_grid"):
                //    ParseCityGrid(child);
                //    break;
                case ("pedestrians"):
                    totalPedestrians.Add(child.GetAttributeAsInt("number"));
                    break;
                case ("graphics"):
                    ParseGraphics(child);
                    break;
            }
        }
    }

    protected void ParseGraphics(XMLNode node)
    {
        foreach (XMLNode child in node.children)
        {
            switch (child.tagName)
            {
                case ("illumination"):
                    isDayLight.Add(!child.GetAttributeAsString("type").ToLower().Equals("night"));
                    break;
            }
        }
    }*/

    protected void ParseCityGrid(XmlNode node)
    {
        for(int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode child = node.ChildNodes[i];
            switch (child.Name)
            {
                case ("horizontal"):
                    hRoads = int.Parse(child.Attributes["value"].Value);
                    break;
                case ("vertical"):
                    vRoads = int.Parse(child.Attributes["value"].Value);
                    break;
                case ("roads"):
                    ParseRoads(child);
                    break;
                case ("blocks"):
                    ParseBlocks(child);
                    break;
            }
        }
    }

    protected void ParseRoads(XmlNode node)
    {
        for(int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode child = node.ChildNodes[i];
            if (child.Name.Equals("road"))
            {
                StringPerc item = new StringPerc();
                item.item = "road" + child.Attributes["type"].Value;
                item.perc = int.Parse(child.Attributes["perc"].Value);
                roadsPercentage.Add(item);
            }
        }
    }

    protected void ParseBlocks(XmlNode node)
    {
        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode child = node.ChildNodes[i];
            if (child.Name.Equals("block"))
            {
                StringPerc item_blocks = new StringPerc();
                item_blocks.item = child.Attributes["floors"].Value;
                item_blocks.perc = int.Parse(child.Attributes["perc"].Value);
                buildingsPercentage.Add(item_blocks);
            }
        }
    }

    /*protected void ParseCityGrid(XMLNode node)
    {
        foreach (XMLNode child in node.children)
        {
            switch (child.tagName)
            {
                case ("horizontal"):
                    hRoads = child.GetAttributeAsInt("value");
                    //hRoadsList.Add(child.GetAttributeAsInt("value"));
                    break;
                case ("vertical"):
                    vRoads = child.GetAttributeAsInt("value");
                    //vRoadsList.Add(child.GetAttributeAsInt("value"));
                    break;
                case ("roads"):
                    ParseRoads(child);
                    break;
                case ("blocks"):
                    ParseBlocks(child);
                    break;
            }
        }
    }

    protected void ParseBlocks(XMLNode node)
    {
        //List<StringPerc> buildingsPercentage = new List<StringPerc>();
        foreach (XMLNode child in node.children)
        {
            if (child.tagName.Equals("block"))
            {
                StringPerc item_blocks = new StringPerc();
                //item_blocks.item = "block" + child.GetAttributeAsString("floors");
                item_blocks.item = child.GetAttributeAsString("floors");
                item_blocks.perc = child.GetAttributeAsInt("perc");
                buildingsPercentage.Add(item_blocks);
            }
        }
        //buildingsPercentageLists.Insert(currentParsedLevel - 1, buildingsPercentage);
    }

    protected void ParseRoads(XMLNode node)
    {
        //List<StringPerc> roadsPercentage = new List<StringPerc>();
        foreach (XMLNode child in node.children)
        {
            if (child.tagName.Equals("road"))
            {
                StringPerc item = new StringPerc();
                item.item = "road" + child.GetAttributeAsString("type");
                item.perc = child.GetAttributeAsInt("perc");
                roadsPercentage.Add(item);
            }
        }
        //roadsPercentageLists.Insert(currentParsedLevel - 1, roadsPercentage);
    }*/
    #endregion

    #region Building Functions
    protected void SetLevelConfigs(int level)
    {
        this.level = level;
        if (level == 0)
        {
            InternalConsole.Instance.Log("Select LEVEL TUTORIAL");
            hRoads = 1;
            vRoads = 2;
            //buildingsPercentage = buildingsPercentageLists[1];
            StringPerc strPerc = new StringPerc();
            strPerc.item = "roadC";
            strPerc.perc = 100;
            roadsPercentage = new List<StringPerc>();
            roadsPercentage.Add(strPerc);
        }
        else
        {
            //hRoads = hRoadsList[level - 1];
            //vRoads = vRoadsList[level - 1];
            //buildingsPercentage = buildingsPercentageLists[level - 1];
            //roadsPercentage = roadsPercentageLists[level - 1];
            InternalConsole.Instance.Log("Select LEVEL " + level.ToString());
        }
        
        crosses.Clear();
        v_roads.Clear();
        h_roads.Clear();
        buildings.Clear();
        border_buildings.Clear();

        city_height = 0.0f;
        city_width = 0.0f;
    }

    protected void BuildCrossList()
    {
        //We create a list of street's sizes
        int totRoads = vRoads + hRoads + 4; // Total number of roads in our map
        int totRoadsAux = 0;
        int numberOfTypes = roadsPercentage.Count;
        List<string> streets = new List<string>();
        foreach (StringPerc p in roadsPercentage)
        {
            int card = (p.perc * totRoads) / 100;
            int i = 0;
            while (i < card && totRoadsAux <= totRoads)
            {
                streets.Add(p.item);
                totRoadsAux = totRoadsAux + 1;
                i = i + 1;
            }
            if (totRoadsAux > totRoads) break;
        }

        //int count = Mathf.Min(totRoads - totRoadsAux, roadsPercentage.Count);
        for (int i = 0; i < totRoads - totRoadsAux; i++) //totRoads - totRoadsAux <=  NumberOfTypes
            streets.Add(roadsPercentage[i % numberOfTypes].item);

        /*The convention is the following: the first hRoads in the vector Streets will be the types of 
         * the horizontal streets -from bottom to top- while the following vRoads in Streets will indicate 
         * the types of the vertical steeets -bottom to top- In order to have a random distribuition of the 
         * elements in Steets, we will rearrange their order 
         */
        int rn; //Random_Number
        int streetsCard = streets.Count;
        //InternalConsole.Instance.Log(streetsCard.ToString());
        List<string> shuffledStreets = new List<string>();
        for (int i = 0; i < streetsCard; i++)
        {
            rn = Random.Range(0, streets.Count - 1);
            shuffledStreets.Add(streets[rn]);
            streets.RemoveAt(rn);
        }

        //InternalConsole.Instance.Log("*******************************");

        string str = string.Empty;
        foreach (string c in shuffledStreets)
            str += c + ", ";
        //InternalConsole.Instance.Log(str);

        //InternalConsole.Instance.Log("*******************************");

        /*We create a list of crosses. Each cross is defined as a couple <height, width> of the croos.
         *The crosses are stored from the leftmost to the rightmost and from bottom to top. 
         */


        for (int i = 0; i < hRoads + 2; i++) //We consider also the 2 additional horizontal border roads
        {
            str = string.Empty;
            for (int j = 0; j < vRoads + 2; j++)
            {
                //Cross cr = new Cross();
                Road cr = new Road();
                switch (shuffledStreets[j + hRoads + 2])
                {
                    case "roadA": cr.size.y = 3.0f; break;
                    case "roadB": cr.size.y = 5.0f; break;
                    case "roadC": cr.size.y = 7.0f; break;
                    case "roadD": cr.size.y = 9.0f; break;
                    default: cr.size.y = 0.0f; break;
                }
                switch (shuffledStreets[i])
                {
                    case "roadA": cr.size.x = 3.0f; break;
                    case "roadB": cr.size.x = 5.0f; break;
                    case "roadC": cr.size.x = 7.0f; break;
                    case "roadD": cr.size.x = 9.0f; break;
                    default: cr.size.x = 0.0f; break;
                }
                cr.position = new Vector3(0.0f, 0.0f, 0.0f);
                crosses.Add(cr);
                str += "(" + cr.size.x + "," + cr.size.y + ")";
            }
            //InternalConsole.Instance.Log(str);
        }
    }

    protected void SetIllumination()
    {
        GameObject lightObj = GameObject.FindGameObjectWithTag("Light");

        if (IsDaylight)
        {
            lightObj.GetComponent<Light>().color = new Color(1.0f, 0.957f, 0.812f);
            lightObj.GetComponent<Light>().intensity = 0.6f;
            lightObj.GetComponent<Light>().shadowStrength = 0.5f;
            RenderSettings.ambientLight = new Color(0.463f, 0.486f, 0.502f);
            RenderSettings.skybox = daySkybox;
            RenderSettings.fogColor = new Color(0.631f, 0.647f, 0.686f);
        }
        else
        {
            lightObj.GetComponent<Light>().color = new Color(0.459f, 0.498f, 0.569f);
            lightObj.GetComponent<Light>().intensity = 0.3f;
            lightObj.GetComponent<Light>().shadowStrength = 0.2f;
            RenderSettings.ambientLight = new Color(0.243f, 0.251f, 0.255f);
            RenderSettings.skybox = nightSkybox;
            RenderSettings.fogColor = new Color(0.03f, 0.033f, 0.4f);
        }
    }

    protected void BuildRoadAndBuildingList()
    {
        hTowerCoord = Random.Range(0, 100000) % (vRoads + 1);
        vTowerCoord = Random.Range(0, 100000) % (hRoads + 1);

        /*
        * We create the lenghts of the horizontal roads
        */
        List<int> dim_h_roads = new List<int>();
        //int city_width = 0;
        int lenght_h_road;
        for (int i = 0; i < vRoads + 1; i++)
        {
            lenght_h_road = Random.Range(1, 4) * 10;

            // if TUTORIAL
            if (this.level == 0)
            {
                hTowerCoord = 1;
                if (i < 2)
                    lenght_h_road = 10;
                else
                    lenght_h_road = 20;
            }
            else
            {
                if (i == hTowerCoord)
                    lenght_h_road = 10;
            }

            dim_h_roads.Add(lenght_h_road);
            city_width = city_width + lenght_h_road;
        }


        /*
         * We create the lenghts of the vertical roads 
         */
        List<int> dim_v_roads = new List<int>();
        //int city_height = 0;
        int lenght_v_road;
        for (int i = 0; i < hRoads + 1; i++)
        {
            lenght_v_road = Random.Range(1, 4) * 10;
            
            // if TUTORIAL
            if (this.level == 0)
            {
                vTowerCoord = 1;
                if (i < 1)
                    lenght_v_road = 30;
                else
                    lenght_v_road = 10;
            }
            else
            {
                if (i == vTowerCoord)
                    lenght_v_road = 10;
            }

            dim_v_roads.Add(lenght_v_road);
            city_height = city_height + lenght_v_road;
        }

        //InternalConsole.Instance.Log("city hight:" + city_height.ToString() + "     " + "city width:" + city_width.ToString());

        /*
         * Here we compute the width of the city  
         */
        for (int i = 0; i < vRoads + 2; i++)
            city_width = city_width + (int)crosses[i].size.y;

        /*
         * Here we compute the height of the city  
         */
        for (int i = 0; i < hRoads + 2; i++)
            city_height = city_height + (int)crosses[i * (vRoads + 2)].size.x;


        //InternalConsole.Instance.Log("city hight:" + city_height.ToString() + "     " + "city width:" + city_width.ToString());


        /*
         * We fix the crosses into the space
         */
        float xx = 0;
        float zz = 0;
        for (int i = 0; i < hRoads + 1; i++)
        {
            xx = 0;
            int j;
            for (j = 0; j < vRoads + 1; j++)
            {
                //Cross c = crosses[j + i * (vRoads + 2)];
                Road c = crosses[j + i * (vRoads + 2)];
                c.position.x = xx;
                c.position.z = zz;
                xx = xx + dim_h_roads[j] + (c.size.y) * 0.5f + (crosses[j + i * (vRoads + 2) + 1].size.y) * 0.5f;
            }
            //Cross cc = crosses[j + i * (vRoads + 2)];
            Road cc = crosses[j + i * (vRoads + 2)];
            cc.position.x = xx;
            cc.position.z = zz;
            zz = zz + dim_v_roads[i] + (crosses[i * (vRoads + 2)].size.x) * 0.5f + (crosses[(i + 1) * (vRoads + 2)].size.x) * 0.5f;
        }
        xx = 0;
        int k;
        for (k = 0; k < vRoads + 1; k++)
        {
            //Cross c = crosses[k + (hRoads + 1) * (vRoads + 2)];
            Road c = crosses[k + (hRoads + 1) * (vRoads + 2)];
            c.position.x = xx;
            c.position.z = zz;
            xx = xx + dim_h_roads[k] + ((c.size.y) * 0.5f) + (crosses[k + ((hRoads + 1) * (vRoads + 2)) + 1].size.y) * 0.5f;
        }
        //Cross kk = crosses[(hRoads + 2) * (vRoads + 2) - 1];
        Road kk = crosses[(hRoads + 2) * (vRoads + 2) - 1];
        kk.position.x = xx;
        kk.position.z = zz;

        /*
         * We create the vertical roads -From bottom to top, left to right -
         */
        for (int i = 0; i < hRoads + 1; i++)
        {
            for (int j = 0; j < vRoads + 2; j++)
            {
                Road new_Road = new Road();
                new_Road.position = new Vector3(crosses[j + i * (vRoads + 2)].position.x,
                                                0.0f,
                                                crosses[j + i * (vRoads + 2)].position.z + (crosses[j + i * (vRoads + 2)].size.x * 0.5f) + dim_v_roads[i] * 0.5f);
                new_Road.size = new Vector2(dim_v_roads[i], crosses[j + i * (vRoads + 2)].size.y);
                v_roads.Add(new_Road);
            }
        }



        /*
         * We create the horizontal roads -From left to right, bottom to top, - 
         */
        for (int i = 0; i < hRoads + 2; i++)
        {
            for (int j = 0; j < vRoads + 1; j++)
            {
                Road new_Road = new Road();
                new_Road.position = new Vector3(crosses[j + i * (vRoads + 2)].position.x + (crosses[j + i * (vRoads + 2)].size.y * 0.5f) + dim_h_roads[j] * 0.5f,
                                                0.0f,
                                                crosses[j + i * (vRoads + 2) + 1].position.z);
                new_Road.size = new Vector2(dim_h_roads[j], crosses[j + i * (vRoads + 2)].size.x);
                h_roads.Add(new_Road);
            }
        }

        roadTextureId = Random.Range(1, 4);

        /*
         * We create the buildings
         */
        for (int i = 0; i < hRoads + 1; i++)
        {
            for (int j = 0; j < vRoads + 1; j++)
            {
                Building new_building = new Building();
                new_building.position = new Vector3(h_roads[j + i * (vRoads + 1)].position.x, 0.0f, v_roads[i * (vRoads + 2)].position.z);
                new_building.size = new Vector2(dim_v_roads[i], dim_h_roads[j]);
                new_building.angleRotation = GetAngleRotation(new_building.size.y, new_building.size.x, true);
                new_building.textureIndex = Random.Range(0, TEXTURE_NUM - 1);
                new_building.colorIndex = Random.Range(0, COLOR_NUM - 1);
                new_building.vaseActive = ((Random.Range(0, 1000000) % 100) > 50);
                new_building.pillarActive = ((Random.Range(0, 1000000) % 100) > 50);
                new_building.balconyActive = ((Random.Range(0, 1000000) % 100) > 50);
                buildings.Add(new_building);
            }
        }

        /*
        * We create boundary of buildings
        */
        float city_hight_aux = city_height;
        float city_width_aux = city_width;

        float buildings_height = 10;

        float h_top_buildings_width = (Random.Range(1, 4) * 10);
        float h_top_x = crosses[(hRoads + 1) * (vRoads + 2)].position.x + (h_top_buildings_width * 0.5f - crosses[(hRoads + 1) * (vRoads + 2)].size.y * 0.5f);
        float h_top_z = (city_height - crosses[0].size.x * 0.5f) + buildings_height * 0.5f;

        float h_bot_buildings_width = (Random.Range(1, 4) * 10);
        float h_bot_x = city_width - (crosses[0].size.y * 0.5f) - (h_bot_buildings_width * 0.5f);
        float h_bot_z = crosses[0].position.y - (crosses[0].size.x * 0.5f) - (buildings_height * 0.5f);

        float v_left_x = crosses[0].position.x - crosses[0].size.y * 0.5f - buildings_height * 0.5f;
        float v_left_buildings_width = (Random.Range(1, 4) * 10);
        float v_left_z = (v_left_buildings_width * 0.5f) - (crosses[0].size.x * 0.5f);

        float v_right_buildings_width = (Random.Range(1, 4) * 10);
        float v_right_x = crosses[(hRoads + 2) * (vRoads + 2) - 1].position.x + (crosses[(hRoads + 2) * (vRoads + 2) - 1].size.y * 0.5f) + (buildings_height * 0.5f);
        float v_right_z = city_height - (crosses[0].size.x * 0.5f) - (v_right_buildings_width * 0.5f);

        int counter = 0;
        while (city_hight_aux > 0)
        {
            //LEFT
            Building new_b_building = new Building();
            new_b_building.position = new Vector3(v_left_x, 0.0f, v_left_z);
            new_b_building.size = new Vector2(buildings_height, v_left_buildings_width);
            new_b_building.angleRotation = 270.0f;
            new_b_building.textureIndex = Random.Range(0, TEXTURE_NUM - 1);
            new_b_building.colorIndex = Random.Range(0, COLOR_NUM - 1);
            new_b_building.vaseActive = ((Random.Range(0, 1000000) % 100) > 50);
            new_b_building.pillarActive = ((Random.Range(0, 1000000) % 100) > 50);
            new_b_building.balconyActive = ((Random.Range(0, 1000000) % 100) > 50);

            border_buildings.Add(new_b_building);

            city_hight_aux = city_hight_aux - (int)v_left_buildings_width;
            v_left_buildings_width = (Random.Range(1, 4) * 10);
            v_left_z = v_left_z + (new_b_building.size.y * 0.5f) + (v_left_buildings_width * 0.5f);

            counter = counter + 1;
        }
        city_hight_aux = city_height;

        //counter = 0;
        while (city_width_aux > 0)
        {
            //TOP
            Building new_b_building2 = new Building();
            new_b_building2.position = new Vector3(h_top_x, 0.0f, h_top_z);
            new_b_building2.size = new Vector2(buildings_height, h_top_buildings_width);
            new_b_building2.angleRotation = 0.0f;
            new_b_building2.textureIndex = Random.Range(0, TEXTURE_NUM - 1);
            new_b_building2.colorIndex = Random.Range(0, COLOR_NUM - 1);
            new_b_building2.vaseActive = ((Random.Range(0, 1000000) % 100) > 50);
            new_b_building2.pillarActive = ((Random.Range(0, 1000000) % 100) > 50);
            new_b_building2.balconyActive = ((Random.Range(0, 1000000) % 100) > 50);
            
            border_buildings.Add(new_b_building2);

            city_width_aux = city_width_aux - (int)h_top_buildings_width;
            h_top_buildings_width = (Random.Range(1, 4) * 10);
            h_top_x = h_top_x + (new_b_building2.size.y * 0.5f) + (h_top_buildings_width * 0.5f);

            counter = counter + 1;
        }
        city_width_aux = city_width;

        //counter = 0;
        while (city_hight_aux > 0)
        {
            //RIGHT
            Building new_b_building3 = new Building();
            new_b_building3.position = new Vector3(v_right_x, 0.0f, v_right_z);
            new_b_building3.size = new Vector2(buildings_height, v_right_buildings_width);
            new_b_building3.angleRotation = 90.0f;
            new_b_building3.textureIndex = Random.Range(0, TEXTURE_NUM - 1);
            new_b_building3.colorIndex = Random.Range(0, COLOR_NUM - 1);
            new_b_building3.vaseActive = ((Random.Range(0, 1000000) % 100) > 50);
            new_b_building3.pillarActive = ((Random.Range(0, 1000000) % 100) > 50);
            new_b_building3.balconyActive = ((Random.Range(0, 1000000) % 100) > 50);
            
            border_buildings.Add(new_b_building3);

            city_hight_aux = city_hight_aux - (int)v_right_buildings_width;
            v_right_buildings_width = (Random.Range(1, 4) * 10);
            v_right_z = v_right_z - (new_b_building3.size.y * 0.5f) - (v_right_buildings_width * 0.5f);

            counter = counter + 1;
        }
        city_hight_aux = city_height;

        //counter = 0;
        while (city_width_aux > 0)
        {
            //BOTTOM
            Building new_b_building4 = new Building();
            new_b_building4.position = new Vector3(h_bot_x, 0.0f, h_bot_z);
            new_b_building4.size = new Vector2(buildings_height, h_bot_buildings_width);
            new_b_building4.angleRotation = 180.0f;
            new_b_building4.textureIndex = Random.Range(0, TEXTURE_NUM - 1);
            new_b_building4.colorIndex = Random.Range(0, COLOR_NUM - 1);
            new_b_building4.vaseActive = ((Random.Range(0, 1000000) % 100) > 50);
            new_b_building4.pillarActive = ((Random.Range(0, 1000000) % 100) > 50);
            new_b_building4.balconyActive = ((Random.Range(0, 1000000) % 100) > 50);
            
            border_buildings.Add(new_b_building4);

            city_width_aux = city_width_aux - (int)h_bot_buildings_width;
            h_bot_buildings_width = (Random.Range(1, 4) * 10);
            h_bot_x = h_bot_x - (new_b_building4.size.y * 0.5f) - (h_bot_buildings_width * 0.5f);
            counter = counter + 1;
        }
        city_width_aux = city_width;
    }

    protected void PlaceCityAssets()
    {
        /*We print the crosses.
         * 
         * We have to keep into account that crosses[i].size.x represets the hight of the cross, while   
         * crosses[i].size.y its width and that the prototype of the function GreCroosGameObject is the
         * following:
         * 
         * GameObject GetCrossGameObject(float height, float width, bool isRotated)
         * 
         */
        bool isRotated;
        worldMin = Vector3.zero;
        worldMax = Vector3.zero;
        for (int i = 0; i < crosses.Count; i++)
        {
            GameObject aux_cross = GetCrossGameObject(crosses[i].size.x, crosses[i].size.y, out isRotated);
            aux_cross.transform.parent = tokenParent;
            aux_cross.name = "cross_" + i;
            aux_cross.transform.position = new Vector3(crosses[i].position.x, 0.0f, crosses[i].position.z);
            if (isRotated)
            {
                crosses[i].angleRotation = 90.0f;
                aux_cross.transform.Rotate(Vector3.up * crosses[i].angleRotation);
            }
            aux_cross.AddComponent<RoadsTextureBehaviour>();
            crosses[i].gameObject = aux_cross;
            

            tokenParent.gameObject.BroadcastMessage("ApplyRoadsTextures", roadTextureId);


            LevelObject lo = aux_cross.AddComponent<LevelObject>();
            lo.Category = "tokens";

            Token tk = aux_cross.AddComponent<Token>();
            tk.isCenterPivot = true;
            tk.type = Token.TokenType.Cross;
            if (isRotated)
            {
                tk.lengthOrRadius = crosses[i].size.y;
                tk.width = crosses[i].size.x;
            }
            else
            {
                tk.lengthOrRadius = crosses[i].size.x;
                tk.width = crosses[i].size.y;
            }
            tk.OnMove();
            crosses[i].token = tk;
            
            Vector3 crossPosition = aux_cross.transform.position;
            if (worldMin.x > crossPosition.x || worldMin.z > crossPosition.z)
                worldMin = crossPosition;

            if (worldMax.x < crossPosition.x || worldMax.z < crossPosition.z)
                worldMax = crossPosition;
        }
        worldMin -= Vector3.one * 10.0f;
        worldMin.y = 0.0f;
        worldMax += Vector3.one * 10.0f;
        worldMax.y = 0.0f;

        /*
         * We print the horizontal roads:
         */
        int streetCounter = 1;
        for (int i = 0; i < h_roads.Count; i++)
        {
            GameObject aux_hRoad = GetRoadGameObject(h_roads[i].size.y, h_roads[i].size.x);
            aux_hRoad.transform.parent = tokenParent;
            aux_hRoad.name = "road_" + i;
            aux_hRoad.transform.position = new Vector3(h_roads[i].position.x, 0.0f, h_roads[i].position.z);
            h_roads[i].angleRotation = 90.0f;
            aux_hRoad.transform.Rotate(Vector3.up * h_roads[i].angleRotation);
            h_roads[i].gameObject = aux_hRoad;
            aux_hRoad.AddComponent<RoadsTextureBehaviour>();

            LevelObject lo = aux_hRoad.AddComponent<LevelObject>();
            lo.Category = "tokens";

            Token tk = aux_hRoad.AddComponent<Token>();
            tk.isCenterPivot = true;
            tk.type = Token.TokenType.Rect;
            tk.lengthOrRadius = h_roads[i].size.x;
            tk.width = h_roads[i].size.y;
            tk.OnMove();
            h_roads[i].token = tk;
            gameManagers.GetComponent<ObstaclesManager>().streetsList.Add(tk);

            streetCounter++;
        }


        /*
         * We print the vertical roads:
         */
        for (int i = 0; i < v_roads.Count; i++)
        {
            GameObject aux_vRoad = GetRoadGameObject(v_roads[i].size.y, v_roads[i].size.x);
            aux_vRoad.transform.parent = tokenParent;
            aux_vRoad.name = "road_" + (i + streetCounter - 1);
            aux_vRoad.transform.position = new Vector3(v_roads[i].position.x, 0.0f, v_roads[i].position.z);
            v_roads[i].gameObject = aux_vRoad;
            aux_vRoad.AddComponent<RoadsTextureBehaviour>();
            
            LevelObject lo = aux_vRoad.AddComponent<LevelObject>();
            lo.Category = "tokens";

            Token tk = aux_vRoad.AddComponent<Token>();
            tk.isCenterPivot = true;
            tk.type = Token.TokenType.Rect;
            tk.lengthOrRadius = v_roads[i].size.x;
            tk.width = v_roads[i].size.y;
            tk.OnMove();
            v_roads[i].token = tk;
            gameManagers.GetComponent<ObstaclesManager>().streetsList.Add(tk);

            streetCounter++;
        }

        tokenParent.gameObject.BroadcastMessage("ApplyRoadsTextures", roadTextureId);

        /*
         * We print the buildings:
         */
        int piano = 0;
        for (int i = 0; i < buildings.Count; i++)
        {
            bool isTower = i == (vRoads + 1) * vTowerCoord + hTowerCoord;
            string type = "building";
            if (isTower)
                type = "tower_building";

            buildings[i].groundFloor = GetBuildingGameObject(type, buildings[i].size.y, buildings[i].size.x);
            buildings[i].groundFloor.transform.parent = buildingParent;
            buildings[i].groundFloor.name = "building" + i + "_" + piano;
            buildings[i].groundFloor.transform.position = new Vector3(buildings[i].position.x, piano * 3.0f, buildings[i].position.z);

            //If in tutorial, last building is not rotate at all
            if (level != 0 || i != buildings.Count - 1)
                buildings[i].groundFloor.transform.Rotate(Vector3.up * buildings[i].angleRotation);

            ColorTextureBehaviour ctComp = buildings[i].groundFloor.AddComponent<ColorTextureBehaviour>();
            ctComp.textureIndex = buildings[i].textureIndex + 1;
            ctComp.colorIndex = buildings[i].colorIndex;

            if (!isTower)
            {
                BuildingAssets baComp = buildings[i].groundFloor.AddComponent<BuildingAssets>();
                baComp.size = buildings[i].size;
                baComp.rotation = buildings[i].angleRotation;
                baComp.vaseActive = buildings[i].vaseActive;
                baComp.pillarActive = buildings[i].pillarActive;
                baComp.balconyActive = buildings[i].balconyActive;
            }
            FootprintComponent footprint = buildings[i].groundFloor.AddComponent<FootprintComponent>();
            footprint.m_static = true;
        }

        for (int i = 0; i < border_buildings.Count; i++)
        {
            bool isTower = i == (vRoads + 1) * vTowerCoord + hTowerCoord;
            string type = "building";
            border_buildings[i].groundFloor = GetBuildingGameObject(type, border_buildings[i].size.y, border_buildings[i].size.x);
            border_buildings[i].groundFloor.transform.parent = buildingParent;
            border_buildings[i].groundFloor.name = "borderBuilding" + i;
            border_buildings[i].groundFloor.transform.position = new Vector3(border_buildings[i].position.x, piano * 3.0f, border_buildings[i].position.z);
            border_buildings[i].groundFloor.transform.Rotate(Vector3.up * (border_buildings[i].angleRotation));

            ColorTextureBehaviour ctComp = border_buildings[i].groundFloor.AddComponent<ColorTextureBehaviour>();
            ctComp.textureIndex = border_buildings[i].textureIndex + 1;
            ctComp.colorIndex = border_buildings[i].colorIndex;

            BuildingAssets baComp = border_buildings[i].groundFloor.AddComponent<BuildingAssets>();
            baComp.size = border_buildings[i].size;
            baComp.rotation = border_buildings[i].angleRotation;
            baComp.vaseActive = border_buildings[i].vaseActive;
            baComp.pillarActive = border_buildings[i].pillarActive;
            baComp.balconyActive = border_buildings[i].balconyActive;

            FootprintComponent footprint = border_buildings[i].groundFloor.AddComponent<FootprintComponent>();
            footprint.m_static = true;
        }
    }

    private int GetCrossIndexByGridCoord(int hCoord, int vCoord)
    {
        int verticalRoads = vRoads + 2;
        return (hCoord + verticalRoads * vCoord);
    }

    private void GetGridCoordByCrossIndex(int c, out int[] coords)
    {
        int verticalRoads = vRoads + 2;
        int horizontalRoads = hRoads + 2;

        coords = new int[2];
        coords[0] = c % verticalRoads;
        coords[1] = (c / verticalRoads) % horizontalRoads;
    }

    protected void BuildTokenGraph()
    {        
        int verticalRoads = vRoads + 2;     //6
        int horizontalRoads = hRoads + 2;   //5
        
        for (int c = 0; c < crosses.Count; c++)
        {
            Token crossToken = crosses[c].token;
            int[] coords;
            GetGridCoordByCrossIndex(c, out coords);
            int hIndex = coords[0]; //0..5
            int vIndex = coords[1]; //0..4
            
            if (vIndex == 0)
            {             
                crossToken.links[0] = v_roads[c].token;
                v_roads[c].token.links[2] = crossToken;
            }
            else if (vIndex < horizontalRoads - 1)
            {
                int index = GetCrossIndexByGridCoord(hIndex, vIndex - 1);
                crossToken.links[0] = v_roads[c].token;
                crossToken.links[2] = v_roads[index].token;
                v_roads[c].token.links[2] = crossToken;
                v_roads[index].token.links[0] = crossToken;
            }
            else
            {
                int index = GetCrossIndexByGridCoord(hIndex, vIndex - 1);
                crossToken.links[2] = v_roads[index].token;
                v_roads[index].token.links[0] = crossToken;
            }

            if (hIndex == 0)
            {
                int index = (vIndex * (verticalRoads - 1) + hIndex);
                crossToken.links[1] = h_roads[index].token;
                h_roads[index].token.links[2] = crossToken;
            }
            else if (hIndex < verticalRoads - 1)
            {
                int index = (vIndex * (verticalRoads - 1) + hIndex);
                int index2 = (vIndex * (verticalRoads - 1) + (hIndex - 1));
                crossToken.links[1] = h_roads[index].token;
                h_roads[index].token.links[2] = crossToken;
                crossToken.links[3] = h_roads[index2].token;
                h_roads[index2].token.links[0] = crossToken;
            }
            else
            {
                int index2 = (vIndex * (verticalRoads - 1) + (hIndex - 1));
                crossToken.links[3] = h_roads[index2].token;
                h_roads[index2].token.links[0] = crossToken;
            }
        }
    }

    protected void BuildFloorList()
    {
        List<Building> completeList = new List<Building>();
        completeList.AddRange(buildings);
        completeList.AddRange(border_buildings);
        
        List<int> floorListTmp = new List<int>();
        
        //We create a list of FLOORS based on their distribuition as specified in config.xml
        int totBuildings = completeList.Count;
        int totBuildingsAux = 0;
        int numberOfTypes = buildingsPercentage.Count;
        foreach (StringPerc p in buildingsPercentage)
        {
            int card = (p.perc * totBuildings) / 100;
            int i = 0;
            while (i < card && totBuildingsAux <= totBuildings)
            {
                floorListTmp.Add((int)int.Parse(p.item));
                totBuildingsAux = totBuildingsAux + 1;
                i = i + 1;
            }
            if (totBuildingsAux > totBuildings) break;
        }

        for (int i = 0; i < totBuildings - totBuildingsAux; i++) //totRoads - totRoadsAux <=  NumberOfTypes
            floorListTmp.Add(int.Parse(buildingsPercentage[i % numberOfTypes].item));

        /*
         * We shuffle the list of building's floors
         */
        int rn; //Random_Number 
        int buildingsCard = floorListTmp.Count;
        floorList.Clear();
        for (int i = 0; i < buildingsCard; i++)
        {
            rn = Random.Range(0, floorListTmp.Count - 1);
            floorList.Add(floorListTmp[rn]);
            //InternalConsole.Instance.Log("N. Piani:" + floorList[rn] + " ");
            floorListTmp.RemoveAt(rn);
        }
    }

    protected void PlaceBuildingsFloors()
    {
        int index = (vRoads + 1) * vTowerCoord + hTowerCoord;
        Building aux_building = buildings[index];
        //Building aux_building = buildings[Random.Range(0, 1000000) % buildings.Count];

        //Building with 6 floors
        float baseY_2 = 4.0f;
        int floorNumber = 4;
        for (int k = 0; k < floorNumber; k++)
        {
            GameObject bb = GetBuildingGameObject("tower_floor", aux_building.size.x, aux_building.size.y);
            bb.transform.parent = aux_building.groundFloor.transform;
            bb.transform.position = new Vector3(aux_building.position.x, baseY_2 + 3.0f * k, aux_building.position.z);
            bb.transform.Rotate(Vector3.up * aux_building.angleRotation);
            // bb.AddComponent<ColorTextureBehaviour>();
            aux_building.middleFloors.Add(bb);
        }

        GameObject roof_2 = GetBuildingGameObject("tower_roof", aux_building.size.x, aux_building.size.y);
        roof_2.transform.parent = aux_building.groundFloor.transform;
        roof_2.transform.position = new Vector3(aux_building.position.x, baseY_2 + 3.0f * floorNumber, aux_building.position.z);
        roof_2.transform.Rotate(Vector3.up * aux_building.angleRotation);
        aux_building.roof = roof_2;

        //aux_building.groundFloor.GetComponent<BuildingAssets>().LoadAssets();


        List<Building> completeList = new List<Building>();
        completeList.AddRange(buildings);
        completeList.AddRange(border_buildings);

        //Prova
        //InternalConsole.Instance.Log("Numero palazzi prima:" + completeList.Count + " Numero palazzi dopo:" + shuffledBuildings.Count);
        
        //TODO: ciclare sui Buildings e aggiungere i piani
        int j = 0; //Index of the j-th building
        foreach (Building b in completeList)
        {
            if (!b.Equals(aux_building))
            {
                int i = 0;
                float baseY = 4.0f;
                for (i = 0; i < floorList[j]; i++)
                {
                    GameObject bb = GetBuildingGameObject("floor", b.size.x, b.size.y);
                    bb.transform.parent = b.groundFloor.transform;
                    bb.transform.position = new Vector3(b.position.x, baseY + 3.0f * i, b.position.z);
                    bb.transform.Rotate(Vector3.up * b.angleRotation);
                    // bb.AddComponent<ColorTextureBehaviour>();
                    b.middleFloors.Add(bb);
                }

                GameObject roof = GetBuildingGameObject("roof", b.size.x, b.size.y);
                roof.transform.parent = b.groundFloor.transform;
                roof.transform.position = new Vector3(b.position.x, baseY + 3.0f * i, b.position.z);
                roof.transform.Rotate(Vector3.up * b.angleRotation);
                b.roof = roof;

                b.groundFloor.GetComponent<BuildingAssets>().LoadAssets();
                j++;
            }
        }

        buildingParent.gameObject.BroadcastMessage("ApplyColours");
    }

    protected void CombineBuidingMeshes()
    {
        List<Building> completeList = new List<Building>();
        completeList.AddRange(buildings);
        completeList.AddRange(border_buildings);

        foreach (Building b in completeList)
            StaticBatchingUtility.Combine(b.groundFloor);
    }

    protected bool CheckIfTargetPlaceable(GameObject obj, TokensManager.TokenHit tokenHit)
    {
        bool firstSide = (tokenHit.token.type != Token.TokenType.Cross);
        bool secondSide = false;
        bool center = false;
        if (firstSide)
        {
            Vector3 centerPoint = obj.transform.TransformPoint(Vector3.right * 2.0f);
            TokensManager.TokenHit centerHit;
            if (TokensManager.Instance.GetToken(centerPoint, out centerHit))
                center = (centerHit.token.type != Token.TokenType.Cross);

            if (center)
            {
                Vector3 secondPoint = obj.transform.TransformPoint(Vector3.right * 4.0f);
                TokensManager.TokenHit secondHit;
                if (TokensManager.Instance.GetToken(secondPoint, out secondHit))
                    secondSide = (secondHit.token.type != Token.TokenType.Cross);

                return secondSide;
            }
        }
        return false;
    }

    protected void DefineCityTargets()
    {
        /* We create a dictionary that associate to each token the list of targets/buildings on it, 
         * and a dictionary that associate to each target the only building it belogs to */
        int targetMaxNum = 10;
        GameplayManager gameplay = gameManagers.GetComponent<GameplayManager>();
        target = int.MaxValue; // gameplay.GetTargetByLevel(level);

        GameObject obj;
        foreach (Building bb in buildings)
        {
            for (int i = 0; i < bb.groundFloor.transform.childCount; i++)
            {
                obj = bb.groundFloor.transform.GetChild(i).gameObject;
                if (obj.name.StartsWith("target"))
                {
                    bool check = TokensManager.Instance.GetToken(obj.transform.position, out currentTokenHit);
                    if (check)
                    {
                        if (CheckIfTargetPlaceable(obj, currentTokenHit))
                        {
                            if (tokenTargets.ContainsKey(currentTokenHit.token))
                            {
                                List<GameObject> listOfTargets;
                                tokenTargets.TryGetValue(currentTokenHit.token, out listOfTargets);
                                listOfTargets.Add(obj);
                                tokenTargets[currentTokenHit.token] = listOfTargets;
                            }
                            else
                            {
                                List<GameObject> listOfTargets = new List<GameObject>();
                                listOfTargets.Add(obj);
                                tokenTargets.Add(currentTokenHit.token, listOfTargets);
                            }
                            targetBuilding.Add(obj, bb);
                        }
                        else
                        {
                            Debug.Log("### EnvironmentManager: " + obj.name + ", " + bb.groundFloor.name);
                        }
                    }
                }
            }
        }

        foreach (Building bb in border_buildings)
        {
            for (int i = 0; i < bb.groundFloor.transform.childCount; i++)
            {
                obj = bb.groundFloor.transform.GetChild(i).gameObject;
                if (obj.name.StartsWith("target"))
                {
                    bool check = TokensManager.Instance.GetToken(obj.transform.position, out currentTokenHit);
                    if (check)
                    {
                        if (CheckIfTargetPlaceable(obj, currentTokenHit))
                        {
                            if (tokenTargets.ContainsKey(currentTokenHit.token))
                            {
                                List<GameObject> listOfTargets;
                                tokenTargets.TryGetValue(currentTokenHit.token, out listOfTargets);
                                listOfTargets.Add(obj);
                                tokenTargets[currentTokenHit.token] = listOfTargets;
                            }
                            else
                            {
                                List<GameObject> listOfTargets = new List<GameObject>();
                                listOfTargets.Add(obj);
                                tokenTargets.Add(currentTokenHit.token, listOfTargets);
                            }
                            targetBuilding.Add(obj, bb);
                        }
                        else
                        {
                            Debug.Log("### EnvironmentManager: " + obj.name + ", " + bb.groundFloor.name);
                        }
                    }
                }
            }
        }

        List<GameObject> listOfTargets2 = new List<GameObject>();
        List<GameObject>[] hTargets = new List<GameObject>[hRoads + 2];
        List<GameObject>[] vTargets = new List<GameObject>[vRoads + 2];
        for (int i = 0; i < hRoads + 2; i++)
            hTargets[i] = new List<GameObject>();
        for (int i = 0; i < vRoads + 2; i++)
            vTargets[i] = new List<GameObject>();

        if (tokenTargets.Count > 0)
        {
            for (int i = 0; i < hRoads + 2; i++)
            {
                for (int j = 0; j < vRoads + 1; j++)
                {
                    if (tokenTargets.ContainsKey(h_roads[j + i * (vRoads + 1)].token))
                    {
                        tokenTargets.TryGetValue(h_roads[j + i * (vRoads + 1)].token, out listOfTargets2);
                        foreach (GameObject ob in listOfTargets2)
                            hTargets[i].Add(ob);
                    }
                }
            }

            //  listOfTargets2.Clear();
            for (int i = 0; i < hRoads + 1; i++)
            {
                for (int j = 0; j < vRoads + 2; j++)
                {
                    if (tokenTargets.ContainsKey(v_roads[j + i * (vRoads + 2)].token))
                    {
                        tokenTargets.TryGetValue(v_roads[j + i * (vRoads + 2)].token, out listOfTargets2);
                        foreach (GameObject ob in listOfTargets2)
                            vTargets[j].Add(ob);
                    }
                }
            }

            //Tutorial
            int startCounter = 0;
            if (level == 0)
                startCounter = 1;
            //Tutorial

            List<int> randomSupport = new List<int>();
            for (int i = startCounter; i < targetMaxNum; i++)
                randomSupport.Add(i + 1);

            int aux_hRoads = 0;
            int aux_vRoads = 0;

            for (int roadCounter = 0; roadCounter < hRoads + 2 + vRoads + 2; roadCounter++)
            {
                if (randomSupport.Count <= 0)
                    break;

                int randomIdx = (Random.Range(0, 10000) % randomSupport.Count);
                int idTexture = randomSupport[randomIdx];
                randomSupport.RemoveAt(randomIdx);

                bool horizontal = (roadCounter % 2 == 0);
                if ((horizontal && aux_hRoads >= hRoads + 2) || (!horizontal && aux_vRoads >= vRoads + 2))
                    horizontal = !horizontal;

                if (horizontal)
                {
                    if (hTargets[aux_hRoads].Count > 0)
                    {
                        int idTarget = 0;
                        if (hTargets[aux_hRoads].Count > 1)
                            idTarget = Random.Range(0, 10000) % hTargets[aux_hRoads].Count;

                        GameObject currTarget = hTargets[aux_hRoads][idTarget];
                        targetBuilding[currTarget].targets.Add(new Tuple<string, int>(currTarget.name, idTexture));
                        aux_hRoads = aux_hRoads + 1;
                    }
                }
                else
                {
                    if (vTargets[aux_vRoads].Count > 0)
                    {
                        int idTarget = 0;
                        if (vTargets[aux_vRoads].Count > 1)
                            idTarget = Random.Range(0, 10000) % vTargets[aux_vRoads].Count;

                        GameObject currTarget = vTargets[aux_vRoads][idTarget];
                        targetBuilding[currTarget].targets.Add(new Tuple<string, int>(currTarget.name, idTexture));
                        aux_vRoads = aux_vRoads + 1;
                    }
                }
            }
        }

        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnTargetPlaced));
    }

    protected void PlaceCityTargets()
    {
        GameplayManager gameplay = gameManagers.GetComponent<GameplayManager>();
        gameplay.targetList.Clear();

        List<Building> completeList = new List<Building>();
        completeList.AddRange(buildings);
        completeList.AddRange(border_buildings);

        foreach (Building b in completeList)
        {
            GameObject groundFloor = b.groundFloor;
            foreach (Tuple<string, int> tuple in b.targets)
            {
                int aux = int.Parse(tuple.First.Substring(6, 1));
                foreach (Material mat in groundFloor.GetComponent<Renderer>().materials)
                {
                    if (mat.name.Contains("buildingShop" + aux.ToString()))
                    {
                        Texture2D texture = gameManagers.GetComponent<ExternalTexturesManager>().GetTextureByName("buildingShop" + tuple.Second.ToString());
                        mat.mainTexture = texture;
                        mat.SetTexture("_Illum", texture);
                    }
                }

                for (int i = 0; i < groundFloor.transform.childCount; i++)
                {
                    GameObject child = groundFloor.transform.GetChild(i).gameObject;
                    if (child.name.Equals(tuple.First))
                    {
                        GameplayManager.Target targetItem = new GameplayManager.Target();
                        targetItem.gameObject = child;
                        targetItem.type = (GameplayManager.TargetType)tuple.Second;
                        gameplay.targetList.Add(targetItem);
                    }
                }
            }
        }

        //TUTORIAL:
        if (level == 0)
        {
            GameObject lastBuilding = buildings[buildings.Count - 1].groundFloor;

            GameplayManager.Target targetItem = new GameplayManager.Target();
            targetItem = new GameplayManager.Target();
            targetItem.type = GameplayManager.TargetType.Bakery;
            targetItem.gameObject = lastBuilding.transform.FindChild("target1").gameObject;
            gameplay.tutorialTarget = targetItem;

            foreach (Material mat in lastBuilding.GetComponent<Renderer>().materials)
            {
                if (mat.name.Contains("buildingShop1"))
                {
                    Texture2D texture = gameManagers.GetComponent<ExternalTexturesManager>().GetTextureByName("buildingShop1");
                    mat.mainTexture = texture;
                    mat.SetTexture("_Illum", texture);
                }
            }
        }
    }
    #endregion

    #region Public Members
    protected List<GameObject> walls = new List<GameObject>();
    public void EnableWallOnToken(Token token, List<int> indexList)
    {
        walls.Clear();
        for (int i = 0; i < indexList.Count; i++)
        {
            float lenght = token.Length;
            float width = token.width;

            if (token.gameObject.transform.rotation.y > 0)
            {
                lenght = token.width;
                width = token.Length;
            }

            GameObject plane = new GameObject();
            plane.layer = LayerMask.NameToLayer("Character");
            plane.name = "wall" + i;
            BoxCollider collider = plane.AddComponent<BoxCollider>();
            float wallWidth = 0.1f;
            collider.size = new Vector3(10.0f, 2.0f, wallWidth);
            collider.center = new Vector3(0.0f, 1.0f, 0.0f);

            float dist = 0.49f; //distance of the wall to the token center
            switch (indexList[i])
            {
                case (0): //forward
                    plane.transform.position = token.transform.position + ((lenght + wallWidth * 0.5f) * dist) * Vector3.forward;
                    break;
                case (1): //right
                    plane.transform.position = token.transform.position + ((width + wallWidth * 0.5f) * dist) * Vector3.right;
                    break;
                case (2): //back
                    plane.transform.position = token.transform.position + ((lenght + wallWidth * 0.5f) * dist) * Vector3.back;
                    break;
                case (3): //left
                    plane.transform.position = token.transform.position + ((width + wallWidth * 0.5f) * dist) * Vector3.left;
                    break;
            }
            plane.transform.LookAt(token.transform.position, Vector3.up);
            walls.Add(plane);
        }
    }

    public void DisableWallsOnToken()
    {
        //return;
        for (int i = 0; i < walls.Count; i++)
            GameObject.Destroy(walls[i]);
        walls.Clear();
    }

    public Token[] GetTutorialTokens()
    {
        Token[] startTokens = new Token[6];
        LevelObject[] tokens = LevelRoot.Instance.Query("tokens");
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i].gameObject.name.Equals("road_9"))
            {
                startTokens[0] = tokens[i].gameObject.GetComponent<Token>();
                startTokens[1] = tokens[i].gameObject.GetComponent<Token>();
                startTokens[2] = tokens[i].gameObject.GetComponent<Token>();
                startTokens[3] = tokens[i].gameObject.GetComponent<Token>();
            }
            if (tokens[i].gameObject.name.Equals("road_6"))
                startTokens[4] = tokens[i].gameObject.GetComponent<Token>();
            if (tokens[i].gameObject.name.Equals("road_11"))
                startTokens[5] = tokens[i].gameObject.GetComponent<Token>();
        }

        return startTokens;

        //Token[] startTokens = new Token[3];
        //LevelObject[] tokens = LevelRoot.Instance.Query("tokens");
        //for (int i = 0; i < tokens.Length; i++)
        //{
        //    if (tokens[i].gameObject.name.Equals("road_9"))
        //        startTokens[0] = tokens[i].gameObject.GetComponent<Token>();
        //    if (tokens[i].gameObject.name.Equals("road_11"))
        //        startTokens[1] = tokens[i].gameObject.GetComponent<Token>();
        //    if (tokens[i].gameObject.name.Equals("road_6"))
        //        startTokens[2] = tokens[i].gameObject.GetComponent<Token>();
        //}

        //return startTokens;
    }

    public void GetAvailableStartPosition(int tutorialPhase, int gate, out SBSVector3 pos, out SBSVector3 tang)
    {
        float longitudinal = 0.05f;
        float trasversal = 0.0f;
        
        Token token;
        bool isReverse = false;

        if (this.level == 0)
        {
            //TUTORIAL
            pos = SBSVector3.zero;
            tang = SBSVector3.forward;

            Token[] startTokens = GetTutorialTokens();
            token = startTokens[tutorialPhase - 1];

            if (tutorialPhase == 6)
                trasversal = 0.6f;
        }
        else
        {
            int crossNum = crosses.Count;
            int middleH = (hRoads + 2) / 2;

            int southIndex = (vRoads + 2) / 2;
            int westIndex = (crossNum / (hRoads + 2)) * middleH;
            int northIndex = crossNum - ((vRoads + 2) / 2);
            int eastIndex = ((crossNum / (hRoads + 2)) * (middleH + 1)) - 1;

            int[] gateCrosses = { southIndex, westIndex, northIndex, eastIndex };
            int randomIdx = gate;
            if (gate < 0)
                randomIdx = Random.Range(0, 10000) % gateCrosses.Length;
            int crossIndex = gateCrosses[randomIdx];

            token = crosses[crossIndex].token.links[randomIdx];
            isReverse = randomIdx > 1;

            if (isReverse)
                longitudinal = 0.95f;

            /*List<Road> totalRoads = new List<Road>();
            totalRoads.AddRange(v_roads);
            totalRoads.AddRange(h_roads);

            int randomIdx = Random.Range(0, 10000) % totalRoads.Count;
            token = totalRoads[randomIdx].token;*/
        }

        token.TokenToWorld(longitudinal, trasversal, out pos, out tang);

        if (isReverse)
            tang = -tang;

        gameManagers.BroadcastMessage("CurrentToken", token);
    }
    #endregion

    #region Protected Members
    protected void GenerateCityMap()
    {
        Color empty = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        Color fill = new Color(1.0f, 1.0f, 1.0f, 0.75f);
        Color stroke = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        Vector3 mapMin = crosses[0].position - new Vector3(crosses[0].size.x * 0.5f, 0.0f, crosses[0].size.y * 0.5f);
        Vector3 mapMax = crosses[crosses.Count - 1].position + new Vector3(crosses[crosses.Count - 1].size.x * 0.5f, 0.0f, crosses[crosses.Count - 1].size.y * 0.5f);

        int textureWidth = Mathf.CeilToInt(mapMax.x - mapMin.x);
        int textureHeight = Mathf.CeilToInt(mapMax.z - mapMin.z);

        Texture2D mapTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
        mapTexture.filterMode = FilterMode.Bilinear;

        Color[] texturePixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < texturePixels.Length; i++)
            texturePixels[i] = empty;
        mapTexture.SetPixels(texturePixels);

        //foreach (Cross cross in crosses)
        foreach (Road cross in crosses)
        {
            int pixelMinX = (int)((cross.position.x - cross.size.y * 0.5f) - mapMin.x);
            int pixelMaxX = (int)((cross.position.x + cross.size.y * 0.5f) - mapMin.x);
            int pixelMinY = (int)((cross.position.z - cross.size.x * 0.5f) - mapMin.z);
            int pixelMaxY = (int)((cross.position.z + cross.size.x * 0.5f) - mapMin.z);

            for (int i = pixelMinX; i < pixelMaxX; i++)
            {
                for (int j = pixelMinY; j < pixelMaxY; j++)
                {
                    mapTexture.SetPixel(i, j, fill);
                }
            }
        }

        foreach (Road road in v_roads)
        {
            int pixelMinX = (int)((road.position.x - road.size.y * 0.5f) - mapMin.x);
            int pixelMaxX = (int)((road.position.x + road.size.y * 0.5f) - mapMin.x);
            int pixelMinY = (int)((road.position.z - road.size.x * 0.5f) - mapMin.z);
            int pixelMaxY = (int)((road.position.z + road.size.x * 0.5f) - mapMin.z);

            for (int i = pixelMinX; i < pixelMaxX; i++)
            {
                for (int j = pixelMinY; j < pixelMaxY; j++)
                {
                    mapTexture.SetPixel(i, j, fill);
                }
            }
        }

        foreach (Road road in h_roads)
        {
            int pixelMinX = (int)((road.position.x - road.size.x * 0.5f) - mapMin.x);
            int pixelMaxX = (int)((road.position.x + road.size.x * 0.5f) - mapMin.x);
            int pixelMinY = (int)((road.position.z - road.size.y * 0.5f) - mapMin.z);
            int pixelMaxY = (int)((road.position.z + road.size.y * 0.5f) - mapMin.z);

            for (int i = pixelMinX; i < pixelMaxX; i++)
            {
                for (int j = pixelMinY; j < pixelMaxY; j++)
                {
                    mapTexture.SetPixel(i, j, fill);
                }
            }
        }
        mapTexture.Apply();

        CQ_Interface ui = GameObject.FindGameObjectWithTag("Interface").GetComponent<CQ_Interface>();
        ui.SetupMap(mapTexture, new Vector2(mapMin.x, mapMin.z), new Vector2(mapMax.x, mapMax.z));
    }

    protected void GeneratePedestrian(int pedNum, PathManagerComponent pathManagerComponent)
    {
        float avgDistance = 20.0f;
        Vector3 playerPos = gameManagers.GetComponent<GameplayManager>().Character.transform.position;

        //List<Cross> supportList = new List<Cross>();
        List<Road> supportList = new List<Road>();
        supportList.AddRange(crosses);
        //supportList.AddRange(v_roads);
        //supportList.AddRange(h_roads);

        int maxPedPerCross = 2 * pedNum / supportList.Count;

        for (int c = 0; c < supportList.Count; c++)
            supportList[c].manhattan = Mathf.Abs(supportList[c].position.x - playerPos.x) + Mathf.Abs(supportList[c].position.z - playerPos.z);

        supportList.Sort();

        int count = 0;
        int num = pedNum;
        for (int c = 0; c < supportList.Count; c++)
        {
            float manhattan = Mathf.Abs(supportList[c].position.x - playerPos.x) + Mathf.Abs(supportList[c].position.z - playerPos.z);
            int weight = Mathf.FloorToInt(manhattan / avgDistance);

            int totalStep = Mathf.Max(maxPedPerCross - weight, 1);

            //int randomPed = Random.Range(1, 3);
            for (int i = 0; i < totalStep; i++)
            {
                if (num <= 0)
                    break;

                int ti = i % 4;
                int li = i / 4;

                //float randomLongitudinal = Random.Range(1, 90) * 0.01f;
                //float randomTrasversal = Random.Range(-80, 80) * 0.01f;
                float randomLongitudinal = 0.2f + li * 0.2f;
                float randomTrasversal = (0.4f + ti * 0.4f) - 1.0f;

                Token tk = supportList[c].token;
                SBSVector3 pos, tang;
                tk.TokenToWorld(randomLongitudinal, randomTrasversal, out pos, out tang);

                GameObject actor = GameObject.Instantiate(pedestrianList[Random.Range(0, 10000) % pedestrianList.Length]) as GameObject;
                SteeringAgentComponent steering = actor.GetComponent<SteeringAgentComponent>();
                float randomFactor = Random.Range(80.0f, 120.0f) * 0.01f;
                steering.m_maxSpeed *= randomFactor;
                actor.GetComponent<PathAgentComponent>().m_pathManager = pathManagerComponent;
                actor.name = "Pedestrian" + count;
                actor.transform.parent = pedestrianParent;
                actor.transform.position = pos + (SBSVector3.up * 1.3f);
                count++;
                num--;
            }

            if (num <= 0)
                break;
        }

        if (num > 0)
            Debug.LogWarning("NOT ALL PEDESTRIANS ARE PLACED: " + num);
    }

    protected Color GetRandomColor()
    {
        return colorList[Random.Range(0, colorList.Count)];
    }

    protected float GetAngleRotation(float height, float width, bool randomRotation)
    {
        float angleRotation = 0.0f;
        int label1 = (int)height / 10;
        int label2 = (int)width / 10;
        bool isRotated = label1 < label2;
        if (!isRotated)
        {
            int tmp = label1;
            label1 = label2;
            label2 = tmp;
            angleRotation = 0.0f;
            int rnd = Random.Range(0, 100);
            if (rnd > 50 && randomRotation)
                angleRotation = 180.0f;
        }
        else
        {
            angleRotation = 90.0f;
            int rnd = Random.Range(0, 100);
            if (rnd > 50 && randomRotation)
                angleRotation = 270.0f;
        }
        return angleRotation;
    }

    //protected GameObject GetBuildingGameObject(string type, float height, float width, bool randomRotation, out float angleRotation)
    protected GameObject GetBuildingGameObject(string type, float height, float width)
    {
        int label1 = (int)height / 10;
        int label2 = (int)width / 10;
        bool isRotated = label1 < label2;
        if (!isRotated)
        {
            int tmp = label1;
            label1 = label2;
            label2 = tmp;
        }

        string key = type + "_" + label1.ToString() + "x" + label2.ToString();
        GameObject aux = (GameObject)GameObject.Instantiate(buildingsSrc[key]);
        return aux;
    }

    protected GameObject GetCrossGameObject(float height, float width, out bool isRotated)
    {
        GameObject aux;
        isRotated = true;
        switch ((int)height)
        {
            case 3:
                {
                    switch ((int)width)
                    {
                        case 3: { aux = (GameObject)GameObject.Instantiate(tokenAxA); isRotated = false; break; }
                        case 5: { aux = (GameObject)GameObject.Instantiate(tokenAxB); isRotated = true; break; }
                        case 7: { aux = (GameObject)GameObject.Instantiate(tokenAxC); isRotated = true; break; }
                        case 9: { aux = (GameObject)GameObject.Instantiate(tokenAxD); isRotated = true; break; }
                        default: aux = (GameObject)GameObject.Instantiate(tokenAxA); break;
                    }
                    break;
                }
            case 5:
                {
                    switch ((int)width)
                    {
                        case 3: { aux = (GameObject)GameObject.Instantiate(tokenAxB); isRotated = false; break; }
                        case 5: { aux = (GameObject)GameObject.Instantiate(tokenBxB); isRotated = false; break; }
                        case 7: { aux = (GameObject)GameObject.Instantiate(tokenBxC); isRotated = true; break; }
                        case 9: { aux = (GameObject)GameObject.Instantiate(tokenBxD); isRotated = true; break; }
                        default: aux = (GameObject)GameObject.Instantiate(tokenAxA); break;
                    }
                    break;
                }
            case 7:
                {
                    switch ((int)width)
                    {
                        case 3: { aux = (GameObject)GameObject.Instantiate(tokenAxC); isRotated = false; break; }
                        case 5: { aux = (GameObject)GameObject.Instantiate(tokenBxC); isRotated = false; break; }
                        case 7: { aux = (GameObject)GameObject.Instantiate(tokenCxC); isRotated = false; break; }
                        case 9: { aux = (GameObject)GameObject.Instantiate(tokenCxD); isRotated = true; break; }
                        default: aux = (GameObject)GameObject.Instantiate(tokenAxA); break;
                    }
                    break;
                }
            case 9:
                {
                    switch ((int)width)
                    {
                        case 3: { aux = (GameObject)GameObject.Instantiate(tokenAxD); isRotated = false; break; }
                        case 5: { aux = (GameObject)GameObject.Instantiate(tokenBxD); isRotated = false; break; }
                        case 7: { aux = (GameObject)GameObject.Instantiate(tokenCxD); isRotated = false; break; }
                        case 9: { aux = (GameObject)GameObject.Instantiate(tokenDxD); isRotated = false; break; }
                        default: aux = (GameObject)GameObject.Instantiate(tokenAxA); break;
                    }
                    break;
                }
            default: aux = (GameObject)GameObject.Instantiate(tokenAxA); break;
        }
        aux.AddComponent<MeshCollider>();
        return aux;
    }

    protected GameObject GetRoadGameObject(float height, float width)
    {
        //InternalConsole.Instance.Log(height + ", ," + width + " ");
        GameObject aux;
        switch ((int)height)
        {
            case 3:
                {
                    switch ((int)width)
                    {
                        case 10: { aux = (GameObject)GameObject.Instantiate(tokenAx1); break; }
                        case 20: { aux = (GameObject)GameObject.Instantiate(tokenAx2); break; }
                        case 30: { aux = (GameObject)GameObject.Instantiate(tokenAx3); break; }
                        default: { aux = (GameObject)GameObject.Instantiate(tokenAxA); break; }
                    }
                    break;
                }
            case 5:
                {
                    switch ((int)width)
                    {
                        case 10: { aux = (GameObject)GameObject.Instantiate(tokenBx1); break; }
                        case 20: { aux = (GameObject)GameObject.Instantiate(tokenBx2); break; }
                        case 30: { aux = (GameObject)GameObject.Instantiate(tokenBx3); break; }
                        default: { aux = (GameObject)GameObject.Instantiate(tokenBxB); break; }
                    }
                    break;
                }
            case 7:
                {
                    switch ((int)width)
                    {
                        case 10: { aux = (GameObject)GameObject.Instantiate(tokenCx1); break; }
                        case 20: { aux = (GameObject)GameObject.Instantiate(tokenCx2); break; }
                        case 30: { aux = (GameObject)GameObject.Instantiate(tokenCx3); break; }
                        default: { aux = (GameObject)GameObject.Instantiate(tokenCxC); break; }
                    }
                    break;
                }
            case 9:
                {
                    switch ((int)width)
                    {
                        case 10: { aux = (GameObject)GameObject.Instantiate(tokenDx1); break; }
                        case 20: { aux = (GameObject)GameObject.Instantiate(tokenDx2); break; }
                        case 30: { aux = (GameObject)GameObject.Instantiate(tokenDx3); break; }
                        default: { aux = (GameObject)GameObject.Instantiate(tokenDxD); break; }
                    }
                    break;
                }
            default: aux = (GameObject)GameObject.Instantiate(tokenAxA); break;
        }
        aux.AddComponent<MeshCollider>();
        return aux;
    }
    #endregion
}
