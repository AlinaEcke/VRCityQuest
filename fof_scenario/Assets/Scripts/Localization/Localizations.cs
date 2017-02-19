using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using SBS.XML; //ToDo: remove dependency (somehow)

[AddComponentMenu("SBS/Localizations")]
public class Localizations : MonoBehaviour
{
    #region Singleton instance

    protected static Localizations instance = null;

    public static Localizations Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Public members
    public TextAsset xml;
    public List<GameObject> listeners = new List<GameObject>();
    #endregion

    #region Protected members
    protected string currentLanguage; // Language currentLanguage;
    protected Dictionary<string, Dictionary<string, string>> strings = new Dictionary<string, Dictionary<string, string>>();
    #endregion


    #region Protected properties
    protected string embeddedLanguage //Language
    {
        get
        {
            /*
            Dictionary<SystemLanguage, string> l = new Dictionary<SystemLanguage, string>();

            l.Add(SystemLanguage.English, "en");
            l.Add(SystemLanguage.Italian, "it");
            
            if (l.ContainsKey(Application.systemLanguage))
                return l[Application.systemLanguage]; //Languages.getLanguage(l[Application.systemLanguage]);
            else
                return "en"; //Languages.getLanguage("en_us");
            */

            string[] srcValueStr = Application.srcValue.Split('?');
            string[] parStr;

            if (srcValueStr.Length > 1)
            {
                string[] srcParamsStr = srcValueStr[1].Split('&');

                foreach (string str in srcParamsStr)
                {
                    if (str.StartsWith("language"))
                    {
                        parStr = str.Split('=');
                        return WWW.UnEscapeURL(parStr[1]);
                    }
                }
            }
            return null;
        }
    }
    #endregion
    
    #region Protected methods
    protected void importXML(string text)
    {
        XMLReader xmlReader = new XMLReader();
        XMLNode root = xmlReader.read(text).children[0] as XMLNode;
        foreach (XMLNode record in root.children)
        {
            Dictionary<string, string> items = new Dictionary<string, string>();
            foreach (XMLNode item in record.children)
                items.Add(item.tagName, item.cdata);
            strings.Add(record.attributes["id"], items);
        }
    }

    protected void chooseLanguage()
    {
        currentLanguage = embeddedLanguage;

        if (null == currentLanguage)
            currentLanguage = "english"; // Languages.getLanguage("en_us");

        foreach (GameObject listener in listeners)
            listener.SendMessage("OnLanguageChanged", currentLanguage, SendMessageOptions.DontRequireReceiver);
    }
    #endregion

    #region Public methods
    public void initialize()
    {
        this.importXML(xml.text);
        this.chooseLanguage();
        Debug.Log("*********** Translations initialize currentLanguage " + currentLanguage);
    }

    public string getString(string identifier)
    {
        //Debug.Log("getString " + identifier + " currentLanguage " + currentLanguage);
        /*
        Dictionary<string, string> items;
        if (strings.TryGetValue(identifier, out items))
            if (null == currentLanguage)
            {
                if (items.ContainsKey(currentLanguage))
                    return items[currentLanguage];
                else
                    return items["en"];
            }
            else
                return items[currentLanguage];
        else
            return "";
        */

        Dictionary<string, string> items;
        if (strings.TryGetValue(currentLanguage, out items))
            if (items.ContainsKey(identifier))
                    return items[identifier];
            
        return "";
    }

    public string replaceText(string text)
    {
        Regex reg = new Regex("{([^{}]*)}");
        return reg.Replace(text, (match) =>
        {
            string str = this.getString(match.Value.Trim('{', '}'));
            if (str.Length > 0)
                return str;
            else
                return match.Value;
        });
    }

    public void changeLanguage(string newLanguage)
    {
        currentLanguage = newLanguage;
       
        foreach (GameObject listener in listeners)
            listener.SendMessage("OnLanguageChanged", currentLanguage, SendMessageOptions.DontRequireReceiver);
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        if (instance != null)
            Debug.LogError("Translations component must be unique!");
        instance = this;
    }

    void OnDestroy()
    {
        if (instance != this)
            Debug.LogError("Translations component must be unique!");
        instance = null;
    }
    #endregion
}
