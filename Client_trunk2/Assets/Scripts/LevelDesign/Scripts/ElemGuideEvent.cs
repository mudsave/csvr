using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GuideEvent
{
    None,
    StartFlySword,
    EndFlySword,
    StartSkillMenu,
    EndSkillMenu,
    StartShield,
    EndShield,
    HoldFlySword,
    TouchSkill,
    CastSkill,
    Move,
}

/// <summary>
/// 完成指定的指引事件才能通关
/// </summary>
namespace LevelDesign
{

    public class ElemGuideEvent : LevelElement
    {
        public string AudioName;
        public float lastTime;
        public string repeatAudioName;
        public float repeatLastTime;

        public float repeatWaitTime;

        public GuideEvent guideEvent = GuideEvent.None;
        public GameObject guidePrefab = null;

        private GameObject _guideGameObject = null;

        private AudioSource _audioSource = null;

        public override void OnActive()
        {
            base.OnActive();

            if (AudioName != "")
                _audioSource = AudioManager.Instance.SoundPlay(AudioName);

            if(repeatAudioName != "")
                InvokeRepeating("RepeatSoundPlay", lastTime + repeatWaitTime, repeatLastTime + repeatWaitTime);

            if (guidePrefab != null)
            {
                _guideGameObject = Instantiate(guidePrefab) as GameObject;
                Transform cameraTransform = VRInputManager.Instance.camera.gameObject.transform;
                _guideGameObject.transform.parent = cameraTransform;
                _guideGameObject.transform.localPosition = Vector3.zero + Vector3.forward;
                _guideGameObject.transform.rotation = cameraTransform.rotation;
            }

            //switch (guideEvent)
            //{
            //    case GuideEvent.StartFlySword:
            //        GlobalEvent.register("GuideEvent_StartFlySword", this, "GuideEvent_StartFlySword");
            //        GlobalEvent.fire("Event_SwordOriginColliderSwitch",true);
            //        break;
            //    case GuideEvent.EndFlySword:
            //        GlobalEvent.register("GuideEvent_EndFlySword", this, "GuideEvent_EndFlySword");
            //        break;
            //    case GuideEvent.StartSkillMenu:
            //        GlobalEvent.register("GuideEvent_StartSkillMenu", this, "GuideEvent_StartSkillMenu");
            //        GlobalEvent.fire("Event_SkillMenuOriginColliderSwitch", true);
            //        break;
            //    case GuideEvent.EndSkillMenu:
            //        GlobalEvent.register("GuideEvent_EndSkillMenu", this, "GuideEvent_EndSkillMenu");
            //        break;
            //    case GuideEvent.StartShield:
            //        GlobalEvent.register("GuideEvent_StartShield", this, "GuideEvent_StartShield");
            //        break;
            //    case GuideEvent.EndShield:
            //        GlobalEvent.register("GuideEvent_EndShield", this, "GuideEvent_EndShield");
            //        break;
            //    case GuideEvent.HoldFlySword:
            //        GlobalEvent.register("GuideEvent_HoldFlySword", this, "GuideEvent_HoldFlySword");
            //        break;
            //    case GuideEvent.TouchSkill_1:
            //        GlobalEvent.register("GuideEvent_TouchSkill_1", this, "GuideEvent_TouchSkill_1");
            //        break;
            //    case GuideEvent.TouchSkill_2:
            //        GlobalEvent.register("GuideEvent_TouchSkill_2", this, "GuideEvent_TouchSkill_2");
            //        break;
            //    case GuideEvent.TouchSkill_3:
            //        GlobalEvent.register("GuideEvent_TouchSkill_3", this, "GuideEvent_TouchSkill_3");
            //        break;
            //    case GuideEvent.TouchSkill_4:
            //        GlobalEvent.register("GuideEvent_TouchSkill_4", this, "GuideEvent_TouchSkill_4");
            //        break;
            //    default:
            //        LevelPass();
            //        break;

            if (guideEvent == GuideEvent.None)
            {
                Pass();
                return;
            }

            GlobalEvent.register("GuideEvent", this, "PlayerGuide_Event");
            GlobalEvent.fire("Event_GuideEvent", guideEvent);
        }

        public void RepeatSoundPlay()
        {
            if (isPassed)
            {
                CancelInvoke("RepeatSoundPlay");
                return;
            }

            _audioSource = AudioManager.Instance.SoundPlay(repeatAudioName);
        }

        //public void GuideEvent_StartFlySword()
        //{
        //    Pass();

        //    GlobalEvent.deregister("GuideEvent_StartFlySword", this, "GuideEvent_StartFlySword");
        //}

        //public void GuideEvent_EndFlySword()
        //{
        //    Pass();

        //    GlobalEvent.deregister("GuideEvent_EndFlySword", this, "GuideEvent_EndFlySword");
        //}

        //public void GuideEvent_StartSkillMenu()
        //{
        //    Pass();

        //    GlobalEvent.deregister("GuideEvent_StartSkillMenu", this, "GuideEvent_StartSkillMenu");
        //}

        //public void GuideEvent_EndSkillMenu()
        //{
        //    Pass();

        //    GlobalEvent.deregister("GuideEvent_EndSkillMenu", this, "GuideEvent_EndSkillMenu");
        //}

        //public void GuideEvent_StartShield()
        //{
        //    Pass();

        //    GlobalEvent.deregister("GuideEvent_StartShield", this, "GuideEvent_StartShield");
        //}

        //public void GuideEvent_EndShield()
        //{
        //    Pass();

        //    GlobalEvent.deregister("GuideEvent_EndShield", this, "GuideEvent_EndShield");
        //}

        //public void GuideEvent_HoldFlySword()
        //{
        //    Pass();

        //    GlobalEvent.deregister("GuideEvent_HoldFlySword", this, "GuideEvent_HoldFlySword");
        //}

        public void PlayerGuide_Event(GuideEvent gEvent)
        {
            if (guideEvent == gEvent)
            {
                Pass();

                GlobalEvent.deregister("GuideEvent");
            }
        }

        public void Pass()
        {
            if (_audioSource)
                _audioSource.Stop();

            if (_guideGameObject)
                Destroy(_guideGameObject);
        
            if (isActive)
                LevelPass();
        }
    }
}
