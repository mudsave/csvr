using UnityEngine;
using System.Collections;

[AddComponentMenu( "Scripts/Effect/Camera Shake" )]
public class CameraShake : MonoBehaviour {

	public enum shakeType
	{
		ST_Horizon, //横向
		ST_Vertical, //纵向
		ST_Zoom, //纵深
		ST_None
	}
    public Camera camera;
	public shakeType cameraShakeType = shakeType.ST_None;
	public float shakeStrength;//振幅
	public float shakeProportion;//最大振幅偏移系数(0~1)
	public float shakeTime;//震动时间
	public float speed;//速度
    public float delayTime;
	private bool canShake = true;
	private float durationTime;
	private float _time;
	private Vector3 vcOffset = new Vector3(0.0f, 0.0f, 0.0f);

	// Use this for initialization
	void Start () {

        _time = Time.time;
	}
	
	// Update is called once per frame
	void Update () {	

		if(canShake)
		{
            float currentTime = Time.time - _time;
            if (currentTime > delayTime)
            {
                durationTime = Time.time - _time-delayTime;
                if (durationTime < shakeTime)
                {
                    camera.transform.position -= vcOffset;
                    if (cameraShakeType == shakeType.ST_Horizon)
                    {
                        vcOffset = transform.right * GetWaveValue();
                    }
                    else if (cameraShakeType == shakeType.ST_Vertical)
                    {
                        vcOffset = transform.up * GetWaveValue();
                    }
                    else if (cameraShakeType == shakeType.ST_Zoom)
                    {
                        vcOffset = transform.forward * GetWaveValue();
                    }
                    camera.transform.position += vcOffset;
                }
                else
                {
                    canShake = false;
                }
            }
		}
	}

	float GetWaveValue()
	{
		if(shakeProportion > 1.0f)
		{
			shakeProportion = 1.0f;
		}
		else if(shakeProportion < 0)
		{
			shakeProportion = 0;
		}
		float fCurAmp = 0;
		float fT1 = shakeTime * shakeProportion;
		float fT2 = shakeTime - fT1;
		if(durationTime <= fT1 && fT1 > 0)
		{
			fCurAmp = shakeStrength / fT1 * durationTime;
		}
		else
		{
			if(fT2 > 0)
			{
				fCurAmp = shakeStrength / fT2 * (shakeTime - durationTime);
			}
		}		
		float fS = fCurAmp * Mathf.Sin(speed * durationTime);
		
		return fS;
	}

	public void setStartShake(bool flag)
	{
		canShake = flag;
		_time = Time.time;
	}
}
