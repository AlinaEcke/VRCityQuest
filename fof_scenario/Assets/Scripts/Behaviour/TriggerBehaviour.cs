using UnityEngine;
using System.Collections;

public class TriggerBehaviour : MonoBehaviour
{
    #region Protected Fields
    protected GameObject player;
    protected GeneralSoundsManager generalSounds = null;
    #endregion

    #region UnityCallbacks
    void Start()
    {
        GameObject gameManagers = GameObject.FindGameObjectWithTag("GameManagers");
        GameplayManager gameplay = gameManagers.GetComponent<GameplayManager>();
        player = gameplay.Character;
        generalSounds = gameManagers.GetComponent<GeneralSoundsManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        other.gameObject.SendMessage("ObstacleTriggerEnter", gameObject.name, SendMessageOptions.DontRequireReceiver);
        if (other.name.Contains("Character"))
        {
            PlayerKinematicBehaviour pk = other.gameObject.GetComponent<PlayerKinematicBehaviour>();
            if (pk.InputEnabled)
                generalSounds.SendMessage("PlayGeneralSound", GeneralSoundsManager.GeneralSounds.static_obstacle);
        }
    }

    void OnTriggerExit(Collider other)
    {
        other.gameObject.SendMessage("ObstacleTriggerExit", SendMessageOptions.DontRequireReceiver);
    }
    #endregion
}
