using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FoF.Utils;
using SBS.XML;
using SBS.Core;
using SBS.Math;
using System.Xml;

public class ObstaclesManager : MonoBehaviour
{
    public static float SafeLenght = 2.5f;
    public static float ObstacleOccupation = 5.0f;

    #region Internal Data Structure
    public enum ProgressSignalType
    {
        OnObstacleXmlParsed = 0,
        OnXmlError,
        OnObstaclePlaced,
        OnObstacleDestroyed
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

    public enum ObstacleClass
    {
        Static = 0,
        Dynamic,
        GroupStatic,
        GroupDynamic
    }

    public class PlaceHolder
    {
        public Token token;
        public Vector3 position;
        public ObstacleItem item;
    }

    public class LevelConfig
    {
        public int staticObstacleNum = 0;
        public int dynamicObstacleNum = 0;
        public DistributionType staticDistribution = DistributionType.City;
        public DistributionType dynamicDistribution = DistributionType.City;
        public int soundVolume = 100;
        public bool staticSoundEnabled = false;
        public bool dynamicSoundEnabled = false;

        public bool soundEnabled = false;
        public List<StringPerc> occurences = new List<StringPerc>();
        public Dictionary<string, int> obstacleWeights = new Dictionary<string, int>();
        public Dictionary<string, float> obstacleAlphas = new Dictionary<string, float>();
        public Dictionary<string, float> obstacleSpeeds = new Dictionary<string, float>();
    }

    [System.Serializable]
    public class ObstacleItem
    {
        public GameObject obstacle;
        public ObstacleClass obsClass;
        public int percentage;
        public float minTrasversal;
        public float maxTrasversal;
        public float alpha;
        public float speed;

        private float weight = 0.0f;
        private int totalInScene = 0;

        public float Weight { get { return weight; } set { weight = value; } }
        public int TotalInScene { get { return totalInScene; } set { totalInScene = value; } }
    }
    #endregion

    #region Public Fields
    public SignalSender progressSignals;
    public List<Obstacle> obstaclesList = new List<Obstacle>();
    public List<Token> streetsList = new List<Token>();
    public ObstacleItem[] obstacleSources;
    public ObstacleItem[] dinamicSources;
    public List<int> volumesByLevel = new List<int>();
    public List<LevelConfig> levelConfigs = new List<LevelConfig>();
    #endregion

    #region Protected Fields
    protected int currentParsedLevel = 1;
    protected GameObject obstacle;
    protected GameObject obstacle2;
    protected GameObject gameManagers;
    protected Transform obstacleParent;
    protected List<List<PlaceHolder>> obstaclesPositionsList = new List<List<PlaceHolder>>();
    protected List<PlaceHolder> obstaclesPositions = new List<PlaceHolder>();
    protected List<Token> activeTokens = new List<Token>();
    protected Dictionary<string, ObstacleItem> obstacleItemsByName = new Dictionary<string, ObstacleItem>();
    protected Dictionary<Token, List<Obstacle>> obstaclesListbyToken = new Dictionary<Token, List<Obstacle>>();
    protected Dictionary<Token, List<PlaceHolder>> placeHolderListbyToken = new Dictionary<Token, List<PlaceHolder>>();
    protected GameplayManager gameplayManager = null;
    #endregion

    #region Public Get/Set
    public List<List<PlaceHolder>> ObstaclesPositionsList
    {
        get
        {
            return obstaclesPositionsList;
        }
    }

    public Dictionary<string, ObstacleItem> ObstacleItemsByName
    {
        get
        {
            return obstacleItemsByName;
        }
    }
    #endregion

    #region Public Members
    public List<Obstacle> ObstacleListByToken(Token token)
    {
        if (obstaclesListbyToken.ContainsKey(token))
            return obstaclesListbyToken[token];
        return new List<Obstacle>();
    }

    public void RemoveObstacleOnToken(Token token)
    {
        if (!obstaclesListbyToken.ContainsKey(token))
            return;

        List<Obstacle> toBeRemoved = obstaclesListbyToken[token];
        foreach (Obstacle obsComp in toBeRemoved)
        {
            obstaclesList.Remove(obsComp);
            DestroyImmediate(obsComp.gameObject);
        }
        obstaclesListbyToken.Remove(token);
    }

    public int GetSoundVolume(int level)
    {
        if (level == 0)
            return 100;
        return volumesByLevel[level - 1];
    }
    #endregion

