using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SwordMoveType
{
    UPDATEDIR,
    UPDATEDIR_REALTIME,
    TYPE1,
    TYPE2,
    TYPE3,
    TYPE4,
    TYPE5,
    TYPE6,
}

public class VRInputFlySwordAttack : MonoBehaviour
{
    public SwordMoveType moveType;
    public Transform sword;
    public float mixVelocity;
    public float speed;
    protected Hand controllerHand = Hand.RIGHT;
    private bool flying = false;
    private Vector3 direction = Vector3.zero;
    private float oldVelocity = 0f;

    public float rMixVelocity;

    protected bool isPressed = false;
    private Transform hand = null;
    private Transform head = null;

    protected VRInputAttackTarget attackTarget = null;
    private BoxCollider swordCollider = null;

    private void OnEnable()
    {
        GlobalEvent.register("OnTriggerPressed", this, "OnPressed");
        GlobalEvent.register("OnTriggerReleased", this, "OnReleased");
    }

    private void OnDisable()
    {
        GlobalEvent.deregister(this);
    }

    public void OnPressed(VRControllerEventArgs e)
    {
        if (e.hand == controllerHand)
        {
            isPressed = true;
        }
    }

    public void OnReleased(VRControllerEventArgs e)
    {
        if (e.hand == controllerHand)
        {
            isPressed = false;
        }
    }

    private void Start()
    {
        if (controllerHand == Hand.RIGHT)
            hand = VRInputManager.Instance.handRight.transform;
        else
            hand = VRInputManager.Instance.handLeft.transform;
        head = VRInputManager.Instance.head.transform;
        attackTarget = VRInputAttackTarget.right;
        swordCollider = sword.GetComponentInChildren<BoxCollider>();
        swordCollider.enabled = false;
    }

    private void Update()
    {
        if (moveType == SwordMoveType.UPDATEDIR)
            UpdateDir();
        else if (moveType == SwordMoveType.UPDATEDIR_REALTIME)
            RealtimeUpdateDir();
        else if (moveType == SwordMoveType.TYPE1)
            UpdateType1();
        else if (moveType == SwordMoveType.TYPE2)
            UpdateType2();
        else if (moveType == SwordMoveType.TYPE3)
            UpdateType3();
        else if (moveType == SwordMoveType.TYPE4)
            UpdateType4();
        else if (moveType == SwordMoveType.TYPE5)
            UpdateType5();
        else if (moveType == SwordMoveType.TYPE6)
            UpdateType6();
    }

    private void UpdateDir()
    {
        Vector3 velocity = VRInputManager.Instance.controlleEventsRight.GetVelocity();
        bool changed = false;
        if (velocity.magnitude > mixVelocity && velocity.magnitude > oldVelocity)
        {
            Debug.Log(velocity.magnitude);
            direction = velocity.normalized;
            changed = true;
        }
        oldVelocity = velocity.magnitude;
        float oldDis, newDis;
        oldDis = Vector3.Distance(sword.position, head.position);
        newDis = Vector3.Distance(sword.position + direction * speed, head.position);
        if ((oldDis < 8 || newDis < oldDis) && (sword.position + direction * speed).y > 0.1f)
        {
            sword.position += direction * speed;
            if (changed)
                sword.forward = direction;
        }
        else if ((sword.position + direction * speed).y > 0.1f)
        {
            //direction = Vector3.zero;
            direction = (direction.normalized + (transform.position - sword.position).normalized) * direction.magnitude;
            direction = direction.normalized;
            sword.forward = direction;
        }
        else
        {
            //direction = Vector3.zero;
            direction = (direction.normalized + Vector3.up) * direction.magnitude;
            direction = direction.normalized;
            sword.forward = direction;
        }
    }

    private void RealtimeUpdateDir()
    {
        Vector3 velocity = VRInputManager.Instance.controlleEventsRight.GetVelocity();
        bool changed = false;
        if (velocity.magnitude > rMixVelocity)
        {
            Debug.Log(velocity.magnitude);
            direction = velocity.normalized;
            changed = true;
        }
        else if (isPressed)
        {
            direction = Vector3.zero;
        }
        float oldDis, newDis;
        oldDis = Vector3.Distance(sword.position, head.position);
        newDis = Vector3.Distance(sword.position + direction * speed, head.position);
        if ((oldDis < 8 || newDis < oldDis) && (sword.position + direction * speed).y > 0.1f)
        {
            sword.position += direction * speed;
            if (changed)
                sword.forward = direction;
        }
        else if ((sword.position + direction * speed).y > 0.1f)
        {
            //direction = Vector3.zero;
            direction = (direction.normalized + (transform.position - sword.position).normalized) * direction.magnitude;
            direction = direction.normalized;
            sword.forward = direction;
        }
        else
        {
            //direction = Vector3.zero;
            direction = (direction.normalized + Vector3.up) * direction.magnitude;
            direction = direction.normalized;
            sword.forward = direction;
        }
    }

