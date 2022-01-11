using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[AddComponentMenu("Character/MovementController/MovementController")]
public abstract class MovementController : MonoBehaviour {
	protected struct EntitySprintParam
	{
		public Vector3 dir;
		public float speed;
		public float time;
		
		public EntitySprintParam( Vector3 d, float s, float t )
		{
			dir = d;
			speed = s;
			time = t;
		}
		
		static EntitySprintParam m_zero = new EntitySprintParam( Vector3.zero, 0.0f, 0.0f );
		public static EntitySprintParam zero
		{
			get { return m_zero; }
		}
	}
	
	protected class EntityMovingParam
	{
		public bool moving = false;
		public bool movingMonitorRuning = false;
		public Vector3 position = Vector3.zero;  // move to
		/// <summary>
		/// face direction relative to.
		/// if direction eq Vector3.zero than not change face direction any time,
		/// if want face to target position, set direction to target position.
		/// </summary>
		public Vector3 direction = Vector3.zero;

		/// whether or not change object direction to current movement direction.
		/// It's will ignore dir param if set to true, else will use dir param to decide direction.
		public bool faceMovement = false;

		public float stoppingDistance = 0.0f;
		public float speed = 0.0f;  // move speed
		public string param = "";
	}
	
	protected class EntityMovingForwardParam
	{
		public bool movingMonitorRuning = false;
		public bool moving = false;
		public float speed = 0.0f;  // move speed
	}

	public float m_angleForSyncDir = 5.0f;  // 差距多少度时同步朝向

	protected Transform m_myTransform;

	// sprint
	protected bool m_isSprinting = false;  // 是否正在冲刺中
	protected bool m_shouldBeStopSprint = false;  // 是否要停止当前的冲刺
	protected EntitySprintParam m_sprintParam = EntitySprintParam.zero;
	
	// move to position
	protected EntityMovingParam m_movingParam = new EntityMovingParam();
	
	// move with direction
	protected EntityMovingForwardParam m_movingForwardParam = new EntityMovingForwardParam();

    //是否是通过遥感或点击地面控制移动
    protected bool m_isMoveByFinger = false;

	protected virtual void Start()
	{
		m_myTransform = transform;
	}

	public bool isSprinting
	{
		get { return m_isSprinting; }
	}

	protected virtual IEnumerator _Sprint()
	{
		m_isSprinting = false;
		m_shouldBeStopSprint = false;
		yield break;
	}

	/* 冲刺功能，指示该对象朝一个方向以一定的速度冲刺一段时间
	 * @param dir: 冲刺方向
	 * @param speed: 冲刺速度
	 * @param time: 冲刺时间
	 * @param faceToDir: 是否把脸转向冲刺的方向
	 */
	public virtual void Sprint( Vector3 dir, float speed, float time, bool faceToDir )
	{
		if (faceToDir)
		{
			// 改变朝向
			// @TODO(penghuawei): add code here...
		}
		
		// 覆盖旧的冲刺参数
		m_sprintParam = new EntitySprintParam(Vector3.Normalize( dir ), speed, time);
		
		if (!m_isSprinting)
		{
			// 如果不在冲刺状态中则重新开始冲刺
			StartCoroutine( _Sprint() );
		}
	}

	public void Sprint( Vector3 dir, float speed, float time )
	{
		Sprint( dir, speed, time, false );
	}

	/* 停止冲刺
	 */
	public void StopSprint()
	{
		if (m_isSprinting)
			m_shouldBeStopSprint = true;
	}

	/// <summary>
	/// 计算自己当前朝向与另一个坐标的夹角
	/// </summary>
	/// <param name="position">Position.</param>
	public float Angle( Vector3 position )
	{
		Vector3 a = m_myTransform.forward;
		Vector3 b = position - m_myTransform.position;
		a.y = b.y = 0.0f;  // 去除高度差距（即仅旋转y轴）
		
		return Vector3.Angle( a, b );
	}

	public float AngleEx( Vector3 position )
	{
		Vector3 a = m_myTransform.forward;
		Vector3 b = position - m_myTransform.position;
		a.y = b.y = 0.0f;  // 去除高度差距（即仅旋转y轴）
		
		float angle = Vector3.Angle( a, b );
		Vector3 tempDir = Vector3.Cross(a, b); //用叉乘判断两个向量 是否同方向
		if (tempDir.y < 0)
			angle = -angle;
		return angle;
	}

	/*
     * 进行旋转到指定目标点
     */
	public void LookAt(Vector3 pos)
	{
		transform.Rotate(0.0f, AngleEx( pos ), 0.0f, Space.World);
	}

