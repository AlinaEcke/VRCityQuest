using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using FoF.Utils;
using System.Threading;

#region BalanceBoardThread Class
public class BalanceBoardThread
{
    #region External Data Structure
    [StructLayout(LayoutKind.Sequential)]
    public struct BBState
    {
        public float TopL;
        public float TopR;
        public float BottomL;
        public float BottomR;
        public float Total;
    }

    [DllImport("BalanceBoardPlugin")]
    private static extern void bbInit();
    [DllImport("BalanceBoardPlugin")]
    private static extern void bbConnect();
    [DllImport("BalanceBoardPlugin")]
    private static extern bool bbIsConnected();
    [DllImport("BalanceBoardPlugin")]
    private static extern void bbDisconnect();
    [DllImport("BalanceBoardPlugin")]
    private static extern void bbGetState(out BBState state);
    [DllImport("BalanceBoardPlugin")]
    private static extern byte bbGetBattery();
    [DllImport("BalanceBoardPlugin")]
    private static extern float bbGetWeigth();
    [DllImport("BalanceBoardPlugin")]
    private static extern void bbSetLed(byte v);
    [DllImport("BalanceBoardPlugin")]
    private static extern void bbReset();
    #endregion

    #region Protected Members
    protected bool isRunning = true;
    protected Thread thread;
    protected BBState state;
    protected object m_readstate = new object();
    #endregion

    #region Ctor
    public BalanceBoardThread()
    {
        thread = new Thread(DoWork);
    }
    #endregion

    #region Public methods
    public void Start()
    {
        thread.Start();
    }

    public void Abort()
    {
        isRunning = false;
    }

    public BBState GetBBState()
    {
        lock (m_readstate)
            return state;
    }

    public bool IsConnected()
    {
        return bbIsConnected();
    }

    public void Reset()
    {
        bbReset();
    }
    #endregion
    
    #region Protected methods
    protected void DoWork()
    {
        bbInit();

        while (!bbIsConnected())
        {
            bbConnect();
            Thread.Sleep(1);
        }

        bbSetLed(0x01);

        while (isRunning)
        {
            lock(m_readstate)
                bbGetState(out state);

            Thread.Sleep(20);
        }

        bbSetLed(0x00);

        bbDisconnect();
    }
    #endregion
}
#endregion

[AddComponentMenu("FoF/BalanceBoardManager")]
public class BalanceBoardManager : MonoBehaviour
{
    #region Internal Data
    public enum ProgressSignalType
    {
        OnBalanceBoardConnected = 0
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

    public enum Directions
    {
        Left = 0,
        Up,
        Right,
        Down
    }
    #endregion

    #region Public Fields
    public SignalSender progressSignals;
    #endregion

    #region Protected Fields
    protected float baseTotal = 0.0f;
    protected float baseTopLeft = 0.0f;
    protected float baseTopRight = 0.0f;
    protected float baseBottomLeft = 0.0f;
    protected float baseBottomRight = 0.0f;
    protected float actualTotal = 0.0f;
    protected float actualTopLeft = 0.0f;
    protected float actualTopRight = 0.0f;
    protected float actualBottomLeft = 0.0f;
    protected float actualBottomRight = 0.0f;
    protected float vertical = 0.0f;
    protected float horizontal = 0.0f;
    protected float prevVertical = 0.0f;
    protected float prevHorizontal = 0.0f;
    protected float lastVertical = 0.0f;
    protected float lastHorizontal = 0.0f;

    protected float currentWeight = 0.0f;
    protected bool readyToRead = false;
    protected bool connSignal = false;
    protected BalanceBoardThread bbThread = new BalanceBoardThread();
    #endregion

    #region Public Get/Set
    public float Total
    {
        get { return baseTotal + actualTotal; }
    }
    public float TopLeft
    {
        get { return baseTopLeft + actualTopLeft; }
    }
    public float TopRight
    {
        get { return baseTopRight + actualTopRight; }
    }
    public float BottomLeft
    {
        get { return baseBottomLeft + actualBottomLeft; }
    }
    public float BottomRight
    {
        get { return baseBottomRight + actualBottomRight; }
    }
    public float Top
    {
        get { return TopLeft + TopRight; }
    }
    public float Bottom
    {
        get { return BottomLeft + BottomRight; }
    }
    public float Left
    {
        get { return TopLeft + BottomLeft; }
    }
    public float Right
    {
        get { return TopRight + BottomRight; }
    }
    public float CPHorizontal
    {
        get { return (Right - Left) / Total; }
    }
    public float CPVertical
    {
        get { return (Top - Bottom) / Total; }
    }
    #endregion

