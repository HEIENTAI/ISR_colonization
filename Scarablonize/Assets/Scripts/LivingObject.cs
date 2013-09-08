using UnityEngine;
using System.Collections;

public class LivingObject : MonoBehaviour {

    public GameObject bodyGo = null;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void HighLight()
    {
        bodyGo.transform.localScale = new Vector3(70, 70, 1);
    }

    public void UnHighLight()
    {
        bodyGo.transform.localScale = new Vector3(64, 64, 1);
    }

    public virtual void MoveUp()
    { 
    
    }

    public virtual void MoveDown()
    {

    }

    public virtual void MoveRight()
    {

    }

    public virtual void MoveLeft()
    {

    }
}
