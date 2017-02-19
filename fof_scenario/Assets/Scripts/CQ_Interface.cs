using UnityEngine;
using System.Globalization;
using System.Collections;
using pumpkin.display;
using pumpkin.events;
using pumpkin.text;
using System.Collections.Generic;
using System;
using SBS.Core;

public class CQ_Interface : FiniteStateMachine
{
    #region Internal Data Structure
    public class MapElement
    {
        public Vector2 position;
        public float angleRotation;
    }
    #endregion
    
    #region Static Members
    static public int Void = -4;
    static public int SplashScreen = -3;
    static public int LoadingPage = -2;
    static public int AdminPage = -1;
    static public int LoginPage = 0;
    static public int StartPage = 1;
    static public int ChoosePage = 2;
    static public int TutorialPage = 3;
    static public int DetectPage = 4;
    static public int NavigationIngamePage = 5;
    static public int ObstaclesIngamePage = 6;
    static public int NavRewardPage = 7;
    static public int ObsRewardPage = 8;
    static public int AdminSessionPage = 9;
    static public int LoadSessionPage = 10;
    #endregion

    #region Public Properties
    
    public Vector2 LoginTextfieldPosition
    {
        get { return loginTextfieldPosition; }
    }
    public Vector2 LoginTextfieldDim
    {
        get { return loginTextfieldDim; }
    }

    public TutorialPages CurrentTutorialState
    {
        get { return currentTutorialState; }
    }

    public bool IsAdminLogged
    {
        get { return mcAdminPage != null && !mcAdminPage.currentLabel.Equals("adminLogin"); }
    }
    #endregion

    #region Protected Members
    protected string SWFPath = "Flash/CityQuest_UI.swf:";
    protected Stage stage;
    protected LocalizationUtils localizationUtils;
    protected BalanceBoardManager bbManager;
    protected GameplayManager gameplayManager;
    protected GeneralSoundsManager genSoundsManager;
    protected EnvironmentManager environmentManager;
    protected LoginBehaviour login;
    protected Preloader preloader;
    protected int levelsNumber = 1;
    protected bool quittingGame = false;
    protected bool interfaceCanProcede = false;

    protected MovieClip mcCursor = null;
    #endregion

    void StartInterface()
    {
        Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height);
        MovieClipOverlayCameraBehaviour.overlayCameraName = "UICamera";
        stage = MovieClipOverlayCameraBehaviour.instance.stage;

        localizationUtils = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<LocalizationUtils>();
        genSoundsManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GeneralSoundsManager>();
        gameplayManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GameplayManager>();
        environmentManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<EnvironmentManager>();
        bbManager = GameObject.FindGameObjectWithTag("CoreManagers").GetComponent<BalanceBoardManager>();
        preloader = GameObject.FindGameObjectWithTag("Preloader").GetComponent<Preloader>();
        login = gameObject.GetComponent<LoginBehaviour>();

        FiniteStateMachine.FSMObject.Function voidFunc = (self, time) => { };
        fsmObject.AddState(Void, voidFunc, voidFunc, voidFunc);
        fsmObject.AddState(SplashScreen, OnSplashEnter, OnSplashExec, OnSplashExit);
        fsmObject.AddState(LoadingPage, OnLoadingEnter, OnLoadingExec, OnLoadingExit);
        fsmObject.AddState(AdminPage, OnAdminEnter, OnAdminExec, OnAdminExit);
        fsmObject.AddState(LoginPage, OnLoginEnter, OnLoginExec, OnLoginExit);
        fsmObject.AddState(AdminSessionPage, OnAdminSessionPageEnter, OnAdminSessionPageExec, OnAdminSessionPageExit);
        fsmObject.AddState(StartPage, OnStartEnter, OnStartExec, OnStartExit);
        fsmObject.AddState(ChoosePage, OnChooseEnter, OnChooseExec, OnChooseExit);
        fsmObject.AddState(TutorialPage, OnTutorialEnter, OnTutorialExec, OnTutorialExit);
        fsmObject.AddState(DetectPage, OnDetectEnter, OnDetectExec, OnDetectExit);
        fsmObject.AddState(NavigationIngamePage, OnNavIngameEnter, OnNavIngameExec, OnNavIngameExit);
        fsmObject.AddState(ObstaclesIngamePage, OnObsIngameEnter, OnObsIngameExec, OnObsIngameExit);
        fsmObject.AddState(NavRewardPage, OnNavRewardEnter, OnNavRewardExec, OnNavRewardExit);
        fsmObject.AddState(ObsRewardPage, OnObsRewardEnter, OnObsRewardExec, OnObsRewardExit);
        fsmObject.AddState(LoadSessionPage, OnLoadSessionPageEnter, OnLoadSessionPageExec, OnLoadSessionPageExit);

        this.State = SplashScreen;
    }

    #region Utils
    
    public void SetupButton(MovieClip mc, EventDispatcher.EventCallback BtnUpCallback)
    {
        mc.gotoAndStop("up");
        if (!mc.hasEventListener(MouseEvent.MOUSE_LEAVE))
            mc.addEventListener(MouseEvent.MOUSE_LEAVE, OnBtnsLeave);
        if (!mc.hasEventListener(MouseEvent.CLICK))
            mc.addEventListener(MouseEvent.CLICK, BtnUpCallback);
        if (!mc.hasEventListener(MouseEvent.MOUSE_ENTER))
            mc.addEventListener(MouseEvent.MOUSE_ENTER, OnBtnsEnter);
        if (!mc.hasEventListener(MouseEvent.MOUSE_UP))
            mc.addEventListener(MouseEvent.MOUSE_UP, OnBtnsLeave);
        if (!mc.hasEventListener(MouseEvent.MOUSE_DOWN))
            mc.addEventListener(MouseEvent.MOUSE_DOWN, OnBtnsDown);

        mc.mouseEnabled = true;
        mc.mouseChildrenEnabled = false;
        
    }

    public void DisableButton(MovieClip mc)
    {
        mc.mouseEnabled = false;

        mc.removeAllEventListeners(MouseEvent.MOUSE_LEAVE);
        mc.removeAllEventListeners(MouseEvent.CLICK);
        mc.removeAllEventListeners(MouseEvent.MOUSE_ENTER); //MOUSE_DOWN
        mc.removeAllEventListeners(MouseEvent.MOUSE_DOWN);  //MOUSE_DOWN
        mc.removeAllEventListeners(MouseEvent.MOUSE_UP);

        mc.gotoAndStop("up");
    }

    public void DisableLocalizedButton(MovieClip mc)
    {
        mc.mouseEnabled = false;

        mc.removeAllEventListeners(MouseEvent.MOUSE_LEAVE);
        mc.removeAllEventListeners(MouseEvent.CLICK);
        mc.removeAllEventListeners(MouseEvent.MOUSE_ENTER); //MOUSE_DOWN
        mc.removeAllEventListeners(MouseEvent.MOUSE_DOWN);  //MOUSE_DOWN
        mc.removeAllEventListeners(MouseEvent.MOUSE_UP);

        mc.getChildByName<MovieClip>("mcButtonLeft").gotoAndStop("gh");
        mc.getChildByName<MovieClip>("mcButtonFill").gotoAndStop("gh");
        mc.getChildByName<MovieClip>("mcButtonRight").gotoAndStop("gh");
    }

    void OnBtnsLeave(CEvent evt)
    { (evt.currentTarget as MovieClip).gotoAndStop("up"); }

    void OnBtnsEnter(CEvent evt)
    { (evt.currentTarget as MovieClip).gotoAndStop("dn"); }

    void OnBtnsDown(CEvent evt)
    { (evt.currentTarget as MovieClip).gotoAndStop("dn"); }

