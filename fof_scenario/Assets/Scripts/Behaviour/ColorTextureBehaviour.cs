using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ColorTextureBehaviour : MonoBehaviour
{
    #region Static Fields
    public static int staticId = 0;
    #endregion

    #region Public Fields
    public int textureIndex = -1;
    public int colorIndex = -1;
    #endregion

    #region Protected Fields
    protected GameObject gameManagers; 
    protected List<Color> colorList = new List<Color>();
    EnvironmentManager environment = null;
    #endregion

    #region Unity Callbacks
    void Awake () {
        gameManagers = GameObject.FindGameObjectWithTag("GameManagers");
        environment = gameManagers.GetComponent<EnvironmentManager>();
        colorList.Add(new Color(1.0f, 1.0f, 1.0f));
        colorList.Add(new Color(0.784f, 0.784f, 0.784f));
        colorList.Add(new Color(0.545f, 0.431f, 0.305f));
        colorList.Add(new Color(0.494f, 0.545f, 0.325f));
        colorList.Add(new Color(1.0f, 0.686f, 0.686f));
        colorList.Add(new Color(0.588f, 0.8f, 0.8f));
        colorList.Add(new Color(0.622f, 0.372f, 0.372f));
    }
    #endregion

    #region Messages
    protected void ApplyColours()
    {
        Color new_color = colorList[colorIndex];

        //int target = gameManagers.GetComponent<GameplayManager>().targetNumByLevel[0];
        List<Material> materials = new List<Material>(gameObject.GetComponent<Renderer>().materials);
        Stack<GameObject> visited = new Stack<GameObject>();
        visited.Push(gameObject);

        GameObject go;
        //Building's color
        while (visited.Count != 0)
        {
            GameObject aux = visited.Pop();
            for (int i = 0; i < aux.transform.childCount; i++)
            {
                go = aux.transform.GetChild(i).gameObject;
                if (null != go.GetComponent<Renderer>())
                {
                    materials.AddRange(go.GetComponent<Renderer>().materials);
                    visited.Push(go);
                }
            }
        }

        foreach (Material mat in materials)
        {
            if (mat.name.StartsWith("fof_buildingTileBase1_mat"))
            {
                mat.color = new_color;
                mat.mainTexture = gameManagers.GetComponent<ExternalTexturesManager>().GetTextureByName("buildingTileBase" + textureIndex.ToString());
            }
            if (mat.name.StartsWith("fof_buildingWindows_mat"))
            {
                Texture2D windowTexture = gameManagers.GetComponent<ExternalTexturesManager>().GetTextureByName("buildingWindows" + textureIndex.ToString());
                mat.mainTexture = windowTexture;
                mat.SetTexture("_Illum", windowTexture);

                if (environment.IsDaylight)
                    mat.shader = Shader.Find("Diffuse");
                else
                    mat.shader = Shader.Find("Self-Illumin/Specular");
            }
            if (mat.name.StartsWith("fof_towerWindows1_mat"))
            {
               if (environment.IsDaylight)
                    mat.shader = Shader.Find("Diffuse");
                else
                    mat.shader = Shader.Find("Self-Illumin/Specular");
            }
            if (mat.name.StartsWith("fof_towerClock1_mat"))
            {
                if (environment.IsDaylight)
                    mat.shader = Shader.Find("Diffuse");
                else
                    mat.shader = Shader.Find("Self-Illumin/Specular");
            }
            if (mat.name.StartsWith("fof_buildingDoors_mat"))
            {
                mat.mainTexture = gameManagers.GetComponent<ExternalTexturesManager>().GetTextureByName("buildingDoor" + textureIndex.ToString());
            }
            if (mat.name.StartsWith("fof_buildingShop"))
            {
                int index_shop = (staticId++ % 16) + 1;
                string strIdx = index_shop.ToString();
                if (index_shop < 10)
                    strIdx = "0" + strIdx;
                Texture2D texture = gameManagers.GetComponent<ExternalTexturesManager>().GetTextureByName("buildingShopGen" + strIdx);
                mat.mainTexture = texture;
                mat.SetTexture("_Illum", texture);
                
                if (environment.IsDaylight)
                    mat.shader = Shader.Find("Diffuse");
                else
                    mat.shader = Shader.Find("Self-Illumin/Specular");
            }
        }
	}
    #endregion
}
