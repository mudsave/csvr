using UnityEngine;
using System.Collections;

public class BaseModelEffect{

	public ModelEffectMgr _mgr;
    public GameObjComponent m_owmerObj;
	
	public bool isPlaying = false;

	public void Init (ModelEffectMgr mgr) 
	{
		_mgr = mgr;
		
	}

    public void SetOwner(GameObjComponent obj)
	{
		m_owmerObj = obj;
	}
	

	// Update is called once per frame
	virtual public void Tick ()
	{
		
	}
	
	virtual public void StartEffect()
	{
	}

    //virtual public void StartColorChangeEffect(float fTime, Vector4 colorValue)
    //{
    //}
	
	virtual public void EndEffect()
	{
		
	}
}