	protected virtual void StartMoveToPosition()
	{
		// I do nothing default, so set moving state to false;
		m_movingParam.moving = false;
	}

	/// <summary>
	/// Moves to position.
	/// </summary>
	/// <returns><c>true</c>, if to position was moved, <c>false</c> otherwise.</returns>
	/// <param name="dst">Dst.</param>
	/// <param name="dir">
	/// face direction relative to.
	/// if direction eq Vector3.zero than not change face direction any time,
	/// if want face to target position, set direction to target position.
	/// </param>
	/// <param name="speed">move speed</param>
	/// <param name="stoppingDistance">Stop within this distance from the target position.</param>
	/// <param name="faceMovement">
	/// whether or not change object direction to current movement direction.
	/// It's will ignore dir param if set to true, else will use dir param to decide direction.
	/// </param>
	/// <param name="userdata">call back param.</param>
    public void MoveToPosition(Vector3 dst, Vector3 dir, float speed, float stoppingDistance, bool faceMovement, string userdata, bool isMoveByFinger)
	{
		// stop move with direction if exist
		if (m_movingForwardParam.moving)
			StopMove();

		m_movingParam.position = dst;
		m_movingParam.direction = dir;
		m_movingParam.stoppingDistance = stoppingDistance;
		m_movingParam.param = userdata;
		m_movingParam.moving = true;
		m_movingParam.speed = speed;
		m_movingParam.faceMovement = faceMovement;
        m_isMoveByFinger = isMoveByFinger;
		StartMoveToPosition();
		SendMessage( "OnEntityBeginToMoveMSG", m_movingParam.param, SendMessageOptions.DontRequireReceiver );
	}

    public void MoveToPosition(Vector3 dst, Vector3 dir, float speed, bool isMoveByFinger = false)
	{
        MoveToPosition(dst, dir, speed, 0.0f, false, "", isMoveByFinger);
	}

    public void MoveToPosition(Vector3 dst, Vector3 dir, float speed, float stoppingDistance, bool isMoveByFinger = false)
	{
        MoveToPosition(dst, dir, speed, stoppingDistance, false, "", isMoveByFinger);
	}

    public void MoveToPosition(Vector3 dst, Vector3 dir, float speed, float stoppingDistance, bool faceMovement, bool isMoveByFinger)
	{
        MoveToPosition(dst, dir, speed, stoppingDistance, faceMovement, "", isMoveByFinger);
	}
	
	protected virtual void OnStopMove()
	{
		// do nothing
	}

	public void StopMove()
	{
		OnStopMove();

        bool isMoving = m_movingParam.moving || m_movingForwardParam.moving;

		if (m_movingParam.moving)
		{
			m_movingParam.moving = false;
		}
		
		if (m_movingForwardParam.moving)
		{
			m_movingForwardParam.moving = false;
		}

        if (isMoving)
        {
            SendMessage("OnEntityMoved", m_movingParam.param, SendMessageOptions.DontRequireReceiver);
        }
	}
	
	public bool moving
	{
		get { return m_movingParam.moving || m_movingForwardParam.moving; }
	}

	protected virtual void StartMoveForward()
	{
		// I do nothing default, so set moving state to false;
		m_movingForwardParam.moving = false;
	}

    protected virtual void StartMoveForward(Vector3 offsetDirection)
    {
        // I do nothing default, so set moving state to false;
        m_movingForwardParam.moving = false;
    }

	/// <summary>
	/// move forward
	/// </summary>
	/// <param name="dir">Dir.</param>
	/// <param name="speed">Speed.</param>
    public void MoveForward(float speed, bool isMoveByFinger = false)
	{
		// stop move to position if exist
		if (m_movingParam.moving)
			StopMove();
		
		m_movingForwardParam.speed = speed;
		m_movingForwardParam.moving = true;
        m_isMoveByFinger = isMoveByFinger;
		StartMoveForward();
        SendMessage("OnEntityBeginToMoveMSG", m_movingParam.param, SendMessageOptions.DontRequireReceiver);
        BroadcastMessage("WeaponAnimationPlayMessage", "run", SendMessageOptions.DontRequireReceiver);
	}

    public void MoveForward(float speed, Vector3 offsetDirection)
    {
        // stop move to position if exist
        if (m_movingParam.moving)
            StopMove();

        m_movingForwardParam.speed = speed;
        m_movingForwardParam.moving = true;
        StartMoveForward(offsetDirection);
        SendMessage("OnEntityBeginToMoveMSG", m_movingParam.param, SendMessageOptions.DontRequireReceiver);
        BroadcastMessage("WeaponAnimationPlayMessage", "run", SendMessageOptions.DontRequireReceiver);
    }

}
