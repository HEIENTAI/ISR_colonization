using UnityEngine;
using System.Collections;

public class GameLifeUI : MonoBehaviour 
{
	public UISprite LifeBG;
	public UILabel LifeDisplay;

	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public int Count
	{
		set
		{
			LifeBG.fillAmount = (float)value / (float)(Const.MapSize * Const.MapSize);
			LifeDisplay.text = value.ToString();
		}
	}
}