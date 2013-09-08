using UnityEngine;
using System.Collections;

public class ChooseBackBtnEvt : MonoBehaviour
{

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnClick()
    {
        GameControl.Instance.ReturnToMain();
        Debug.Log("Back");
    }
}
