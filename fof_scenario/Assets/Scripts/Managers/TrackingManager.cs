using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FoF/TrackingManager")]
public class TrackingManager : MonoBehaviour
{
    #region Singleton
    protected static TrackingManager instance;
    public static TrackingManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Internal Data Structures
    public enum Command
    {
        Event = 0,
        Action
    }

    public class TrackLog
    {
        public string target = string.Empty;
        public Command command = Command.Event;
        public string type = string.Empty;
        public string details = string.Empty;
        public Vector3 xypos = Vector3.zero;
        public float timestamp = 0.0f;
        public float deltatime = 0.0f;
    }

    public class NavigationSummaryLog
    {
        public string target = string.Empty;
        public float actualDistance = 0.0f;
        public float manhattanDistance = 0.0f;
        public float timeElapsed = 0.0f;
        public int stars = 0;
        public int obstaclePassed = 0;
        public int obstacleHit = 0;
        public int pedestrianHit = 0;
        public int level = 0;
    }

    public class ObstacleSummaryLog
    {
        public int bonusTaken = 0;
        public float timeLevel = 0.0f;
        public int obstaclePassed = 0;
        public int obstacleHit = 0;
        public int level = 0;
        public int pedestrianHit = 0;
    }
    #endregion

    #region Constants
    private const string STRSEP = "\t";
    #endregion

    #region Public Fields
    #endregion

    #region Protected Fields
    protected string startTime = string.Empty;
    protected string currentFilePath = string.Empty;
    protected string version = string.Empty;
    protected bool bbEnabled = false;
    protected bool ovrEnabled = false;
    protected string userId = string.Empty;
    protected string sessionName = string.Empty;
    protected GameplayManager.GameplayType gameplay = GameplayManager.GameplayType.Navigation;
    protected List<string> outputLines = new List<string>();
    protected List<TrackLog> trackLogs = new List<TrackLog>();
    protected List<NavigationSummaryLog> navigationSummaryLogs = new List<NavigationSummaryLog>();
    protected List<ObstacleSummaryLog> obstacleSummaryLogs = new List<ObstacleSummaryLog>();
    protected List<Vector3> intersectionPos = new List<Vector3>();
    protected List<Vector3> targetPos = new List<Vector3>();
    #endregion

    #region Get/Set
    public string Version
    {
        get
        {
            return version;
        }
        set
        {
            version = value;
        }
    }
    public bool BBEnabled
    {
        get
        {
            return bbEnabled;
        }
        set
        {
            bbEnabled = value;
        }
    }
    public bool OVREnabled
    {
        get
        {
            return ovrEnabled;
        }
        set
        {
            ovrEnabled = value;
        }
    }
    public string UserId
    {
        get
        {
            return userId;
        }
        set
        {
            userId = value;
        }
    }
    public string SessionName
    {
        get
        {
            return sessionName;
        }
        set
        {
            sessionName = value;
        }
    }
    #endregion

    #region Protected Methods
    protected void SetHeaderInfo()
    {
        //IFormatProvider culture = new System.Globalization.CultureInfo("en", true);
        outputLines.Add("Desktop" + STRSEP + Screen.width + "x" + Screen.height + " " + Application.targetFrameRate + "Hz");
        outputLines.Add("Date" + STRSEP + DateTime.Today.ToShortDateString());
        outputLines.Add("BalanceBoard" + STRSEP + bbEnabled.ToString().ToUpper());
        outputLines.Add("StartTime" + STRSEP + startTime);
        outputLines.Add("EndTime" + STRSEP + DateTime.Now.ToShortTimeString());
        outputLines.Add("Participant Code" + STRSEP + userId);
        outputLines.Add("Session" + STRSEP + sessionName);
        outputLines.Add("Gameplay" + STRSEP + gameplay.ToString());
        outputLines.Add("");
        outputLines.Add("");
    }

    protected void SetLogEntries()
    {
        outputLines.Add("Target" + STRSEP + "Command" + STRSEP + "Type" + STRSEP + "Details" + STRSEP + "XYPos" + STRSEP + "TimeStamp" + STRSEP + "DeltaTime");
        for (int i = 0; i < trackLogs.Count; i++)
            outputLines.Add(trackLogs[i].target + STRSEP + trackLogs[i].command + STRSEP + trackLogs[i].type + STRSEP + trackLogs[i].details + 
                STRSEP + "(" + trackLogs[i].xypos.x + "; " + trackLogs[i].xypos.z + ")" + STRSEP + trackLogs[i].timestamp + STRSEP + trackLogs[i].deltatime);

        outputLines.Add("");
        outputLines.Add("");
    }

