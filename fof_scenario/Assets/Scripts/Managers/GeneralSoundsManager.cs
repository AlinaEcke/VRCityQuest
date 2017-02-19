using UnityEngine;
using System.Collections;
using SBS.XML;
using FoF.Utils;
using System.Xml;

public class GeneralSoundsManager : MonoBehaviour
{
    #region Internal Data Structure
    public enum ProgressSignalType
    {
        OnSoundXmlParsed = 0,
        OnXmlError
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
    #endregion

    #region Public Fields
    public SignalSender progressSignals;
    public AudioClip click;
    public AudioClip clue_button_ko;
    public AudioClip clue_button_ok;
    public AudioClip pos_jingle;
    public AudioClip popup;
    public AudioClip loop_ambient;
    public AudioClip loop_ambient_no_ped;
    public AudioClip loop_ambient_night;
    public AudioClip loop_ambient_night_no_ped;
    public AudioClip music;
    public AudioClip staticObstacle;
    public AudioClip targetBad;
    public AudioClip targetNormal;
    public AudioClip targetGood;
    public AudioClip obstPassed;
    #endregion

    #region Protected Members
    protected bool soundsActive = false;
    protected bool soundsEnabled = false;
    protected bool musicEnabled = false;
    protected float soundsVolume = 1.0f;
    protected float musicVolume = 1.0f;
    protected AudioSource musicSource;
    protected AudioSource jingleSource;
    protected AudioSource ambientSource;
    protected AudioSource[] sources;

    protected bool playingJingle = false;

    protected GameObject gameManagers = null;
    #endregion

    #region public Properties
    public bool SoundsActive { get { return soundsActive; } }
    public bool SoundsEnabled { get { return soundsEnabled; } }
    public bool MusicEnabled { get { return musicEnabled; } }
    public float SoundsVolume { get { return soundsVolume; } }
    public float MusicVolume { get { return musicVolume; } }
    #endregion

    #region Public Enums

    public enum GeneralSounds
    { 
        clueOK,
        clueKO,
        popup,
        click,
        static_obstacle,
        target_bad,
        target_normal,
        target_good,
        obst_Passed
    }

    #endregion

    #region Functions

    AudioSource FreeSource(AudioSource[] source)
    {
        foreach (AudioSource a in source)
            if (!a.isPlaying)
                return a;
        return null;
    }

    #endregion

    #region Messages
    void LoadGeneralConf(string configText)
    {
        this.StartCoroutine(this.LoadConfigXml(configText));
    }

    void PlayGeneralSound(GeneralSounds s)
    {
        if (!soundsEnabled)
            return;

        AudioSource source = FreeSource(sources);
        if(source != null)
        {
            switch (s)
            { 
                case GeneralSounds.click:
                    source.clip = click;
                    break;
                case GeneralSounds.clueKO:
                    source.clip = clue_button_ko;
                    break;
                case GeneralSounds.clueOK:
                    source.clip = clue_button_ok;
                    break;
                case GeneralSounds.popup:
                    source.clip = popup;
                    break;
                case GeneralSounds.static_obstacle:
                    source.clip = staticObstacle;
                    break;
                case GeneralSounds.target_bad:
                    source.clip = targetBad;
                    break;
                case GeneralSounds.target_good:
                    source.clip = targetGood;
                    break;
                case GeneralSounds.target_normal:
                    source.clip = targetNormal;
                    break;
                case GeneralSounds.obst_Passed:
                    source.clip = obstPassed;
                    break;
            }
            SoundsManager.Instance.PlaySource(source);
        }
    }

    void PlayJingle()
    {
        if (!soundsEnabled)
            return;
        musicSource.volume = 0.0f;
        playingJingle = true;
        jingleSource.clip = pos_jingle;
        SoundsManager.Instance.PlaySource(jingleSource);
    }