    private void UpdateType1()
    {
        sword.rotation = hand.rotation;
        sword.position = head.TransformPoint(head.InverseTransformPoint(hand.position) * 8f) + Vector3.up;
    }

    private void UpdateType2()
    {
        Vector3 velocity = VRInputManager.Instance.controlleEventsRight.GetVelocity();
        Rigidbody r = sword.GetComponent<Rigidbody>();
        //if (velocity.magnitude > mixVelocity)
        //{
        //    r.AddForce(velocity.normalized * 20f, ForceMode.Acceleration);
        //}

        ////if (r.velocity.magnitude > 3f)
        ////{
        ////    r.AddForce(Vector3.zero);
        ////    r.velocity = r.velocity.normalized * 3f;
        ////}
        //sword.forward = r.velocity;
        //Debug.Log(r.velocity.magnitude);
        if (Input.GetKey(KeyCode.UpArrow))
        {
            r.AddForce(Vector3.forward * 5f);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            r.AddForce(-Vector3.forward * 5f);
        }
        Debug.Log(r.velocity.magnitude);
    }

    public LineRenderer line;
    public LineRenderer lineNew;
    private List<Vector3> posList = new List<Vector3>();
    private bool newList = true;
    private List<Vector3> posListNew = new List<Vector3>();
    private int index = 0;
    private float t = 0;

    private void UpdateType3()
    {
        Vector3 velocity = VRInputManager.Instance.controlleEventsRight.GetVelocity();
        if (velocity.magnitude > mixVelocity)
        {
            if (!newList) posList.Clear();
            newList = true;
            posList.Add(hand.position);
        }
        else if (newList)
        {
            newList = false;
            posListNew.Clear();
            foreach (Vector3 v in posList)
            {
                posListNew.Add((v - posList[0]) * 10f + sword.position);
            }
            lineNew.SetVertexCount(posListNew.Count);
            lineNew.SetPositions(posListNew.ToArray());
            flying = true;
            index = 0;
        }
        line.SetVertexCount(posList.Count);
        line.SetPositions(posList.ToArray());

        if (flying)
        {
            if (index < posListNew.Count - 1)
            {
                if (Vector3.Distance(posListNew[index + 1], sword.position) < 0.01f)
                {
                    sword.position = posListNew[index + 1];
                    index++;
                    t = 0;
                }
                else
                {
                    Vector3 p = Vector3.Lerp(posListNew[index], posListNew[index + 1], t += 0.1f);

                    sword.forward = posListNew[index + 1] - sword.position;
                    sword.position = p;
                }
            }
            else
            {
                flying = false;
            }
        }
        else
        {
            Vector3 p = Vector3.Lerp(sword.position, head.position + head.forward * 2 + head.up, 0.2f);
            sword.position = p;
            sword.forward = head.forward;
        }
    }

    private void UpdateType4()
    {
        Vector3 velocity = VRInputManager.Instance.controlleEventsRight.GetVelocity();
        bool changed = false;
        //if (velocity.magnitude > mixVelocity && velocity.magnitude > oldVelocity)
        if (velocity.magnitude > mixVelocity)
        {
            if (isPressed)
            {
                if (attackTarget != null)
                    attackTarget.UpdateAttactPoint();
                if (attackTarget.entity != null)
                {
                    // if (Vector3.Distance(attackTarget.entity.position, sword.position) > 5)
                    {
                        direction = (attackTarget.entity.position - sword.position).normalized * 3f;

                        changed = true;
                    }
                }
            }
            else
            {
                direction = velocity.normalized;
                changed = true;
            }
        }
        oldVelocity = velocity.magnitude;
        float oldDis, newDis;
        oldDis = Vector3.Distance(sword.position, head.position);
        newDis = Vector3.Distance(sword.position + direction * speed, head.position);
        if ((oldDis < 15 || newDis < oldDis) && (sword.position + direction * speed).y > 0.1f)
        {
            sword.position += direction * speed;
            if (changed)
                sword.forward = direction;
        }
        else
        {
            direction = Vector3.zero;
        }
    }

    private List<Vector3> path5 = new List<Vector3>();
    private float speed5 = 5f;
    private Vector3 dir5;
    private float tmpDis5;
    public LayerMask firstRaycastLayer;
    private bool isAttack = false;
    private Transform target = null;

