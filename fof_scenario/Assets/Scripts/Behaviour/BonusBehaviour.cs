using UnityEngine;
using System.Collections;

[AddComponentMenu("FoF/BonusBehaviour")]
public class BonusBehaviour : MonoBehaviour
{
    #region Internal Classes
    public enum ColorType
    {
        Green = 0,
        Yellow,
        Red
    }
    #endregion

    #region Serialized Protected Fields
    [SerializeField]
    protected ColorType colorType;
    [SerializeField]
    protected int score;
    [SerializeField]
    protected bool isEnabled;
    [SerializeField]
    protected float distFromPlayer;
    [SerializeField]
    protected GameObject emitter;
    #endregion

    #region Protected Fields
    protected GameplayManager gameplay = null;
    protected GameObject playerRef = null;
    protected float collectedTimer = -1.0f;
    protected Vector3 startPosition;
    protected float startTime;
    protected float floatAmplitude = 0.06f;
    protected float floatSpeed = 5.0f;
    protected Material crossMaterial = null;
    protected Color startColor = Color.white;
    protected Color endColor = Color.white;
    protected float colorDuration = 1.0f;
    protected float t = 0.0f;
    protected Vector3 initialPos = Vector3.zero;
    protected GameObject currentEmitter;
    #endregion

    #region Constants
    const float COLLECT_DIST = 1.0f;
    const float RESPAWN_TIME = 40.0f;
    #endregion

    #region UnityCallbacks
    void Start()
    {
        gameplay = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<GameplayManager>();

        startPosition = this.transform.localPosition;
        startTime = TimeManager.Instance.MasterSource.TotalTime + UnityEngine.Random.Range(0.0f, 1.0f);

        //renderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.7f);
        initialPos = transform.position;

        currentEmitter = GameObject.Instantiate(emitter, transform.position, transform.rotation) as GameObject;
    }

    void Update()
    {
        if (isEnabled)
        {
            if (null == playerRef)
            {
                playerRef = gameplay.Character;
            }
            else
            {
                Vector3 dist = playerRef.transform.position - transform.position;
                dist.y = 0.0f;
                distFromPlayer = dist.magnitude;
                if (distFromPlayer < COLLECT_DIST)
                {
                    collectedTimer = TimeManager.Instance.MasterSource.TotalTime;
                    Collect();
                }
            }

            UpdateMovemets();
        }
        else
        {
            float time = TimeManager.Instance.MasterSource.TotalTime;
            //if (time - collectedTimer > RESPAWN_TIME)
            //{
               // Activate();
            //}
        }

        UpdateCrossColor();
    }
    #endregion

    #region Protected Methods
    void UpdateMovemets()
    {
        //Rotate
        //Float
        float now = TimeManager.Instance.MasterSource.TotalTime;
        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        float elapsedTime = TimeManager.Instance.MasterSource.TotalTime - startTime;
        float newY = startPosition.y + floatAmplitude * Mathf.Sin(elapsedTime * floatSpeed);

        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
        gameObject.transform.rotation = Quaternion.AngleAxis(now * 360.0f * 0.2f, Vector3.up);
    }

    protected void UpdateCrossColor()
    {
        if (null != crossMaterial)
        {
            crossMaterial.color = Color.Lerp(startColor, endColor, t);
            t += TimeManager.Instance.MasterSource.DeltaTime;
        }
    }
    #endregion

    #region Messages
    void Activate()
    {
        isEnabled = true;
        transform.position = initialPos;
    }

    void Collect()
    {
        if (isEnabled)
        {
            isEnabled = false;
            transform.position = initialPos - (Vector3.up * 100.0f);
            GetComponent<AudioSource>().Play();
            gameplay.SendMessage("BonusCollected", score);
            collectedTimer = TimeManager.Instance.MasterSource.TotalTime;
            currentEmitter.GetComponent<ParticleSystem>().Play();
        }
    }

    void ColorCross(GameObject crossGO)
    {
        if (isEnabled)
        {
            if (null == crossMaterial)
            {
                for (int i = 0; i < crossGO.GetComponent<Renderer>().materials.Length; i++)
                {
                    if (crossGO.GetComponent<Renderer>().materials[i].name.Contains("StreetTile"))
                        crossMaterial = crossGO.GetComponent<Renderer>().materials[i];
                }
            }

            if (null != crossMaterial)
            {
                endColor = crossMaterial.color;

                startColor = new Color(0.1804f, 0.5725f, 0.2667f);
                /*switch (colorType)
                {
                    case (ColorType.Green):
                        startColor = new Color(0.1804f, 0.5725f, 0.2667f);
                        break;
                    case (ColorType.Yellow):
                        startColor = new Color(0.7412f, 0.7412f, 0.2667f);
                        break;
                    case (ColorType.Red):
                        startColor = new Color(0.7804f, 0.2510f, 0.2510f);
                        break;
                }*/

                t = 0.0f;
            }
        }
    }
    #endregion
}
