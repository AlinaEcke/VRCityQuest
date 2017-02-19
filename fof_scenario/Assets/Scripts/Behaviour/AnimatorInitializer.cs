using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimatorInitializer : MonoBehaviour
{
    #region Public fields
    #endregion

    #region Unity Callbacks
    void Start ()
    {
        GetComponent<Animation>().Play("anim_t_sc_r_s");
	}
    #endregion
}