    private void UpdateType5()
    {
        if (isPressed)
        {
            Vector3 velocity = VRInputManager.Instance.controlleEventsRight.GetVelocity();
            if (velocity.magnitude > mixVelocity)
            {
                path5.Add(hand.forward * 5f);
                lineNew.SetVertexCount(path5.Count);
                lineNew.SetPositions(path5.ToArray());
            }
            if (!isAttack)
            {
                //射线检测
                RaycastHit hitInfo;
                Vector3 fwd = hand.forward;
                if (Physics.Raycast(hand.position, fwd, out hitInfo, 30, firstRaycastLayer))
                {
                    Debug.DrawLine(hand.position, hitInfo.point, Color.blue);
                    if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Entity"))
                    {
                        isAttack = true;
                        target = hitInfo.transform;
                    }
                }
            }
        }
        else
        {
            path5.Clear();
            lineNew.SetVertexCount(0);
        }

        if (isAttack)
        {
            if (target != null)
            {
                if (Vector3.Distance(target.position, sword.position) > 1f)
                    direction = (target.position - sword.position).normalized;
                else target = null;

                float oldDis, newDis;
                oldDis = Vector3.Distance(sword.position, head.position);
                newDis = Vector3.Distance(sword.position + direction * speed, head.position);
                if ((oldDis < 15 || newDis < oldDis) && (sword.position + direction * speed).y > 0.1f)
                {
                    sword.position += direction * speed;
                    sword.forward = direction;
                    //Debug.LogError(sword.position);
                }
                else
                {
                    direction = Vector3.zero;
                }
            }
            else
            {
                isAttack = false;
            }
        }
        else
        {
            if (path5.Count > 0)
            {
                dir5 = path5[0] - sword.position;
                tmpDis5 = Vector3.Distance(path5[0], sword.position);
                float newDis5 = Vector3.Distance(path5[0], sword.position + dir5.normalized * speed5 * Time.deltaTime);
                if (tmpDis5 < newDis5)
                {
                    sword.position = path5[0];
                    path5.RemoveAt(0);
                }
                else
                {
                    sword.position += dir5.normalized * speed5 * Time.deltaTime;
                    sword.forward = dir5;
                }
            }
        }
    }

    private Vector3 targetPointEnd;
    public Transform swordOrigin;
    private bool ding = false;
    private bool hui = false;

    private void UpdateType6()
    {
        Vector3 velocity = VRInputManager.Instance.controlleEventsRight.GetVelocity();
        if (velocity.magnitude > mixVelocity)
        {
            hui = true;
        }

        if (!ding)
        {
            if (isPressed)
            {
                if (!isAttack)
                {
                    Vector3 originPoint = sword.position;
                    Vector3 targetPoint = hand.forward * 5f + hand.position;

                    lineNew.SetVertexCount(2);
                    lineNew.SetPosition(0, hand.position);
                    lineNew.SetPosition(1, targetPoint);

                    sword.position = Vector3.Lerp(originPoint, targetPoint, Time.deltaTime * 3);

                    if (Vector3.Distance(originPoint, sword.position) > 0.02f)
                    {
                        sword.LookAt(targetPoint);
                    }

                    //射线检测
                    RaycastHit hitInfo;
                    Vector3 fwd = hand.forward;
                    if (Physics.Raycast(hand.position, fwd, out hitInfo, 30, firstRaycastLayer))
                    {
                        Debug.DrawLine(hand.position, hitInfo.point, Color.blue);
                        if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Entity"))
                        {
                            isAttack = true;
                            target = hitInfo.transform;
                        }
                    }
                }
            }
            else if (!isAttack)
            {
                Debug.Log("Attack end");
                target = null;
                isAttack = false;
                flying = false;
                if (Vector3.Distance(sword.position, swordOrigin.position) > 0.1f)
                {
                    sword.position = Vector3.Lerp(sword.position, swordOrigin.position, Time.deltaTime * 10f);
                    sword.LookAt(swordOrigin.position);
                }
                else
                {
                    sword.position = swordOrigin.position;
                    sword.rotation = swordOrigin.rotation;
                }
            }

            if (isAttack)
            {
                if (target != null)
                {
                    Vector3 currentDir = (target.position - sword.position).normalized;
                    Vector3 offset = currentDir * 5f;
                    Vector3 newPosition = Vector3.Lerp(sword.position, target.position + offset, Time.deltaTime * 8);
                    Vector3 newDir = (target.position - newPosition).normalized;

                    //没到达目标点
                    if (Vector3.Dot(currentDir, newDir) > 0 && !flying)
                    {
                        swordCollider.enabled = true;
                        sword.position = newPosition;
                        sword.LookAt(target.position);
                        targetPointEnd = target.position + offset;
                        flying = false;
                        hui = false;
                    }
                    else
                    {
                        swordCollider.enabled = false;
                        flying = true;
                    }
                    //到达目标点开始自由飞
                    if (flying)
                    {
                        if (Vector3.Distance(sword.position, targetPointEnd) > 0.1f)
                        {
                            sword.position = Vector3.Lerp(sword.position, targetPointEnd, Time.deltaTime * 3);
                            sword.LookAt(targetPointEnd);
                        }
                        //结束攻击
                        else
                        {
                            Debug.Log("Attack end");
                            target = null;
                            isAttack = false;
                            flying = false;
                            ding = true;
                        }
                    }
                }
                else
                {
                    isAttack = false;
                }
            }
        }
        else
        {
            if (hui)
                ding = false;
        }
    }
}