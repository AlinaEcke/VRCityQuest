using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour
{
    public Animator animator;
    public float hSpeed;

    #region Unity callbacks
    void Start ()
    {

	}
	
	void Update ()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            hSpeed -= .01f;
        if (Input.GetKey(KeyCode.RightArrow))
            hSpeed += .01f;

        hSpeed = Mathf.Clamp(hSpeed, -1.0f, 1.0f);

        if (null != animator)
            animator.SetFloat("HSpeed", hSpeed);
    }

    void OnGUI()
    {
    }
    #endregion
}
