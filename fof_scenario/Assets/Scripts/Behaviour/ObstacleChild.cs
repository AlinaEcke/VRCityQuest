using UnityEngine;
using System.Collections;

public class ObstacleChild : MonoBehaviour
{
    public string name;

    #region Messages
    public bool isEnterActive = true;
    public bool isExitActive = false;
    #endregion
    protected GeneralSoundsManager generalSounds = null;

    void Start()
    {
        generalSounds = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GeneralSoundsManager>();
    }

    #region Messages
    void Activate()
    {
        isEnterActive = true;
        //isExitActive = true;
    }

    void Deactivate()
    {
        //isEnterActive = false;
        //isExitActive = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isEnterActive && other.name.Contains("Character"))
        {
            isExitActive = true;
            isEnterActive = false;
            other.gameObject.SendMessage("ObstacleTriggerEnter", name, SendMessageOptions.DontRequireReceiver);
            if (/*soundEnabled && */other.name.Contains("Character"))
            {
                PlayerKinematicBehaviour pk = other.gameObject.GetComponent<PlayerKinematicBehaviour>();
                if (pk.InputEnabled)
                    generalSounds.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.static_obstacle);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (isExitActive && other.name.Contains("Character"))
        {
            isEnterActive = true;
            isExitActive = false;
            other.gameObject.SendMessage("ObstacleTriggerExit", SendMessageOptions.DontRequireReceiver);
        }
    }
    #endregion
}
