using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class RoadsTextureBehaviour : MonoBehaviour
{
    #region Public Fields
    #endregion

    #region Protected Fields
    protected GameObject gameManagers;
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        gameManagers = GameObject.FindGameObjectWithTag("GameManagers");
    }
    #endregion

    #region Messages
    protected void ApplyRoadsTextures(int index)
    {
        List<Material> materials = new List<Material>(gameObject.GetComponent<Renderer>().materials);
        GameObject go;
        //int index = Random.Range(1, 4);
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            go = gameObject.transform.GetChild(i).gameObject;
            materials.AddRange(go.GetComponent<Renderer>().materials);
        }
       
        foreach (Material mat in materials)
        {
            if (mat.name.StartsWith("fof_StreetTile1_mat"))
            {
                Texture2D texture = gameManagers.GetComponent<ExternalTexturesManager>().GetTextureByName("StreetTile" + index.ToString());
                mat.mainTexture = texture;
            }
        }
    }
    #endregion
}
