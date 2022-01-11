using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EffectManager : MonoBehaviour {


	[System.Serializable]
	public class EffectResource
	{
        public string effectName;
		public int  	effectType = 1;		
		public GameObject effectPrefabs;

	}

	public static EffectManager Instance; //this script
	
	public List<EffectResource> m_effects = new List<EffectResource>();
    private Dictionary<string, EffectResource> m_effectRes = new Dictionary<string, EffectResource>();

	void Awake ()
	{
		if (Instance != null)
			throw new System.Exception( "EffectMan ager::Awake(), reuplicate instance!" );

		Instance = this;
		DontDestroyOnLoad(this.gameObject);

        //生成字典
        int size = m_effects.Count;
        for (int i = 0; i < size; i++)
        {
            m_effectRes[m_effects[i].effectName] = m_effects[i];
        }

        //loadAllEffect();
	}

	public GameObject GetEffectObject(string name)
	{

		if(name != "" && m_effectRes.ContainsKey(name))
		{
            return m_effectRes[name].effectPrefabs;
		}
	
		return null;

	}

	public GameObject PlayEffect(string effectName, Transform ObjTransform , bool bindEffect = false)
	{      
		GameObject effectInstacne = null;
        if (effectName != "" && m_effectRes.ContainsKey(effectName))
        {
            GameObject effectPrefabs = m_effectRes[effectName].effectPrefabs;
            if (effectPrefabs != null)
            {

                Vector3 pos = new Vector3(0, 0, 0);

                pos = ObjTransform.position;
                pos.y += 0.3f;


                //effectInstacne=  Instantiate(effectPrefabs, pos, ObjTransform.rotation) as GameObject;
                effectInstacne = effectPrefabs.Spawn(pos, ObjTransform.rotation);
                if (bindEffect)
                {
                    effectInstacne.transform.parent = ObjTransform;
                    effectInstacne.transform.position = ObjTransform.position;
                    effectInstacne.transform.rotation = ObjTransform.rotation;
                }

            }

        }
		return effectInstacne;
	}

    public GameObject PlayEffect(string effectName, Vector3 position)
    {
        GameObject effectInstacne = null;
        if (effectName != "" && m_effectRes.ContainsKey(effectName))
        {
            GameObject effectPrefabs = m_effectRes[effectName].effectPrefabs;
            if (effectPrefabs != null)
            {
                effectInstacne = effectPrefabs.Spawn(position);
            }

        }
        return effectInstacne;
    }


    public GameObject PlayEffect(string effectName, Transform transform, float dis)
	{
		GameObject effectInstacne = null;
        if (effectName != "" && m_effectRes.ContainsKey(effectName))
        {
            GameObject effectPrefabs = m_effectRes[effectName].effectPrefabs;
            if (effectPrefabs != null)
            {
                Vector3 pos = new Vector3();
                pos = transform.position + transform.forward.normalized * dis;
                pos.y += 0.3f;
                //effectInstacne = Instantiate(effectPrefabs, pos, transform.rotation) as GameObject;
                effectInstacne = effectPrefabs.Spawn(pos, transform.rotation);
            }
        }
		return effectInstacne;
	}


    //public GameObject PlayEffect(string effectName, Transform transform, Vector3 endPosition)
    //{
    //    GameObject effectInstacne = null;
    //    if (effectName != "" && m_effectRes.ContainsKey(effectName))
    //    {
    //        GameObject effectPrefabs = m_effectRes[effectName].effectPrefabs;
    //        if (effectPrefabs != null)
    //        {
    //            Vector3 pos = new Vector3();
    //            pos = transform.position;
    //            pos.y += 0.3f;
    //            //effectInstacne = Instantiate(effectPrefabs, pos, transform.rotation) as GameObject;
    //            effectInstacne = effectPrefabs.Spawn(pos, transform.rotation);
    //            ParticleTrack _effectTrack = effectInstacne.GetComponent<ParticleTrack>();
    //            if (_effectTrack)
    //            {
    //                Vector3 dir = endPosition - effectInstacne.transform.position;
    //                Vector3.Normalize(dir);
    //                Vector3 moveDirection = dir;
    //                effectInstacne.transform.localRotation = Quaternion.LookRotation(moveDirection);
    //                _effectTrack.SetPosition(effectInstacne.transform, endPosition);
    //            }
    //        }
    //    }
    //    return effectInstacne;
    //}

    //public void setEffectSpeed(GameObject effectObj, float speedRatio)
    //{
    //    DestroyEffectOnTime destorTime = effectObj.GetComponent<DestroyEffectOnTime>();
    //    if(destorTime)
    //    {
    //        float fTime = destorTime.time;
    //        destorTime.time = fTime * (1.0f/speedRatio);
    //    }

    //    Transform[] transforms = effectObj.GetComponentsInChildren<Transform>();			
    //    for (int i = 0, imax = transforms.Length; i < imax; ++i) 
    //    {
    //        Transform t = transforms[i];
    //        Animator animator =  t.gameObject.GetComponent<Animator>();
    //        if(animator)
    //        {
    //            float fSpeed = animator.speed;
    //            animator.speed = fSpeed * speedRatio;
    //        }
			
    //        ParticleSystem particle = t.gameObject.GetComponent<ParticleSystem>();
    //        if(particle)
    //        {
    //            float startSpeed = particle.startSpeed;
    //            particle.startSpeed = startSpeed * speedRatio;
    //            particle.startLifetime = particle.startLifetime * (1.0f/speedRatio);
    //            particle.startDelay = particle.startDelay * (1.0f/speedRatio);
    //        }
    //    }
    //}

    //初始化对象池，使其贴图可以初始化进内存
    public void loadAllEffect()
    {
        //不用foreach,据说这个foreach效率比较低，未测试
        int size = m_effects.Count;
        for (int i = 0; i < size; i++)
        {
            if (m_effects[i].effectPrefabs != null)
            {
                ObjectPool.CreatePool(m_effects[i].effectPrefabs, 1);
            }
            else
            {
                Debug.LogError("The EffectManager effect: " + m_effects[i].effectName + " prefab is missing");
            }
        }
    }
}
