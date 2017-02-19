using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FoF.Utils;
using SBS.XML;
using System.IO;


[AddComponentMenu("FoF/BuildingAssetsManager")]
public class BuildingAssetsManager : MonoBehaviour
{
    #region Internal Data Structure
    public enum ProgressSignalType
    {
        OnBuidingAssetsLoaded = 0
    }

    public struct ProgressSignal
    {
        public ProgressSignalType type;
        public object param;

        public ProgressSignal(ProgressSignalType type)
        {
            this.type = type;
            this.param = null;
        }

        public ProgressSignal(ProgressSignalType type, object param)
        {
            this.type = type;
            this.param = param;
        }
    }

    public class AssetClass
    {
        public const string Vase = "vase";
        public const string Pillar = "pillar";
        public const string Balcony = "balcony";
        public const string Lamp = "lamp";
    }

    public class BuildingAsset
    {
        public GameObject assetSource;
        public AssetLocation location;

        public BuildingAsset(GameObject go, AssetLocation loc)
        {
            assetSource = go;
            location = loc;
        }
    }
    #endregion

    #region Public Fields
    public Dictionary<string, int> percentages = new Dictionary<string, int>();
    #endregion

    #region Public Fields
    public SignalSender progressSignals;
    #endregion

    #region Protected Fields
    Dictionary<string, BuildingAsset> assets = new Dictionary<string, BuildingAsset>();
    #endregion

    #region Unity Callback
    void Awake()
    {
        percentages.Add(AssetClass.Vase, 50);
        percentages.Add(AssetClass.Pillar, 50);
        percentages.Add(AssetClass.Balcony, 50);
        percentages.Add(AssetClass.Lamp, 100);
    }
    #endregion

    #region Messages
    void LoadConfiguration(string configText)
    {
        assets.Clear();
        assets.Add("vase_1x1", new BuildingAsset(Resources.Load("Buildings/vase_1x1") as GameObject, AssetLocation.Ground));
        assets.Add("vase_1x2", new BuildingAsset(Resources.Load("Buildings/vase_1x2") as GameObject, AssetLocation.Ground));
        assets.Add("vase_1x3", new BuildingAsset(Resources.Load("Buildings/vase_1x3") as GameObject, AssetLocation.Ground));
        assets.Add("vase_2x2", new BuildingAsset(Resources.Load("Buildings/vase_2x2") as GameObject, AssetLocation.Ground));
        assets.Add("vase_2x3", new BuildingAsset(Resources.Load("Buildings/vase_2x3") as GameObject, AssetLocation.Ground));
        assets.Add("vase_3x3", new BuildingAsset(Resources.Load("Buildings/vase_3x3") as GameObject, AssetLocation.Ground));
        assets.Add("pillar_1x1", new BuildingAsset(Resources.Load("Buildings/pillar_1x1") as GameObject, AssetLocation.Ground));
        assets.Add("pillar_1x2", new BuildingAsset(Resources.Load("Buildings/pillar_1x2") as GameObject, AssetLocation.Ground));
        assets.Add("pillar_1x3", new BuildingAsset(Resources.Load("Buildings/pillar_1x3") as GameObject, AssetLocation.Ground));
        assets.Add("pillar_2x2", new BuildingAsset(Resources.Load("Buildings/pillar_2x2") as GameObject, AssetLocation.Ground));
        assets.Add("pillar_2x3", new BuildingAsset(Resources.Load("Buildings/pillar_2x3") as GameObject, AssetLocation.Ground));
        assets.Add("pillar_3x3", new BuildingAsset(Resources.Load("Buildings/pillar_3x3") as GameObject, AssetLocation.Ground));
        assets.Add("pillar-floor_1x1", new BuildingAsset(Resources.Load("Buildings/pillar-floor_1x1") as GameObject, AssetLocation.Floor));
        assets.Add("pillar-floor_1x2", new BuildingAsset(Resources.Load("Buildings/pillar-floor_1x2") as GameObject, AssetLocation.Floor));
        assets.Add("pillar-floor_1x3", new BuildingAsset(Resources.Load("Buildings/pillar-floor_1x3") as GameObject, AssetLocation.Floor));
        assets.Add("pillar-floor_2x2", new BuildingAsset(Resources.Load("Buildings/pillar-floor_2x2") as GameObject, AssetLocation.Floor));
        assets.Add("pillar-floor_2x3", new BuildingAsset(Resources.Load("Buildings/pillar-floor_2x3") as GameObject, AssetLocation.Floor));
        assets.Add("pillar-floor_3x3", new BuildingAsset(Resources.Load("Buildings/pillar-floor_3x3") as GameObject, AssetLocation.Floor));
        assets.Add("balcony_1x1", new BuildingAsset(Resources.Load("Buildings/balcony_1x1") as GameObject, AssetLocation.Floor));
        assets.Add("balcony_1x2", new BuildingAsset(Resources.Load("Buildings/balcony_1x2") as GameObject, AssetLocation.Floor));
        assets.Add("balcony_1x3", new BuildingAsset(Resources.Load("Buildings/balcony_1x3") as GameObject, AssetLocation.Floor));
        assets.Add("balcony_2x2", new BuildingAsset(Resources.Load("Buildings/balcony_2x2") as GameObject, AssetLocation.Floor));
        assets.Add("balcony_2x3", new BuildingAsset(Resources.Load("Buildings/balcony_2x3") as GameObject, AssetLocation.Floor));
        assets.Add("balcony_3x3", new BuildingAsset(Resources.Load("Buildings/balcony_3x3") as GameObject, AssetLocation.Floor));
        assets.Add("lamp_1x1", new BuildingAsset(Resources.Load("Buildings/lamp_1x1") as GameObject, AssetLocation.Ground));
        assets.Add("lamp_1x2", new BuildingAsset(Resources.Load("Buildings/lamp_1x2") as GameObject, AssetLocation.Ground));
        assets.Add("lamp_1x3", new BuildingAsset(Resources.Load("Buildings/lamp_1x3") as GameObject, AssetLocation.Ground));
        assets.Add("lamp_2x2", new BuildingAsset(Resources.Load("Buildings/lamp_2x2") as GameObject, AssetLocation.Ground));
        assets.Add("lamp_2x3", new BuildingAsset(Resources.Load("Buildings/lamp_2x3") as GameObject, AssetLocation.Ground));
        assets.Add("lamp_3x3", new BuildingAsset(Resources.Load("Buildings/lamp_3x3") as GameObject, AssetLocation.Ground));

        progressSignals.SendSignal(this, new ProgressSignal(ProgressSignalType.OnBuidingAssetsLoaded));
    }
    #endregion

    #region Coroutines
    #endregion

    #region Public Members
    public GameObject GetAssetSource(string assetName)
    {
        if (assets.ContainsKey(assetName))
            return assets[assetName].assetSource;
        return null;
    }
    #endregion
}
