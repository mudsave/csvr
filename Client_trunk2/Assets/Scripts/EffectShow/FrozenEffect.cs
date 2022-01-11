using UnityEngine;
using System.Collections;

public class FrozenEffect : BaseModelEffect
{

    FrozenEffectMethod method;

    public override void StartEffect()
	{
		isPlaying = true;
        method = m_owmerObj.gameObject.AddComponent<FrozenEffectMethod>();
	}

	public override void EndEffect()
	{
		isPlaying = false;
        method.OnCompleteNull();
	}
}
