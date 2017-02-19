using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FoF.Utils;
using SBS.XML;
using System.IO;


[AddComponentMenu("FoF/ExternalTexturesManager")]
public class ExternalTexturesManager : MonoBehaviour
{
    #region Internal Data Structure
    public enum ProgressSignalType
    {
        OnTexturesXmlParsed = 0,
        OnXmlError,
        OnTexturesLoaded
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

    [System.Serializable]
    public class TextureItem
    {
        public string filename;
        public Texture2D image;
    }
    #endregion

    #region Public Fields
    public SignalSender progressSignals;
    public Texture2D[] internalTextures;
    #endregion

    #region Protected Fields
    protected Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>(); 
    #endregion

    #region Unity Callback
    #endregion

    #region Messages
    void LoadGeneralConf(string configText)
    {
        this.StartCoroutine(this.LoadConfigXml(configText));
    }
    #endregion

    #region Coroutines
    IEnumerator LoadConfigXml(string configText)
    {
        /*
        XMLReader reader = new XMLReader();
        XMLNode rootNode = reader.read(configText).children[0] as XMLNode;
        foreach (XMLNode xmlNode in rootNode.children)
        {
        }
        */
        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnTexturesXmlParsed));

        yield return this.StartCoroutine(this.LoadExternalTextures());
    }

    IEnumerator LoadExternalTextures()
    {
        string[] filenames = Directory.GetFiles(@"ExternalTextures");
        foreach (string filename in filenames)
        {
            string[] splittedName = filename.Split('\\');
            string textureName = splittedName[splittedName.Length-1];
            splittedName = textureName.Split('.');
            textureName = splittedName[0];
            WWW www = new WWW("file://" + Path.GetFullPath(filename));
            www.texture.filterMode = FilterMode.Bilinear;
            www.texture.anisoLevel = 1;

            //hack in order to have mipmapping
            Texture2D text = new Texture2D(www.texture.width, www.texture.height);
            text.SetPixels(www.texture.GetPixels());
            text.Apply();

            textures.Add(textureName, text);
        }

        foreach (Texture2D internalTexture in internalTextures)
        {
            if (!textures.ContainsKey(internalTexture.name))
                textures.Add(internalTexture.name, internalTexture);
        }

        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnTexturesLoaded));
        yield return new WaitForEndOfFrame();
    }    
    #endregion

    #region Public Members
    public Texture2D GetTextureByName(string name)
    {
        if (textures.ContainsKey(name))
            return textures[name];

        return null;
    }
    #endregion
}
