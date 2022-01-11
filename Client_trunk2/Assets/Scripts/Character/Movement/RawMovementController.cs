using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 不使用任何组件的移动，直接改变位置，通常用于测试某些功能
/// </summary>
[AddComponentMenu("Character/MovementController/Raw")]
public class RawMovementController : MovementController {
	Animator m_animator;

	// Use this for initialization
	protected override void Start () {
		base.Start();
		m_animator = GetComponent<Animator>();
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
			m_myTransform.Translate( m_sprintParam.dir * m_sprintParam.speed * Time.deltaTime, Space.World );

			yield return new WaitForFixedUpdate();
			m_sprintParam.time -= Time.deltaTime;
			if (m_shouldBeStopSprint)
				break;
		}
		// 冲刺完成或中断都必须重置参数
		m_isSprinting = false;
		m_shouldBeStopSprint = false;
	}
	
	IEnumerator _MovingMonitor()
	{
		m_movingParam.movingMonitorRuning = true;
		while (m_movingParam.moving)
		{
			if ( m_movingParam.faceMovement &&
			    Angle( m_movingParam.position ) >= m_angleForSyncDir )
			{
				LookAt(m_movingParam.position);
			}
			else if ( m_movingParam.direction != Vector3.zero &&
			         Angle( m_movingParam.direction ) >= m_angleForSyncDir )
			{
				// 移动目标与当前朝向相关一定角度时才校正朝向，以避免由于频繁的转向带来的抖动
				LookAt(m_movingParam.direction);
			}
			
			// change position direct.
			Vector3 moveDir = m_movingParam.position - m_myTransform.position;
			Vector3 moveTo = moveDir.normalized * m_movingParam.speed * Time.deltaTime;
			if (moveDir.magnitude > moveTo.magnitude)
			{
				m_myTransform.Translate( moveTo, Space.World );
			}
			else
			{
				m_myTransform.position = m_movingParam.position;
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
		m_animator.SetBool("run", true);
		if (!m_movingParam.movingMonitorRuning)
		{
			StartCoroutine( _MovingMonitor() );
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
			// change position direct.
			m_myTransform.Translate( m_myTransform.forward * m_movingForwardParam.speed * Time.deltaTime, Space.World );

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
			StartCoroutine( _MoveForwardMonitor() );
		}
	}

}
