using UnityEngine;
using System.Collections;

public class AnimateTexture : MonoBehaviour 
{
	public enum AnimWrapMode
	{
		Loop,
		Once,
		OnceWithoutDestroy
	}
	
	public enum AnimPlayState
	{
		Playing,
		Pause
	}
	
	public int _uvTieX = 1;
	public int _uvTieY = 1;
	public int _fps = 10;
	public AnimWrapMode wrapMode = AnimWrapMode.Loop;
	public AnimPlayState playState = AnimPlayState.Playing;
 
	private Vector2 _size;
	private Renderer _myRenderer;
	private int _lastIndex = -1;

	int index = 0;
	float time = 0;
	
	public int CurrentIndex
	{
		get
		{
			return index;
		}
		set
		{
			if(value >= 0 && value <(_uvTieX * _uvTieY))
			{
				index = value;
				time = value;
			}
		}
	}
	
	void Start () 
	{
		_size = new Vector2 (1.0f / _uvTieX , 1.0f / _uvTieY);
		_myRenderer = renderer;
		if(_myRenderer == null)
		{
			enabled = false;
		}
		//_myRenderer.material.SetTextureScale ("_MainTex", _size);	
		UpdateAnimateTex();
	}
	// Update is called once per frame
	void Update()
	{
		if (playState == AnimPlayState.Pause)
			return;
		
		UpdateAnimateTex();
	}
	
	public void UpdateAnimateTex()
	{
		// Calculate index
		//int index = (int)(Time.timeSinceLevelLoad * _fps) % (_uvTieX * _uvTieY);
    	if(index != _lastIndex)
		{
			// split into horizontal and vertical index
			int uIndex = index % _uvTieX;
			int vIndex = index / _uvTieX;
 
			// build offset
			// v coordinate is the bottom of the image in opengl so we need to invert.
			
			Vector2 offset = new Vector2 (uIndex * _size.x , 1.0f - _size.y - vIndex * _size.y );
			//Debug.Log("animate offset " + _difOffet + "   n " + _myRenderer.sharedMaterial.mainTexture.name + "  width " + _myRenderer.sharedMaterial.mainTexture.width);
 
			
			_myRenderer.material.SetTextureOffset ("_MainTex", offset);
			_myRenderer.material.SetTextureScale ("_MainTex", _size);
 
			_lastIndex = index;
		}
		time += Time.deltaTime * _fps;
		
		
		if(wrapMode == AnimWrapMode.Once)
		{
			index = (int)time;
			if(index >= _uvTieX * _uvTieY -1)
			{
				if(gameObject.transform.parent)
					Destroy(gameObject.transform.parent.gameObject);
				else
					Destroy(gameObject);
			}
		}else if(wrapMode == AnimWrapMode.Loop)
		{
			index = (int)(time%(_uvTieX * _uvTieY));
		}else if(wrapMode == AnimWrapMode.OnceWithoutDestroy)
		{
			index = (int)time;
			if(index >= _uvTieX * _uvTieY +1)
			{
				index = 1;
				time = 1;
				playState = AnimPlayState.Pause;	
			}
		}
	}
}