    void PlayAmbientSound()
    {
        if (!soundsEnabled)
            return;

        GameplayManager gp = gameManagers.GetComponent<GameplayManager>();
        EnvironmentManager env = gameManagers.GetComponent<EnvironmentManager>();
        ambientSource.clip = loop_ambient;
        if (env.IsDaylight)
        {
            if (env.PedestrianNum > 0 && gp.LearningPhaseDone)
                ambientSource.clip = loop_ambient;
            else
                ambientSource.clip = loop_ambient_no_ped;
        }
        else
        {
            if (env.PedestrianNum > 0 && gp.LearningPhaseDone)
                ambientSource.clip = loop_ambient_night;
            else
                ambientSource.clip = loop_ambient_night_no_ped;
        }

        ambientSource.volume = musicVolume;
        SoundsManager.Instance.PlaySource(ambientSource);
        soundsActive = true;
    }

    void PlayMusic()
    {
        if (!musicEnabled)
            return;
        musicSource.volume = musicVolume;
        musicSource.clip = music;
        SoundsManager.Instance.PlayMusicSource(musicSource);
    }

    void StopMusic()
    {
        musicSource.Stop();
    }

    void StopAllSources()
    {
        foreach (AudioSource a in sources)
            a.Stop();
        ambientSource.Stop();
        musicSource.Stop();
        jingleSource.Stop();
        soundsActive = false;
    }

    void StopAmbientSound()
    {
        ambientSource.Stop();
    }

    void OnPause()
    {
        StopAllSources();
    }

    void OnResume()
    {
        PlayAmbientSound();
        PlayMusic();
    }
    #endregion

    #region Coroutines
    IEnumerator LoadConfigXml(string configText)
    {
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
                    case ("sounds"):
                        ParseSounds(child);
                        break;
                }
            }
            progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnSoundXmlParsed));
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
                case ("sounds"):
                    ParseSounds(xmlNode);
                    break;
            }
        }

        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnSoundXmlParsed));
        yield return new WaitForEndOfFrame();*/
    }
    #endregion

    #region Parsing Functions
    protected void ParseSounds(XmlNode node)
    {
        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode child = node.ChildNodes[i];
            switch (child.Name)
            {
                case ("effects"):
                    soundsEnabled = int.Parse(child.Attributes["volume"].Value) > 0;
                    soundsVolume = int.Parse(child.Attributes["volume"].Value) * 0.01f;
                    break;
                case ("music"):
                    musicEnabled = int.Parse(child.Attributes["volume"].Value) > 0;
                    musicVolume = int.Parse(child.Attributes["volume"].Value) * 0.01f;
                    break;
            }
        }
    }

    protected void ParseSounds(XMLNode node)
    {
        foreach (XMLNode xmlNode in node.children)
        {
            switch (xmlNode.tagName)
            {
                case ("effects"):
                    soundsEnabled = xmlNode.GetAttributeAsString("active").ToLower().Equals("true");
                    break;
                case ("music"):
                    musicEnabled = xmlNode.GetAttributeAsString("active").ToLower().Equals("true");
                    break;
            }
        }
    }
    #endregion

    #region Unity Callbacks

    void Start()
    {
        sources = new AudioSource[2];
        for(int i = 0; i< sources.Length; i++ )
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].loop = false;
            sources[i].volume = 1.0f;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = 0.0f;

        jingleSource = gameObject.AddComponent<AudioSource>();
        jingleSource.playOnAwake = false;
        jingleSource.loop = false;
        jingleSource.volume = musicVolume;

        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.playOnAwake = false;
        ambientSource.loop = true;
        ambientSource.volume = soundsVolume;

        gameManagers = GameObject.FindGameObjectWithTag("GameManagers");
	}

    void Update()
    {
        if (playingJingle)
        {
            if (!jingleSource.isPlaying)
            {
                musicSource.volume = musicVolume;
                playingJingle = false;
            }
        }
    }

    #endregion
}
