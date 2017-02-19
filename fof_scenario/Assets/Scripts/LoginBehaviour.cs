using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using SBS.Math;
using SBS.Core;

public class LoginBehaviour : MonoBehaviour {

    protected bool initialized = false;
    protected string patientCode = "";
    protected string adminPassword ="";
    protected CQ_Interface _interface;

    protected string codedAdminPassword = "123456";

    private int textfieldWidth;
    private int textfieldHeigth;
    private int textfieldX;
    private int textfieldY;
#if UNITY_IPHONE
	private TouchScreenKeyboard keyboard = null;
#endif

    public GUIStyle customStyle;

    #region Unity Callbacks
    void Awake()
    {
        textfieldWidth = (Screen.width == 1260 ? 337 : 337);
        textfieldHeigth = (Screen.height == 720 ? 74 : 74);
        textfieldX = (int)(Screen.width * 0.5f - textfieldWidth * 0.5f);
        textfieldY = (int)(Screen.height * 0.5f - textfieldHeigth * 0.5f) + 80;
    }

    void Start()
    {
        _interface = gameObject.GetComponent<CQ_Interface>();        
    }

	void Update()
	{
#if UNITY_IPHONE
		if (keyboard != null)
		{
			if (keyboard.active)
			{
				if (keyboard.text.Length > 10)
				{
					keyboard.text = keyboard.text.Substring(0, 10);
				}
				keyboard.text = Regex.Replace(keyboard.text, @"[^Z0-9]", "");

                if (_interface.State == CQ_Interface.LoginPage)
					patientCode = keyboard.text;

                if (_interface.State == CQ_Interface.AdminPage)
					adminPassword = keyboard.text;
			}
			if (keyboard.done)
				keyboard = null;
		}
#endif
	}
	
    void OnGUI()
    {
        if (!initialized)
            return;

        GUI.skin.textField.fontSize = 50;
        GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
		
#if UNITY_IPHONE
		GUI.skin.label.fontSize = 50;
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		
		if (_interface.State == CQ_Interface.LoginPage)
        {
			if (Event.current.type == EventType.MouseUp && new Rect(textfieldX, textfieldY, textfieldWidth, textfieldHeigth).Contains(Event.current.mousePosition))
			{
    			keyboard = TouchScreenKeyboard.Open(patientCode, TouchScreenKeyboardType.NumberPad);
			}
			
			GUI.Label(new Rect(textfieldX, textfieldY, textfieldWidth, textfieldHeigth), patientCode, customStyle);
		}
		
		if (_interface.State == CQ_Interface.AdminPage)
        {
			if (Event.current.type == EventType.MouseUp && new Rect(textfieldX, textfieldY, textfieldWidth, textfieldHeigth).Contains(Event.current.mousePosition))
			{
    			keyboard = TouchScreenKeyboard.Open(adminPassword, TouchScreenKeyboardType.NumberPad);
			}
			
			GUI.Label(new Rect(textfieldX, textfieldY, textfieldWidth, textfieldHeigth), adminPassword, customStyle);
		}
#else
        if (_interface.State == CQ_Interface.LoginPage)
		{
            patientCode = GUI.TextField(new Rect(textfieldX, textfieldY, textfieldWidth, textfieldHeigth), patientCode, 10, customStyle);
            patientCode = Regex.Replace(patientCode, @"[^Z0-9]", "");
		}

        if (_interface.State == CQ_Interface.AdminPage && !_interface.IsAdminLogged)
		{			
			adminPassword = GUI.PasswordField(new Rect(textfieldX, textfieldY, textfieldWidth, textfieldHeigth), adminPassword, '*', 10, customStyle);
            adminPassword = Regex.Replace(adminPassword, @"[^Z0-9]", "");
		}
#endif
    }

    #endregion

    #region Public Memebers
    public string GetUserId()
    {
        return patientCode;
    }

    public string GetAdminPassword()
    {
        return adminPassword;
    }

    public string CodedAdminPassword
    {
        get { return codedAdminPassword; }
    }

    public void Initialize()
    {
        initialized = true;
    }
    #endregion

    #region Messages

     

    #endregion

}
