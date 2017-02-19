using UnityEngine;
using System.Collections;

public class TestToken : MonoBehaviour {

    public bool hitFlag;
    public string CurrentToken;
    public float Long;
    public float Trasv;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        TokensManager.TokenHit hit;

        CurrentToken = "none";

        hitFlag = TokensManager.Instance.GetToken(transform.position, out hit);
        if (hitFlag)
        {
            Long = hit.longitudinal;
            Trasv = hit.trasversal;
            CurrentToken = hit.token.name;
        }
        //GameObject.Find("token").GetComponent<Token>().WorldToToken(new SBS.Math.SBSVector3(transform.position.x, transform.position.y, transform.position.z), out Long, out Trasv);
	}
}