    #region Public Members
    public bool GetKeyDown(KeyCode code)
    {
        bool bbInput = false;
        bool keyboardInput = Input.GetKeyDown(code);

        switch (code)
        {
            case (KeyCode.UpArrow):
                bbInput = lastVertical > 0;
                break;
            case (KeyCode.DownArrow):
                bbInput = lastVertical < 0;
                break;
            case (KeyCode.LeftArrow):
                bbInput = lastHorizontal < 0;
                break;
            case (KeyCode.RightArrow):
                bbInput = lastHorizontal > 0;
                break;
        }

        return (bbInput && Total > 10) || keyboardInput;
    }

    public bool GetKey(KeyCode code)
    {
        bool bbInput = false;
        bool keyboardInput = Input.GetKey(code);

        switch (code)
        {
            case (KeyCode.UpArrow):
                bbInput = vertical > 0;
                break;
            case (KeyCode.DownArrow):
                bbInput = vertical < 0;
                break;
            case (KeyCode.LeftArrow):
                bbInput = horizontal < 0;
                break;
            case (KeyCode.RightArrow):
                bbInput = horizontal > 0;
                break;
        }

        return (bbInput && Total > 10) || keyboardInput;
    }

    public bool GetDirectionOnce(Directions direction)
    {
        switch (direction)
        {
            case (Directions.Up):
                return GetKeyDown(KeyCode.UpArrow);
            case (Directions.Down):
                return GetKeyDown(KeyCode.DownArrow);
            case (Directions.Left):
                return GetKeyDown(KeyCode.LeftArrow);
            case (Directions.Right):
                return GetKeyDown(KeyCode.RightArrow);
            default:
                return false;
        }
    }

    public void Reset()
    {
        baseTotal = -actualTotal;
        baseTopLeft = -actualTopLeft;
        baseTopRight = -actualTopRight;
        baseBottomLeft = -actualBottomLeft;
        baseBottomRight = -actualBottomRight;
    }
    #endregion

    #region Unity Callbacks
    void Start()
    {
        bbThread.Start();
    }

    void Update()
    {
      //  Debug.Log("Update balanceboardmanager");
        if (bbThread.IsConnected())
        {
            Debug.Log("bbThread.IsConnected()");
            actualTopLeft = actualTopRight = actualBottomLeft = actualBottomRight = actualTotal = 0.0f;
            BalanceBoardThread.BBState s = bbThread.GetBBState();

            actualTotal = s.Total;
            if (actualTotal != actualTotal)
                actualTotal = 0.0f;

            actualTopLeft = s.TopL;
            if (actualTopLeft != actualTopLeft)
                actualTopLeft = 0.0f;

            actualTopRight = s.TopR;
            if (actualTopRight != actualTopRight)
                actualTopRight = 0.0f;

            actualBottomLeft = s.BottomL;
            if (actualBottomLeft != actualBottomLeft)
                actualBottomLeft = 0.0f;

            actualBottomRight = s.BottomR;
            if (actualBottomRight != actualBottomRight)
                actualBottomRight = 0.0f;

            if (Total > 10)
            {
                lastVertical = 0.0f;
                lastHorizontal = 0.0f;
                vertical = 0.0f;                
                horizontal = 0.0f;

                if ((Top - Bottom) / Total > 0.4f)
                {
                    if (prevVertical == 0.0f)
                        lastVertical = 1.0f;
                    vertical = 1.0f;
                }
                else if ((Top - Bottom) / Total < -0.4f)
                {
                    if (prevVertical == 0.0f)
                        lastVertical = -1.0f;
                    vertical = -1.0f;
                }
                else if ((Right - Left) / Total > 0.3f)
                {
                    if (prevHorizontal == 0.0f)
                        lastHorizontal = 1.0f;
                    horizontal = 1.0f;
                }
                else if ((Right - Left) / Total < -0.3f)
                {
                    if (prevHorizontal == 0.0f)
                        lastHorizontal = -1.0f;
                    horizontal = -1.0f;
                }

                prevVertical = vertical;
                prevHorizontal = horizontal;
            }

            if (!connSignal)
            {
                connSignal = true;
                progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnBalanceBoardConnected));
                Reset();
            }
        }
    }

    void OnDestroy()
    {
        bbThread.Abort();
    }

#if UNITY_EDITOR
    /*void OnGUI()
    {
        GUI.Box(new Rect(2, 350, 300, 150), "BB Test: " + Total);
        GUI.Label(new Rect(10, 400, 100, 20), "TL: " + TopLeft);
        GUI.Label(new Rect(150, 400, 100, 20), "TR: " + TopRight);
        GUI.Label(new Rect(10, 430, 100, 20), "BL: " + BottomLeft);
        GUI.Label(new Rect(150, 430, 100, 20), "BR: " + BottomRight);
        GUI.Label(new Rect(10, 460, 100, 20), "VER: " + vertical);
        GUI.Label(new Rect(150, 460, 100, 20), "HOR: " + horizontal);
    }*/
#endif

    #endregion

    #region Messages
    public void Initialize()
    {
        bbThread.Start();
    }
    #endregion

    #region Coroutines
    #endregion
}