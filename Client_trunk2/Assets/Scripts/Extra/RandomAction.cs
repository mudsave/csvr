using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAction : MonoBehaviour {

    private Animator animator;
    private float triggerTime=0.0f;        //触发时间
    public float timeRate = 15f;           //触发频率
    public float playProbability = 0.2f;   //随机动作播放触发概率
    public int[] actions;

    // Use this for initialization
    void Start ()
    {
        animator = gameObject.GetComponent<Animator>();
    }

	// Update is called once per frame
	void Update ()
    {
        AnimatorStateInfo idleInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (idleInfo.IsName("Base Layer.idle"))
        {
            triggerTime += Time.deltaTime;
            if (triggerTime >= timeRate)
            {
                triggerTime = 0.0f;
                float probability = Random.value;              
                if(probability<playProbability)
                {
                   int checkedAction = actions[Random.Range(0, actions.Length)];           
                   animator.SetInteger("randomAction",checkedAction);
                 }
            }
        }
        else
        {
            animator.SetInteger("randomAction", 0);
        }
    }
}
    
