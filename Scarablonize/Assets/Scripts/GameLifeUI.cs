using UnityEngine;
using System.Collections;

public class GameLifeUI : MonoBehaviour 
{
	public UISprite LifeBG;
	public UILabel LifeDisplay;
	
	float _barLength;
	float _lifeDisplaytopPos;
	
	void Initialize()
	{
		Count = 0;
	}
	
	void Awake()
	{
		_barLength = LifeBG.cachedTransform.localScale.y;
		_lifeDisplaytopPos = LifeDisplay.cachedTransform.localPosition.y;
		Initialize();
	}

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
			LifeDisplay.cachedTransform.localPosition = new Vector3(0, _lifeDisplaytopPos - (_barLength * (1 - LifeBG.fillAmount)), 0);
		}
	}
}