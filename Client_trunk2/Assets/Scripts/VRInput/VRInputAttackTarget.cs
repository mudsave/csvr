using System.Collections;
using UnityEngine;

public class VRInputAttackTarget : MonoBehaviour
{
    public static VRInputAttackTarget left { get; private set; }
    public static VRInputAttackTarget right { get; private set; }

    public Vector3 targetPoint { get; private set; }
    public Vector3 targetDirection { get; private set; }
    public Transform nib { get; private set; }
    public int entityID { get; private set; }   //-1表示没有
    public Transform entity { get; private set; }

    [Tooltip("射线检查层")]
    public LayerMask firstRaycastLayer;

    [Tooltip("射线检查层")]
    public LayerMask secondRaycastLayer;

    private Transform head = null;
    public Hand controllerHand = Hand.RIGHT;

    private bool updating = false;

    private GameObject testBall = null;
    private Color testBallColor = Color.black;

    private void Start()
    {
        head = VRInputManager.Instance.head.transform;
        if (VRInputDefined.isHMDConnected)
        {
            if (controllerHand == Hand.LEFT)
                nib = VRInputManager.Instance.tip_nib_left;
            else
                nib = VRInputManager.Instance.tip_nib_right;
        }
        else
        {
            GameObject go = new GameObject("tmpAttackTargetNib");
            nib = go.transform;
            nib.parent = head;
            nib.position = head.position + head.forward;
        }
        if (controllerHand == Hand.LEFT) left = this;
        else right = this;
    }

    private void Update()
    {
        if (updating)
            UpdateAttactPoint();
    }

    public void UpdateAttactPoint()
    {
        Transform tmpEntity = null;

        RaycastHit hitInfo;
        Vector3 fwd = nib.position - head.transform.position;
        targetDirection = fwd.normalized;
        if (Physics.Raycast(nib.position, fwd, out hitInfo, 30, firstRaycastLayer))
        {
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Entity"))
            {
                Debug.DrawLine(nib.position, hitInfo.point, Color.blue);
                Debug.DrawLine(hitInfo.point, hitInfo.transform.position, Color.blue);
                tmpEntity = hitInfo.transform;
                targetPoint = hitInfo.transform.position;
                testBallColor = Color.blue;
            }
            else
            {
                Debug.DrawLine(nib.position, hitInfo.point, Color.green);
                targetPoint = hitInfo.point;
                testBallColor = Color.green;
            }
        }
        else
        {
            Vector3 end = nib.position + fwd.normalized * 10;
            RaycastHit hit;
            Vector3 f = Vector3.down;
            if (Physics.Raycast(end, f, out hit, 30, secondRaycastLayer))
            {
                Debug.DrawLine(nib.position, end, Color.yellow);
                Debug.DrawLine(end, hit.point, Color.yellow);
                targetPoint = hit.point;
                testBallColor = Color.yellow;
            }
            else
            {
                Debug.DrawLine(nib.position, end, Color.red);
                targetPoint = end;
                testBallColor = Color.red;
            }
        }

        if (testBall != null)
        {
            testBall.transform.position = targetPoint;
            testBall.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_MainTint", testBallColor);
        }

        if (entity != tmpEntity)
        {
            OnEntityChanged(tmpEntity);
        }
    }

    private void OnEntityChanged(Transform _entity)
    {
        if (_entity != null)
        {
            //选中entity
            //Debug.Log("选中：" + entity.name);
            if (VRInputManager.Instance.head.GetComponent<HighlightingSystem.HighlightingRenderer>() == null)
            {
                VRInputManager.Instance.head.AddComponent<HighlightingSystem.HighlightingRenderer>();
            }
            HighlightingSystem.Highlighter lighter = _entity.GetComponent<HighlightingSystem.Highlighter>();
            if (lighter == null)
                lighter = _entity.gameObject.AddComponent<HighlightingSystem.Highlighter>();
            lighter.ConstantOn();
        }
        if (entity != null)
        {
            //取消oldEntity选中
            //Debug.Log("取消选中：" + oldEntity.name);
            HighlightingSystem.Highlighter lighter = entity.GetComponent<HighlightingSystem.Highlighter>();
            if (lighter != null)
            {
                lighter.ConstantOff();
            }
        }
        entity = _entity;

        //获取entityID
        //if (entity != null)
        //{
        //    MonsterComponent monster = entity.GetComponent<MonsterComponent>();
        //    if (monster == null) { entityID = -1; }
        //    else { entityID = monster.entity.id; }
        //}
        //else { entityID = -1; }
        //Debug.Log(entityID);
    }

    public void StartUpdate()
    {
        updating = true;
        UpdateAttactPoint();
        if (testBall != null)
            testBall.SetActive(true);
    }

    public void StopUpdate()
    {
        updating = false;
        OnEntityChanged(null);
        if (testBall != null)
            testBall.SetActive(false);
    }

    public void SetTestBall(GameObject prefab)
    {
        if (testBall != null) DestroyImmediate(testBall);
        testBall = Instantiate(prefab);
        testBall.transform.parent = transform;
        testBall.transform.localPosition = Vector3.zero;
        testBall.transform.localRotation = Quaternion.identity;
        testBall.transform.localScale = Vector3.one * 5;
        Collider[] colliders = testBall.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
        testBall.SetActive(false);
    }
}