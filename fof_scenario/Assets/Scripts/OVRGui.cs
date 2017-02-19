using UnityEngine;
using System.Collections;

public class OVRGui : VRGUI {
    public Texture2D arrowLeft;
    public Texture2D arrowCenter;
    public Texture2D arrowRight;

    protected bool showLeft = false;
    protected bool showCenter = false;
    protected bool showRight = false;

    public override void OnVRGUI()
    {
       // Debug.Log("*************///////////////************ onvrgui " + arrowLeft + " " + arrowCenter + " " + arrowRight);
        float xCenter = Screen.width * 0.5f;
        float yCenter = Screen.height * 0.5f;

        if (showLeft)
            GUI.Label(new Rect(xCenter - 64f - 256f, yCenter - 128f, 256f, 256f), arrowLeft);
        
        if (showCenter)
            GUI.Label(new Rect(xCenter - 64f, yCenter - 64f, 128f, 128f), arrowCenter);
        
        if (showRight)
            GUI.Label(new Rect(xCenter + 64f, yCenter - 128f, 256f, 256f), arrowRight);
    }

    #region Messages
    public void ShowLeftArrow()
    {
        showLeft = true;
    }

    public void ShowCenterArrow()
    {
        showCenter = true;
    }

    public void ShowRightArrow()
    {
        showRight = true;
    }

    void HideArrows()
    {
        showLeft = false;
        showCenter = false;
        showRight = false;
    }
    #endregion
}
