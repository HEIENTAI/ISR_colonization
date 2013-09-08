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
		if(bodyGo == null)
			return;
        bodyGo.transform.localScale = new Vector3(100, 100, 1);
		//bodyGo.renderer.sharedMaterial.shader = Shader.Find("Transparent/Diffuse");
    }

    public void UnHighLight()
    {
		if(bodyGo == null)
			return;
        bodyGo.transform.localScale = new Vector3(64, 64, 1);
		//bodyGo.renderer.sharedMaterial.shader = Shader.Find("Unlit/Transparent");
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
