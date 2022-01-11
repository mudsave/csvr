using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魔法晶石掉落轨迹
/// </summary>
public class MagicSparTrack : MonoBehaviour {


    private float _InitialSpeed = 2;      //晶石掉落时的初速度
    private float _speed = 5.0f;          //晶石被吸收时的初速度
    private float _acceleration = 1;    //晶石被吸收时的加速度
    private float _targetDistance;        //目标的距离                
    private bool _move = true;            //飞行标记
    private EffectComponent _eComponent;  //晶石特效管理组件
    private Rigidbody _rigidbody;

    private Vector3 _dropPos;
    private float _shakeTime;              //抖动时长
    private float _shakeAmount = 1.2f;     //抖动振幅

    private bool isAbsorb = false;

    public void Init()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _eComponent = gameObject.GetComponent<EffectComponent>();

        //随机一个上抛初速度
        Vector3 randomDir = Random.onUnitSphere;
        _rigidbody.velocity = new Vector3(randomDir.x, Random.Range(3.5f, 5.0f), randomDir.z);

        _shakeTime = Random.Range(0.5f,1.0f);

        StartCoroutine(DelayAbsorb());
    }

    IEnumerator Absorb()
    {
        isAbsorb = true;
        //预留时间做一个掉落的表现
        yield return new WaitForSeconds(1.0f);
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.useGravity = false;

        //抖动表现
        yield return new WaitForSeconds(0.5f);
        _dropPos = transform.position;
        while (_shakeTime > 0)
        {
            transform.position = _dropPos + Random.onUnitSphere * Time.deltaTime * _shakeAmount;
            _shakeTime -= Time.deltaTime;

            //等下一帧继续进行
            yield return null;
        }
        
        //晶石被吸收时抛物线飞行
        while (_move)
        {
            //Vector3 targetPos = VRInputManager.Instance.handLeft.transform.position;
            //_targetDistance = Vector3.Distance(transform.position, VRInputManager.Instance.handLeft.transform.position);
            //transform.LookAt(targetPos);
            //float angle = Mathf.Min(1, Vector3.Distance(transform.position, targetPos) / _targetDistance) * -15;
            //transform.rotation = transform.rotation * Quaternion.Euler(Mathf.Clamp(-angle, -45, 45), 0, 0);
            transform.position = Vector3.MoveTowards(transform.position, VRInputManager.Instance.handRight.transform.position, Time.deltaTime * 20);

            float currentDist = Vector3.Distance(transform.position, VRInputManager.Instance.handRight.transform.position);

            if (currentDist < 0.5f)
            {
                _move = false;
                _eComponent.DestroyEffect();
                VRInputManager.Instance.playerComponent.MagicSparCountChange(1);
            }
            //transform.Translate(Vector3.forward * Mathf.Min(_speed * Time.deltaTime, currentDist));
            //_speed += _acceleration * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator DelayAbsorb()
    {
        yield return new WaitForSeconds(3f);
        if (!isAbsorb)
        {
            StartCoroutine(Absorb());
        }
    }

    void OnCollisionEnter(Collision other)
    {
        //晶石触碰到地板就准备被吸收
        if (other.gameObject.layer == (int)eLayers.Diban && !isAbsorb)
        {
            StartCoroutine(Absorb());
        }
    }
}