    #region Unity Callbacks
    void Start()
    {
        for (int i = 0; i < obstacleSources.Length; i++)
            obstacleItemsByName.Add(obstacleSources[i].obstacle.name, obstacleSources[i]);
        for (int i = 0; i < dinamicSources.Length; i++)
            obstacleItemsByName.Add(dinamicSources[i].obstacle.name, dinamicSources[i]);
    }
    #endregion

    #region Messages
    void LoadConfiguration(string configText)
    {
        gameManagers = GameObject.FindGameObjectWithTag("GameManagers");
        gameplayManager = gameManagers.GetComponent<GameplayManager>();
        obstacleParent = GameObject.FindGameObjectWithTag("StaticObjects").transform;
        //obstacle = Resources.Load("StaticObjects/obstacle") as GameObject;
        //obstacle2 = Resources.Load("StaticObjects/obstacle2") as GameObject;
        this.StartCoroutine(this.LoadConfigXml(configText));
    }

    void ResetConfiguration()
    {
        activeTokens.Clear();
        obstaclesPositions.Clear(); 
        streetsList.Clear();
        obstaclesListbyToken.Clear();
        placeHolderListbyToken.Clear();    
        obstaclesPositionsList.Clear();

        for (int i = 0; i < obstacleParent.childCount; i++)
            GameObject.Destroy(obstacleParent.GetChild(i).gameObject);

        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnObstacleDestroyed));
    }

    public void DefineObstacles(int level)
    {
        obstaclesPositionsList.Clear();
        //obstaclesPositions.Clear();

        if (level == 0)
        {
            obstaclesPositions = new List<PlaceHolder>();
            //TUTORIAL
            EnvironmentManager envManager = gameManagers.GetComponent<EnvironmentManager>();
            //Token token = envManager.GetTutorialTokens()[1];
            Token token = envManager.GetTutorialTokens()[5];


            SBSVector3 pos1, tang1;
            //token.TokenToWorld(0.3f, 0.7f, out pos1, out tang1);
            token.TokenToWorld(0.45f, 0.7f, out pos1, out tang1);

            SBSVector3 pos2, tang2;
            //token.TokenToWorld(0.6f, -0.45f, out pos2, out tang2);
            token.TokenToWorld(0.85f, -0.45f, out pos2, out tang2);

            PlaceHolder staticObstacle = new PlaceHolder();
            staticObstacle.item = obstacleSources[3]; //bike
            staticObstacle.token = token;
            staticObstacle.position = pos1;

            obstaclesPositions.Add(staticObstacle);

            PlaceHolder dynamicObstacle = new PlaceHolder();
            dynamicObstacle.item = dinamicSources[2]; //garbage
            dynamicObstacle.token = token;
            dynamicObstacle.position = pos2;

            obstaclesPositions.Add(dynamicObstacle);
            obstaclesPositionsList.Add(obstaclesPositions);
        }
        else
        {
            //for (int i = 0; i < GameplayManager.LevelCount; i++)
            for (int i = 0; i < gameplayManager.GetLevelCount(); i++)
            {
                obstaclesPositions = new List<PlaceHolder>();
                obstaclesListbyToken = new Dictionary<Token, List<Obstacle>>();
                placeHolderListbyToken = new Dictionary<Token, List<PlaceHolder>>();

                int staticTotal = 0;
                foreach (ObstacleItem item in obstacleSources)
                {
                    string key = item.obstacle.name;
                    item.percentage = levelConfigs[i].obstacleWeights[key];
                    staticTotal += item.percentage;
                }

                int dynamicTotal = 0;
                foreach (ObstacleItem item in dinamicSources)
                {
                    string key = item.obstacle.name;
                    item.percentage = levelConfigs[i].obstacleWeights[key];
                    dynamicTotal += item.percentage;
                }

                int total = staticTotal + dynamicTotal;
                
                int st = 0,
                    dyn = 0;

                if (total > 0)
                {
                    st = Mathf.CeilToInt((staticTotal / (float)total) * 100.0f);
                    dyn = 100 - st;
                }

                List<StringPerc> differentiation = new List<StringPerc>();
                differentiation.Add(new StringPerc());
                differentiation.Add(new StringPerc());

                differentiation[0].item = "static";
                differentiation[0].perc = st;

                differentiation[1].item = "dynamic";
                differentiation[1].perc = dyn;

                obstaclesPositions = GetPositionsByOccourences(levelConfigs[i].occurences, differentiation);
                obstaclesPositionsList.Add(obstaclesPositions);
            }
        }
    }

    void PlaceObstacles(int level)
    {        
        int objCounter = 1;

        if (gameManagers.GetComponent<GameplayManager>().LearningPhaseDone || level == 0)
        {
            if (level == 0)
                level = 1;
            foreach (PlaceHolder placeholder in obstaclesPositionsList[level - 1])
            {
                ObstacleItem item = placeholder.item;

                if (item.obstacle.name.Contains("turret"))
                    item.obsClass = ObstacleClass.GroupDynamic;
                else if (item.obstacle.name.Contains("pole") || item.obstacle.name.Contains("ball"))
                    item.obsClass = ObstacleClass.GroupStatic;

                GameObject obstacleObject = null;
                Obstacle obsComp = null;

                if (item.obsClass == ObstacleClass.Static)
                {
                    obstacleObject = (GameObject)GameObject.Instantiate(item.obstacle);
                    obsComp = obstacleObject.AddComponent<Obstacle>();
                    obsComp.name = item.obstacle.name;
                    obsComp.currentToken = placeholder.token;
                    obsComp.isAnimated = false;
                    obsComp.hasChild = false;
                    obstacleObject.name = "staticObject" + objCounter;
                    //BoxCollider coll = obstacleObject.AddComponent<BoxCollider>();
                    //coll.isTrigger = true;
                }
                else if (item.obsClass == ObstacleClass.Dynamic)
                {
                    obstacleObject = (GameObject)GameObject.Instantiate(item.obstacle);
                    obsComp = obstacleObject.AddComponent<Obstacle>();
                    obsComp.name = item.obstacle.name;
                    obsComp.currentToken = placeholder.token;
                    obsComp.isAnimated = true;
                    obsComp.hasChild = false;
                    obsComp.stopTrasversal = item.minTrasversal;
                    //obsComp.isActive = true;
                    obstacleObject.name = "dynamicObject" + objCounter;
                    if (level > 0)
                        obsComp.SetSpeed(levelConfigs[level - 1].obstacleSpeeds[item.obstacle.name]);
                }
                else if (item.obsClass == ObstacleClass.GroupStatic)
                {
                    float childWidth = 1.5f;
                    if (item.obstacle.name.Contains("ball"))
                        childWidth = 2.0f;

                    obstacleObject = new GameObject("stObstacleGroup" + objCounter);
                    float trasvGap = placeholder.item.maxTrasversal - placeholder.item.minTrasversal;
                    float tokenGap = trasvGap * 0.5f * placeholder.token.width;
                    int childNum = Mathf.CeilToInt(tokenGap / childWidth);
                    for (int i = 0; i < childNum; i++)
                    {
                        GameObject child = (GameObject)GameObject.Instantiate(item.obstacle);
                        child.name = "stgroup" + objCounter.ToString() + "_" + i.ToString();
                        child.transform.parent = obstacleObject.transform;
                        Vector3 childPos = new Vector3(0.0f, 0.0f, childWidth * i);
                        child.transform.position = childPos;
                    }

                    obsComp = obstacleObject.AddComponent<Obstacle>();
                    obsComp.name = item.obstacle.name;
                    obsComp.currentToken = placeholder.token;
                    obsComp.isAnimated = false;
                    obsComp.hasChild = true;
                    obsComp.isActive = true;
                    obsComp.stopTrasversal = item.minTrasversal;
                }
                else if (item.obsClass == ObstacleClass.GroupDynamic)
                {
                    float childWidth = 1.5f;

                    obstacleObject = new GameObject("dyObstacleGroup" + objCounter);
                    float trasvGap = placeholder.item.maxTrasversal - placeholder.item.minTrasversal;
                    float tokenGap = trasvGap * 0.5f * placeholder.token.width;
                    int childNum = Mathf.CeilToInt(tokenGap / childWidth);
                    for (int i = 0; i < childNum; i++)
                    {
                        GameObject child = (GameObject)GameObject.Instantiate(item.obstacle);
                        child.name = "dygroup" + objCounter.ToString() + "_" + i.ToString();
                        child.transform.parent = obstacleObject.transform;
                        Vector3 childPos = new Vector3(0.0f, 0.0f, childWidth * i);
                        child.transform.position = childPos;
                    }

                    obsComp = obstacleObject.AddComponent<Obstacle>();
                    obsComp.name = item.obstacle.name;
                    obsComp.currentToken = placeholder.token;
                    obsComp.isAnimated = true;
                    obsComp.hasChild = true;
                    //obsComp.isActive = true;
                    obsComp.stopTrasversal = item.minTrasversal;
                    if (level > 0)
                        obsComp.SetSpeed(levelConfigs[level - 1].obstacleSpeeds[item.obstacle.name]);
                }

                if (level == 0)
                {
                    obsComp.soundEnabled = true;
                    obsComp.soundVolume = 100;
                }
                else
                {
                    obsComp.soundEnabled = (volumesByLevel[level - 1] > 0);
                    obsComp.soundVolume = volumesByLevel[level - 1];
                }

                if (level > 0)
                    obsComp.SetAlpha(levelConfigs[level - 1].obstacleAlphas[item.obstacle.name]);

                obstaclesList.Add(obsComp);
                obstacleObject.transform.parent = obstacleParent;
                obstacleObject.transform.position = placeholder.position;

                Vector3 tokenDirection = placeholder.token.gameObject.transform.TransformDirection(Vector3.forward);
                bool toBeRotated = (90.0f - Vector3.Angle(Vector3.forward, tokenDirection)) > 10.0f;
                if (toBeRotated)
                    obstacleObject.transform.RotateAround(Vector3.up, -Mathf.PI * 0.5f);

                objCounter++;
                AddObstacleByToken(placeholder.token, obsComp);
            }
        
        }

        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnObstaclePlaced));
    }

    void CurrentToken(Token currentToken)
    {
        foreach (Token token in activeTokens)
        {            
            List<Obstacle> obsList = obstaclesListbyToken[token];
            foreach (Obstacle obs in obsList)
                obs.BroadcastMessage("Deactivate");
        }

        activeTokens.Clear();
        if (obstaclesListbyToken.ContainsKey(currentToken))
            activeTokens.Add(currentToken);

        /*foreach (Token link in currentToken.links)
        {
            if (null != link && obstaclesListbyToken.ContainsKey(link))
                activeTokens.Add(link);
        }*/

        foreach (Token token in activeTokens)
        {
            List<Obstacle> obsList = obstaclesListbyToken[token];
            foreach (Obstacle obs in obsList)
                obs.BroadcastMessage("Activate");
        }
    }
    #endregion

    #region Coroutines
    IEnumerator LoadConfigXml(string configText)
    {
        try
        {
            levelConfigs = new List<LevelConfig>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(configText);

            XmlNode root = xmlDoc.FirstChild;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode child = root.ChildNodes[i];
                switch (child.Name)
                {
                    case ("levels"):
                        ParseLevels(child);
                        break;
                }
            }
            progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnObstacleXmlParsed));
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
                case ("level"):
                    currentParsedLevel = xmlNode.GetAttributeAsInt("id");
                    levelConfigs.Add(new LevelConfig());
                    ParseLevel(xmlNode);
                    break;
            }
        }

        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnObstacleXmlParsed));

        yield return new WaitForEndOfFrame();*/
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
                levelConfigs.Add(new LevelConfig());
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
                case ("obstacle_occurrences"):
                    ParseObstacleOccurences(child);
                    break;
                case ("obstacles"):
                    //levelConfigs[currentParsedLevel - 1].soundVolume = int.Parse(child.Attributes["sound"].Value);
                    volumesByLevel.Add(int.Parse(child.Attributes["sound"].Value));
                    ParseObstacles(child);
                    break;
            }
        }
    }

    protected void ParseObstacleOccurences(XmlNode node)
    {
        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode child = node.ChildNodes[i];
            StringPerc strPerc = new StringPerc();
            strPerc.item = child.Attributes["number"].Value;
            strPerc.perc = int.Parse(child.Attributes["perc"].Value);
            levelConfigs[currentParsedLevel - 1].occurences.Add(strPerc);
        }
    }

    protected void ParseObstacles(XmlNode node)
    {
        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode child = node.ChildNodes[i];

            if (child.Name.Equals("occurrences"))
            {
                StringPerc strPerc = new StringPerc();
                strPerc.item = child.Attributes["number"].Value;
                strPerc.perc = int.Parse(child.Attributes["perc"].Value);
                levelConfigs[currentParsedLevel - 1].occurences.Add(strPerc);
            }
            else if (child.Name.Equals("obstacle"))
            {
                string obstacleClass = child.Attributes["class"].Value;

                int obstacleClassWeigth = int.Parse(child.Attributes["weight"].Value);
                levelConfigs[currentParsedLevel - 1].obstacleWeights.Add(obstacleClass, obstacleClassWeigth);

                float obstacleClassAlpha = float.Parse(child.Attributes["alpha"].Value);
                levelConfigs[currentParsedLevel - 1].obstacleAlphas.Add(obstacleClass, obstacleClassAlpha);

                if (null != child.Attributes["speed"])
                {
                    float obstacleClassSpeed = float.Parse(child.Attributes["speed"].Value);
                    levelConfigs[currentParsedLevel - 1].obstacleSpeeds.Add(obstacleClass, obstacleClassSpeed);
                }
            }
        }
    }

    /*protected void ParseLevel(XMLNode node)
    {
        foreach (XMLNode child in node.children)
        {
            switch (child.tagName)
            {
                case ("obstacles"):
                    ParseObstacles(child);
                    break;
            }
        }
    }

    protected void ParseObstacles(XMLNode node)
    {
        foreach (XMLNode child in node.children)
        {
            if (child.tagName.Equals("occurrences"))
            {
                StringPerc strPerc = new StringPerc();
                strPerc.item = child.GetAttributeAsString("number");
                strPerc.perc = child.GetAttributeAsInt("perc");
                levelConfigs[currentParsedLevel - 1].occurences.Add(strPerc);
            }
            else if (child.tagName.Equals("obstacle"))
            {
                string obstacleClass = child.GetAttributeAsString("class");
                
                int obstacleClassWeigth = child.GetAttributeAsInt("weight");
                levelConfigs[currentParsedLevel - 1].obstacleWeights.Add(obstacleClass, obstacleClassWeigth);

                float obstacleClassAlpha = child.GetAttributeAsFloat("alpha");
                levelConfigs[currentParsedLevel - 1].obstacleAlphas.Add(obstacleClass, obstacleClassAlpha);

                if (child.HasAttribute("speed"))
                {
                    float obstacleClassSpeed = child.GetAttributeAsFloat("speed");
                    levelConfigs[currentParsedLevel - 1].obstacleSpeeds.Add(obstacleClass, obstacleClassSpeed);
                }
            }
        }
    }*/
    #endregion

    #region Protected Members
    protected void AddObstacleByToken(Token token, Obstacle obs)
    {
        if (obstaclesListbyToken.ContainsKey(token))
        {
            obstaclesListbyToken[token].Add(obs);
        }
        else
        {
            List<Obstacle> obsList = new List<Obstacle>();
            obsList.Add(obs);
            obstaclesListbyToken.Add(token, obsList);
        }
    }

    protected void AddPlaceHolderByToken(Token token, PlaceHolder obs)
    {
        if (placeHolderListbyToken.ContainsKey(token))
        {
            placeHolderListbyToken[token].Add(obs);
        }
        else
        {
            List<PlaceHolder> obsList = new List<PlaceHolder>();
            obsList.Add(obs);
            placeHolderListbyToken.Add(token, obsList);
        }
    }

    protected void SetPlaceHolderByOccurrences(List<StringPerc> occurences, List<StringPerc> differentiation)
    {
        //occQuantity: how many streets with a certain num of obstacles
        int[] occQuantity = new int[occurences.Count];
        int obstacleTotal = 0;
        for (int i = 0; i < occurences.Count; i++)
        {
            occQuantity[i] = Mathf.FloorToInt(streetsList.Count * occurences[i].perc * 0.01f);
            obstacleTotal += occQuantity[i] * int.Parse(occurences[i].item);
        }


        int[] obsTotalByClass = new int[2];
        obsTotalByClass[0] = Mathf.CeilToInt(obstacleTotal * differentiation[0].perc * 0.01f);     //statics
        obsTotalByClass[1] = obstacleTotal - obsTotalByClass[0];                                    //dynamics

        List<PlaceHolder> staticList = GetPlaceHolderList(obsTotalByClass[0], ObstacleClass.Static);
        List<PlaceHolder> dynamicList = GetPlaceHolderList(obsTotalByClass[1], ObstacleClass.Dynamic);
        
        int[] obstacleNumByToken = new int[streetsList.Count];
        for (int i = 0; i < obstacleNumByToken.Length; i++)
            obstacleNumByToken[i] = 0;
        
        List<int> indexInToken = new List<int>();
        for (int i = 0; i < streetsList.Count; i++)
            indexInToken.Add(i);

        for (int i = 0; i < occQuantity.Length; i++)
        {
            for (int j = 0; j < occQuantity[i]; j++)
            {
                if (indexInToken.Count > 0)
                {
                    int randomIdx = Random.Range(0, 100000) % indexInToken.Count;
                    int index = indexInToken[randomIdx];
                    obstacleNumByToken[index] = int.Parse(occurences[i].item);
                    indexInToken.RemoveAt(randomIdx);
                }
                else
                {
                    break;
                }
            }
        }

        System.Array.Sort(obstacleNumByToken);
        streetsList.Sort();

        int staticListCounter = 0;
        int dynamicListCounter = 0;
        for( int i = 0; i < obstacleNumByToken.Length; i++)
        {
            int obstacleNum = obstacleNumByToken[i];
            if (obstacleNum > 0)
            {
                bool exhausted = false;
                for (int j = 0; j < obstacleNum; j++)
                {
                    /*
                    int limit = 8;
                    if (streetsList[i].lengthOrRadius <= 10)
                        limit = 1;
                    else if (streetsList[i].lengthOrRadius <= 20)
                        limit = 4;
                    */
                    int limit = Mathf.FloorToInt((streetsList[i].lengthOrRadius - (SafeLenght * 2.0f)) / ObstacleOccupation);

                    if (placeHolderListbyToken.ContainsKey(streetsList[i]))
                    {
                        if (placeHolderListbyToken[streetsList[i]].Count >= limit)
                            break;
                    }
                    
                    int classIndex = Random.Range(0, 100000) % 2;
                    if (obsTotalByClass[classIndex] <= 0)
                    {
                        classIndex = (classIndex + 1) % 2;
                        if (obsTotalByClass[classIndex] <= 0)
                        {
                            exhausted = true;
                            break;
                        }
                    }

                    obsTotalByClass[classIndex]--;

                    PlaceHolder placeHolder;
                    if (classIndex == 0)
                    {
                        placeHolder = staticList[staticListCounter];
                        staticListCounter++;
                    }
                    else
                    {
                        placeHolder = dynamicList[dynamicListCounter];
                        dynamicListCounter++;
                    }

                    AddPlaceHolderByToken(streetsList[i], placeHolder);
                    obstacleNumByToken[i]--;
                }

                if (exhausted)
                    break;
            }
        }
    }

    protected List<PlaceHolder> GetPositionsByOccourences(List<StringPerc> occurences, List<StringPerc> differentiation)
    {
        float safeLenght = SafeLenght;

        // Followings fill placeHolderListbyToken
        SetPlaceHolderByOccurrences(occurences, differentiation);

        List<PlaceHolder> retList = new List<PlaceHolder>();
        foreach (KeyValuePair<Token, List<PlaceHolder>> item in placeHolderListbyToken)
        {
            Token currentToken = item.Key;
            List<PlaceHolder> placeHolderList = item.Value;

            float safeLongitudinal = safeLenght / currentToken.lengthOrRadius;
            float availableLongitudinal = 1.0f - (safeLongitudinal * 2);

            int currentIndex = 0;
            foreach (PlaceHolder placeHolder in placeHolderList)
            {
                float trasversal;
                if (placeHolder.item.obsClass == ObstacleClass.Static)
                {
                    trasversal = Random.Range(placeHolder.item.minTrasversal, placeHolder.item.maxTrasversal);
                    if (currentToken.width < 7 && placeHolder.item.obstacle.name.Contains("pole") || placeHolder.item.obstacle.name.Contains("ball"))
                    {
                        trasversal = placeHolder.item.maxTrasversal;
                    }
                }
                else
                {
                    trasversal = placeHolder.item.maxTrasversal;
                }

                float step = (availableLongitudinal / (float)(placeHolderList.Count + 1));
                float longitudinal = safeLongitudinal + (step * (currentIndex + 1));

                SBSVector3 pos;
                SBSVector3 tan;
                currentToken.TokenToWorld(longitudinal, trasversal, out pos, out tan);
                placeHolder.token = currentToken;
                placeHolder.position = pos;
                retList.Add(placeHolder);
                currentIndex++;
            }
        }

        return retList;
    }    

    protected List<PlaceHolder> GetPositions(LevelConfig config)
    {
        int staticNum = Mathf.Max(0, config.staticObstacleNum);
        int dynamicNum = Mathf.Max(0, config.dynamicObstacleNum);
        int staticPerStreet = (config.staticDistribution == DistributionType.Street) ? staticNum : 0;
        int dynamicPerStreet = (config.dynamicDistribution == DistributionType.Street) ? dynamicNum : 0;
        int staticPerCity = (config.staticDistribution == DistributionType.City) ? staticNum : 0;
        int dynamicPerCity = (config.dynamicDistribution == DistributionType.City) ? dynamicNum : 0;

        // Followings fill placeHolderListbyToken
        SetPlaceHolderByStreet(staticPerStreet, dynamicPerStreet);
        SetPlaceHolderByCity(staticPerCity, dynamicPerCity);

        List<PlaceHolder> retList = new List<PlaceHolder>();
        foreach (KeyValuePair<Token, List<PlaceHolder>> item in placeHolderListbyToken)
        {
            Token currentToken = item.Key;
            List<PlaceHolder> placeHolderList = item.Value;

            int currentIndex = 0;
            foreach (PlaceHolder placeHolder in placeHolderList)
            {
                float trasversal;
                if (placeHolder.item.obsClass == ObstacleClass.Static)
                {
                    trasversal = Random.Range(placeHolder.item.minTrasversal, placeHolder.item.maxTrasversal);
                }
                else
                {
                    trasversal = placeHolder.item.maxTrasversal;
                }

                float step = (1.0f / (float)(placeHolderList.Count + 1));
                float longitudinal = step * (currentIndex + 1);

                SBSVector3 pos;
                SBSVector3 tan;
                currentToken.TokenToWorld(longitudinal, trasversal, out pos, out tan);
                placeHolder.token = currentToken;
                placeHolder.position = pos;
                retList.Add(placeHolder);
                currentIndex++;
            }
        }

        return retList;
    }

    protected List<PlaceHolder> GetPlaceHolderList(int num, ObstacleClass oClass)
    {
        int maxPercent = 0;
        int[] indexList = new int[num];
        int listBaseIndex = 0;
        int maxIndex = 0;
        int maxValue = 0;

        ObstacleItem[] sources;
        if (oClass == ObstacleClass.Static)
        {
            sources = new ObstacleItem[obstacleSources.Length];
            obstacleSources.CopyTo(sources, 0);
        }
        else
        {
            sources = new ObstacleItem[dinamicSources.Length];
            dinamicSources.CopyTo(sources, 0);
        }

        for (int obsIndex = 0; obsIndex < sources.Length; obsIndex++)
        {
            ObstacleItem item = sources[obsIndex];
            if (item.percentage > maxValue)
            {
                maxValue = item.percentage;
                maxIndex = obsIndex;
            }
            maxPercent += item.percentage;
        }

        //firstpass
        for (int obsIndex = 0; obsIndex < sources.Length; obsIndex++)
        {
            ObstacleItem item = sources[obsIndex];
            item.Weight = item.percentage / (float)maxPercent;
            int firstPassTotal = Mathf.FloorToInt(item.Weight * num);
            int i = 0;
            for (i = 0; i < firstPassTotal; i++)
            {
                indexList[listBaseIndex + i] = obsIndex;
                item.TotalInScene++;
            }

            listBaseIndex += i;
        }

        //secondpass
        if (listBaseIndex < num)
        {
            int difference = num - listBaseIndex;
            for (int i = 0; i < difference; i++)
            {
                indexList[listBaseIndex + i] = maxIndex;
                sources[maxIndex].TotalInScene++;
            }
        }
        
        //scrumble
        List<int> supportList = new List<int>(indexList);
        List<PlaceHolder> returnList = new List<PlaceHolder>();
        for (int i = 0; i < indexList.Length; i++)
        {
            int randomIdx = Random.Range(0, supportList.Count);
            int sourceIndex = supportList[randomIdx];
            supportList.RemoveAt(randomIdx);

            PlaceHolder pl = new PlaceHolder();
            ObstacleItem item = sources[sourceIndex];
            pl.item = item;

            returnList.Add(pl);
        }

        return returnList;
    }

    protected void SetPlaceHolderByStreet(int staticNum, int dynamicNum)
    {
        List<PlaceHolder> staticList = GetPlaceHolderList(staticNum * streetsList.Count, ObstacleClass.Static);
        List<PlaceHolder> dynamicList = GetPlaceHolderList(dynamicNum * streetsList.Count, ObstacleClass.Dynamic);

        int tokenCount = 0;
        foreach (Token token in streetsList)
        {
            ObstacleClass[] obsInToken = new ObstacleClass[staticNum + dynamicNum];
            List<int> index = new List<int>();
            
            //static/dynamic per token
            for (int i = 0; i < obsInToken.Length; i++)
            {
                if (i < staticNum)
                    obsInToken[i] = ObstacleClass.Static;
                else
                    obsInToken[i] = ObstacleClass.Dynamic;

                index.Add(i);
            }

            //scrumble
            List<int> rndIndex = new List<int>();
            rndIndex.AddRange(index);
            if (rndIndex.Count > 1)
            {
                for (int i = 0; i < rndIndex.Count; i++)
                {
                    int ri = Random.Range(0, 10000) % index.Count;
                    rndIndex[i] = index[ri];
                    index.Remove(index[ri]);
                }
            }

            int staticCounter = 0;
            int dynamicCounter = 0;
            for (int i = 0; i < obsInToken.Length; i++)
            {
                int idx = rndIndex[i];
                ObstacleClass obj = obsInToken[idx];

                PlaceHolder placeHolder;
                //float trasversal;
                if (obj == ObstacleClass.Static)
                {
                    placeHolder = staticList[(tokenCount * staticNum) + staticCounter];
                    staticCounter++;
                }
                else
                {
                    placeHolder = dynamicList[(tokenCount * dynamicNum) + dynamicCounter];
                    dynamicCounter++;
                }
                AddPlaceHolderByToken(token, placeHolder);
            }

            tokenCount++;
        }
    }


    protected void SetPlaceHolderByCity(int staticNum, int dynamicNum)
    {
        List<PlaceHolder> staticList = GetPlaceHolderList(staticNum, ObstacleClass.Static);
        List<PlaceHolder> dynamicList = GetPlaceHolderList(dynamicNum, ObstacleClass.Dynamic);

        List<int> indexList = new List<int>();

        int[] countTokenList = new int[streetsList.Count];
        for (int i = 0; i < countTokenList.Length; i++)
            countTokenList[i] = 0;

        for (int i = 0; i < staticNum + dynamicNum; i++)
        {
            if (indexList.Count <= 0)
            {
                for (int index = 0; index < streetsList.Count; index++)
                    indexList.Add(index);
            }

            int randomIdx = Random.Range(0, indexList.Count);
            countTokenList[indexList[randomIdx]]++;
            indexList.RemoveAt(randomIdx);
        }
        
        ObstacleClass[] obsInCity = new ObstacleClass[staticNum + dynamicNum];
        List<int> indexes = new List<int>();

        //static/dynamic per token
        for (int i = 0; i < obsInCity.Length; i++)
        {
            if (i < staticNum)
                obsInCity[i] = ObstacleClass.Static;
            else
                obsInCity[i] = ObstacleClass.Dynamic;

            indexes.Add(i);
        }

        //scrumble
        List<int> rndIndex = new List<int>();
        rndIndex.AddRange(indexes);
        if (rndIndex.Count > 1)
        {
            for (int i = 0; i < rndIndex.Count; i++)
            {
                int ri = Random.Range(0, 10000) % indexes.Count;
                rndIndex[i] = indexes[ri];
                indexes.Remove(indexes[ri]);
            }
        }

        int staticCounter = 0;
        int dynamicCounter = 0;
        int counter = 0;

        for (int i = 0; i < countTokenList.Length; i++)
        {
            for (int j = 0; j < countTokenList[i]; j++)
            {
                int idx = rndIndex[counter];
                counter++;
                ObstacleClass obj = obsInCity[idx];

                PlaceHolder placeHolder;
                if (obj == ObstacleClass.Static)
                {
                    placeHolder = staticList[staticCounter];
                    staticCounter++;
                }
                else
                {
                    placeHolder = dynamicList[dynamicCounter];
                    dynamicCounter++;
                }
                AddPlaceHolderByToken(streetsList[i], placeHolder);
            }
        }
    }
    #endregion
}


