using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FoF.Utils;

public class BuildingAssets : MonoBehaviour
{
    #region Public Fields
    public Vector2 size;
    public float rotation;
    public bool vaseActive = false;
    public bool pillarActive = false;
    public bool balconyActive = false;
    #endregion

    #region Protected Fields
    protected EnvironmentManager env;
    protected BuildingAssetsManager manager;
    protected List<GameObject> floorList = new List<GameObject>();
    protected string sizeString = string.Empty;
    protected bool assetLoaded = false;
    #endregion

    #region Public Get/Set
    public bool Loaded
    {
        get { return assetLoaded; }
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        manager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<BuildingAssetsManager>();
        env = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<EnvironmentManager>();
    }
    #endregion

    #region Public Members
    public void LoadAssets()
    {
        InitializeFloors();
        if (vaseActive)
            PlaceAsset(BuildingAssetsManager.AssetClass.Vase, true, false);
        if (pillarActive)
            PlaceAsset(BuildingAssetsManager.AssetClass.Pillar, true, true);
        if (balconyActive)
            PlaceAsset(BuildingAssetsManager.AssetClass.Balcony, false, true);
        if (!env.IsDaylight)
            PlaceAsset(BuildingAssetsManager.AssetClass.Lamp, true, false);

        assetLoaded = true;
    }
    #endregion

    #region Protected Members
    protected void InitializeFloors()
    {
        Transform tr = gameObject.transform;
        for (int i = 0; i < tr.childCount; i++)
        {
            GameObject child = tr.GetChild(i).gameObject;
            if (child.name.StartsWith("floor"))
                floorList.Add(child);
        }
    }

    protected void PlaceAsset(string assetBaseName, bool ground, bool floor)
    {
        int percentage = manager.percentages[assetBaseName];
        int random = Random.Range(1, 10000) % 100;

        bool skip = true;

        if (random < percentage || skip)
        {
            if (size.x > size.y)
            {
                float tmp = size.x;
                size.x = size.y;
                size.y = tmp;
            }

            if (ground)
            {
                string sourceName = assetBaseName + "_" + (size.x * 0.1f) + "x" + (size.y * 0.1f);
                GameObject source = manager.GetAssetSource(sourceName);
                if (null != source)
                {
                    GameObject obj = GameObject.Instantiate(source) as GameObject;
                    obj.transform.position = gameObject.transform.position;
                    obj.transform.Rotate(Vector3.up * rotation);
                    obj.transform.parent = gameObject.transform;
                }
            }

            if (floor)
            {
                if (assetBaseName.Equals(BuildingAssetsManager.AssetClass.Pillar))
                    assetBaseName += "-floor";

                string sourceName = assetBaseName + "_" + (size.x * 0.1f) + "x" + (size.y * 0.1f);
                GameObject source = manager.GetAssetSource(sourceName);
                if (null != source)
                {
                    foreach (GameObject go in floorList)
                    {
                        GameObject obj = GameObject.Instantiate(source) as GameObject;
                        obj.transform.position = go.transform.position;
                        obj.transform.Rotate(Vector3.up * rotation);
                        obj.transform.parent = go.transform;
                    }
                }
            }
        }
    }
    #endregion
}