    protected void SetSummary()
    {
        outputLines.Add("Summary");
        if (gameplay == GameplayManager.GameplayType.Navigation)
        {
            outputLines.Add("Target" + STRSEP + "Actual distance" + STRSEP + "Manhattan distance" + STRSEP + "Time Elapsed" + STRSEP + "Stars" + STRSEP + "Obstacle Passed" +
                STRSEP + "Obstacle Hit" + STRSEP + "Pedestrian Hit" + STRSEP + "Level");

            for (int i = 0; i < navigationSummaryLogs.Count; i++)
            {
                string level = navigationSummaryLogs[i].level > 0 ? navigationSummaryLogs[i].level.ToString() : (navigationSummaryLogs[i].level < 0 ? "L" : "T");
                outputLines.Add(navigationSummaryLogs[i].target + STRSEP + navigationSummaryLogs[i].actualDistance + STRSEP + navigationSummaryLogs[i].manhattanDistance + STRSEP + navigationSummaryLogs[i].timeElapsed +
                    STRSEP + navigationSummaryLogs[i].stars + STRSEP + navigationSummaryLogs[i].obstaclePassed + STRSEP + navigationSummaryLogs[i].obstacleHit + STRSEP + navigationSummaryLogs[i].pedestrianHit + STRSEP + level);
            }
        }
        else
        {
            outputLines.Add("Bonus Taken" + STRSEP + "Level Time" + STRSEP + "Obstacle Passed" + STRSEP + "Obstacle Hit" + STRSEP + "Pedestrian Hit" + STRSEP + "Level");

            for (int i = 0; i < obstacleSummaryLogs.Count; i++)
            {
                string level = obstacleSummaryLogs[i].level > 0 ? obstacleSummaryLogs[i].level.ToString() : (obstacleSummaryLogs[i].level < 0 ? "L" : "T");
                outputLines.Add(obstacleSummaryLogs[i].bonusTaken + STRSEP + obstacleSummaryLogs[i].timeLevel + STRSEP + obstacleSummaryLogs[i].obstaclePassed + STRSEP + obstacleSummaryLogs[i].obstacleHit + STRSEP + obstacleSummaryLogs[i].pedestrianHit + STRSEP + level);
            }
        }
        outputLines.Add("");
        outputLines.Add("");
    }

    protected void SetMapInfo()
    {
        outputLines.Add("Map Info");

        string intersections = string.Empty;
        for (int i = 0; i < intersectionPos.Count; i++)
            intersections += "(" + intersectionPos[i].x + "," + intersectionPos[i].z + ")";

        intersectionPos.Clear();

        string targets = string.Empty;
        for (int i = 0; i < targetPos.Count; i++)
            targets += "(" + targetPos[i].x + "," + targetPos[i].z + ")";

        targetPos.Clear();

        outputLines.Add("Intersections" + STRSEP + intersections);
        outputLines.Add("Targets" + STRSEP + targets);

        outputLines.Add("");
        outputLines.Add("");
    }

    protected void ResetLog()
    {
        outputLines.Clear();
        trackLogs.Clear();
        navigationSummaryLogs.Clear();
        obstacleSummaryLogs.Clear();
    }

    protected void SaveLog(string path)
    {
        outputLines.Clear();

        SetHeaderInfo();
        SetLogEntries();
        SetSummary();
        SetMapInfo();

        File.WriteAllLines(path, outputLines.ToArray());
    }
    #endregion

    #region Public Methods
    public void LogEntry(Command comm, string currentTarget, Vector3 playerPos, string type, string details)
    {
        TrackLog log = new TrackLog();
        log.target = currentTarget;
        log.command = comm;
        log.type = type;
        log.details = details;
        log.xypos = playerPos;
        log.timestamp = TimeManager.Instance.MasterSource.TotalTime;

        float delta = 0.0f;
        int trackCount = trackLogs.Count;
        if (trackCount > 0)
            delta = log.timestamp - trackLogs[trackCount - 1].timestamp;

        log.deltatime = delta;

        trackLogs.Add(log);
    }

    public void LogSummaryRow(string target, float actualDistance, float manhattanDistance, float timeElapsed, int stars, int obstaclePassed, int obstacleHit, int pedestrianHit, int level)
    {
        NavigationSummaryLog sLog = new NavigationSummaryLog();
        sLog.target = target;
        sLog.actualDistance = actualDistance;
        sLog.manhattanDistance = manhattanDistance;
        sLog.timeElapsed = timeElapsed;
        sLog.stars = stars;
        sLog.obstaclePassed = obstaclePassed;
        sLog.obstacleHit = obstacleHit;
        sLog.pedestrianHit = pedestrianHit;
        sLog.level = level;

        navigationSummaryLogs.Add(sLog);
    }

    public void LogSummaryRow(int bonusTaken, float timeLevel, int obstaclePassed, int obstacleHit, int pedestrianHit, int level)
    {
        ObstacleSummaryLog sLog = new ObstacleSummaryLog();
        sLog.bonusTaken = bonusTaken;
        sLog.timeLevel = timeLevel;
        sLog.obstaclePassed = obstaclePassed;
        sLog.obstacleHit = obstacleHit;
        sLog.level = level;
        sLog.pedestrianHit = pedestrianHit;

        obstacleSummaryLogs.Add(sLog);
    }

    public void StartTracking()
    {
        ResetLog();

        string fileSessionName = sessionName.Split('.')[0];
        currentFilePath = "./Logs/log_" + userId + "_" + fileSessionName + "_" + DateTime.UtcNow.Ticks +".txt";
    }

    public void StopTracking()
    {
        gameplay = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GameplayManager>().Gameplay;

        if (currentFilePath.Length > 0)
            SaveLog(currentFilePath);

        currentFilePath = string.Empty;
    }

    public void AddTargetPos(Vector3 tPos)
    {
        targetPos.Add(tPos);
    }
    #endregion

    #region Unity Callbacks
    void Awake ()
    {
        instance = this;
        startTime = DateTime.Now.ToShortTimeString();
	}
    #endregion

    #region Messages
    void OnEnvironmentReady(int level)
    {
        EnvironmentManager env = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<EnvironmentManager>();
        intersectionPos.Clear();
        for (int i = 0; i < env.Crosses.Count; i++)
            intersectionPos.Add(env.Crosses[i].position);
        
        gameplay = GameplayManager.GameplayType.Navigation;
    }
    #endregion
}
