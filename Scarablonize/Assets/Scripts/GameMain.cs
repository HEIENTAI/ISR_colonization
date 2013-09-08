using UnityEngine;
using System.Collections;

public class GameMain : MonoBehaviour {

    private GameControl _control = null;

	// Use this for initialization
	void Start () {
        _control = GameControl.Instance;
        DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
        if (_control != null)
            _control.Update();
	}
}
