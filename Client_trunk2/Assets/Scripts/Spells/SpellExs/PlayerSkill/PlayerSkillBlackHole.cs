using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPELL
{
    /// <summary>
    /// 次元门技能
    /// </summary>
    public class PlayerSkillBlackHole : PlayerSkillBase
    {
        [Header("技能表现模块")]
        [Tooltip("次元门 [ 特效名 ]")]
        public string blackHoleEffect = "blackhole";

        [Tooltip("画符绘制 [ 预制体 ]")]
        public GameObject paintPrefab = null;

        [Tooltip("绘制指引 [ 预制体 ]")]
        public GameObject guidePrefab = null;

        [Header("技能战斗相关模块")]
        [Tooltip("目标 [ 关系 ]")]
        public eTargetRelationship[] relation;

        [Tooltip("吸收范围 [ 半径 ]")]
        public float absorbRadius = 8.0f;

        [Tooltip("吸收持续 [ 时间 ]")]
        public float absorbLastTime = 8.0f;

        [Tooltip("单次吸收表现 [ 时间 ]")]
        public float absorbTime = 3.0f;

        [Tooltip("时间缩放 [ 倍率 ]")]
        public float timeScale = 0.1f;

        [Tooltip("技能 [ 效果ID ]")]
        public int[] triggerEffectsID;

        private SpellEffect[] triggerEffects;
        private List<AvatarComponent> attackTargets;
        private GameObject _paintEffectObj = null;
        private Transform tip_nib;
        private PaintPath paintPath = null;
        private bool guideFlag = true;
        private bool paintCompletedFlag = false;

        private GameObject guideGameObject = null;

        public override void Init()
        {
            base.Init();

            //if (castHand == Hand.LEFT)
            //{
            //    tip_nib = VRInputManager.Instance.tip_nib_left;
            //}
            //else
            //{
            //    tip_nib = VRInputManager.Instance.tip_nib_right;
            //}

            var _triggerEffects = new List<SpellEffect>();

            foreach (int id in triggerEffectsID)
            {
                _triggerEffects.Add(SpellLoader.instance.GetEffect(id));
            }

            triggerEffects = _triggerEffects.ToArray();

            attackTargets = new List<AvatarComponent>();
        }

        protected override void FireStart()
        {
            base.FireStart();

            _paintEffectObj = Instantiate(paintPrefab) as GameObject;
            Vector3 pos = player.transform.position + player.transform.forward * 0.5f;
            pos.y += 1.6f;
            _paintEffectObj.transform.position = pos;
            _paintEffectObj.transform.forward = player.transform.forward;
            paintPath = _paintEffectObj.GetComponentInChildren<PaintPath>();
            paintPath.onPaintCompleted += () => OnPaintCompleted();

            if (guideFlag)
            {
                guideGameObject = Instantiate(guidePrefab,pos, _paintEffectObj.transform.rotation) as GameObject;
                guideGameObject.transform.SetParent(_paintEffectObj.transform);
                paintPath.onFirstTouch += () => OnFirstTouch();
            }
            //OnPaintCompleted();

            Time.timeScale = timeScale;
        }

        protected override bool FireUpdate()
        {
            //符文绘制完毕，施法状态提前结束
            if (paintCompletedFlag)
            {
                return false;
            }
            return true;
        }

        protected override void FireEnd()
        {
            base.FireEnd();

            if (_paintEffectObj != null)
                Destroy(_paintEffectObj);

            if (!paintCompletedFlag)
            {
                Time.timeScale = 1;
            }

            paintPath = null;
            paintCompletedFlag = false;
            attackTargets.Clear();
        }

        private void OnFirstTouch()
        {
            if (guideGameObject)
            {
                guideGameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 符文绘制成功
        /// </summary>
        private void OnPaintCompleted()
        {
            guideFlag = false;
            paintCompletedFlag = true;

            if (_paintEffectObj != null)
                Destroy(_paintEffectObj);

            EffectComponent eComponent = player.effectManager.AddEffect(blackHoleEffect, paintPath.mirrorTransform.position);
            StartCoroutine(Absorbing(eComponent));
        }

        private IEnumerator Absorbing(EffectComponent eComponent)
        {
            float currentTime = 0.0f;

            while (true)
            {
                yield return new WaitForEndOfFrame();
                currentTime += Time.unscaledDeltaTime;
                if (currentTime > absorbLastTime)
                {
                    Time.timeScale = 1;
                    yield break;
                }
                List<AvatarComponent> objs = AvatarComponent.AvatarInRange(absorbRadius, player, eComponent.transform.position);
                foreach (AvatarComponent obj in objs)
                {
                    if (attackTargets.Contains(obj))
                    {
                        continue;
                    }

                    for (int i = 0; i < relation.Length; i++)
                    {
                        if (player.CheckRelationship(obj) == relation[i] && obj.status != eEntityStatus.Death && !obj.HasEffectStatus(eEffectStatus.SuperBody))
                        {
                            attackTargets.Add(obj);
                            foreach (SpellEffect effect in triggerEffects)
                            {
                                effect.Cast(player, obj, null, null);
                            }
                            //开始执行吸收表现
                            StartCoroutine(_Action(obj, eComponent.gameObject.transform, absorbTime));
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 吸收表现
        /// </summary>
        /// <param name="component"></param>
        /// <param name="holeTransform"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public IEnumerator _Action(AvatarComponent component, Transform holeTransform, float time)
        {
            component.animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            component.animator.SetBool("struggle", true);
            component.EffectStatusCounterIncr((int)eEffectStatus.HitBy);

            yield return new WaitForSecondsRealtime(1.0f);
            time -= 1.0f;

            float scale = 0.0f;

            int rotate_x = Random.Range(-60, 60);  //X轴旋转量
            int rotate_y = Random.Range(-60, 60);  //Y轴旋转量
            int rotate_z = Random.Range(-60, 60);  //Z轴旋转量

            Vector3 startPoint = component.transform.position;  //起始点
            Vector3 endPoint = holeTransform.position;          //目标点

            Vector3 startScale = component.transform.localScale;
            Vector3 endScale = component.transform.localScale * scale;

            float Len = Vector3.Distance(component.transform.position, endPoint);

            float speed = Len / time;

            while (Vector3.Distance(component.transform.position, endPoint) > 0.1f)
            {
                yield return new WaitForEndOfFrame();
                component.transform.Rotate(new Vector3(Time.unscaledDeltaTime * rotate_x, Time.unscaledDeltaTime * rotate_y, Time.unscaledDeltaTime * rotate_z));
                component.transform.localScale = (scale + (Vector3.Distance(component.transform.position, endPoint) - 0.1f) / (Len - 0.1f)) * startScale;
                component.transform.position = Vector3.MoveTowards(component.transform.position, endPoint, speed * Time.unscaledDeltaTime);
            }
            component.magicSpar = 0;
            component.BloodBead.Clear();
            component.transform.localScale = Vector3.zero;
            component.animator.updateMode = AnimatorUpdateMode.Normal;
            component.receiveDamage(player, 100000, CDeadType.Normal);
            yield break;
        }
    }
}

