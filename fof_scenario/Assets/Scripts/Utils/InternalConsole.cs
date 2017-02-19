using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("SBS/Utils/InternalConsole")]
public class InternalConsole : MonoBehaviour
{
    #region Singleton
    protected static InternalConsole instance;
    public static InternalConsole Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion
    
    #region Protected Members
    public KeyCode activationKey;
    public bool isVisible;
    #endregion

    #region Protected Members
    protected Rect consoleRect = new Rect(10, 10, 640, 480);
    protected Rect consoleLocalRect;
    protected Vector3 scrollVector;
    protected List<string> debug = new List<string>();
    protected string consoleText = string.Empty;

    protected void ConsoleWindow(int windowId)
    {
        Rect scrollArea = new RectOffset(10, 10, 20, 20).Remove(consoleLocalRect);
        scrollVector = GUI.BeginScrollView(scrollArea, scrollVector, new Rect(0, 0, scrollArea.width - 40, 3000));
        //float y = 0.0f;
        consoleText = string.Empty;
        foreach (string str in debug)
        {
            consoleText += str + "\n";
        }
        
        GUILayout.BeginArea(new Rect(0, 0, scrollArea.width - 20, 3000));
        GUILayout.TextArea(consoleText, "Label");
        GUILayout.EndArea();
        GUI.EndScrollView();
        GUI.DragWindow();
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        instance = this;
        consoleLocalRect = consoleRect;
    }

    void Start()
    {
        debug.Add("Console started...");
    }

    void Update ()
    {
        if (Input.GetKeyDown(activationKey))
            isVisible = !isVisible;
	}

	void OnGUI ()
    {
        if (isVisible)
        {
            consoleRect = GUI.Window(128, consoleRect, ConsoleWindow, "Internal Console", "Window");
        }
    }
    #endregion

    #region Public Members
    public void Log(string text)
    {
        Debug.Log(text);
        debug.Add(text);
        if (debug.Count > 100)
            debug.RemoveAt(0);
    }
    #endregion
}
