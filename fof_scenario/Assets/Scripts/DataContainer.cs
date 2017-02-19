using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DataContainer
{
    #region Singleton
    public static DataContainer _instance = null;
    public static DataContainer Instance
    {
        get
        {
            if (null == _instance)
            {
                _instance = new DataContainer();
                _instance.InitializeDataContainer();
            }
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }
    #endregion

    #region Internal Data Structure
    [Serializable]
    public class SerializedRoad
    {
        public float[] position = new float[3];
        public float[] size = new float[2];
        public float angleRotation = 0.0f;
        public int textureIndex = -1;
    }

    [Serializable]
    public class SerializedBuilding
    {
        public float[] position = new float[3];
        public float[] size = new float[2];
        public float angleRotation = 0.0f;
        public int floors = 0;
        public int colorIndex = -1;
        public int textureIndex = -1;
        public bool vaseActive = false;
        public bool pillarActive = false;
        public bool balconyActive = false;
        public string[] targetNames;
        public int[] targetIds;
    }

    [Serializable]
    public class SerializedObstaclePlaceholder
    {
        public string tokenName;
        public float[] position = new float[3];
        public string itemName = null;
    }
    #endregion

    #region Statcs
    public static string filename = "";
    #endregion

    #region Public Fields
    public SerializedRoad[] vRoadsArray = null;
    public SerializedRoad[] hRoadsArray = null;
    public SerializedRoad[] crosses = null;
    public SerializedBuilding[] buildings = null;
    public SerializedBuilding[] borderBuildings = null;
    public SerializedObstaclePlaceholder[][] obstaclePlaceholdersList = null;
    public bool[] daylight = null;
    public bool[] showTargets = null;
    public bool[] showMaps = null;
    public int[] obstacleVolumes = null;
    public int[] pedestrians = null;
    public int[] targets = null;
    public int[] floors = null;
    public int vTowerCoord = 0;
    public int hTowerCoord = 0;
    public int vRoads = 0;
    public int hRoads = 0;
    public float cityHeight = 0.0f;
    public float cityWidth = 0.0f;
    public int roadTextureId = -1;

    public int[,] levelsTargets;
    public int[,] levelsGates;

    public string[] obstClasses = null;
    public float[][] obstAlphas = null;
    public float[][] obstSpeeds = null;

    public bool learningEnabled = true;
    public bool learningMap = true;
    public bool learningTarget = true;
    #endregion

    #region Protected Fields
    [NonSerialized()]
    protected EnvironmentManager environmentManager = null;
    [NonSerialized()]
    protected TokensManager tokenManager = null;
    [NonSerialized()]
    protected ObstaclesManager obstacleManager = null;
    [NonSerialized()]
    protected GameplayManager gameplayManager = null;
    [NonSerialized()]
    protected CQ_Interface interfaces = null;
    [NonSerialized()]
    protected Dictionary<string, Token> tokenByName = null;
    #endregion

    #region Protected Members
    protected void InitializeDataContainer()
    {
        GameObject gameManagers = GameObject.FindGameObjectWithTag("GameManagers");
        environmentManager = gameManagers.GetComponent<EnvironmentManager>();
        tokenManager = gameManagers.GetComponent<TokensManager>();
        obstacleManager = gameManagers.GetComponent<ObstaclesManager>();
        gameplayManager = gameManagers.GetComponent<GameplayManager>();
        interfaces = GameObject.FindGameObjectWithTag("Interface").GetComponent<CQ_Interface>();
        tokenByName = new Dictionary<string, Token>();
    }

    protected SerializedRoad GetSerializedRoad(EnvironmentManager.Road road)
    {
        SerializedRoad sRoad = new SerializedRoad();
        sRoad.position[0] = road.position.x;
        sRoad.position[1] = road.position.y;
        sRoad.position[2] = road.position.z;
        sRoad.size[0] = road.size.x;
        sRoad.size[1] = road.size.y;
        sRoad.angleRotation = road.angleRotation;
        return sRoad;
    }

    protected SerializedBuilding GetSerializedBuilding(EnvironmentManager.Building building)
    {
        SerializedBuilding sBuilding = new SerializedBuilding();
        sBuilding.position[0] = building.position.x;
        sBuilding.position[1] = building.position.y;
        sBuilding.position[2] = building.position.z;
        sBuilding.size[0] = building.size.x;
        sBuilding.size[1] = building.size.y;
        sBuilding.angleRotation = building.angleRotation;
        sBuilding.floors = building.floors;
        sBuilding.colorIndex = building.colorIndex;
        sBuilding.textureIndex = building.textureIndex;
        sBuilding.vaseActive = building.vaseActive;
        sBuilding.pillarActive = building.pillarActive;
        sBuilding.balconyActive = building.balconyActive;

        int targetNum = building.targets.Count;
        sBuilding.targetNames = new string[targetNum];
        sBuilding.targetIds = new int[targetNum];
        for (int i = 0; i < targetNum; i++)
        {
            sBuilding.targetNames[i] = building.targets[i].First;
            sBuilding.targetIds[i] = building.targets[i].Second;
        }

        return sBuilding;
    }

    protected SerializedObstaclePlaceholder GetSerializedObstaclePlaceholder(ObstaclesManager.PlaceHolder placeholder)
    {
        SerializedObstaclePlaceholder sPlaceholder = new SerializedObstaclePlaceholder();
        sPlaceholder.tokenName = placeholder.token.name;
        sPlaceholder.position[0] = placeholder.position.x;
        sPlaceholder.position[1] = placeholder.position.y;
        sPlaceholder.position[2] = placeholder.position.z;
        sPlaceholder.itemName = placeholder.item.obstacle.name;
        return sPlaceholder;
    }

    protected EnvironmentManager.Road GetEnvironmentRoad(SerializedRoad sRoad)
    {
        EnvironmentManager.Road road = new EnvironmentManager.Road();
        road.position = new Vector3(sRoad.position[0], sRoad.position[1], sRoad.position[2]);
        road.size = new Vector2(sRoad.size[0], sRoad.size[1]);
        road.angleRotation = sRoad.angleRotation;
        return road;
    }

    protected EnvironmentManager.Building GetEnvironmentBuilding(SerializedBuilding sBuilding)
    {
        EnvironmentManager.Building building = new EnvironmentManager.Building();
        building.position = new Vector3(sBuilding.position[0], sBuilding.position[1], sBuilding.position[2]);
        building.size = new Vector2(sBuilding.size[0], sBuilding.size[1]);
        building.angleRotation = sBuilding.angleRotation;
        building.floors = sBuilding.floors;
        building.colorIndex = sBuilding.colorIndex;
        building.textureIndex = sBuilding.textureIndex;
        building.vaseActive = sBuilding.vaseActive;
        building.pillarActive = sBuilding.pillarActive;
        building.balconyActive = sBuilding.balconyActive;

        building.targets.Clear();
        for (int i = 0; i < sBuilding.targetNames.Length; i++)
            building.targets.Add(new SBS.Core.Tuple<string,int>(sBuilding.targetNames[i], sBuilding.targetIds[i]));

        return building;
    }

    protected ObstaclesManager.PlaceHolder GetObstaclePlaceholder(SerializedObstaclePlaceholder sPlaceholder)
    {
        ObstaclesManager.PlaceHolder placeholder = new ObstaclesManager.PlaceHolder();
        placeholder.token = tokenByName[sPlaceholder.tokenName];
        placeholder.position = new Vector3(sPlaceholder.position[0], sPlaceholder.position[1], sPlaceholder.position[2]);
        placeholder.item = obstacleManager.ObstacleItemsByName[sPlaceholder.itemName];
        return placeholder;
    }
    #endregion

    #region Public Members
    public void LoadDataStructures()
    {
        int crossesNum = environmentManager.Crosses.Count;
        crosses = new SerializedRoad[crossesNum];
        for (int i = 0; i < crossesNum; i++)
            crosses[i] = GetSerializedRoad(environmentManager.Crosses[i]);

        int vRoadsNum = environmentManager.VerticalRoads.Count;
        vRoadsArray = new SerializedRoad[vRoadsNum];
        for (int i = 0; i < vRoadsNum; i++)
            vRoadsArray[i] = GetSerializedRoad(environmentManager.VerticalRoads[i]);

        int hRoadsNum = environmentManager.HorizontalRoads.Count;
        hRoadsArray = new SerializedRoad[hRoadsNum];
        for (int i = 0; i < hRoadsNum; i++)
            hRoadsArray[i] = GetSerializedRoad(environmentManager.HorizontalRoads[i]);

        int buildingsNum = environmentManager.Buildings.Count;
        buildings = new SerializedBuilding[buildingsNum];
        for (int i = 0; i < buildingsNum; i++)
            buildings[i] = GetSerializedBuilding(environmentManager.Buildings[i]);

        int borderBuildingsNum = environmentManager.BorderBuildings.Count;
        borderBuildings = new SerializedBuilding[borderBuildingsNum];
        for (int i = 0; i < borderBuildingsNum; i++)
            borderBuildings[i] = GetSerializedBuilding(environmentManager.BorderBuildings[i]);

        //obstaclePlaceholdersList = new SerializedObstaclePlaceholder[GameplayManager.LevelCount][];
        //for (int j = 0; j < GameplayManager.LevelCount; j++)
        obstaclePlaceholdersList = new SerializedObstaclePlaceholder[gameplayManager.GetLevelCount()][];
        for (int j = 0; j < gameplayManager.GetLevelCount(); j++)
        {
            int placeholderNum = obstacleManager.ObstaclesPositionsList[j].Count;
            obstaclePlaceholdersList[j] = new SerializedObstaclePlaceholder[placeholderNum];
            for (int i = 0; i < placeholderNum; i++)
                obstaclePlaceholdersList[j][i] = GetSerializedObstaclePlaceholder(obstacleManager.ObstaclesPositionsList[j][i]);
        }

        floors = environmentManager.FloorList.ToArray();

        hTowerCoord = environmentManager.HTowerCoord;
        vTowerCoord = environmentManager.VTowerCoord;
        hRoads = environmentManager.HRoads;
        vRoads = environmentManager.VRoads;
        cityHeight = environmentManager.CityHeight;
        cityWidth = environmentManager.CityWidth;
        roadTextureId = environmentManager.RoadTextureId;

        int dayLightNum = environmentManager.isDayLight.Count;
        daylight = new bool[dayLightNum];
        for (int i = 0; i < dayLightNum; i++)
            daylight[i] = environmentManager.isDayLight[i];

        int obstacleVolumesNum = obstacleManager.volumesByLevel.Count;
        obstacleVolumes = new int[obstacleVolumesNum];
        for (int i = 0; i < obstacleVolumesNum; i++)
            obstacleVolumes[i] = obstacleManager.volumesByLevel[i];

        int obstClassesNum = obstacleManager.levelConfigs[0].obstacleAlphas.Count;
        obstClasses = new string[obstClassesNum];
        int c = 0;
        foreach (string key in obstacleManager.levelConfigs[0].obstacleAlphas.Keys)
        {
            obstClasses[c] = key;
            c++;
        }

        int obstLevelNum = obstacleManager.levelConfigs.Count;
        obstAlphas = new float[obstLevelNum][];
        obstSpeeds = new float[obstLevelNum][];
        for (int i = 0; i < obstLevelNum; i++)
        {
            int obstacleAlphasNum = obstacleManager.levelConfigs[i].obstacleAlphas.Count;
            obstAlphas[i] = new float[obstacleAlphasNum];
            obstSpeeds[i] = new float[obstacleAlphasNum];
            for (int j = 0; j < obstacleAlphasNum; j++)
            {
                obstAlphas[i][j] = obstacleManager.levelConfigs[i].obstacleAlphas[obstClasses[j]];
                if (obstacleManager.levelConfigs[i].obstacleSpeeds.ContainsKey(obstClasses[j]))
                    obstSpeeds[i][j] = obstacleManager.levelConfigs[i].obstacleSpeeds[obstClasses[j]];
                else
                    obstSpeeds[i][j] = 0.0f;
            }
        }

        int pedNum = environmentManager.totalPedestrians.Count;
        pedestrians = new int[pedNum];
        for (int i = 0; i < pedNum; i++)
            pedestrians[i] = environmentManager.totalPedestrians[i];

        int targetsNum = gameplayManager.targetNumByLevel.Count;
        targets = new int[targetsNum];
        for (int i = 0; i < targetsNum; i++)
            targets[i] = gameplayManager.targetNumByLevel[i];

        //Denis
        int levelCount = gameplayManager.GetLevelCount();
        int targetCount = targets[0];
        levelsTargets = new int[levelCount,targetCount];
        levelsGates = new int[levelCount,targetCount];
        //Debug.Log("@@@@@ START NEW PARTS IN LoadDataStructures levelCount = " + levelCount);
        for (int i = 0; i < levelCount; i++)
        {
            for (int k = 0; k < targetCount; k++)
            {
                if (gameplayManager.TargetIndexStack.Count > 0)
                {
                    levelsTargets[i, k] = gameplayManager.TargetIndexStack.Pop();
                }
                else
                {
                    if (k < targetCount - 1)
                    {
                        Debug.Log("targetCount: " + targetCount + ", levelCount " + levelCount + ", " + gameplayManager.TargetIndexStack.Count);
                        interfaces.SetTfSessionResult(true, "Error processing XML. Please try again", false);
                        interfaces.SetTfSaveResult(false);
                        interfaces.DisableSaveButton();
                    }
                }

                //levelsGates[i,k] = gameplayManager.GateIndexStack.Pop();
                //Debug.Log("@@@@@ levelsTargets["+ i +"][" + k + "] = " + levelsTargets[i,k]);
            }
        }
        //End Denis

        //show target on map
        int showTargetsNum = gameplayManager.showOnMapByLevel.Count;
        showTargets = new bool[showTargetsNum];
        for (int i = 0; i < showTargetsNum; i++)
            showTargets[i] = gameplayManager.showOnMapByLevel[i];

        int showMapssNum = gameplayManager.showMapByLevel.Count;
        showMaps = new bool[showMapssNum];
        for (int i = 0; i < showMapssNum; i++)
            showMaps[i] = gameplayManager.showMapByLevel[i];

        learningEnabled = gameplayManager.learningPhase;
        learningMap = gameplayManager.mapOnLearning;
        learningTarget = gameplayManager.targetOnLearning;

        //Debug.LogWarning("learningEnabled: " + learningEnabled);
    }

    public void UnloadDataStructures()
    {
        environmentManager.Crosses.Clear();
        for (int i = 0; i < crosses.Length; i++)
            environmentManager.Crosses.Add(GetEnvironmentRoad(crosses[i]));

        environmentManager.VerticalRoads.Clear();
        for (int i = 0; i < vRoadsArray.Length; i++)
            environmentManager.VerticalRoads.Add(GetEnvironmentRoad(vRoadsArray[i]));

        environmentManager.HorizontalRoads.Clear();
        for (int i = 0; i < hRoadsArray.Length; i++)
            environmentManager.HorizontalRoads.Add(GetEnvironmentRoad(hRoadsArray[i]));

        environmentManager.Buildings.Clear();
        for (int i = 0; i < buildings.Length; i++)
            environmentManager.Buildings.Add(GetEnvironmentBuilding(buildings[i]));

        environmentManager.BorderBuildings.Clear();
        for (int i = 0; i < borderBuildings.Length; i++)
            environmentManager.BorderBuildings.Add(GetEnvironmentBuilding(borderBuildings[i]));

        environmentManager.FloorList = new List<int>(floors);

        environmentManager.HTowerCoord = hTowerCoord;
        environmentManager.VTowerCoord = vTowerCoord;
        environmentManager.HRoads = hRoads;
        environmentManager.VRoads = vRoads;
        environmentManager.CityHeight = cityHeight;
        environmentManager.CityWidth = cityWidth;
        environmentManager.RoadTextureId = roadTextureId;
        
        crosses = null;
        vRoadsArray = null;
        hRoadsArray = null;
        buildings = null;
        borderBuildings = null;
        hTowerCoord = 0;
        vTowerCoord = 0;
        hRoads = 0;
        vRoads = 0;
        cityHeight = 0.0f;
        cityWidth = 0.0f;
        roadTextureId = -1;

        environmentManager.isDayLight.Clear();
        for (int i = 0; i < daylight.Length; i++)
            environmentManager.isDayLight.Add(daylight[i]);

        gameplayManager.SetLevelCount(daylight.Length);

        environmentManager.totalPedestrians.Clear();
        for (int i = 0; i < pedestrians.Length; i++)
            environmentManager.totalPedestrians.Add(pedestrians[i]);

        gameplayManager.targetNumByLevel.Clear();
        for (int i = 0; i < targets.Length; i++)
            gameplayManager.targetNumByLevel.Add(targets[i]);

        gameplayManager.showOnMapByLevel.Clear();
        for (int i = 0; i < showTargets.Length; i++)
            gameplayManager.showOnMapByLevel.Add(showTargets[i]);

        gameplayManager.showMapByLevel.Clear();
        for (int i = 0; i < showMaps.Length; i++)
            gameplayManager.showMapByLevel.Add(showMaps[i]);

        gameplayManager.learningPhase = learningEnabled;
        gameplayManager.mapOnLearning = learningMap;
        gameplayManager.targetOnLearning = learningTarget;
    }

    public void UnloadLevelData()
    {
        tokenByName.Clear();
        Token[] tokens = tokenManager.Tokens;
        for (int i = 0; i < tokens.Length; i++)
            tokenByName.Add(tokens[i].name, tokens[i]);

        obstacleManager.ObstaclesPositionsList.Clear();
        for (int j = 0; j < obstaclePlaceholdersList.Length; j++)
        {
            List<ObstaclesManager.PlaceHolder> obstaclePositions = new List<ObstaclesManager.PlaceHolder>();
            for (int i = 0; i < obstaclePlaceholdersList[j].Length; i++)
                obstaclePositions.Add(GetObstaclePlaceholder(obstaclePlaceholdersList[j][i]));

            obstacleManager.ObstaclesPositionsList.Add(obstaclePositions);
        }

        obstacleManager.volumesByLevel = new List<int>();
        for (int i = 0; i < obstacleVolumes.Length; i++)
            obstacleManager.volumesByLevel.Add(obstacleVolumes[i]);

        obstacleManager.levelConfigs = new List<ObstaclesManager.LevelConfig>();
        for (int i = 0; i < obstAlphas.Length; i++)
        {
            ObstaclesManager.LevelConfig lc = new ObstaclesManager.LevelConfig();
            lc.obstacleAlphas = new Dictionary<string, float>();
            lc.obstacleSpeeds = new Dictionary<string, float>();
            for (int j = 0; j < obstAlphas[i].Length; j++)
            {
                lc.obstacleAlphas[obstClasses[j]] = obstAlphas[i][j];
                lc.obstacleSpeeds[obstClasses[j]] = obstSpeeds[i][j];
            }
            obstacleManager.levelConfigs.Add(lc);
        }

        int targetsNum = gameplayManager.targetNumByLevel.Count;
        gameplayManager.TargetStack.Clear();
        int totalTarget = targets[0];
        Debug.Log("totalTarget" + totalTarget);
        int levelCount = gameplayManager.GetLevelCount();

        string levelindexes = "";
        for (int i = 0; i < levelCount; i++)
        {
            levelindexes = "level: " + i + " indexes: ";
            for (int c = 0; c < totalTarget; c++)
                levelindexes += " " + levelsTargets[i, c];

            //Debug.Log("§§§ " + levelindexes);
        }

        
        for (int i = 0; i < totalTarget; i++)
        {
            //Debug.Log("DataContainer.Instance.levelsTargets[level - 1," + i + "] = " + DataContainer.Instance.levelsTargets[gameplayManager.Level - 1, i]);
            int index = 0;

            for (int c = 0; c < gameplayManager.TargetList.Count; c++)
            {
                if ((int)gameplayManager.TargetList[c].type == levelsTargets[gameplayManager.Level - 1, i])
                {
                    index = c;
                    break;
                }
            }

            //Debug.Log("[[[[[[[[ index" + index + " totaltarget " + totalTarget + " gameplayManager.TargetList.Count " + gameplayManager.TargetList.Count);
            gameplayManager.TargetStack.Push(gameplayManager.TargetList[index]);
        }
    }
    #endregion

    #region Static Functions
    public static void SetFileName(string name)
    {
        filename = name;
    }

    public static void SaveToFile()
    {
#if !UNITY_WEBPLAYER
        MemoryStream memStream = new MemoryStream();
        BinaryFormatter binFormatter = new BinaryFormatter();
        binFormatter.Serialize(memStream, DataContainer.Instance);
        byte[] byteArray = memStream.ToArray();
        File.WriteAllBytes(filename, byteArray);
#endif
    }

    public static void LoadFromFile()
    {
#if !UNITY_WEBPLAYER
        try
        {
            BinaryFormatter binFormatter = new BinaryFormatter();
            byte[] data = File.ReadAllBytes(filename);
            MemoryStream memStream = new MemoryStream(data);
            DataContainer.Instance = (DataContainer)(binFormatter.Deserialize(memStream));
            DataContainer.Instance.InitializeDataContainer();
        }
        catch (FileNotFoundException e)
        {
            InternalConsole.Instance.Log("FILE " + filename + " no exixst!");
        }
#endif
    }
    #endregion
}
