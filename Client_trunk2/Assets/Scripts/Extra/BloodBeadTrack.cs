using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodBeadTrack : MonoBehaviour
{
    public int hpValue;
    private Rigidbody _rigidbody;

    public void Init(int hp)
    {
        //_rigidbody = gameObject.GetComponent<Rigidbody>();

        ////随机一个上抛初速度
        //Vector3 randomDir = Random.onUnitSphere;
        //_rigidbody.velocity = new Vector3(randomDir.x, Random.Range(3.5f, 5.0f), randomDir.z);

        hpValue = hp;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "RightHandCollider" || other.name == "LeftHandCollider")
        {
            VRInputManager.Instance.playerComponent.RecoveryHp(hpValue);
            VRInputManager.Instance.playerComponent.effectManager.AddEffect("Helth", other.transform);
            Destroy(gameObject);
        }
    }
}
