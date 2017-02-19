using UnityEngine;
using System.Collections;

public class ObjectSounds : MonoBehaviour {

    #region Public members
    public AudioClip objSound;
    #endregion
    
    #region Protected members
    protected AudioSource objSource;
    protected GeneralSoundsManager genSoundsManager;
    protected Animation objAnim;
    protected bool iWasPlaying = false;
    #endregion
    
    #region Messages

    void PlayObjSound()
    {
        if (!genSoundsManager.SoundsEnabled || !genSoundsManager.SoundsActive)
            return;

        SoundsManager.Instance.PlaySource(objSource);
    }

    void StopObjSource()
    {
        objSource.Stop();
    }

    #endregion
    
    #region Unity Callbacks

    void Awake()
    {
        objSource = gameObject.AddComponent<AudioSource>();
        objSource.volume = 1.0f;
        objSource.loop = true;
        objSource.playOnAwake = false;
        objSource.clip = objSound;

        objAnim = gameObject.GetComponentInChildren<Animation>();
    }

    void Start()
    {
        genSoundsManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GeneralSoundsManager>();
    }

    /*void Update()
    {
        if (objAnim.IsPlaying("move") && !objSource.isPlaying)
            PlayObjSound();

        if (!objAnim.IsPlaying("move") && objSource.isPlaying)
            StopObjSource();
    }*/

  

    void PlayObstacleSound(float volume)
    {
        objSource.volume = volume;
        PlayObjSound();
    }

    void StopObstacleSound()
    {
        StopObjSource();
    }

    void OnPause()
    {
        iWasPlaying = objSource.isPlaying;
        objSource.Pause();
    }

    void OnResume()
    {
        if (iWasPlaying)
            objSource.Play();
    }
    #endregion
}
