using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 使用CharacterController组件的移动，自定义移动，不使用CharacterMotor_cs组件
/// </summary>
[AddComponentMenu("Character/MovementController/character motor 2")]
[RequireComponent(typeof(CharacterController))]
public class CharacterMovementController2 : MovementController
{
    /// <summary>
    /// 推力，力越大推动的速度越快
    /// 小于等于0时则无法推动任何人
    /// </summary>
    public float m_athrust = 0;

    Animator m_animator;
    CharacterController m_characterController;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        m_animator = GetComponent<Animator>();
        m_characterController = GetComponent<CharacterController>();

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
            m_characterController.Move(m_sprintParam.dir * m_sprintParam.speed * Time.deltaTime);

            yield return new WaitForFixedUpdate();
            m_sprintParam.time -= Time.deltaTime;
            if (m_shouldBeStopSprint)
                break;
        }
        // 冲刺完成或中断都必须重置参数
        m_isSprinting = false;
        m_shouldBeStopSprint = false;
    }

    /* @TODO(phw): 这个是让移动对象在侧面碰到东西时停止移动，
     * 但这样做会导致它遇到别的有碰撞体的怪物对象也会停止下来，
     * 而且碰到高度低于CharacterController.StepOffset的台阶也会停下来，
     * 这真是个让人纠结的选择……
    void OnControllerColliderHit (ControllerColliderHit hit) 
    {
        // 正面碰到碰撞体，需要停下来
        if (m_movingParam.moving && (hit.controller.collisionFlags & CollisionFlags.Sides) != 0)
        {
            StopMove();
        }
    }
    */

    IEnumerator _MovingMonitor()
    {
        m_movingParam.movingMonitorRuning = true;
        while (m_movingParam.moving)
        {
            if (m_movingParam.faceMovement &&
                Angle(m_movingParam.position) >= m_angleForSyncDir)
            {
                LookAt(m_movingParam.position);
            }
            else if (m_movingParam.direction != Vector3.zero &&
                     Angle(m_movingParam.direction) > m_angleForSyncDir)
            {
                // 移动目标与当前朝向相关一定角度时才校正朝向，以避免由于频繁的转向带来的抖动
                LookAt(m_movingParam.direction);
            }

            // change position direct.
            Vector3 moveDir = m_movingParam.position - m_myTransform.position;
            Vector3 moveTo = moveDir.normalized * m_movingParam.speed * Time.deltaTime;
            // 注：CharacterController.SkinWidth参数的大小会影响这个的距离判断，值越大，需要的冗余就越大
            if (Vector3.Distance(m_myTransform.position, m_movingParam.position) > m_movingParam.stoppingDistance)
            {

                /* @TODO(hsj):
                 * 由于AnimationController脚本已经废弃
                 * 则不会出现人物缓慢下落的问题
                 * 就已经不需要矫正下落
                 */
                //moveTo.y -= m_characterController.stepOffset;

                m_characterController.Move(moveTo);
            }
            else
            {
                //m_myTransform.position = m_movingParam.position;
                StopMove();
                break;
            }

            AthrustObject();

            yield return new WaitForFixedUpdate();
        }

        // end of monitoring

        m_movingParam.movingMonitorRuning = false;
        yield break;
    }

    protected override void StartMoveToPosition()
    {
        m_animator.SetBool("run", true);
        if (!m_movingParam.movingMonitorRuning)
        {
            StartCoroutine(_MovingMonitor());
        }
    }

    protected override void OnStopMove()
    {
        m_animator.SetBool("run", false);
    }

    IEnumerator _MoveForwardMonitor()
    {
        m_movingForwardParam.movingMonitorRuning = true;
        while (m_movingForwardParam.moving)
        {
            Vector3 moveTo = m_myTransform.forward * m_movingForwardParam.speed * Time.deltaTime;

            /* @TODO(hsj):
             * 由于AnimationController脚本已经废弃
             * 则不会出现人物缓慢下落的问题
             * 就已经不需要矫正下落
             */
            //moveTo.y -= m_characterController.stepOffset;

            m_characterController.Move(moveTo);

            AthrustObject();

            yield return new WaitForFixedUpdate();
        }

        // end of monitoring
        StopMove();
        m_movingForwardParam.movingMonitorRuning = false;
        yield break;
    }

    protected override void StartMoveForward()
    {
        m_animator.SetBool("run", true);
        if (!m_movingForwardParam.movingMonitorRuning)
        {
            StartCoroutine(_MoveForwardMonitor());
        }
    }

    /// <summary>
    /// 推挤一个对象
    /// </summary>
    /// <param name="distance">推挤距离我多远的对象（即检测我前面多远的对象）.</param>
    void AthrustObject()
    {
        if (m_athrust <= 0.0f)
            return;

        RaycastHit hit;
        Vector3 p1 = transform.position + m_characterController.center + Vector3.up * -m_characterController.height * 0.5F;
        Vector3 p2 = p1 + Vector3.up * m_characterController.height;
        // 这个0.08就是Skin Width参数，但由于CharacterController下取不到这个参数，所以只好直接写死
        // 0.05则是发现SkinWidth的2倍还不能碰到对象而加的一个偏移，实测的相对合理的结果，并没有什么算法
        float distance = 0.05f + 0.08f * 2.0f;
        bool result = Physics.CapsuleCast(p1, p2, m_characterController.radius, transform.forward, out hit, distance);
        if (result && hit.transform.tag == "Monster")
        {
            Vector3 dir = (hit.transform.position - m_myTransform.position).normalized;
            MovementController objMC = hit.transform.GetComponent<MovementController>();
            if (objMC)
                objMC.Sprint(dir, m_athrust, Time.deltaTime);
        }
    }
}