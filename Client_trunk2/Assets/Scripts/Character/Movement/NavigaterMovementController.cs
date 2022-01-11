using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 使用NavMeshAgent组件的移动（导航移动）
/// </summary>
[AddComponentMenu("Character/MovementController/Navigater")]
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class NavigaterMovementController : MovementController
{
    Animator m_animator;
    UnityEngine.AI.NavMeshAgent m_navMeshAgent;
    public string stateName = "run";
    private CapsuleCollider m_capCollide = null;
    Vector3 moveDir = Vector3.zero;
    float distance = 0.0f;
    static int s_raycastIgnore;


    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        m_animator = GetComponentInChildren<Animator>();
        m_navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        m_capCollide = GetComponent<CapsuleCollider>();
        s_raycastIgnore = 1 << LayerMask.NameToLayer("Stop");
    }

    void OnDisable()
    {
        m_movingParam.movingMonitorRuning = false;
        m_movingForwardParam.movingMonitorRuning = false;
    }

    /* 冲刺功能的内部实现，
     * 同一时间仅会存在一个冲刺功能，后来者会把前面的覆盖。
     */
    protected override IEnumerator _Sprint()
    {
        m_isSprinting = true;
        while (m_sprintParam.time > 0)
        {
            m_myTransform.Translate(m_sprintParam.dir * m_sprintParam.speed * Time.deltaTime, Space.World);

            yield return new WaitForFixedUpdate();
            m_sprintParam.time -= Time.deltaTime;
            if (m_shouldBeStopSprint)
                break;
        }
        // 冲刺完成或中断都必须重置参数
        m_isSprinting = false;
        m_shouldBeStopSprint = false;
    }

    //检测碰撞
    void CheckCollide(Vector3 dir, float distance)
    {
        RaycastHit hit;
        Vector3 pos = m_myTransform.position;
        pos.y += m_capCollide.height * 0.5f;  // 把位置向半中腰移动

        //加0.2f为了提前检测更加精确
        if (Physics.Raycast(pos, dir, out hit, m_capCollide.radius + 0.2f, s_raycastIgnore))
        {
            Quaternion q = Quaternion.FromToRotation(Vector3.right, hit.normal);
            Vector3 forword = q * Vector3.forward;
            forword.Normalize();

            float angle = 180.0f - Vector3.Angle(dir, hit.normal);
            float dis = distance * Mathf.Sin(angle * Mathf.Deg2Rad);
            float tempDir = Vector3.Cross(dir, hit.normal).y;
            if (tempDir >= 0)
            {
                m_navMeshAgent.destination = m_navMeshAgent.transform.position + dis * forword;               
            }
            else
            {
                m_navMeshAgent.destination = m_navMeshAgent.transform.position + dis * (-forword);
            }

            //做二次检验
            Vector3 pos1 = m_navMeshAgent.steeringTarget;
            pos1.y += m_capCollide.height * 0.5f;

            RaycastHit hit0;
            if (Physics.Raycast(pos1, -hit.normal, out hit0, m_capCollide.radius + 0.2f, s_raycastIgnore))
            {
                if (hit0.distance < m_capCollide.radius)
                {
                    m_navMeshAgent.destination = m_navMeshAgent.destination + hit.normal * (m_capCollide.radius - hit0.distance);
                }
            }

            //检测目标路径上是否有碰撞，如果有则停止移动
            RaycastHit hit1;
            Vector3 vDir = m_navMeshAgent.steeringTarget - m_navMeshAgent.transform.position;
            if (Physics.Raycast(pos, vDir.normalized, out hit1, m_capCollide.radius + 0.2f, s_raycastIgnore))
            {
                StopMove();
            }
        }
    }

    IEnumerator _MovingMonitor()
    {  
        m_movingParam.movingMonitorRuning = true;
        while (m_movingParam.moving && m_navMeshAgent.enabled)
        {
            if (m_isMoveByFinger) // 只有玩家控制角色移动的时候做碰撞检测
            {
                if (m_capCollide)
                {
                    Vector3 vecPos1 = m_navMeshAgent.destination;
                    vecPos1.y = 0.0f;
                    Vector3 vecPos2 = m_myTransform.position;
                    vecPos2.y = 0.0f;

                    moveDir = vecPos1 - vecPos2;
                    distance = Vector3.Distance(vecPos1, vecPos2);
                    CheckCollide(moveDir.normalized, distance);
                }        
            }
               
            if (!m_movingParam.faceMovement && m_movingParam.direction != Vector3.zero &&
                     Angle(m_movingParam.direction) > m_angleForSyncDir)
            {
                // 移动目标与当前朝向相关一定角度时才校正朝向，以避免由于频繁的转向带来的抖动
                LookAt(m_movingParam.direction);
            }

            if (m_navMeshAgent.remainingDistance <= m_navMeshAgent.stoppingDistance && !m_navMeshAgent.pathPending)
            {
                StopMove();
                break;
            }

            yield return new WaitForFixedUpdate();
        }

        // end of monitoring

        m_movingParam.movingMonitorRuning = false;
        yield break;
    }

    protected override void StartMoveToPosition()
    {
        m_navMeshAgent.ResetPath();
        m_navMeshAgent.speed = m_movingParam.speed;
        m_navMeshAgent.destination = m_movingParam.position;
        m_navMeshAgent.stoppingDistance = m_movingParam.stoppingDistance;
        m_navMeshAgent.updateRotation = m_movingParam.faceMovement;       
        m_animator.SetBool(stateName, true);
        if (!m_movingParam.movingMonitorRuning)
        {
            StartCoroutine(_MovingMonitor());
        }
    }

    protected override void OnStopMove()
    {
        bool isMoving = m_movingParam.moving || m_movingForwardParam.moving;

        if (isMoving)
        { 
            m_navMeshAgent.Stop();
            m_animator.SetBool(stateName, false);
        }      
    }

    IEnumerator _MoveForwardMonitor()
    {
        m_movingForwardParam.movingMonitorRuning = true;
        while (m_movingForwardParam.moving)
        {
            //m_navMeshAgent.destination = m_myTransform.position + m_myTransform.forward;
            m_navMeshAgent.SetDestination(m_myTransform.position + m_myTransform.forward);
            if (m_isMoveByFinger) // 只有玩家控制角色移动的时候做碰撞检测
            {
                if (m_capCollide)
                {
                    Vector3 vecPos1 = m_navMeshAgent.destination;
                    vecPos1.y = 0.0f;
                    Vector3 vecPos2 = m_myTransform.position;
                    vecPos2.y = 0.0f;
                    moveDir = vecPos1 - vecPos2;
                    distance = Vector3.Distance(vecPos1, vecPos2);
                    CheckCollide(moveDir.normalized, distance);
                }
            }

            yield return new WaitForFixedUpdate();
        }

        // end of monitoring
        StopMove();
        m_movingForwardParam.movingMonitorRuning = false;
        yield break;
    }

    protected override void StartMoveForward()
    {
        m_navMeshAgent.Resume();
        m_navMeshAgent.stoppingDistance = 0.1f;
        m_navMeshAgent.speed = m_movingForwardParam.speed;

        //m_animator.SetBool(stateName, true);
        if (!m_movingForwardParam.movingMonitorRuning)
        {
            StartCoroutine(_MoveForwardMonitor());
        }
    }

}
