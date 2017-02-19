using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FoF.Utils;

[AddComponentMenu("FoF/PedestrianTexture")]
public class PedestrianTexture : MonoBehaviour
{
    #region Internal Data Structures
    [System.Serializable]
    public class MaterialTextureItem
    {
        public Material targetMaterial;
        public List<Texture2D> textures;
    }
    #endregion

    #region Public Fields
    public List<MaterialTextureItem> matTextList;
    public GameObject modelRoot;
    public GameObject hairRoot = null;
    public List<Texture2D> hairTextures;
    #endregion

    #region Protected Fields
    #endregion

    #region Unity Callbacks
    void Start()
    {
        for (int i = 0; i < matTextList.Count; i++)
        {
            MaterialTextureItem item = matTextList[i];
            int index = Random.Range(0, 100000) % item.textures.Count;
            ApplyTextures(item.targetMaterial.name, item.textures[index]);

            if (hairRoot != null)
            {
                if (item.targetMaterial.name.StartsWith("f_head"))
                {
                    if (index > 1)
                        hairRoot.SetActive(false);
                    else
                    {
                        foreach (Material mat in hairRoot.GetComponent<Renderer>().materials)
                        {
                            if (mat.name.StartsWith("f_hair"))
                                mat.mainTexture = hairTextures[index];
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Protected Members
    protected void ApplyTextures(string matName, Texture2D texture)
    {
        foreach (Material mat in modelRoot.GetComponent<Renderer>().materials)
        {
            if (mat.name.StartsWith(matName))
                mat.mainTexture = texture;
        }
    }
    #endregion
}
