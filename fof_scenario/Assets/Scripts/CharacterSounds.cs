using UnityEngine;
using System.Collections;

public class CharacterSounds : MonoBehaviour {
    
    #region Public members
    public AudioClip[] footsteps;

    #endregion

    #region Protected members
    protected AudioSource characterSource;
    protected GeneralSoundsManager genSoundsManager;
    protected AnimationBehaviour charAnim;

    protected bool firstFootstepPlayed = false;
    #endregion

    #region Public properties

    #endregion

    #region Messages
    void PlayFootsteps()
    {
        if (!genSoundsManager.SoundsEnabled)
            return;
        characterSource.clip = footsteps[UnityEngine.Random.Range(0, footsteps.Length)];
        SoundsManager.Instance.PlaySource(characterSource);
    }

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        characterSource = gameObject.AddComponent<AudioSource>();
        characterSource.volume = 1.0f;
        characterSource.loop = false;
        characterSource.playOnAwake = false;

        charAnim = gameObject.GetComponent<AnimationBehaviour>();
    }

    void Start()
    {
        genSoundsManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GeneralSoundsManager>();
    }

    void Update()
    { 
        float animNormalTime = charAnim.CharacterAnimation.IsPlaying("walk") ? charAnim.CharacterAnimation["walk"].normalizedTime : 0.0f;

        //bool firstStep = ((animNormalTime - (int)animNormalTime) >= 0.12f) && ((animNormalTime - (int)animNormalTime) <= 0.16f);
        //bool secondStep = ((animNormalTime - (int)animNormalTime) >= 0.52f) && ((animNormalTime - (int)animNormalTime) <= 0.56f);
        bool firstStep = ((animNormalTime - (int)animNormalTime) >= 0.08f) && ((animNormalTime - (int)animNormalTime) <= 0.12f);
        bool secondStep = ((animNormalTime - (int)animNormalTime) >= 0.56f) && ((animNormalTime - (int)animNormalTime) <= 0.60f);

        if (firstStep && !firstFootstepPlayed)
        {
            PlayFootsteps();
            firstFootstepPlayed = true;
        }

        if (secondStep && firstFootstepPlayed)
        {
            PlayFootsteps();
            firstFootstepPlayed = false;
        }
        
    }
    #endregion
}