#endregion

    #region Functions
    public static string MillisecondsToTime(float time, bool printCents)
    {
        time = Mathf.Max(time, 0.0f);

        int cents = Mathf.FloorToInt(time * 100.0f) % 100,
            secs = Mathf.FloorToInt(time) % 60,
            mins = Mathf.FloorToInt(time / 60.0f);

        if (printCents)
            return string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}:{2:D2}", mins, secs, cents);
        else
            return string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}", mins, secs);
    }

    void InitializeTutorial()
    {
        preloader.LoadLevel(0);
    }

    #endregion

    #region Messages
    
    void InterfaceCanProcede()
    {
        Debug.Log("INTERFACE CAN PROCEDE");
        interfaceCanProcede = true;
    }

    void LoadNextInterfaceState()
    {
        if (this.State == LoadingPage)
        {
            if (gameplayManager.Level == 0)
                this.State = TutorialPage;
            else
                
                this.State = gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation ? NavigationIngamePage : ObstaclesIngamePage;
        }
    }

    void GoToNextInterfaceState()
    {
        if (this.State == NavigationIngamePage || this.State == ObstaclesIngamePage)
        {
            Debug.Log("+++++++++++++ gameplayManager.Level: " + gameplayManager.Level + ", gameplayManager.GetLevelCount(): " + gameplayManager.GetLevelCount());
            if (gameplayManager.Level > gameplayManager.GetLevelCount() && !quittingGame)
                this.State = gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation ? NavRewardPage : ObsRewardPage;
            else if (quittingGame)
            {
                this.State = StartPage;
                quittingGame = false;
            }
            else
                this.State = LoadingPage;
        }
        else if (this.State == TutorialPage)
            this.State = LoadingPage;
    }

    //void SetPlayerIsAMan(bool isAMan)
    //{
    //    playerIsAMan = isAMan;
        
    //}

    #endregion

    #region States
    #region SplashPage
    protected MovieClip mcSplashPage = null;
    protected float splashTimer = -1.0f;
    protected bool splashCanProcede = false;
    protected TextField splashMessage = null;

    void OnSplashEnter(FSMObject self, float time)
    {

        mcSplashPage = new MovieClip(SWFPath + "mcSplashClass");
        mcSplashPage.x = Screen.width * 0.5f;
        mcSplashPage.y = Screen.height * 0.5f;
        //mcSplashPage.y = 0f;

        splashMessage = mcSplashPage.getChildByName<TextField>("tfSplashMessage");
        splashMessage.text = string.Empty;

        stage.addChild(mcSplashPage);
        splashTimer = TimeManager.Instance.MasterSource.TotalTime;
    }

    void OnSplashExec(FSMObject self, float time)
    {
        float now = TimeManager.Instance.MasterSource.TotalTime;

        if (splashTimer >= 0.0f && now - splashTimer > 3.0f)
        {
            splashTimer = -1.0f;
            splashCanProcede = true;
        }

        if (splashCanProcede && interfaceCanProcede)
        {
            splashCanProcede = false;
            if (!preloader.useOVR)
            {
                this.State = LoginPage;
            }
            else
            {
                patientCode = "OVR";
                TrackingManager.Instance.UserId = patientCode;
                GameplayManager.HasDoneTutorial = true;
                gameplayManager.Level = 0;
                TrackingManager.Instance.StartTracking();
                TrackingManager.Instance.SessionName = "scene_definition.xml";
                gameplayManager.Gameplay = GameplayManager.GameplayType.ObstacleAvoidance;
                preloader.CheckSession("ObstacleAvoidance/scene_definition.xml");
            }
        }
    }

    void OnSplashExit(FSMObject self, float time)
    {
        stage.removeChild(mcSplashPage);
        mcSplashPage = null;
        interfaceCanProcede = false;
    }

    public void SetSplashMessage(string message)
    {
        if (null != splashMessage)
            splashMessage.text = message;
    }

    #endregion

    #region LoadingPage
    protected MovieClip mcSPBackground = null;
    protected MovieClip mcLoadingPage = null;
    protected MovieClip mcLoadingBar = null;
    protected TextField tfLevelName = null;
    protected TextField tfLoadingLabel = null;

    void OnLoadingEnter(FSMObject self, float time)
    {
        mcSPBackground = new MovieClip(SWFPath + "mcStartPageBackgroundClass");
        //mcSPBackground.x = 0f;
        //mcSPBackground.y = 0f;
        //mcSPBackground.x = Screen.width * 0.5f;
        //mcSPBackground.x = Screen.height * 0.5f;
        mcSPBackground.visible = true;

        mcLoadingPage = new MovieClip(SWFPath + "mcLoginClass");
        mcLoadingPage.gotoAndStop("loading");
        mcLoadingBar = mcLoadingPage.getChildByName<MovieClip>("mcLoadingBar");
        mcLoadingBar.gotoAndStop(1);
        //mcLoadingPage.x = Screen.height * 0.5f;

        tfLevelName = mcLoadingPage.getChildByName<TextField>("tfLevelName");
        tfLoadingLabel = mcLoadingPage.getChildByName<TextField>("tfLoadingLabel");

        if (gameplayManager.Level == 0)
        {
            localizationUtils.AddTranslationText(tfLevelName, "Tutorial");
        }
        else
        {
            localizationUtils.AddTranslationText(tfLevelName, "Loading");
            //if (gameplayManager.learningPhase)
            //{
            GameplayManager.HasDoneTutorial = true;
            PlayerPrefs.SetString(patientCode + "_TUTORIAL", "true");
            PlayerPrefs.Save();
            //}
        }

        localizationUtils.AddTranslationText(tfLoadingLabel, "");

        //tfLevelName.text = "LEVEL TEST";
        //tfLoadingLabel.text = "Loading...";
        stage.addChild(mcSPBackground);
        stage.addChild(mcLoadingPage);            

        preloader.LoadLevel(gameplayManager.Level);
    }

    void OnLoadingExec(FSMObject self, float time)
    { }

    void OnLoadingExit(FSMObject self, float time)
    {
        stage.removeChild(mcSPBackground);
        stage.removeChild(mcLoadingPage);
        mcLoadingPage = null;
        mcSPBackground = null;
    }

    #region Loading Functions
    public void SetLoadingPerc(int perc)
    {
        if (null != mcLoadingBar)
            mcLoadingBar.gotoAndStop(perc);
    }
    #endregion
    #endregion

    #region AdminSessionPage
    protected MovieClip mcSessionConfig = null;
    protected MovieClip btBackSession = null;
    protected MovieClip btLoadSession = null;
    protected MovieClip btSaveSession = null;
    protected MovieClip mcSessionPopup = null;
    protected MovieClip mcSessionRow = null;
    protected MovieClip mcFileBox = null;
    protected MovieClip[] btTests = new MovieClip[8];
    protected TextField tfSessionName = null;
    protected TextField tfSaveName = null;

    protected int numberOfSession = 0;
    bool loadingSessionOk = false;
    string fileName = "session name";
    int filenameX;
    int filenameY;
    int filenameW;
    int filenameH;
    GUIStyle customStyle;
    
    void OnAdminSessionPageEnter(FSMObject self, float time)
    {
        mcSessionConfig = new MovieClip(SWFPath + "mcAdminClass");
        mcSessionConfig.gotoAndStop("adminXML");
        mcSessionRow = mcSessionConfig.getChildByName<MovieClip>("mcSessionRow");
        btBackSession = mcSessionConfig.getChildByName<MovieClip>("btBack");
        btLoadSession = mcSessionConfig.getChildByName<MovieClip>("btLoadSession");
        btSaveSession = mcSessionConfig.getChildByName<MovieClip>("btSaveSession");
        mcFileBox = mcSessionConfig.getChildByName<MovieClip>("mcFileBox");
        tfSessionName = mcSessionRow.getChildByName<TextField>("tfSessionName");
        tfSaveName = mcSessionConfig.getChildByName<TextField>("tfSaveName");
        tfSaveName.text = "";

        mcSessionConfig.getChildByName<MovieClip>("mcSessionsPopup").visible = false;
        UpdateSessionRow(mcSessionRow, false);

        string btBackText = comingFromAdminPage ? "Close" : "Continue";
        localizationUtils.AddTranslationText(tfSessionName, "");
        localizationUtils.AddTranslationButton(btBackSession, btBackText, OnBtBackSessionRelease, LocalizedButton.Alignment.center);
        localizationUtils.AddTranslationButton(btLoadSession, "Load XML File", OnBtLoadSessionRelease, LocalizedButton.Alignment.center);
        localizationUtils.AddTranslationButton(btSaveSession, "Save", OnBtSaveSessionRelease, LocalizedButton.Alignment.center);
        SetupButton(btBackSession, OnBtBackSessionRelease);

        mcSessionPopup = mcSessionConfig.getChildByName<MovieClip>("mcAdminPopup");
        mcSessionPopup.visible = false;

        for (int i = 0; i < btTests.Length; i++)
        {
            btTests[i] = mcSessionConfig.getChildByName<MovieClip>("btTest"+ (i+1).ToString());
            localizationUtils.AddTranslationButton(btTests[i], "Test" + (i + 1).ToString(), OnBtTestRelease, LocalizedButton.Alignment.center);
            btTests[i].visible = false;
        }

        SetPageGhostState(true);
        SetTfSessionResult(false);
        SetTfSaveResult(false);

        stage.addChild(mcSessionConfig);

        loadingSessionOk = false;
        filenameX = (int)mcFileBox.x;
        filenameY = (int)mcFileBox.y;
        filenameW = (int)mcFileBox.width;
        filenameH = (int)mcFileBox.height * 2;

        customStyle = gameObject.GetComponent<LoginBehaviour>().customStyle;
    }
    
    void OnAdminSessionPageExec(FSMObject self, float time)
    { }
    
    void OnAdminSessionPageExit(FSMObject self, float time)
    {
        stage.removeChild(mcSessionConfig);
        mcSessionConfig = null;
    }

    #region AdminSessionPage Functions
    void SetPageGhostState(bool ghost)
    {
        if (ghost)
        {
            //mcSessionRow.visible = false;
            tfSaveName.visible = false;

            localizationUtils.GetLocalizedButton(btSaveSession).DisableLocalizedButton();

            for (int i = 0; i < btTests.Length; i++)
                localizationUtils.GetLocalizedButton(btTests[i]).DisableLocalizedButton();
        }
        else
        {
            mcSessionRow.visible = true;
            tfSaveName.visible = false;

            localizationUtils.GetLocalizedButton(btSaveSession).SetButtonEvents();
            //int b = numberOfSession < 4 ? 4 : 0;

            return;
            for (int i = 0; i < btTests.Length; i++)
            {
                if (numberOfSession < 4)
                {
                    if (i < 4 || i > 4 + numberOfSession)
                    {
                        btTests[i].visible = false;
                    }
                    else
                    {
                        btTests[i].visible = true;
                        localizationUtils.GetLocalizedButton(btTests[i]).SetButtonEvents();
                    }
                }
                else
                {
                    if (i > numberOfSession)
                    {
                        btTests[i].visible = false;
                    }
                    else
                    {
                        btTests[i].visible = true;
                        localizationUtils.GetLocalizedButton(btTests[i]).SetButtonEvents();
                    }
                }
            }
            //SetTfSessionResult(true, "Session Uploaded!");
        }
    }

    public void SetTfSessionResult(bool visibility, string text = "", bool success = true)
    {
        Color txtColor = success ? new Color(0.1490f, 0.3333f, 0.0588f) : new Color(0.8f, 0.0f, 0.0f);
        txtColor = (text.StartsWith("Wait")) ? new Color(0.2588f, 0.2588f, 0.2588f) : txtColor;
        mcSessionConfig.getChildByName<TextField>("tfSessionResults").textFormat.color = txtColor;
        mcSessionConfig.getChildByName<TextField>("tfSessionResults").text = text;
        mcSessionConfig.getChildByName<TextField>("tfSessionResults").visible = true;
    }

    public void DisableSaveButton()
    {
        localizationUtils.GetLocalizedButton(btSaveSession).DisableLocalizedButton();
    }

    public void SetTfSaveResult(bool visibility, string text = "", bool success = true)
    {
        Color txtColor = success ? new Color(0.1490f, 0.3333f, 0.0588f) : new Color(0.8f, 0.0f, 0.0f);
        mcSessionConfig.getChildByName<TextField>("tfSaveResults").textFormat.color = txtColor;
        mcSessionConfig.getChildByName<TextField>("tfSaveResults").text = text;
        mcSessionConfig.getChildByName<TextField>("tfSaveResults").visible = true;
    }

    void UpdateSessionRow(MovieClip mcRow, bool ghost, string fileName = "", string fileTime = "")
    {
        mcRow.visible = true;
        mcRow.gotoAndStop(ghost ? 2 : 1);
        mcRow.getChildByName<TextField>("tfSessionName").visible = !ghost;
        mcRow.getChildByName<TextField>("tfSessionDate").visible = !ghost;
        mcRow.getChildByName<TextField>("tfSessionName").text = ghost ? "" : fileName;
        mcRow.getChildByName<TextField>("tfSessionDate").text = ghost ? "" : fileTime;
    }

    #endregion

    protected void XMLError(string message)
    {
        SetSplashMessage(message);
        SetTfSessionResult(true, message, false);
    }

    protected void ConfigurationReady()
    {
        if (!preloader.useOVR)
            SetTfSessionResult(true, "Configuration Ready", true);
        numberOfSession = gameplayManager.GetLevelCount();

        if (AdminSessionPage == State)
        {
            SetPageGhostState(false);
            environmentManager.StartCreateEnvironment();
        }
        else
        {
            //is loading only the configuration
            preloader.SetLoaded(false);
        }

        if (preloader.useOVR)
            ChooseGenderOperations(true);
    }

    #region LoadSessionPage Buttons Callbacks
    void OnBtTestRelease(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("up");

        //TODO GESTIONE TEST BUTTONS
    }

    void OnBtBackSessionRelease(CEvent evt)
    {
        //Debug.LogWarning("UNLOAD");
        preloader.UnloadLevel();

        (evt.currentTarget as MovieClip).gotoAndStop("dn");
        if (comingFromAdminPage)
            this.State = LoginPage;
        else
            this.State = StartPage;

        m_fileBrowser = null;
    }

    void OnBtLoadSessionRelease(CEvent evt)
    {
        //(evt.currentTarget as MovieClip).gotoAndStop("dn");
        SetTfSessionResult(true, "Wait please...", true);
        SetTfSaveResult(false);
        SetPageGhostState(true);
        //StartCoroutine(ShowConfigurationDialog());

        if (m_fileBrowser == null)
        {
            localizationUtils.GetLocalizedButton(btLoadSession).DisableLocalizedButton();
            
            //(evt.currentTarget as MovieClip).gotoAndStop("dn");

            m_fileBrowser = new FileBrowser(
                    new Rect((Screen.width * 0.5f) - 300, (Screen.height * 0.5f) - 200, 600, 400),
                    "Choose XML Scene",
                    SceneCallback,
                    "./Configurations/xml"
                );
            m_fileBrowser.SelectionPattern = "*.xml";
            m_fileBrowser.DirectoryImage = m_directoryImage;
            m_fileBrowser.FileImage = m_fileImage;
        }
    }

    protected void SceneCallback(string path)
    {
        m_fileBrowser = null;
        localizationUtils.GetLocalizedButton(btLoadSession).SetButtonEvents();
        if (null != path)
        {
            string result = System.IO.Path.GetFileName(path);
            UpdateSessionRow(mcSessionRow, false, result);
            preloader.CheckSession(path);
            SetTfSaveResult(true, "Edit the session name and click Save", true);
            loadingSessionOk = true;
        }
    }

    /*IEnumerator ShowConfigurationDialog()
    {
        yield return new WaitForSeconds(0.5f);  //hack in order to show the wait message
        System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
        ofd.InitialDirectory = "Configurations\\xml";
        ofd.Filter = "XML Session (*.xml)|*.xml";
        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string result = System.IO.Path.GetFileName(ofd.FileName);
            UpdateSessionRow(false, result);
            preloader.CheckSession(ofd.FileName);
            loadingSessionOk = true;
            SetTfSaveResult(true, "Edit the session name and click Save", true);
        }
        else if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel || ofd.ShowDialog() == System.Windows.Forms.DialogResult.Abort)
        {
            SetTfSessionResult(false);
        }
    }*/

    void OnBtSaveSessionRelease(CEvent evt)
    {
        //(evt.currentTarget as MovieClip).gotoAndStop("dn");
        //if (comingFromAdminPage)

        DataContainer.SetFileName("Sessions\\session_" + fileName + ".ses");
        DataContainer.SaveToFile();
        SetTfSaveResult(true, "Session saved", true);
    }

    #endregion
    #endregion

    #region AdminPage
    protected MovieClip mcAdminPage = null;
    protected MovieClip mcAdminPopup = null;
    protected MovieClip btAdminPopupContinue = null;
    protected MovieClip mcSubmitBar = null;
    
    protected MovieClip mcAdminTextfield = null;
    protected MovieClip btAdminLogin = null;
    protected MovieClip btAdminBack = null;
    protected TextField tfInsertPaass = null;

    protected bool comingFromAdminPage = false;

    void OnAdminEnter(FSMObject self, float time)
    {
        mcAdminPage = new MovieClip(SWFPath + "mcAdminClass");


        //if (comingFromAdminPage)
        //{
            comingFromAdminPage = false;
        //    InitAdminPage("adminPanel");
        //}
        //else
            InitAdminPage("adminLogin");

        stage.addChild(mcAdminPage);
    }

    void OnAdminExec(FSMObject self, float time)
    { }

    void OnAdminExit(FSMObject self, float time)
    {
        stage.removeChild(mcAdminPage);
        mcAdminPage = null;
    }

    #region AdminPage Functions

    void InitAdminPage(string page)
    {
        switch (page)
        {
            case "adminLogin": 
                mcAdminPage.gotoAndStop("adminLogin");
                localizationUtils.AddTranslationText(mcAdminPage.getChildByName<TextField>("tfInsertPass"), "INSERT ADMIN PASSWORD");
                btAdminLogin = mcAdminPage.getChildByName<MovieClip>("btConnection");
                btAdminBack = mcAdminPage.getChildByName<MovieClip>("btAdminBack");
                localizationUtils.AddTranslationButton(btAdminLogin, "CONNECTION", OnAdminBtConnectionRelease, LocalizedButton.Alignment.center);
                localizationUtils.AddTranslationButton(btAdminBack, "BACK", OnBtAdminBackRelease, LocalizedButton.Alignment.left);

                mcAdminPopup = mcAdminPage.getChildByName<MovieClip>("mcAdminPopup");
                mcAdminPopup.visible = false;
                break;
            case "adminPanel":
                mcAdminPage.gotoAndStop("adminPanel");
                localizationUtils.AddTranslationButton(mcAdminPage.getChildByName<MovieClip>("btCreate"), "SESSION", OnAdminBtSubmitRelease, LocalizedButton.Alignment.center);
                localizationUtils.AddTranslationButton(mcAdminPage.getChildByName<MovieClip>("btBack"), "BACK", OnBtAdminBackRelease, LocalizedButton.Alignment.left);
                localizationUtils.AddTranslationText(mcAdminPage.getChildByName<TextField>("tfLabel1"), "UPLOAD DATA");
                localizationUtils.AddTranslationText(mcAdminPage.getChildByName<TextField>("tfLabel2"), "");
                mcAdminPopup = mcAdminPage.getChildByName<MovieClip>("mcAdminPopup");
                mcAdminPopup.visible = false;
            break;
        }
    }

    void HideAdminPopup()
    {
        mcAdminPopup.visible = false;
    }

    void ShowAdminPopup(string type)
    {
        switch (type)
        {
            case "incorrectPassword":
                Debug.Log("SHOWPOPUP PASSWORD ERRATA");
                mcAdminPopup.gotoAndStop(type);
                localizationUtils.AddTranslationText(mcAdminPopup.getChildByName<TextField>("tfLabel"), "WRONG PASSWORD");
                localizationUtils.AddTranslationButton(mcAdminPopup.getChildByName<MovieClip>("btContinue"), "BACK", OnAdminIncorrectPasswordRelease, LocalizedButton.Alignment.center);
                break;
            case "submitRequest":
                mcAdminPopup.gotoAndStop(type);
                localizationUtils.AddTranslationText(mcAdminPopup.getChildByName<TextField>("tfLabel"), "ARE YOU SURE? ");
                localizationUtils.AddTranslationButton(mcAdminPopup.getChildByName<MovieClip>("btBack"), "BACK", OnAdminPopupBtBackRelease, LocalizedButton.Alignment.center);
                localizationUtils.AddTranslationButton(mcAdminPopup.getChildByName<MovieClip>("btCreate"), "SUBMIT", OnAdminPopupBtSubmitRelease, LocalizedButton.Alignment.center);
                break;
            case "submitProgress":
                mcAdminPopup.gotoAndStop(type);
                localizationUtils.AddTranslationText(mcAdminPopup.getChildByName<TextField>("tfLabel"), "UPLOADING DATAS");
                mcSubmitBar = mcAdminPopup.getChildByName<MovieClip>("mcSubmitBar");
                mcSubmitBar.gotoAndStop(1);
                break;
            case "submitComplete":
                mcAdminPopup.gotoAndStop("incorrectPassword");
                localizationUtils.AddTranslationText(mcAdminPopup.getChildByName<TextField>("tfLabel"), "UPLOAD COMPLETE");
                localizationUtils.AddTranslationButton(mcAdminPopup.getChildByName<MovieClip>("btContinue"), "OK", OnAdminSubmitCompleteRelease, LocalizedButton.Alignment.center);
                break;
        }
        mcAdminPopup.visible = true;
    }

    #endregion

    #region AdminPage Buttons Callbacks

    void OnAdminBtConnectionRelease(CEvent evt)
    {
        if (login.GetAdminPassword() == login.CodedAdminPassword)
        {
            comingFromAdminPage = true;
            this.State = AdminSessionPage;

            //InitAdminPage("adminPanel");
        }
        else
            ShowAdminPopup("incorrectPassword");
    }

    void OnAdminBtSubmitRelease(CEvent evt)
    {
        comingFromAdminPage = true;
        this.State = AdminSessionPage;

        //ShowAdminPopup("submitRequest");
    }

    void OnBtAdminBackRelease(CEvent evt)
    {
        this.State = LoginPage;
    }

    void OnAdminIncorrectPasswordRelease(CEvent evt)
    {
        HideAdminPopup();
    }

    void OnAdminSubmitCompleteRelease(CEvent evt)
    {
        HideAdminPopup();
    }

    void OnAdminPopupBtBackRelease(CEvent evt)
    {
        mcAdminPopup.visible = false;
    }

    void OnAdminPopupBtSubmitRelease(CEvent evt)
    {
        ShowAdminPopup("submitProgress");
        //TODO submission progress bar;
    }


    #endregion

    #endregion

    #region LoginPage
    protected MovieClip mcLoginPage = null;
    protected MovieClip btConnection = null;
    protected MovieClip btAdmin = null;
    protected TextField tfPatient = null;
    protected TextField tfNameLabel = null;
    protected MovieClip mcLoginTextfield = null;

    protected MovieClip mcStartCheat = null;

    protected Vector2 loginTextfieldPosition;
    protected Vector2 loginTextfieldDim;

    protected string patientCode;

    void OnLoginEnter(FSMObject self, float time)
    {
        Debug.Log("<<< LOGIN PAGE >>>");

        MovieClipOverlayCameraBehaviour.overlayCameraName = "UICamera";
        stage = MovieClipOverlayCameraBehaviour.instance.stage;

        mcLoginPage = new MovieClip(SWFPath + "mcLoginClass");
        mcClose = mcLoginPage.getChildByName<MovieClip>("mcClose");
        mcLoginPage.gotoAndStop("login");
        btConnection = mcLoginPage.getChildByName<MovieClip>("btConnection");
        btAdmin = mcLoginPage.getChildByName<MovieClip>("btAdmin");
        localizationUtils.AddTranslationButton(btConnection, "CONNECTION", OnBtConnectionRelease, LocalizedButton.Alignment.center);
        localizationUtils.AddTranslationButton(btAdmin, "ADMIN", OnBtAdminRelease, LocalizedButton.Alignment.right);

        localizationUtils.AddTranslationButton(mcLoginPage.getChildByName<MovieClip>("btStartCheat"), "GoToLevel", OnStartCheatRelease, LocalizedButton.Alignment.right);

        tfPatient = mcLoginPage.getChildByName<TextField>("tfPatient");
        tfNameLabel = mcLoginPage.getChildByName<TextField>("tfNameLabel");
        localizationUtils.AddTranslationText(tfPatient, "Participant Code");
        localizationUtils.AddTranslationText(tfNameLabel, "(Unique name)");

        stage.addChild(mcLoginPage);
        login.Initialize();

        SetupButton(mcClose, OnLoginBtCloseRelease);

        //gameplayManager.Gameplay = GameplayManager.GameplayType.Navigation;
    }

    void OnLoginExec(FSMObject self, float time)
    { }

    void OnLoginExit(FSMObject self, float time)
    {
        stage.removeChild(mcLoginPage);
        mcLoginPage = null;
    }

    #region LoginPage Functions

    void OnLoginBtCloseRelease(CEvent evt)
    {
        Application.Quit();
    }

    bool ValidateLogin()
    {
        patientCode = gameObject.GetComponent<LoginBehaviour>().GetUserId();
#if UNITY_EDITOR
        if (patientCode == "")
            patientCode = "1";
#endif
        if (patientCode != "")
        {
            if (!PlayerPrefs.HasKey(patientCode + "_NAME"))
            {
                Debug.Log("NEW PATIENT CODE");
                PlayerPrefs.SetString(patientCode + "_NAME", patientCode);
                PlayerPrefs.SetString(patientCode + "_TUTORIAL", "false");
                GameplayManager.HasDoneTutorial = false;
            }
            else
            {
                Debug.Log("EXISTING PATIENT CODE");
                GameplayManager.HasDoneTutorial = PlayerPrefs.GetString(patientCode + "_TUTORIAL", "false").Equals("true");
            }

            TrackingManager.Instance.UserId = patientCode;
            return true;
        }
        //GameplayManager.HasDoneTutorial = false;
        return false;
    }

    #endregion

    #region LoginPage Buttons Callbacks

    void OnBtConnectionRelease(CEvent evt)
    {
        genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.click);
        if (ValidateLogin())
        {
            gameplayManager.Level = 0;
            this.State = LoadSessionPage;
        }
        //this.State = StartPage;
    }

    void OnBtAdminRelease(CEvent evt)
    {
        genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.click);
        this.State = AdminPage;
    }

    void OnStartCheatRelease(CEvent evt)
    {
        this.State = LoadingPage;
        
    }

    #endregion
    #endregion

    #region StartPage
    protected MovieClip mcStartPage = null;
    protected MovieClip btPlaySP = null;
    protected MovieClip btLogout = null;
    protected MovieClip mcCheckTutorial = null;
    protected MovieClip mcCheck = null;
    protected MovieClip mcCheckGreen = null;

    protected TextField tfTutorialEnabled = null;
    
    void OnStartEnter(FSMObject self, float time)
    {
        Debug.Log("<<< START PAGE ENTER >>>");

        mcStartPage = new MovieClip(SWFPath + "mcStartpageClass");
        btPlaySP = mcStartPage.getChildByName<MovieClip>("btPlaySP");
        btLogout = mcStartPage.getChildByName<MovieClip>("btLogout");
        localizationUtils.AddTranslationButton(btPlaySP, "PLAY", OnBtPlaySPRelease, LocalizedButton.Alignment.center, true, true, gameplayManager.Gameplay.ToString());
        localizationUtils.AddTranslationButton(btLogout, "LOGOUT", OnStartPageBtLogoutRelease, LocalizedButton.Alignment.center);

        stage.addChild(mcStartPage);

        mcCheckTutorial = mcStartPage.getChildByName<MovieClip>("mcCheckTutorial");
        mcCheck = mcCheckTutorial.getChildByName<MovieClip>("mcCheck");
        tfTutorialEnabled = mcCheckTutorial.getChildByName<TextField>("tfTutorial");
        localizationUtils.AddTranslationText(tfTutorialEnabled, "TUTORIAL");

        if (!mcCheck.hasEventListener(MouseEvent.CLICK))
            mcCheck.addEventListener(MouseEvent.CLICK, OnTutorialCheckClick);

        mcCheckGreen = mcCheck.getChildByName<MovieClip>("mcCheckSmall");
        mcCheckGreen.gotoAndStop(1);
        mcCheckGreen.visible = !GameplayManager.HasDoneTutorial;

        genSoundsManager.SendMessage("PlayMusic");
        gameplayManager.LearningPhaseDone = false;

        gameplayManager.SendMessage("ResetNewSession", SendMessageOptions.DontRequireReceiver);

        TrackingManager.Instance.StopTracking();
    }

    void OnStartExec(FSMObject self, float time)
    {}

    void OnStartExit(FSMObject self, float time)
    {
        gameplayManager.Score = 0;
        stage.removeChild(mcStartPage);
        mcStartPage = null;
    }

    #region StartPage Buttons Callbacks

    void OnTutorialCheckClick(CEvent evt)
    {
        mcCheckGreen.visible = !mcCheckGreen.visible;

        if (!GameplayManager.HasDoneTutorial && !mcCheckGreen.visible)
        {
            PlayerPrefs.SetString(patientCode + "_TUTORIAL", "true");
            GameplayManager.HasDoneTutorial = true;
        }
        else
            GameplayManager.HasDoneTutorial = false;
    }

    void OnBtPlaySPRelease(CEvent evt)
    {
        genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.click);
        //this.State = DetectPage;
        gameplayManager.Level = 0;
        this.State = ChoosePage;
        //InitializeTutorial();
        TrackingManager.Instance.StartTracking();
    }

    void OnStartPageBtLogoutRelease(CEvent evt)
    {
        genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.click);
        this.State = LoginPage;
    }

    #endregion
    #endregion

    #region ChoosePage
    protected MovieClip mcChoosePage = null;
    protected MovieClip btChooseMan = null;
    protected MovieClip btChooseWoman = null;
    protected TextField tfChooseLabel = null;

    void OnChooseEnter(FSMObject self, float time)
    {
        mcChoosePage = new MovieClip(SWFPath + "mcLoginClass");
        mcChoosePage.gotoAndStop("choose");
        btChooseMan = mcChoosePage.getChildByName<MovieClip>("btChooseMan");
        btChooseWoman = mcChoosePage.getChildByName<MovieClip>("btChooseWoman");
        localizationUtils.AddTranslationButton(btChooseMan, "MAN", OnBtChooseMenRelease, LocalizedButton.Alignment.center);
        localizationUtils.AddTranslationButton(btChooseWoman, "WOMAN", OnBtChooseWomanRelease, LocalizedButton.Alignment.center);
        mcSPBackground = new MovieClip(SWFPath + "mcStartPageBackgroundClass");
        mcSPBackground.x = 0.0f;
        mcSPBackground.y = 0.0f;
        mcSPBackground.visible = true;
        stage.addChild(mcSPBackground);

        stage.addChild(mcChoosePage);
    }

    void OnChooseExec(FSMObject self, float time)
    { }

    void OnChooseExit(FSMObject self, float time)
    {
        stage.removeChild(mcChoosePage);
        mcChoosePage = null;
        stage.removeChild(mcSPBackground);
    }

    void ChooseGenderOperations(bool male)
    {
        genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.click);
        GameplayManager.CharacterGender g = male ? GameplayManager.CharacterGender.Male : GameplayManager.CharacterGender.Female;
        gameplayManager.SendMessage("SetCharacterGender", g);

        if (preloader.useBalanceBoard)
            this.State = DetectPage;
        else
            this.State = LoadingPage;
    }

    #region ChoosePage Buttons Callbacks

    void OnBtChooseMenRelease(CEvent evt)
    {
        //Message: Men chosen
        bool isAMan = true;
        ChooseGenderOperations(isAMan);
    }

    void OnBtChooseWomanRelease(CEvent evt)
    {
        //Message: Woman chosen
        bool isAMan = false;
        ChooseGenderOperations(isAMan);
    }

    #endregion

    #endregion

    #region TutorialPage
    protected MovieClip mcTutorialPage = null;
    protected MovieClip mcTutorialHud = null;
    protected TextField tfTutorial = null;
    protected TextField tfFollowLabel = null;
    protected MovieClip mcTutorialPopup = null;
    protected MovieClip mcTutorialAnimation = null;
    protected MovieClip mcInstructions = null;
    protected MovieClip mcPopupStars = null;

    protected MovieClip mcBalanceBoard = null;
    protected MovieClip mcBBPressurePoint = null;

    protected TutorialPages currentTutorialState = TutorialPages.startTutorial;
    protected float tutorialTimer = -1.0f;
    protected float tutorialInterval = 3.0f;

    protected bool animatedTutorial = false;
    protected bool animatedOneWayTutorial = false;
    protected bool oneWayBackwardAnim = false;

    protected TutorialManager _tutManager = null;
    protected BalanceBoardManager _bbManager = null;

    protected float animationTimer = -1.0f;
    protected float animationStepTimer = -1.0f;
    protected float animStepInterval = 1.0f;
    protected bool animGoingBackwards = false;
    protected string startAnimLabel = "";
    protected string stopAnimLabel = "";

    public enum TutorialPages
    {
        startTutorial = 0,
        message,
        messageInstruction,
        instructions,
        turnLeft,
        turnRight,
        shortLeft,
        shortRight,
        shortStraight,
        shortStraight2,
        returnToCenter,
        shortBack,
        stepDown,
        speedUp,
        slowDown,
        avoidMovingObstacleBoth,
        avoidMovingObstacleBothBis,
        avoidObstacleRight,
        avoidObstacleRightBis,
        avoidObstacleLeft,
        avoidObstacleLeftBis,
        stopOnTarget,
        startAgain,
        retroFront
    }

    void OnTutorialEnter(FSMObject self, float time)
    {
        SetupInstructions();
        _tutManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<TutorialManager>();
        _bbManager = GameObject.FindGameObjectWithTag("CoreManagers").GetComponent<BalanceBoardManager>();
        mcTutorialPage = new MovieClip(SWFPath + "mcTutorialClass");
        mcTutorialPage.x = Screen.height * 0.5f;
        stage.addChild(mcTutorialPage);
        mcTutorialHud = new MovieClip(SWFPath + "mcTutorialHudClass");
        mcTutorialHud.x = Screen.height * 0.5f;
        stage.addChild(mcTutorialHud);

        mcBalanceBoard = new MovieClip(SWFPath + "mcBalanceBoardClass");
        stage.addChild(mcBalanceBoard);
        mcBalanceBoard.x = Screen.width * 0.5f;
        mcBalanceBoard.y = Screen.height * 0.5f;
        mcBBPressurePoint = mcBalanceBoard.getChildByName<MovieClip>("mcBBPressurePoint");

        mcClose = mcTutorialHud.getChildByName<MovieClip>("mcClose");
        mcPause = mcTutorialHud.getChildByName<MovieClip>("mcPause");
        SetupButton(mcClose, OnIngameBtCloseRelease);
        SetupButton(mcPause, OnIngameBtPauseRelease);

        mcPopupPause = new MovieClip(SWFPath + "mcPopupPauseClass");
        mcPopupPause.x = Screen.width * 0.5f;
        mcPopupPause.y = Screen.height * 0.5f;
        stage.addChild(mcPopupPause);
        mcPopupPause.visible = false;


        _tutManager.SendMessage("StartTutorial");

        Debug.Log("<<< Interface.TUTORIAL PAGE >>> mcTutorialPage: "+ mcTutorialPage.name);

        InitializeFade();
    }

    float hor = 0.0f;
    float ver = 0.0f;


    void OnTutorialExec(FSMObject self, float time)
    {
        UpdateFade();
        UpdateTutorialAnim();
        UpdateOneWayTutorialAnim();

        if(_bbManager != null)
            UpdateBBPressurePointPosition(_bbManager.CPHorizontal, _bbManager.CPVertical);

#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Keypad8))
        {
            ver -= 0.1f;
            UpdateBBPressurePointPosition(hor, ver);
        }
        if(Input.GetKeyDown(KeyCode.Keypad2))
        {
            ver += 0.1f;
            UpdateBBPressurePointPosition(hor, ver);
        }
        if(Input.GetKeyDown(KeyCode.Keypad4))
        {
            hor -= 0.1f;
            UpdateBBPressurePointPosition(hor, ver);
        }
        if(Input.GetKeyDown(KeyCode.Keypad6))
        {
            hor += 0.1f;
            UpdateBBPressurePointPosition(hor, ver);
        }
#endif
    }

    void OnTutorialExit(FSMObject self, float time)
    {
        ResetFade();
        Debug.Log("<<< OnTutorialExit >>>");
        ShowHideTutorial(false);
        DestroyInstructions();
        stage.removeChild(mcTutorialPage);
        stage.removeChild(mcPopupPause);
        stage.removeChild(mcBalanceBoard);
        stage.removeChild(mcTutorialHud);
        mcTutorialPage = null;
        mcBalanceBoard = null;
    }

    #region Tutorial Page Functions

    void TutorialCompleted()
    {
        this.State = gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation ? NavigationIngamePage : ObstaclesIngamePage;
    }

    public void SetAnimatedMessage(string label)
    {
        if (mcTutorialPopup != null)
        {
            string newPopupText = mcTutorialPopup.getChildByName<TextField>("tfLabel").text;
            mcTutorialPopup.gotoAndStop(label);
            mcTutorialPopup.getChildByName<TextField>("tfLabel").text = newPopupText;

            if (label.Equals("image"))
            {
                bool isAMan = gameplayManager.Gender == GameplayManager.CharacterGender.Male;
                mcTutorialPopup.getChildByName<MovieClip>("mcSilhouette").gotoAndStop(isAMan ? "man" : "woman");
            }
        }
    }

    public void GoToTutorialState(TutorialPages state, string text, bool completeAnim = true)
    {
        InitBBPressurePoint();
        bool playerIsAMan = gameplayManager.Gender == GameplayManager.CharacterGender.Male;

        switch (state)
        {
            case TutorialPages.startTutorial:
                mcTutorialPage.gotoAndStop("startTutorial");
                tfTutorial = mcTutorialPage.getChildByName<TextField>("tfTutorial");
                tfFollowLabel = mcTutorialPage.getChildByName<TextField>("tfFollowLabel");
                localizationUtils.AddTranslationText(tfTutorial, "TUTORIAL");
                localizationUtils.AddTranslationText(tfFollowLabel, text);
                currentTutorialState = TutorialPages.startTutorial;
                animatedTutorial = false;
                mcBalanceBoard.visible = false;
                break;
            case TutorialPages.message:
                mcTutorialPage.gotoAndStop("message");
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("text");
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); // "Let's see how to walk");
                currentTutorialState = TutorialPages.message;
                //StartTutorialAnim(1.5f);
                mcBalanceBoard.visible = false;
                break;
            case TutorialPages.turnLeft:
                mcTutorialPage.gotoAndStop("turnLeft");
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("turnLeft");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").gotoAndStop("left");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").visible = true;
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "Lean to turn to the left");
                currentTutorialState = TutorialPages.turnLeft;
                if(completeAnim) StartTutorialAnim(1.5f); else StartOneWayAnim(1.5f, false);//StartTutorialAnim(1.5f);
                mcBalanceBoard.visible = true;
                break;
            case TutorialPages.turnRight:
                mcTutorialPage.gotoAndStop("turnRight");
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("turnRight");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").gotoAndStop("left");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").visible = true;
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "Lean to turn to the right");
                currentTutorialState = TutorialPages.turnRight;
                if(completeAnim) StartTutorialAnim(1.5f); else StartOneWayAnim(1.5f, false);//StartTutorialAnim(1.5f);
                mcBalanceBoard.visible = true;
                break;
            case TutorialPages.shortStraight:
                mcTutorialPage.gotoAndStop("shortStraight");
                mcTutorialPage.getChildByName<MovieClip>("mcShortStraight").visible = false;
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("leanFwd");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").gotoAndStop("right");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").visible = true;
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "Bend forward to walk");
                currentTutorialState = TutorialPages.shortStraight;
                if(completeAnim) StartTutorialAnim(1.5f); else StartOneWayAnim(1.5f, false);
                mcBalanceBoard.visible = true;
                break;
            case TutorialPages.returnToCenter:
                mcTutorialPage.gotoAndStop("shortStraight");
                mcTutorialPage.getChildByName<MovieClip>("mcShortStraight").visible = false;
                mcTutorialPopup.gotoAndStop("leanFwd");
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").gotoAndStop("left");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").visible = true;
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "Bend forward to walk");
                currentTutorialState = TutorialPages.shortStraight;
                if (completeAnim) StartTutorialAnim(1.5f); else StartOneWayAnim(1.5f, true);
                mcBalanceBoard.visible = true;
                break;
            case TutorialPages.speedUp:
                mcTutorialPage.gotoAndStop("shortStraight");
                mcTutorialPage.getChildByName<MovieClip>("mcShortStraight").visible = false;
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("leanFwd");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").gotoAndStop("right");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").visible = true;
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "Bend forward to speed up");
                currentTutorialState = TutorialPages.speedUp;
                if(completeAnim) StartTutorialAnim(1.5f); else StartOneWayAnim(1.5f, false);//StartTutorialAnim(1.5f);
                mcBalanceBoard.visible = true;
                break;
           
            case TutorialPages.avoidObstacleRightBis:
                mcTutorialPage.gotoAndStop("shortRight");
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("turnRight");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").gotoAndStop("left");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").visible = true;
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "Avoid the obstacle");
                currentTutorialState = TutorialPages.avoidObstacleRightBis;
                if (completeAnim) StartTutorialAnim(1.0f); else StartOneWayAnim(1.0f, false);//StartTutorialAnim(1.0f);
                mcBalanceBoard.visible = true;
                break;
       
            case TutorialPages.avoidObstacleLeftBis:
                mcTutorialPage.gotoAndStop("shortLeft");
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("turnLeft");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").gotoAndStop("left");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").visible = true;
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "Avoid the obstacle");
                currentTutorialState = TutorialPages.avoidObstacleLeftBis;
                if (completeAnim) StartTutorialAnim(1.0f); else StartOneWayAnim(1.0f, false);//StartTutorialAnim(1.0f);
                mcBalanceBoard.visible = true;
                break;
            case TutorialPages.shortBack:
                mcTutorialPage.gotoAndStop("shortBack");
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("leanBack");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").gotoAndStop("left");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").visible = true;
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "Bend backward to stop");
                currentTutorialState = TutorialPages.shortBack;
                if(completeAnim) StartTutorialAnim(1.5f); else StartOneWayAnim(1.5f, false);//StartTutorialAnim(1.5f);
                mcBalanceBoard.visible = true;
                break;
            case TutorialPages.retroFront:
                mcTutorialPage.gotoAndStop("shortBack");
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("leanBack");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").gotoAndStop("left");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").visible = true;
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "Bend backward to turn back");
                currentTutorialState = TutorialPages.shortBack;
                if(completeAnim) StartTutorialAnim(1.5f); else StartOneWayAnim(1.5f, false);//StartTutorialAnim(1.5f);
                mcBalanceBoard.visible = true;
                break;
            case TutorialPages.stopOnTarget:
                mcTutorialPage.gotoAndStop("shortBack");
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("leanBack");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").gotoAndStop("left");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").visible = true;
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "Stop in front of the BAKERY");
                currentTutorialState = TutorialPages.stopOnTarget;
                if(completeAnim) StartTutorialAnim(1.5f); else StartOneWayAnim(1.5f, false);//StartTutorialAnim(1.5f);
                mcBalanceBoard.visible = true;
                break;
            case TutorialPages.slowDown:
                mcTutorialPage.gotoAndStop("shortBack");
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("leanBack");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").gotoAndStop("left");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").visible = true;
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "Bend backward to slow down");
                currentTutorialState = TutorialPages.slowDown;
                if(completeAnim) StartTutorialAnim(1.5f); else StartOneWayAnim(1.5f, false);//StartTutorialAnim(1.5f);
                mcBalanceBoard.visible = true;
                break;
            case TutorialPages.stepDown:
                mcTutorialPage.gotoAndStop("shortBack");
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("getOff");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").gotoAndStop("left");
                mcTutorialPopup.getChildByName<MovieClip>("mcArrow").visible = true;
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "Stop and step down from the balance board");
                currentTutorialState = TutorialPages.stepDown;
                if(completeAnim) StartTutorialAnim(1.5f); else StartOneWayAnim(1.5f, false);//StartTutorialAnim(1.5f);
                mcBalanceBoard.visible = true;
                break;
            case TutorialPages.startAgain:
                mcTutorialPage.gotoAndStop("shortBack");
                mcTutorialPopup = mcTutorialPage.getChildByName<MovieClip>("mcTutorialPopup");
                mcTutorialPopup.gotoAndStop("textOnly");
                localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), text); //  "START AGAIN");
                currentTutorialState = TutorialPages.startAgain;
                animatedTutorial = false;
                mcBalanceBoard.visible = true;
                break;
        }
    }

    public void SetMessageText(string messageText)
    {
        localizationUtils.AddTranslationText(mcTutorialPopup.getChildByName<TextField>("tfLabel"), messageText);
    }

    void StartOneWayAnim(float interval, bool backward)
    {
        oneWayBackwardAnim = backward;
        mcTutorialAnimation = mcTutorialPopup.getChildByName<MovieClip>("mcFigure");
        bool isAMan = gameplayManager.Gender == GameplayManager.CharacterGender.Male;
        animatedOneWayTutorial = true;
        //animationStepTimer = TimeManager.Instance.MasterSource.TotalTime;

        //mcTutorialAnimation.gotoAndStop(startAnimLabel);
        animStepInterval = interval;

        if (backward)
        {
            startAnimLabel = isAMan ? "startMan" : "startWoman";
            stopAnimLabel = isAMan ? "stopMan" : "stopWoman";
            mcTutorialAnimation.gotoAndStop(stopAnimLabel);
            PlayBackAnimation(TimeManager.Instance.MasterSource.TotalTime);
        }
        else
        {
            startAnimLabel = isAMan ? "startMan" : "startWoman";
            stopAnimLabel = isAMan ? "stopMan" : "stopWoman";
            mcTutorialAnimation.gotoAndStop(startAnimLabel);
            PlayAnimation(TimeManager.Instance.MasterSource.TotalTime);
        }
        animationStepTimer = -1.0f;
        //startLabelTimer = ;
    }

    float startLabelTimer = -1.0f;
    void UpdateOneWayTutorialAnim()
    {
        if (animatedOneWayTutorial)
        {
            if (mcTutorialPopup != null && mcTutorialPage.visible)
            {
                float now = TimeManager.Instance.MasterSource.TotalTime;

                if (animationStepTimer > 0.0f && now - animationStepTimer >= animStepInterval)
                {
                    if (mcTutorialAnimation.currentLabel.Equals(stopAnimLabel))
                    {
                        animationStepTimer = -1.0f;
                        mcTutorialAnimation.gotoAndStop(startAnimLabel);
                        startLabelTimer = TimeManager.Instance.MasterSource.TotalTime;
                    }
                    else if ((oneWayBackwardAnim && mcTutorialAnimation.currentLabel.Equals(startAnimLabel)))
                    {
                        animationStepTimer = -1.0f;
                        mcTutorialAnimation.gotoAndStop(stopAnimLabel);
                        startLabelTimer = TimeManager.Instance.MasterSource.TotalTime;
                    }
                }

                if(startLabelTimer > 0 && now - startLabelTimer > 1.0f)
                {
                    startLabelTimer = -1.0f;
                    if (!oneWayBackwardAnim)
                        PlayAnimation();
                    else
                        PlayBackAnimation();
                }
                
            }
        }
    }

    void StartTutorialAnim(float interval)
    {
        mcTutorialAnimation = mcTutorialPopup.getChildByName<MovieClip>("mcFigure");
        bool isAMan = gameplayManager.Gender == GameplayManager.CharacterGender.Male;
        animatedTutorial = true;
        //animationStepTimer = TimeManager.Instance.MasterSource.TotalTime;
        startAnimLabel = isAMan ? "startMan" : "startWoman";
        stopAnimLabel = isAMan ? "stopMan" : "stopWoman";
        mcTutorialAnimation.gotoAndStop(startAnimLabel);
        animStepInterval = interval;
        
        PlayAnimation();
        animationStepTimer = -1.0f;
    }

    void UpdateTutorialAnim()
    {
        if (animatedTutorial)
        {
            if (mcTutorialPopup != null && mcTutorialPage.visible)
            {
                float now = TimeManager.Instance.MasterSource.TotalTime;

                if (animationStepTimer > 0.0f && now - animationStepTimer >= animStepInterval)
                {
                    animationStepTimer = -1.0f;

                    if (mcTutorialAnimation.currentLabel.Equals(startAnimLabel))
                        PlayAnimation();
                    else
                        PlayBackAnimation();
                }
            }
        }
    }

    void PlayAnimation(float startFrameTime = -1.0f)
    {
        if (startFrameTime < 0)
        {
            mcTutorialAnimation.gotoAndPlay(startAnimLabel);
            mcTutorialAnimation.addFrameScript(stopAnimLabel, n =>
            {
                mcTutorialAnimation.gotoAndStop(stopAnimLabel);
                animationStepTimer = TimeManager.Instance.MasterSource.TotalTime;
                mcTutorialAnimation.getChildByName<MovieClip>("mcDiagonal").gotoAndPlay(1);
            });
        }
        else
            startLabelTimer = startFrameTime;
    }

    void PlayBackAnimation(float startFrameTime = -1.0f)
    { 
        if (startFrameTime < 0)
        {
            mcTutorialAnimation.gotoAndStop(stopAnimLabel);
            mcTutorialAnimation.playBackwards();
            mcTutorialAnimation.addFrameScript(startAnimLabel, n =>
            {
                mcTutorialAnimation.gotoAndStop(startAnimLabel); 
                animationStepTimer = TimeManager.Instance.MasterSource.TotalTime;
                mcTutorialAnimation.getChildByName<MovieClip>("mcDiagonal").gotoAndPlay(1);
            });
        }
        else
            startLabelTimer = startFrameTime;
    }

    MovieClip mcBB;
    void InitBBPressurePoint()
    {
        mcBB = mcBalanceBoard.getChildByName<MovieClip>("mcBB");
        mcBBPressurePoint.x = mcBB.x;
        mcBBPressurePoint.y = mcBB.y;
    }

    void UpdateBBPressurePointPosition(float h, float v)
    {
        float horizontal = mcBB.x + h * 100;
        float vertical = mcBB.y + v * 60;
        mcBBPressurePoint.x = horizontal;
        mcBBPressurePoint.y = -vertical;
    }

    public void SetupInstructions()
    {
        mcPopupWellDone = new MovieClip(SWFPath + "mcPopupWellDoneClass");
        stage.addChild(mcPopupWellDone);
        mcPopupWellDone.x = Screen.width * 0.5f;
        mcPopupWellDone.y = Screen.height * 0.05f;
        mcPopupWellDone.visible = false;

        mcPopupStars = mcPopupWellDone.getChildByName<MovieClip>("mcStars");
    }

    public void DestroyInstructions()
    {
        mcPopupWellDone.visible = false;
        //mcInstructions.visible = false;
        stage.removeChild(mcPopupWellDone);
        stage.removeChild(mcInstructions);
        mcInstructions = null;
    }

    public void ViewInstructions(string text, string shoplabel, bool checkVisible, bool welldone, int stars = 0)
    {
        string message = string.Empty;
        GeneralSoundsManager.GeneralSounds sound = GeneralSoundsManager.GeneralSounds.target_bad;
        if (stars == 0)
        {
            sound = GeneralSoundsManager.GeneralSounds.target_normal;
        }
        else if (stars == 1)
        {
            text = "Target reached!";
            message = "Follow a shorter route to get more stars.";
            sound = GeneralSoundsManager.GeneralSounds.target_bad;
        }
        else if (stars == 2)
        {
            text = "Well done!";
            message = "Find the optimal route to get three stars.";
            sound = GeneralSoundsManager.GeneralSounds.target_normal;
        }
        else if (stars == 3)
        {
            text = "Amazing!";
            message = "You found the best route to get here!";
            sound = GeneralSoundsManager.GeneralSounds.target_good;
        }

        mcPopupWellDone.gotoAndStop(1);
        mcPopupWellDone.visible = true;
        mcInstructions = mcPopupWellDone.getChildByName<MovieClip>("mcGeneralPopup");
        mcInstructions.getChildByName<TextField>("tfLabel").text = text;
        mcInstructions.getChildByName<TextField>("tfLabel2").text = message;
        mcInstructions.getChildByName<MovieClip>("mcGems").visible = gameplayManager.Gameplay == GameplayManager.GameplayType.ObstacleAvoidance && this.State != TutorialPage;
        mcInstructions.getChildByName<MovieClip>("mcPopupCheck").visible = checkVisible;
        mcPopupStars = mcInstructions.getChildByName<MovieClip>("mcStars");
        ShowStars(mcInstructions, stars);
        
        mcInstructions.getChildByName<MovieClip>("mcPopupCheck").gotoAndStop(welldone ? 1 : 2);
        mcInstructions.getChildByName<MovieClip>("mcShopSigns").gotoAndStop(shoplabel);

        TextField tfPopupTimeLabel = mcInstructions.getChildByName<TextField>("tfTimeLabel");
        TextField tfPopupTime = mcInstructions.getChildByName<TextField>("tfTime");
        tfPopupTimeLabel.visible = gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation && this.State != TutorialPage && stars > 0;
        tfPopupTime.visible = gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation && this.State != TutorialPage && stars > 0;
        localizationUtils.AddTranslationText(tfPopupTimeLabel, "Time");
        tfPopupTime.text = StringUtils.MillisecondsToTime(gameplayManager.LapElapsedTime, false);

        Debug.Log("tfPopupTime.text = " + tfPopupTime.text + " gameplayManager.LapElapsedTime " + gameplayManager.LapElapsedTime);
        
        if (welldone)
            genSoundsManager.SendMessage("PlayGeneralSound", sound);
        else
            genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
    }

    public void ShowResultsPopup(string text, int stars = 0)
    {
        string message = string.Empty;
        GeneralSoundsManager.GeneralSounds sound = GeneralSoundsManager.GeneralSounds.target_bad;
       
        mcPopupWellDone.gotoAndStop(2);
        mcPopupWellDone.visible = true;
        MovieClip mcResultsPopup = mcPopupWellDone.getChildByName<MovieClip>("mcResultsPopup");

        ShowStars(mcResultsPopup, stars);
        bool isNavigationGameplay = gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation;
        localizationUtils.AddTranslationText(mcResultsPopup.getChildByName<TextField>("tfTitle"), text);
        localizationUtils.AddTranslationText(mcResultsPopup.getChildByName<TextField>("tfBonusLabel"), isNavigationGameplay ? "Time" : "Gems");
        localizationUtils.AddTranslationText(mcResultsPopup.getChildByName<TextField>("tfObstacleAvoidedLabel"), "Obstacles Avoided");
        localizationUtils.AddTranslationText(mcResultsPopup.getChildByName<TextField>("tfObstacleHitLabel"), "Obstacles Hit");
        string levelBonusText = isNavigationGameplay ? StringUtils.MillisecondsToTime(gameplayManager.TotalLevelElapsedTime, false) : gameplayManager.BonusCount.ToString(); //numTarget.ToString() + "/" + totalTarget.ToString()
        mcResultsPopup.getChildByName<TextField>("tfBonus").text = levelBonusText;
        mcResultsPopup.getChildByName<TextField>("tfObstacleAvoided").text = gameplayManager.ObstaclePassedCounter.ToString();
        mcResultsPopup.getChildByName<TextField>("tfObstacleHit").text = gameplayManager.ObstacleHitCounter.ToString();

        genSoundsManager.SendMessage("PlayGeneralSound", sound);
    }

    public void ShowStars(MovieClip parent, int numOfStars)
    {
        mcPopupStars = parent.getChildByName<MovieClip>("mcStars");
        mcPopupStars.visible = numOfStars > 0;
        mcPopupStars.getChildByName<MovieClip>("mcStar1").gotoAndStop(numOfStars > 0 ? 2 : 1);
        mcPopupStars.getChildByName<MovieClip>("mcStar2").gotoAndStop(numOfStars > 1 ? 2 : 1);
        mcPopupStars.getChildByName<MovieClip>("mcStar3").gotoAndStop(numOfStars > 2 ? 2 : 1);
    }

    public void HideInstructions()
    {
        if(mcPopupWellDone != null)
        mcPopupWellDone.visible = false;
    }

    public void ShowHideTutorial(bool _visible)
    {
        if (mcTutorialPage != null)
        mcTutorialPage.visible = _visible;
    }

    #endregion

    #endregion

    #region DetectPage
    protected MovieClip mcBBSetup = null;
    protected MovieClip mcBBfootstep = null;
    protected MovieClip mcArrowUp = null;
    protected MovieClip mcArrowDown = null;
    protected MovieClip mcCheckFeedback = null;
    protected MovieClip mcSilChosenON = null;
    protected MovieClip mcSilChosenOFF = null;
    protected MovieClip mcCheckingBar = null;
    protected TextField tfCheckingLabel = null;
    protected MovieClip btSkipButton = null;

    //protected bool playerIsAMan = true;
    protected bool checkingSuccess = false;
    protected float bbDetectTimer = -1.0f;
    protected float bbDetectStateInterval = 5.0f;

    protected DetectBBStates currentBBState = DetectBBStates.stepOn;

    public enum DetectBBStates
    { 
        stepOn,
        checkingOn,
        successOn,
        failedOn,
        stepDown,
        checkingDown,
        successDown,
        failedDown
    }

    void OnDetectEnter(FSMObject self, float time)
    {
        mcBBSetup = new MovieClip(SWFPath + "mcBBSetupClass");
        mcBBSetup.x = Screen.height* 0.5f;
        mcBBfootstep = mcBBSetup.getChildByName<MovieClip>("mcBBfootstep");
        mcArrowUp = mcBBSetup.getChildByName<MovieClip>("mcArrowUp");
        mcArrowDown = mcBBSetup.getChildByName<MovieClip>("mcArrowDown");
        mcCheckFeedback = mcBBSetup.getChildByName<MovieClip>("mcCheckFeedback");
        mcSilChosenON = mcBBSetup.getChildByName<MovieClip>("mcSilChosenON");
        mcSilChosenOFF = mcBBSetup.getChildByName<MovieClip>("mcSilChosenOFF");
        mcCheckingBar = mcBBSetup.getChildByName<MovieClip>("mcCheckingBar");
        tfCheckingLabel = mcBBSetup.getChildByName<TextField>("tfCheckingLabel");
        btSkipButton = mcBBSetup.getChildByName<MovieClip>("btSkip");
        btSkipButton.visible = false;
        GoToDetectBBState(DetectBBStates.stepOn);
        
        mcSPBackground = new MovieClip(SWFPath + "mcStartPageBackgroundClass");
        mcSPBackground.x = Screen.height* 0.5f;
       // mcSPBackground.y = Screen.height * 0.5f;
        mcSPBackground.visible = true;
        stage.addChild(mcSPBackground);
        
        stage.addChild(mcBBSetup);
    }

    void OnDetectExec(FSMObject self, float time) 
    {
        float now = TimeManager.Instance.MasterSource.TotalTime;

        if(currentBBState == DetectBBStates.stepOn)
        {
            if (bbDetectTimer < 0)
                bbDetectTimer = now;

            if (now - bbDetectTimer > 3.0f)
            {
                GoToDetectBBState(DetectBBStates.checkingOn);
                bbDetectTimer = now;
            }
            /*if (bbManager.Total > 10.0f)
            {
                genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                GoToDetectBBState(DetectBBStates.checkingOn);
                bbDetectTimer = now;
            }*/
        }

        if(currentBBState == DetectBBStates.checkingOn)
        {
            if (bbDetectTimer > 0)
            {
                float barPercent = (now - bbDetectTimer) / bbDetectStateInterval * 100;
                int barProgress = (int)barPercent; // Mathf.Clamp(barPercent, 0, 100);
                mcCheckingBar.gotoAndStop(barProgress);
            }
            if (bbDetectTimer > 0.0f && now - bbDetectTimer > bbDetectStateInterval)
            {
                if (bbManager.Total > 10.0f)
                {
                    checkingSuccess = true;
                    genSoundsManager.SendMessage("PlayJingle");
                    GoToDetectBBState(DetectBBStates.successOn);
                }
                else
                {
                    genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueKO);
                    checkingSuccess = false;
                    GoToDetectBBState(DetectBBStates.failedOn);
                }

                bbDetectTimer = now;
            }
        }

        if(currentBBState == DetectBBStates.successOn)
        {
            if (bbDetectTimer > 0.0f && now - bbDetectTimer > 5.0f)
            {                
                //genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                //GoToDetectBBState(DetectBBStates.stepDown);
                this.State = LoadingPage;
                return;
            }
        }

        if (currentBBState == DetectBBStates.failedOn)
        {
            if (bbDetectTimer > 0.0f && now - bbDetectTimer > 3.0f)
            {
                bbDetectTimer = now;
                GoToDetectBBState(DetectBBStates.stepDown);
            }
        }

        if(currentBBState == DetectBBStates.stepDown)
        {
            if (now - bbDetectTimer > 3.0f)
            {
                GoToDetectBBState(DetectBBStates.checkingDown);
                bbDetectTimer = now;
            }

            /*if (bbManager.Total <= 10.0f)
            {
                genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                GoToDetectBBState(DetectBBStates.checkingDown);
                bbDetectTimer = now;
            }*/
        }

        if(currentBBState == DetectBBStates.checkingDown)
        {
            float barPercent = (now - bbDetectTimer) / 5 * 100;
            int barProgress = (int)barPercent;// Mathf.Clamp(barPercent, 0, 100);
            mcCheckingBar.gotoAndStop(barProgress);
            if (bbDetectTimer > 0.0f && now - bbDetectTimer > bbDetectStateInterval)
            {
                if (bbManager.Total <= 10.0f)
                {
                    genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueOK);
                    checkingSuccess = true;
                    GoToDetectBBState(DetectBBStates.successDown);
                    bbManager.Reset();
                }
                else
                {
                    genSoundsManager.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.clueKO);
                    bbDetectTimer = now;
                    GoToDetectBBState(DetectBBStates.stepDown);
                    //checkingSuccess = false;
                    //GoToDetectBBState(DetectBBStates.failedDown);
                }

                bbDetectTimer = now;
            }
        }

        if(currentBBState == DetectBBStates.successDown)
        {
            if (bbDetectTimer > 0.0f && now - bbDetectTimer > 3.0f)
            {
                //genSoundsManager.SendMessage("PlayJingle");
                //this.State = TutorialPage;
                bbDetectTimer = now;
                GoToDetectBBState(DetectBBStates.stepOn);
            }
        }

        if(currentBBState == DetectBBStates.failedDown)
        {
            // NOT USED!!
            //if (bbDetectTimer > 0.0f && now - bbDetectTimer > 3.0f)
            //    GoToDetectBBState(DetectBBStates.stepDown);
        }
    }

    void OnDetectExit(FSMObject self, float time)
    {
        stage.removeChild(mcSPBackground);
        mcSPBackground = null;
        stage.removeChild(mcBBSetup);
        mcBBSetup = null;
    }

    #region DetectBBPage Functions

    void GoToDetectBBState(DetectBBStates state)
    {
        //TEMP CHEAT
        mcSilChosenOFF.gotoAndStop(gameplayManager.Gender == GameplayManager.CharacterGender.Male ? "man" : "woman");
        mcSilChosenON.gotoAndStop(gameplayManager.Gender == GameplayManager.CharacterGender.Male ? "man" : "woman");
        mcCheckFeedback.gotoAndStop(checkingSuccess ? "yes" : "no");

        switch (state)
        {
            case DetectBBStates.stepOn:
                mcBBfootstep.visible = true;
                mcArrowUp.visible = true;
                mcArrowDown.visible = false;
                mcCheckFeedback.visible = false;
                mcSilChosenON.visible = true;
                mcSilChosenOFF.visible = false;
                mcCheckingBar.visible = false;
                localizationUtils.AddTranslationText(tfCheckingLabel, "Position your feet on the balance board");
                currentBBState = DetectBBStates.stepOn;
                break;
            case DetectBBStates.checkingOn:
                mcBBfootstep.visible = true;
                mcArrowUp.visible = false;
                mcArrowDown.visible = false;
                mcCheckFeedback.visible = false;
                mcSilChosenON.visible = true;
                mcSilChosenOFF.visible = false;
                mcCheckingBar.visible = true;
                mcCheckingBar.gotoAndStop(1);
                localizationUtils.AddTranslationText(tfCheckingLabel, "Checking...");
                currentBBState = DetectBBStates.checkingOn;
                break;
            case DetectBBStates.successOn:
                mcBBfootstep.visible = true;
                mcArrowUp.visible = false;
                mcArrowDown.visible = false;
                mcCheckFeedback.visible = true;
                mcSilChosenON.visible = true;
                mcSilChosenOFF.visible = false;
                mcCheckingBar.visible = false;
                localizationUtils.AddTranslationText(tfCheckingLabel, "Success!");
                currentBBState = DetectBBStates.successOn;
                break;
            case DetectBBStates.failedOn:
                mcBBfootstep.visible = true;
                mcArrowUp.visible = false;
                mcArrowDown.visible = false;
                mcCheckFeedback.visible = true;
                mcSilChosenON.visible = true;
                mcSilChosenOFF.visible = false;
                mcCheckingBar.visible = false;
                localizationUtils.AddTranslationText(tfCheckingLabel, "Failed, wait...");
                currentBBState = DetectBBStates.failedOn;
                break;
            case DetectBBStates.stepDown:
                mcBBfootstep.visible = false;
                mcArrowUp.visible = false;
                mcArrowDown.visible = true;
                mcCheckFeedback.visible = false;
                mcSilChosenON.visible = false;
                mcSilChosenOFF.visible = true;
                mcCheckingBar.visible = false;
                localizationUtils.AddTranslationText(tfCheckingLabel, "Step down from the balance board");
                currentBBState = DetectBBStates.stepDown;
                break;
            case DetectBBStates.checkingDown:
                mcBBfootstep.visible = false;
                mcArrowUp.visible = false;
                mcArrowDown.visible = false;
                mcCheckFeedback.visible = false;
                mcSilChosenON.visible = false;
                mcSilChosenOFF.visible = true;
                mcCheckingBar.visible = true;
                mcCheckingBar.gotoAndStop(1);
                localizationUtils.AddTranslationText(tfCheckingLabel, "Checking...");
                currentBBState = DetectBBStates.checkingDown;
                break;
            case DetectBBStates.successDown:
                mcBBfootstep.visible = false;
                mcArrowUp.visible = false;
                mcArrowDown.visible = false;
                mcCheckFeedback.visible = true;
                mcSilChosenON.visible = false;
                mcSilChosenOFF.visible = true;
                mcCheckingBar.visible = false;
                localizationUtils.AddTranslationText(tfCheckingLabel, "Success!");
                currentBBState = DetectBBStates.successDown;
                break;
            case DetectBBStates.failedDown:
                mcBBfootstep.visible = false;
                mcArrowUp.visible = false;
                mcArrowDown.visible = false;
                mcCheckFeedback.visible = true;
                mcSilChosenON.visible = false;
                mcSilChosenOFF.visible = true;
                mcCheckingBar.visible = false;
                localizationUtils.AddTranslationText(tfCheckingLabel, "Failed, wait...");
                currentBBState = DetectBBStates.failedDown;
                break;
        }
    }

    #endregion

    void ShowHideDetectBB(bool _visible)
    {
        mcBBSetup.visible = _visible;
    }
    #endregion

    #region IngamePages
    protected MovieClip mcIngamePage = null;
    protected MovieClip mcClose = null;
    protected MovieClip mcPause = null;
    protected MovieClip mcSigns = null;
    protected TextField tfScore = null;
    protected TextField tfTarget = null;
    protected TextField tfScoreLabel = null;
    protected TextField tfTargetLabel = null;
    protected TextField tfCurrentLabel = null;
    protected TextField tfLevelLabel = null;
    protected TextField tfGameTime = null;

    protected MovieClip mcPopupPause = null;
    protected MovieClip btNo = null;
    protected MovieClip btYes = null;
    protected MovieClip btResume = null;
    protected MovieClip mcPopupWellDone = null;
    protected MovieClip mcPopupCheck = null;
    protected MovieClip mcLeftTurnArrow = null;
    protected MovieClip mcRightTurnArrow = null;
    protected MovieClip mcGoStraightArrow = null;
    protected MovieClip mcSvolazScore = null;
    protected TextField tfWalkingSpeed = null;
    protected bool isMapShown = false;

    protected int totalTarget = 0;
    protected int numTarget = 0;

    protected LinkedList<string> popups = new LinkedList<string>();

    #region Ingame Messages
    void UpdateWalkingSpeed(int speed)
    {
        if (tfWalkingSpeed != null)
        {
            tfWalkingSpeed.visible = speed > 0;
            localizationUtils.AddTranslationText(tfWalkingSpeed, "Walking Speed: ");
            tfWalkingSpeed.text = tfWalkingSpeed.text + speed.ToString();
        }
    }

    void UpdateScore(int s)
    {
        if (tfScore != null)
        {
            tfScore.text = s.ToString();
            tfScore.visible = !(gameplayManager.learningPhase && !gameplayManager.learningPhaseDone);
        }
    }

    void UpdateTotalTarget(int totTarget)
    {
        totalTarget = totTarget;
        UpdateTargetTexts();
    }

    void UpdateTargets(int t)
    {
        numTarget += t;
        UpdateTargetTexts();
    }

    void UpdateTargetTexts()
    {
        if (tfTarget != null)
            tfTarget.text = numTarget.ToString() + "/" + totalTarget.ToString();

        tfTarget.visible = !(gameplayManager.learningPhase && !gameplayManager.learningPhaseDone);
    }

    void UpdateTargetSigns(string shoplabel)
    {
        mcSigns.gotoAndStop(shoplabel);
    }

    void SetTargetSignsVisibility(bool v)
    {
        mcSigns.visible = v;
    }

    void InitFlyier(string fText)
    {
        if (null != mcSvolazScore)
        {
            mcSvolazScore.visible = true;
            mcSvolazScore.gotoAndPlay(1);
            Color color = new Color(1.0f, 0.8f, 0.0f);
            if (int.Parse(fText) < 0)
                color = new Color(0.8f, 0.0f, 0.0f);
            mcSvolazScore.getChildByName<MovieClip>("mcText").getChildByName<TextField>("tfText").colorTransform = color;
            mcSvolazScore.getChildByName<MovieClip>("mcText").getChildByName<TextField>("tfText").text = fText;
            mcSvolazScore.addFrameScript("end", StopFlyier);
        }
    }

    void StopFlyier(CEvent evt)
    {
        mcSvolazScore.visible = false;
        mcSvolazScore.gotoAndStop(1);
        mcSvolazScore.addFrameScript("end", null);
    }
    #endregion

    void OnNavIngameEnter(FSMObject self, float time)
    {
        SetupInstructions();
        InitCommonIngameElements();

        numTarget = 0;
        UpdateTargetTexts();

        InitIngamePopups();
        InitDirectionsArrows();
        
        AddMapToStage();

        UpdateScore(gameplayManager.Score);
        genSoundsManager.SendMessage("PlayAmbientSound");

        if (gameplayManager.learningPhase && !gameplayManager.learningPhaseDone)
        {
            tfTargetLabel.visible = false;
            tfTarget.visible = false;
            tfScoreLabel.visible = false;
            tfScore.visible = false;
            tfLevelLabel.text = "Learning Phase";
        }
        else
        {
            tfTargetLabel.visible = true;
            tfScoreLabel.visible = true;
            tfTarget.visible = true;
            tfScore.visible = true;
        }

        InitializeFade();
        StartFadeOut(0.0f, null);
        UpdateFade();
        StartFadeIn(0.5f, null);

        quittingGame = false;

        SetGameTimeVisibility(false);

        UpdateTotalTarget(gameplayManager.GetTargetByLevel(gameplayManager.Level));
    }

    void OnNavIngameExec(FSMObject self, float time)
    {
        UpdateFade();
        UpdateMap();
    }

    void OnNavIngameExit(FSMObject self, float time)
    {
        SetTargetSignsVisibility(false);
        ResetFade();
        RemoveMapFromStage();
        DestroyInstructions();

        genSoundsManager.SendMessage("StopAmbientSound");

        stage.removeChild(mcLeftTurnArrow);
        mcLeftTurnArrow = null;
        stage.removeChild(mcRightTurnArrow);
        mcRightTurnArrow = null;
        stage.removeChild(mcGoStraightArrow);
        mcGoStraightArrow = null;
        stage.removeChild(mcIngamePage);
        mcIngamePage = null;
        stage.removeChild(mcPopupPause);
        mcIngamePage = null;
        stage.removeChild(mcPopupWellDone);
        mcPopupWellDone = null;
    }

    void OnObsIngameEnter(FSMObject self, float time)
    {
        SetupInstructions();
        InitCommonIngameElements();

        gameplayManager.BonusCount = 0;
        //UpdateTargetTexts();

        InitIngamePopups();
        InitDirectionsArrows();

        AddMapToStage();

        UpdateBonusInterface();
        genSoundsManager.SendMessage("PlayAmbientSound");
        tfCurrentLabel.visible = false;
        //SetGameTimeVisibility(false);
        UpdateTime();

        InitializeFade();
        StartFadeOut(0.0f, null);
        UpdateFade();
        StartFadeIn(0.5f, null);

        quittingGame = false;

        if (preloader.useOVR)
        {
            gameplayManager.characterOVR.BroadcastMessage("OnDestroyBackground", SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnObsIngameExec(FSMObject self, float time)
    {
        UpdateFade();
        UpdateMap();
        UpdateTime();
    }

    void OnObsIngameExit(FSMObject self, float time)
    {
        SetTargetSignsVisibility(false);
        ResetFade();
        RemoveMapFromStage();
        DestroyInstructions();

        genSoundsManager.SendMessage("StopAmbientSound");

        stage.removeChild(mcLeftTurnArrow);
        mcLeftTurnArrow = null;
        stage.removeChild(mcRightTurnArrow);
        mcRightTurnArrow = null;
        stage.removeChild(mcGoStraightArrow);
        mcGoStraightArrow = null;
        stage.removeChild(mcIngamePage);
        mcIngamePage = null;
        stage.removeChild(mcPopupPause);
        mcIngamePage = null;
        stage.removeChild(mcPopupWellDone);
        mcPopupWellDone = null;
    }

    #region IngamePage Functions
    public void UpdateBonusInterface()
    {
        if (gameplayManager.Gameplay == GameplayManager.GameplayType.ObstacleAvoidance)
        {
            tfTarget.text = gameplayManager.BonusCount.ToString();
            tfScore.text = gameplayManager.Score.ToString();
        }
    }

    void InitDirectionsArrows()
    {
       // Debug.Log("InitDirectionsArrows");
        mcLeftTurnArrow = new MovieClip(SWFPath + "mcArrowCurvedLeftClass");
        // mcLeftTurnArrow.x = 435.0f;
        mcLeftTurnArrow.x = (Screen.width * 0.5f) - 150;
        mcLeftTurnArrow.y = 168.0f;
        mcRightTurnArrow = new MovieClip(SWFPath + "mcArrowCurvedRightClass");
       // mcRightTurnArrow.x = 844.0f;
        mcRightTurnArrow.x = (Screen.width * 0.5f) + 150;
        mcRightTurnArrow.y = 168.0f;
        mcGoStraightArrow = new MovieClip(SWFPath + "mcArrowShortClass");
        mcGoStraightArrow.x = (Screen.width * 0.5f);
       // mcGoStraightArrow.x = 640.0f;
       // mcGoStraightArrow.y = 110.0f;
        mcGoStraightArrow.y = 110.0f;
        HideDirectionArrows();

        stage.addChild(mcLeftTurnArrow);
        stage.addChild(mcRightTurnArrow);
        stage.addChild(mcGoStraightArrow); 

    }
    void InitIngamePopups()
    {
        mcPopupPause = new MovieClip(SWFPath + "mcPopupPauseClass");
        mcPopupPause.x = Screen.width * 0.5f;
        mcPopupPause.y = Screen.height * 0.5f;

        //mcPopupWellDone = new MovieClip(SWFPath + "mcPopupWellDoneClass");
        //mcPopupCheck = mcPopupWellDone.getChildByName<MovieClip>("mcPopupCheck");
        //mcPopupCheck.gotoAndStop("yes");
        //mcPopupCheck.visible = false;

        mcPopupPause.visible = false;
        //mcPopupWellDone.visible = false;
        //stage.addChild(mcPopupCheck);
        stage.addChild(mcPopupPause);
    }

    void InitCommonIngameElements()
    {
        mcIngamePage = new MovieClip(SWFPath + "mcIngameClass");
        //mcIngamePage.x = Screen.width * 0.5f;
        //mcIngamePage.x = Screen.height * 0.5f;
        mcClose = mcIngamePage.getChildByName<MovieClip>("mcClose");
        mcPause = mcIngamePage.getChildByName<MovieClip>("mcPause");
        mcSigns = mcIngamePage.getChildByName<MovieClip>("mcSigns");
        tfGameTime = mcIngamePage.getChildByName<TextField>("tfGameTime");
        tfGameTime.visible = false;
        tfScore = mcIngamePage.getChildByName<TextField>("tfScore");
        tfTarget = mcIngamePage.getChildByName<TextField>("tfTarget");
        tfCurrentLabel = mcIngamePage.getChildByName<TextField>("tfCurrentLabel");
        tfTargetLabel = mcIngamePage.getChildByName<TextField>("tfTargetLabel");
        tfScoreLabel = mcIngamePage.getChildByName<TextField>("tfScoreLabel");
        tfLevelLabel = mcIngamePage.getChildByName<TextField>("tfLevelLabel");
        localizationUtils.AddTranslationText(tfCurrentLabel, "Current target:");
        localizationUtils.AddTranslationText(tfTargetLabel, gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation ? "Target:" : "Gems:");
        localizationUtils.AddTranslationText(tfScoreLabel, "Score:");
        localizationUtils.AddTranslationText(tfLevelLabel, "Level");

        if (preloader.useOVR)
        {
            mcClose.visible = false;
            mcPause.visible = false;

            //float xOffset = 300;
            float xOffset = 0;
           // float yOffset = -250f;
            float yOffset = 0;

            tfScoreLabel.x += xOffset;
            tfScoreLabel.y += yOffset;
            tfScore.x += xOffset;
            tfScore.y += yOffset;
            tfTargetLabel.x += xOffset;
            tfTargetLabel.y += yOffset;
            tfTarget.x += xOffset;
            tfTarget.y += yOffset;

            //tfLevelLabel.x += 430f;
        }
        else
        {
            SetupButton(mcClose, OnIngameBtCloseRelease);
            SetupButton(mcPause, OnIngameBtPauseRelease);
        }

        tfLevelLabel.text = tfLevelLabel.text + " " + gameplayManager.Level;
        //UpdateTime();

        tfWalkingSpeed = mcIngamePage.getChildByName<TextField>("tfWalkingSpeedLabel");
        localizationUtils.AddTranslationText(tfWalkingSpeed, "");

        mcSigns.visible = false;
        mcSigns.gotoAndStop(gameplayManager.CurrentTarget);
        
        mcSvolazScore = mcIngamePage.getChildByName<MovieClip>("mcSvolazScore");
        if (preloader.useOVR)
        {
            mcSvolazScore.y -= 200.0f;
        }
        mcSvolazScore.visible = false;
        mcSvolazScore.gotoAndStop(1);

        stage.addChild(mcIngamePage);
    }

    public void HideDirectionArrows()
    {
        if (this.State == NavigationIngamePage || this.State == ObstaclesIngamePage)
        {
            mcLeftTurnArrow.visible = false;
            mcRightTurnArrow.visible = false;
            mcGoStraightArrow.visible = false;
        }
    }

    public void ShowDirectionArrows(bool forward, bool right, bool left)
    {
        Debug.Log("ShowDirectionArrows forward "+ forward + " right  " + right + " left " + left);
        if (this.State == NavigationIngamePage || this.State == ObstaclesIngamePage)
        {
           // Debug.Log("state " + this.State);
            HideDirectionArrows();
            mcLeftTurnArrow.visible = left;
            mcRightTurnArrow.visible = right;
            mcGoStraightArrow.visible = forward;
        }
    }
    #endregion

    #region IngamePage Map Functions
    Texture2D cityMapTexture = null;
    Vector2 cityMin = Vector2.zero;
    Vector2 cityMax = Vector2.zero;
    float cityWidth = 0.0f;
    float cityHeight = 0.0f;
    float mapMaxSize = 200.0f;
    float mapPosX = 8.0f;
    float mapPosY = 60.0f;
    float mapSizeX = 0.0f;
    float mapSizeY = 0.0f;
    protected MovieClip mcMap = null;
    protected MovieClip playerCheck = null;
    protected MovieClip targetCheck = null;

    public void ShowTargetOnMap(Vector3 worldPos, bool force = false)
    {
        if (!gameplayManager.GetShowTarget() && !force)
        {
            return;
        }

        MapElement targetElement = WorldToMap(worldPos);

        targetCheck.x = targetElement.position.x;
        targetCheck.y = targetElement.position.y;

        targetCheck.visible = true;
    }

    public void HideTargetOnMap()
    {
        targetCheck.visible = false;
    }

    public void SetupMap(Texture2D mapTexture, Vector2 min, Vector2 max)
    {
        cityMin = min;
        cityMax = max;
        cityMapTexture = mapTexture;
    }

    public void AddMapToStage() //Texture2D mapTexture, Vector2 min, Vector2 max)
    {
        //cityMin = min;
        //cityMax = max;
        Texture2D mapTexture = cityMapTexture;

        cityWidth = cityMax.x - cityMin.x;
        cityHeight = cityMax.y - cityMin.y;

        mapSizeX = mapMaxSize;
        mapSizeY = mapMaxSize;
        mcMap = new MovieClip();

        if (mapTexture.width > mapTexture.height)
            mapSizeY = ((float)mapTexture.height / (float)mapTexture.width) * mapMaxSize;
        else if (mapTexture.width < mapTexture.height)
            mapSizeX = ((float)mapTexture.width / (float)mapTexture.height) * mapMaxSize;

        Rect textureRect = new Rect(0.0f, 0.0f, mapSizeX, mapSizeY);

        mcMap.graphics.drawRectUV(mapTexture, new Rect(0.0f, 0.0f, 1.0f, 1.0f), textureRect);
        mcMap.x = mapPosX;
        mcMap.y = mapPosY;

        playerCheck = new MovieClip(SWFPath + "mcCheckPlayerClass");
        playerCheck.x = mapPosX;
        playerCheck.y = mapPosY;

        targetCheck = new MovieClip(SWFPath + "mcCheckTargetClass");
        targetCheck.visible = false;

        stage.addChild(mcMap);
        stage.addChild(targetCheck);
        stage.addChild(playerCheck);

        isMapShown = gameplayManager.GetShowMap();
        if (preloader.useOVR)
            isMapShown = false;
        mcMap.visible = isMapShown;
        playerCheck.visible = isMapShown;
    }

    protected MapElement WorldToMap(Transform tr)
    {
        MapElement check = WorldToMap(tr.position);

        float sign = (Vector3.Cross(Vector3.forward, tr.forward).y > 0) ? 1.0f : -1.0f;
        float angle = Vector3.Angle(Vector3.forward, tr.forward);

        if (angle < 10)
            angle = 0.0f;
        else if (angle > 85 && angle < 95)
            angle = 90.0f;
        else if (angle > 170)
            angle = 180.0f;

        check.angleRotation = sign * angle;
        return check;
    }

    protected MapElement WorldToMap(Vector3 pos)
    {
        MapElement check = new MapElement();

        Vector3 playerPos = pos;
        playerPos.x -= cityMin.x;
        playerPos.z -= cityMin.y;

        float posX = (cityMin.x / cityWidth) + mapSizeX * (playerPos.x / cityWidth);
        float posY = mapSizeY - ((cityMin.y / cityHeight) + mapSizeY * (playerPos.z / cityHeight));

        check.position.x = mapPosX + posX;
        check.position.y = mapPosY + posY;

        check.angleRotation = 0.0f;
        return check;
    }

    protected void UpdateMap()
    {
        if (isMapShown)
        {
            MapElement check = WorldToMap(gameplayManager.Character.transform);

            playerCheck.x = check.position.x;
            playerCheck.y = check.position.y;
            playerCheck.rotation = check.angleRotation;

			mcMap.visible = Input.GetAxis ("showmapbutton") > 0.1f;
        }
    }

    public void RemoveMapFromStage()
    {
        isMapShown = false;
        if (null != mcMap)
        {
            stage.removeChild(mcMap);
            mcMap = null;
        }

        stage.removeChild(playerCheck);
        playerCheck = null;
        stage.removeChild(targetCheck);
        targetCheck = null;
    }
    #endregion

    #region IngamePage Buttons Callbacks
    void OnIngameBtCloseRelease(CEvent evt)
    {
        this.ShowPopup("quit");
    }

    void OnIngameBtPauseRelease(CEvent evt)
    {
        this.ShowPopup("paused");
    }

    void OnIngameBtResumeRelease(CEvent evt)
    {
        this.RemovePopup();
    }

    void OnIngameBtQuitRelease(CEvent evt)
    {
        this.RemovePopup();
        quittingGame = true;
        preloader.UnloadLevel();
        if(this.State == TutorialPage)
            this.State = StartPage;
    }

    #endregion

    #region Ingame Functions
    public void SetGameTimeVisibility(bool v)
    {
        tfGameTime.visible = v;
        Debug.Log("SetGameTimeVisibility " + v);
    }
    protected void UpdateTime()
    {
        tfGameTime.text = MillisecondsToTime(gameplayManager.GameplayTime, false);
    }

    protected void ShowPopup(string name, bool firstTime)
    {
        bool doAnimation = 0 == popups.Count;
        if (firstTime)
        {
            popups.AddLast(name);

            //if (!this.IsAPopupTutorial(name))
            TimeManager.Instance.MasterSource.Pause();
        }

        if (this.IsAPopupTutorial(name))
        {
            if ("tutorial" == name)
            {
                mcPopupPause.visible = false;
                //if (this.State == NavigationIngamePage || this.State == ObstaclesIngamePage)
                //    mcPopupWellDone.visible = true;
            }
            else
            {
                mcPopupPause.visible = false;
                //if (this.State == NavigationIngamePage || this.State == ObstaclesIngamePage)
                //    mcPopupWellDone.visible = false;  // true
            }
        }
        else
        {
            mcPopupPause.visible = true;
            //if (this.State == NavigationIngamePage || this.State == ObstaclesIngamePage)
            //    mcPopupWellDone.visible = false;
        }

        Debug.Log("ShowPopup " + name);

        switch (name)
        {
            case "paused":
                mcPopupPause.visible = true;
                mcPopupPause.gotoAndStop(2);
                btResume = mcPopupPause.getChildByName<MovieClip>("btResume");
                localizationUtils.AddTranslationButton(btResume, "RESUME", OnIngameBtResumeRelease, LocalizedButton.Alignment.center);
                localizationUtils.AddTranslationText(mcPopupPause.getChildByName<TextField>("tfLabel"), "PAUSE");
                break;
            case "quit":
                mcPopupPause.visible = true;
                mcPopupPause.gotoAndStop(1);
                btNo = mcPopupPause.getChildByName<MovieClip>("btNo");
                btYes = mcPopupPause.getChildByName<MovieClip>("btYes");
                localizationUtils.AddTranslationButton(btNo, "NO", OnIngameBtResumeRelease, LocalizedButton.Alignment.left);
                localizationUtils.AddTranslationButton(btYes, "YES", OnIngameBtQuitRelease, LocalizedButton.Alignment.right);
                localizationUtils.AddTranslationText(mcPopupPause.getChildByName<TextField>("tfLabel"), "QUIT THE GAME?");
                break;
            case "tutorial":
                break;
        }
    }

    public void ShowPopup(string name)
    {
        LinkedListNode<string> node = popups.Find(name);
        if (node != null)
        {
            popups.Remove(node);
            popups.AddLast(node);

            this.ShowPopup(name, false);
        }
        else
        {
            this.ShowPopup(name, true);
        }
    }

    public string RemovePopup()
    {
        if (0 == popups.Count)
            return null;

        string popupName = popups.Last.Value;
        popups.RemoveLast();

        mcPopupPause.visible = false;

        TimeManager.Instance.MasterSource.Resume();

        if (popups.Count > 0)
            this.ShowPopup(popups.Last.Value, false);

        return popupName;
    }

    protected bool IsAPopupTutorial(string name)
    {
        return name.IndexOf("tutorial") > -1;
    }

    protected bool IsAQuitPopup(string name)
    {
        return name.IndexOf("quit") > -1;
    }

    #endregion

    #endregion

    #region NavRewardPage
    protected MovieClip mcNavRewardPage = null;
    protected MovieClip btHome = null;
    protected TextField tfRewardScoreLabel = null;
    protected TextField tfRewardTimeLabel = null;
    protected TextField tfObstaclesAvoidedLabel = null;
    protected TextField tfObstaclesHitLabel = null;
    protected TextField tfRewardScore = null;
    protected TextField tfRewardTime = null;
    protected TextField tfObstaclesAvoided = null;
    protected TextField tfObstaclesHit = null;

    void OnNavRewardEnter(FSMObject self, float time)
    {
        mcNavRewardPage = new MovieClip(SWFPath + "mcRewardClass");
        stage.addChild(mcNavRewardPage);

        btHome = mcNavRewardPage.getChildByName<MovieClip>("btHome");
        tfRewardScoreLabel = mcNavRewardPage.getChildByName<TextField>("tfScoreLabel");
        tfRewardTimeLabel = mcNavRewardPage.getChildByName<TextField>("tfTimeLabel");
        tfObstaclesAvoidedLabel = mcNavRewardPage.getChildByName<TextField>("tfObstaclesAvoidedLabel");
        tfObstaclesHitLabel = mcNavRewardPage.getChildByName<TextField>("tfObstaclesHitLabel");
        tfRewardScore = mcNavRewardPage.getChildByName<TextField>("tfScore");
        tfRewardTime = mcNavRewardPage.getChildByName<TextField>("tfTime");
        tfObstaclesAvoided = mcNavRewardPage.getChildByName<TextField>("tfObstaclesAvoided");
        tfObstaclesHit = mcNavRewardPage.getChildByName<TextField>("tfObstaclesHit");

        localizationUtils.AddTranslationText(tfRewardScoreLabel, "FINAL SCORE:");
        localizationUtils.AddTranslationText(tfRewardTimeLabel, "TIME:");
        localizationUtils.AddTranslationText(tfObstaclesAvoidedLabel, "OBSTACLES AVOIDED:");
        localizationUtils.AddTranslationText(tfObstaclesHitLabel, "OBSTACLES HIT:");

        tfRewardScore.text = gameplayManager.Score.ToString();
        tfRewardTime.text = StringUtils.MillisecondsToTime(gameplayManager.TotalElapsedTime, false);
        tfObstaclesAvoided.text = gameplayManager.TotalObstaclePassedCounter.ToString();
        tfObstaclesHit.text = gameplayManager.TotalObstacleHitCounter.ToString();

        localizationUtils.AddTranslationButton(btHome, "CONTINUE", OnRewardBtContinueRelease, LocalizedButton.Alignment.center);

        genSoundsManager.SendMessage("PlayJingle");
    }

    void OnNavRewardExec(FSMObject self, float time)
    { }

    void OnNavRewardExit(FSMObject self, float time)
    {
        gameplayManager.Score = 0;
        gameplayManager.TotalElapsedTime = 0;
        stage.removeChild(mcNavRewardPage);
        mcNavRewardPage = null;

        //TrackingManager.Instance.StopTracking();
    }

    #region Reward Buttons Callbacks
    void OnRewardBtContinueRelease(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("up");
        this.State = StartPage;
    }
    #endregion
    #endregion

    #region ObsRewardPage
    protected MovieClip mcObsRewardPage = null;
    protected TextField tfRewardBonusLabel = null;
    protected TextField tfRewardBonus = null;

    void OnObsRewardEnter(FSMObject self, float time)
    {
        mcObsRewardPage = new MovieClip(SWFPath + "mcRewardObstaclesClass");
        stage.addChild(mcObsRewardPage);

        btHome = mcObsRewardPage.getChildByName<MovieClip>("btHome");
        tfRewardScoreLabel = mcObsRewardPage.getChildByName<TextField>("tfScoreLabel");
        tfRewardBonusLabel = mcObsRewardPage.getChildByName<TextField>("tfBonusLabel");
        tfObstaclesAvoidedLabel = mcObsRewardPage.getChildByName<TextField>("tfObstaclesAvoidedLabel");
        tfObstaclesHitLabel = mcObsRewardPage.getChildByName<TextField>("tfObstaclesHitLabel");
        tfRewardScore = mcObsRewardPage.getChildByName<TextField>("tfScore");
        tfRewardBonus = mcObsRewardPage.getChildByName<TextField>("tfBonus");
        tfObstaclesAvoided = mcObsRewardPage.getChildByName<TextField>("tfObstaclesAvoided");
        tfObstaclesHit = mcObsRewardPage.getChildByName<TextField>("tfObstaclesHit");

        localizationUtils.AddTranslationText(tfRewardScoreLabel, "YOUR SCORE:");
        localizationUtils.AddTranslationText(tfRewardBonusLabel, "GEMS TAKEN:");
        localizationUtils.AddTranslationText(tfObstaclesAvoidedLabel, "OBSTACLES AVOIDED:");
        localizationUtils.AddTranslationText(tfObstaclesHitLabel, "OBSTACLES HIT:");

        tfRewardScore.text = gameplayManager.Score.ToString();
        tfRewardBonus.text = gameplayManager.TotalBonusTaken.ToString();
        tfObstaclesAvoided.text = gameplayManager.TotalObstaclePassedCounter.ToString();
        tfObstaclesHit.text = gameplayManager.TotalObstacleHitCounter.ToString();

        localizationUtils.AddTranslationButton(btHome, "CONTINUE", OnRewardBtContinueRelease, LocalizedButton.Alignment.center);

        genSoundsManager.SendMessage("PlayJingle");
        genSoundsManager.SendMessage("PlayJingle");
    }

    void OnObsRewardExec(FSMObject self, float time)
    { }

    void OnObsRewardExit(FSMObject self, float time)
    {
        gameplayManager.Score = 0;
        gameplayManager.TotalBonusTaken = 0;
        gameplayManager.BonusCount = 0;
        gameplayManager.TotalObstacleHitCounter = 0;
        gameplayManager.TotalObstaclePassedCounter = 0;
        stage.removeChild(mcObsRewardPage);
        mcNavRewardPage = null;
    }

    #endregion

    #region LoadSessionPage
    protected MovieClip mcNavigationSessionRow = null;
    protected MovieClip mcObstaclesSessionRow = null;
    protected MovieClip btPlaySession = null;
    //protected MovieClip btTutorial = null;
    protected MovieClip btNavigationMode = null;
    protected MovieClip btObstaclesMode = null;
    protected MovieClip btTutorial = null;
    protected bool tutorialFromLoadSessionPage = false;

    void OnLoadSessionPageEnter(FSMObject self, float time)
    {
        mcSessionConfig = new MovieClip(SWFPath + "mcAdminClass");
        mcSessionConfig.gotoAndStop("loadSession");

        mcNavigationSessionRow = mcSessionConfig.getChildByName<MovieClip>("mcNavigationSessionRow");
        mcObstaclesSessionRow = mcSessionConfig.getChildByName<MovieClip>("mcObstaclesSessionRow");

        //mcSessionRow = mcSessionConfig.getChildByName<MovieClip>("mcSessionRow");
        btBackSession = mcSessionConfig.getChildByName<MovieClip>("btBack");
        btNavigationMode = mcSessionConfig.getChildByName<MovieClip>("btNavigationMode");
        btObstaclesMode = mcSessionConfig.getChildByName<MovieClip>("btObstaclesMode");
        btPlaySession = mcSessionConfig.getChildByName<MovieClip>("btPlaySession");
        //btTutorial = mcSessionConfig.getChildByName<MovieClip>("btTutorial");

        mcSessionConfig.getChildByName<MovieClip>("mcSessionsPopup").visible = false;
        Debug.Log("mcSessionConfig: " + mcSessionConfig);

        UpdateSessionRow(mcNavigationSessionRow, true);
        UpdateSessionRow(mcObstaclesSessionRow, true);
        SetTfSessionResult(false);

        localizationUtils.AddTranslationText(tfSessionName, "");
        localizationUtils.AddTranslationButton(btBackSession, "BACK", OnBtBackToLoginPageRelease, LocalizedButton.Alignment.center);
        localizationUtils.AddTranslationButton(btPlaySession, "Continue", OnBtPlaySessionRelease, LocalizedButton.Alignment.center);
        localizationUtils.AddTranslationButton(btNavigationMode, "NAVIGATION", OnBtNavigationLoadRelease, LocalizedButton.Alignment.center, false);
        localizationUtils.AddTranslationButton(btObstaclesMode, "OBSTACLES AVOIDANCE", OnBtObstacleLoadRelease, LocalizedButton.Alignment.center, false);
        localizationUtils.GetLocalizedButton(btPlaySession).DisableLocalizedButton();
        
        stage.addChild(mcSessionConfig);
    }

    void OnLoadSessionPageExec(FSMObject self, float time)
    { }

    void OnLoadSessionPageExit(FSMObject self, float time)
    {
        stage.removeChild(mcSessionConfig);
        mcSessionConfig = null;
    }

    #region LoadSessionPage Buttons Callbacks
   
    void OnBtPlaySessionRelease(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("up");

        //TODO PLAY
        this.State = StartPage;
        m_fileBrowser = null;
    }

    void OnBtBackToLoginPageRelease(CEvent evt)
    {
        (evt.currentTarget as MovieClip).gotoAndStop("dn");
        this.State = LoginPage;
        m_fileBrowser = null;
    }

    void OnBtNavigationLoadRelease(CEvent evt)
    {
        preloader.UnloadLevel();
        MovieClip mc = evt.currentTarget as MovieClip;
        Debug.Log("CLICK ON BUTTON " + mc.name);
        gameplayManager.Gameplay = mc.name.Equals("btNavigationMode") ? GameplayManager.GameplayType.Navigation : GameplayManager.GameplayType.ObstacleAvoidance;

        if (m_fileBrowser == null)
        {
            localizationUtils.GetLocalizedButton(btNavigationMode).DisableLocalizedButton();
            localizationUtils.GetLocalizedButton(btObstaclesMode).DisableLocalizedButton();

            m_fileBrowser = new FileBrowser(
                    new Rect((Screen.width * 0.5f) - 300, (Screen.height * 0.5f) - 200, 600, 400),
                    "Choose Navigation Session File",
                    LoadCallback,
                    "./Sessions"
                );
            m_fileBrowser.SelectionPattern = "*.ses";
            m_fileBrowser.DirectoryImage = m_directoryImage;
            m_fileBrowser.FileImage = m_fileImage;
        }
    }

    protected void LoadCallback(string path)
    {
        if (null != path)
        {
            string result = System.IO.Path.GetFileName(path);
            UpdateSessionRow(gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation ? mcNavigationSessionRow : mcObstaclesSessionRow, false, result); //mcSessionRow
            UpdateSessionRow(gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation ? mcObstaclesSessionRow : mcNavigationSessionRow, true); //mcSessionRow

            SetTfSessionResult(true, "Session Ready", true);
            localizationUtils.GetLocalizedButton(btPlaySession).SetButtonEvents();
            DataContainer.filename = path;
            TrackingManager.Instance.SessionName = result;
        }

        localizationUtils.GetLocalizedButton(btNavigationMode).SetButtonEvents(); //btLoadSession
        localizationUtils.GetLocalizedButton(btObstaclesMode).SetButtonEvents(); //btLoadSession
        m_fileBrowser = null;
    }

    void OnBtObstacleLoadRelease(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        Debug.Log("CLICK ON BUTTON " + mc.name);
        gameplayManager.Gameplay = mc.name.Equals("btNavigationMode") ? GameplayManager.GameplayType.Navigation : GameplayManager.GameplayType.ObstacleAvoidance;

        if (m_fileBrowser == null)
        {
            localizationUtils.GetLocalizedButton(btNavigationMode).DisableLocalizedButton();
            localizationUtils.GetLocalizedButton(btObstaclesMode).DisableLocalizedButton();

            m_fileBrowser = new FileBrowser(
                    new Rect((Screen.width * 0.5f) - 300, (Screen.height * 0.5f) - 200, 600, 400),
                    "Choose XML Scene",
                    LoadObstacleCallback,
                    "./ObstacleAvoidance"
                );
            m_fileBrowser.SelectionPattern = "*.xml";
            m_fileBrowser.DirectoryImage = m_directoryImage;
            m_fileBrowser.FileImage = m_fileImage;
        }
    }

    protected void LoadObstacleCallback(string path)
    {
        m_fileBrowser = null;
        if (null != path)
        {
            string result = System.IO.Path.GetFileName(path);
            UpdateSessionRow(gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation ? mcNavigationSessionRow : mcObstaclesSessionRow, false, result); //mcSessionRow
            UpdateSessionRow(gameplayManager.Gameplay == GameplayManager.GameplayType.Navigation ? mcObstaclesSessionRow : mcNavigationSessionRow, true); //mcSessionRow

            SetTfSessionResult(true, "XML Ready", true);
            localizationUtils.GetLocalizedButton(btPlaySession).SetButtonEvents();
            TrackingManager.Instance.SessionName = result;
            
            preloader.CheckSession(path);
        }

        localizationUtils.GetLocalizedButton(btNavigationMode).SetButtonEvents(); //btLoadSession
        localizationUtils.GetLocalizedButton(btObstaclesMode).SetButtonEvents(); //btLoadSession
    }
    
    #endregion
    #endregion

    #endregion

    #region Fade
    protected MovieClip mcFade = null;
    protected float _fadeEndTime;
    protected float _fadeStartTime;
    protected float _fadeStartValue;
    protected float _fadeEndValue;
    protected Action _fadeCallback;

    public void UpdateFade()
    {
        if (_fadeEndTime > 0.0f)
        {
            if (_fadeEndTime > _fadeStartTime)
            {
                float now = TimeManager.Instance.MasterSource.TotalTime;
                float t = Mathf.Clamp01((now - _fadeStartTime) / (_fadeEndTime - _fadeStartTime)),
                      s = 1.0f - t;
                mcFade.alpha = _fadeStartValue * s + _fadeEndValue * t;

                if (now > _fadeEndTime)
                {
                    mcFade.alpha = _fadeEndValue;
                    _fadeEndTime = -1.0f;

                    FadeCompleted();
                }
            }
            else
            {
                mcFade.alpha = _fadeEndValue;
                _fadeEndTime = -1.0f;

                FadeCompleted();
            }
        }
    }

    public void InitializeFade()
    {
        _fadeEndTime = -1.0f;

        MovieClipOverlayCameraBehaviour.overlayCameraName = "UICamera";
        stage = MovieClipOverlayCameraBehaviour.instance.stage;

        mcFade = new MovieClip("Flash/Utils.swf:mcFadeClass");
        mcFade.scaleX = Screen.width / 2.0f;
        mcFade.scaleY = Screen.height / 2.0f;
        mcFade.x = -Screen.width / 2.0f;
        mcFade.y = -Screen.height / 2.0f;
        mcFade.alpha = 0.0f;
        mcFade.mouseEnabled = false;

        stage.addChild(mcFade);
    }

    public void FadeCompleted()
    {
        _fadeEndTime = -1.0f;

        if (_fadeCallback != null)
        {
            _fadeCallback();
            _fadeCallback = null;
        }
    }

    public void ResetFade()
    {
        _fadeEndTime = -1.0f;

        if (null == mcFade)
            return;

        stage.removeChild(mcFade);

        //if (_fadeCallback != null)
        //{
        //    _fadeCallback();
        //    _fadeCallback = null;
        //}
    }
    
    public void ResetFadeCallback()
    {
        _fadeCallback = null;
    }

    public void StartFadeOut(float duration, Action callback)
    {
        if (null == mcFade)
        {
            return; // InitializeFade();
        }

        _fadeStartTime = TimeManager.Instance.MasterSource.TotalTime; //Time.realtimeSinceStartup;
        _fadeStartValue = mcFade.alpha;
        _fadeEndValue = 1.0f;
        _fadeEndTime = _fadeStartTime + duration * Mathf.Abs(_fadeEndValue - _fadeStartValue);
        _fadeCallback = callback;
    }

    public void StartFadeIn(float duration, Action callback)
    {
        if (null == mcFade)
        {
            return; // InitializeFade();
        }

        _fadeStartTime = TimeManager.Instance.MasterSource.TotalTime; //Time.realtimeSinceStartup;
        _fadeStartValue = mcFade.alpha;
        _fadeEndValue = 0.0f;
        _fadeEndTime = _fadeStartTime + duration * Mathf.Abs(_fadeStartValue - _fadeEndValue);

        _fadeCallback = callback;
    }
    #endregion

    #region Unity Callbacks
    protected string m_textPath;

    protected FileBrowser m_fileBrowser;

    [SerializeField]
    protected Texture2D m_directoryImage,
                        m_fileImage;

    public GUISkin metalSkin;
    void OnGUI()
    {

        if (this.State == AdminSessionPage)
        {
            if (loadingSessionOk)
            {
                fileName = GUI.TextField(new Rect(filenameX, filenameY, filenameW, filenameH), fileName, 14, customStyle);
            }
        }

        GUI.skin = metalSkin;

        if (m_fileBrowser != null)
        {
            m_fileBrowser.OnGUI();
        }
        //else
        //{
        //    OnGUIMain();
        //}
    }

    protected void FileSelectedCallback(string path)
    {
        m_fileBrowser = null;
        m_textPath = path;
    }

    protected void OnGUIMain()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Text File", GUILayout.Width(100));
        GUILayout.FlexibleSpace();
        GUILayout.Label(m_textPath ?? "none selected");
        if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
        {
            m_fileBrowser = new FileBrowser(
                new Rect(100, 100, 600, 500),
                "Choose Text File",
                FileSelectedCallback
            );
            m_fileBrowser.SelectionPattern = "*.ses";
            m_fileBrowser.DirectoryImage = m_directoryImage;
            m_fileBrowser.FileImage = m_fileImage;
        }
        GUILayout.EndHorizontal();
    }
    #endregion
}



