using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FoF.Utils;

[AddComponentMenu("FoF/PedestrianTextureConfig")]
public class PedestrianTextureConfig : MonoBehaviour
{
    #region Public Fields
    public GameplayManager.CharacterGender gender;
    public GameObject modelRoot;
    public List<Texture2D> heads;
    public List<Texture2D> body1;
    public List<Texture2D> body2;
    public List<Texture2D> oldman;
    #endregion

    #region Protected Fields
    Texture2D headTexture = null;
    Texture2D body1Texture = null;
    Texture2D body2Texture = null;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        int randomOld = Random.Range(0, 100);
        if (gender == GameplayManager.CharacterGender.Male && oldman.Count > 0 && randomOld < 10)
        {
            headTexture = oldman[0];
            body1Texture = oldman[1];
            ApplyTextures("cop_passm_head01_mat", "cop_passm01_mat", string.Empty);
        }
        else
        {
            if (heads.Count > 0)
            {
                int randomHeadIdx = Random.Range(0, 10000) % heads.Count;
                headTexture = heads[randomHeadIdx];
            }

            if (body1.Count > 0)
            {
                int randomHeadIdx = Random.Range(0, 10000) % body1.Count;
                body1Texture = body1[randomHeadIdx];
            }

            if (body2.Count > 0)
            {
                int randomHeadIdx = Random.Range(0, 10000) % body2.Count;
                body2Texture = body2[randomHeadIdx];
            }

            if (gender == GameplayManager.CharacterGender.Female)
            {
                ApplyTextures("cop_passf_head01", "cop_passf01", "cop_passf_skirt01");
            }
            else if (gender == GameplayManager.CharacterGender.Male)
            {
                ApplyTextures("cop_passm_head01", "cop_passm01", string.Empty);
            }
        }
    }
    #endregion

    #region Protected Members
    protected void ApplyTextures(string headMatName, string body1MatName, string body2MatName)
    {
        foreach (Material mat in modelRoot.GetComponent<Renderer>().materials)
        {
            //Debug.LogWarning("mat.name: " + mat.name + ", headMatName: " + headMatName + " = " + mat.name.StartsWith(headMatName));
            if (mat.name.StartsWith(headMatName))
            {
                mat.mainTexture = headTexture;
            }
            else if (mat.name.StartsWith(body1MatName))
            {
                mat.mainTexture = body1Texture;
            }
            else if (mat.name.StartsWith(body2MatName))
            {
                mat.mainTexture = body2Texture;
            }
        }
    }
    #endregion

    //protected List<Material> skinMaterials;
    //protected List<Material> headMaterials_normal;
    //protected List<Material> headMaterials_angry;
    //protected List<Material> headMaterials_sad;
    //protected List<Material> hairMaterials;
    //protected List<Material> mustacheMaterials;
    //protected List<Material> bandanaMaterials;
    //protected List<Material> hatMaterials;

    //protected bool hasSleeves;
    //protected bool hasGloves;
    //protected bool hasHairMesh;
    //protected bool hasMustache;
    //protected bool hasPlayerBandana;
    //protected bool hasBanditBandana;

    //protected int instancesSkinIndex;
    //protected List<int> skinIndices;

    //protected CharacterNodesProvider nodes;
    
    //protected static void SetupIndices(List<int> indicesList, int numIndices)
    //{
    //    for (int i = 0; i < numIndices; i++)
    //        indicesList.Add(i);
    //}

    //public virtual void Setup(GameObject character)
    //{
    //    SetupSkin(character);
    //    SetupHead(character);
    //    SetupHair(character);
    //    SetupMustache(character);
    //    SetupBandanaPlayer(character);
    //    SetupBandanaBandit(character);
    //    ScaleCharacter(character, new Vector3(1.0f, 1.0f, 1.0f));
    //    PutHatOn(character);
    //}

    //protected void PutHatOn(GameObject character)
    //{
    //    Character.CharacterClasses type = character.GetComponent<Character>().characterClass;
        
    //    GameObject hat = HatManager.Instance.GetUnusedHat(type);
    //    if (hat != null)
    //    {
    //        hat.transform.parent = character.GetComponent<Character>().hatAnchorBoneTr;
    //        hat.transform.localPosition = GetHatOffset();   
    //        hat.transform.localRotation = GetHatRotation(); 
    //        hat.SetActiveRecursively(true);
            
    //        if (hatMaterials == null)
    //            return;
    //        Material hatMat = hatMaterials[NextHatIndex()];
    //        hat.renderer.sharedMaterial = hatMat;
    //    }
    //}

    //protected virtual Vector3 GetHatOffset()
    //{
    //    return new Vector3(0.01280262f, 0.01272665f, 0.0f);
    //}

    //protected virtual Quaternion GetHatRotation()
    //{
    //    return Quaternion.Euler(14.10944f, 270.0f, 180.0f);
    //}

    //protected void SetupSkin(GameObject character)
    //{
    //    if (skinMaterials == null)
    //        return;
        
    //    Material skin = skinMaterials[NextSkinIndex()];

    //    SkinArms(character, skin);
    //    SkinBody(character, skin);
    //    SkinGloves(character, skin);
    //    SkinSleeves(character, skin);
    //}

    //protected void SkinArms(GameObject character, Material skin)
    //{
    //    character.transform.Find(nodes.ArmRight).GetComponent<SkinnedMeshRenderer>().sharedMaterial = skin;
    //    character.transform.Find(nodes.ArmLeft).GetComponent<SkinnedMeshRenderer>().sharedMaterial = skin;
    //}

    //protected void SkinBody(GameObject character, Material skin)
    //{
    //    character.transform.Find(nodes.Body).GetComponent<SkinnedMeshRenderer>().sharedMaterial = skin;
    //}

    //protected void SkinGloves(GameObject character, Material skin)
    //{
    //    if (string.IsNullOrEmpty(nodes.GloveRight) || string.IsNullOrEmpty(nodes.GloveLeft))
    //        return;

    //    SkinnedMeshRenderer[] gloves = new SkinnedMeshRenderer[2] { character.transform.Find(nodes.GloveRight).GetComponent<SkinnedMeshRenderer>(),
    //                                                                character.transform.Find(nodes.GloveLeft).GetComponent<SkinnedMeshRenderer>()   };
    //    if (hasGloves)
    //    {
    //        gloves[0].sharedMaterial = skin;
    //        gloves[1].sharedMaterial = skin;
    //    }
    //    else
    //    {
    //        gloves[0].enabled = false;
    //        gloves[1].enabled = false;
    //    }
    //}

    //protected void SkinSleeves(GameObject character, Material skin)
    //{
    //    if (string.IsNullOrEmpty(nodes.SleeveRight) || string.IsNullOrEmpty(nodes.SleeveLeft))
    //        return;

    //    SkinnedMeshRenderer[] sleeves = new SkinnedMeshRenderer[2] { character.transform.Find(nodes.SleeveRight).GetComponent<SkinnedMeshRenderer>(),
    //                                                                 character.transform.Find(nodes.SleeveLeft).GetComponent<SkinnedMeshRenderer>()   };

    //    if (hasSleeves)
    //    {
    //        sleeves[0].sharedMaterial = skin;
    //        sleeves[1].sharedMaterial = skin;
    //    }
    //    else
    //    {
    //        sleeves[0].enabled = false;
    //        sleeves[1].enabled = false;
    //    }
    //}

    //protected void SetupHead(GameObject character)
    //{
    //    if (headMaterials_normal == null)
    //        return;

    //    int headIndex = NextHeadIndex();

    //    Material head_normal = headMaterials_normal[headIndex];
    //    character.transform.Find(nodes.Head).GetComponent<SkinnedMeshRenderer>().sharedMaterial = head_normal;

    //    Material head_angry = head_normal;
    //    Material head_sad = head_normal;

    //    if (headMaterials_angry != null)
    //        if (headIndex < headMaterials_angry.Count)
    //            head_angry = headMaterials_angry[headIndex];

    //    if (headMaterials_sad != null)
    //        if (headIndex < headMaterials_sad.Count)
    //            head_sad = headMaterials_sad[headIndex];

    //    AddFacialExpressionsComponent(character, head_normal, head_angry, head_sad);
    //}

    //protected void AddFacialExpressionsComponent(GameObject character, Material expr_normal, Material expr_angry, Material expr_sad)
    //{
    //    FacialExpressions_Behaviour exprBehav = character.AddComponent<FacialExpressions_Behaviour>();
    //    exprBehav.node_head = nodes.Head;
    //    exprBehav.face_normal = expr_normal;
    //    exprBehav.face_angry = expr_angry;
    //    exprBehav.face_sad = expr_sad;
    //}

    //protected void SetupHair(GameObject character)
    //{
    //    if (string.IsNullOrEmpty(nodes.Hair))
    //        return;

    //    SkinnedMeshRenderer hairMeshRnd = character.transform.Find(nodes.Hair).GetComponent<SkinnedMeshRenderer>();
    //    hairMeshRnd.enabled = false;

    //    if (hairMaterials == null || !hasHairMesh)
    //    {
    //        return;
    //    }

    //    if (hasHairMesh)
    //    {
    //        GameObject hairObj = HairManager.Instance.GetUnusedHair();
    //        if (hairObj != null)
    //        {
    //            hairObj.GetComponent<HairReleaser>().owner = character.gameObject;

    //            Material hairMat = hairMaterials[NextHairIndex()];
    //            hairObj.renderer.sharedMaterial = hairMat;
    //            hairObj.transform.parent = character.GetComponent<Character>().hatAnchorBoneTr;
    //            hairObj.transform.localPosition = new Vector3(0.01560538f, -0.002786034f, -0.01008553f);
    //            hairObj.transform.localRotation = Quaternion.Euler(8.555908f, 270.0f, 180.0f);
    //            hairObj.SetActiveRecursively(true);
    //        }
    //    }
    //}

    //protected void SetupMustache(GameObject character)
    //{        
    //    if (mustacheMaterials == null || !hasMustache)
    //        return;

    //    MaterialRepository matRepository = GameObject.FindGameObjectWithTag("NPCsManager").GetComponent<MaterialRepository>();
    //    if (null == MustacheManager.Instance)
    //        return;
    //    GameObject mustache = MustacheManager.Instance.GetUnusedMustache();
    //    if (mustache != null)
    //    {
    //        mustache.GetComponent<MustacheReleaser>().owner = character.gameObject;

    //        Material mustacheMat = mustacheMaterials[NextMustacheIndex()];
    //        mustache.renderer.sharedMaterial = mustacheMat;
    //        mustache.transform.parent = character.GetComponent<Character>().hatAnchorBoneTr;
    //        mustache.transform.localPosition = new Vector3(-0.0653636f, 0.107318f, 0.0f);
    //        mustache.transform.localRotation = Quaternion.Euler(270.0f, 90.0f, 0.0f);
    //        mustache.SetActiveRecursively(true);
    //    }

    //}

    //protected void SetupBandanaPlayer(GameObject character)
    //{
    //    if (string.IsNullOrEmpty(nodes.BandanaPlayer))
    //        return;

    //    SkinnedMeshRenderer bandanaMeshRnd = character.transform.Find(nodes.BandanaPlayer).GetComponent<SkinnedMeshRenderer>();

    //    if (!hasPlayerBandana)
    //        bandanaMeshRnd.enabled = false;
    //}

    //protected void SetupBandanaBandit(GameObject character)
    //{
    //    if (string.IsNullOrEmpty(nodes.BandanaBandit))
    //        return;

    //    SkinnedMeshRenderer bandanaMeshRnd = character.transform.Find(nodes.BandanaBandit).GetComponent<SkinnedMeshRenderer>();
    //    bandanaMeshRnd.enabled = hasBanditBandana;
    //}

    //protected virtual int NextSkinIndex()
    //{
    //    if (skinMaterials.Count == 1)
    //        return 0;
        
    //    if (skinIndices == null)
    //        return Random.Range(0, skinMaterials.Count);
                

    //    if (instancesSkinIndex % skinMaterials.Count == 0)
    //    {
    //        ShuffleIndices(skinIndices);
    //    }

    //    return skinIndices[(instancesSkinIndex % skinMaterials.Count)];
    //}

    //protected void ShuffleIndices(List<int> indicesList)
    //{
    //    for (int i = 0; i < indicesList.Count - 1; i++)
    //    {
    //        int idxToSwap = Random.Range(i + 1, indicesList.Count);
          
    //        int firstIdx = indicesList[i];
    //        int secondIdx = indicesList[idxToSwap];

    //        indicesList[i] = secondIdx;
    //        indicesList[idxToSwap] = firstIdx;
    //    }
    //}
    
    //protected int NextHeadIndex()
    //{
    //    if (headMaterials_normal.Count == 1)
    //        return 0;

    //    return Random.Range(0, headMaterials_normal.Count);
    //}

    //protected int NextHairIndex()
    //{
    //    if (hairMaterials.Count == 1)
    //        return 0;

    //    return Random.Range(0, hairMaterials.Count);
    //}


    //protected int NextBandanaIndex()
    //{
    //    if (bandanaMaterials.Count == 1)
    //        return 0;

    //    return Random.Range(0, bandanaMaterials.Count);
    //}

    //protected int NextMustacheIndex()
    //{
    //    if (mustacheMaterials.Count == 1)
    //        return 0;

    //    return Random.Range(0, mustacheMaterials.Count);
    //}

    //protected virtual int NextHatIndex()
    //{
    //    if (hatMaterials.Count == 1)
    //        return 0;

    //    return Random.Range(0, hatMaterials.Count);
    //}

    //protected void ScaleCharacter(GameObject character, Vector3 scale)
    //{
    //    character.transform.localScale = scale;
    //}
}
