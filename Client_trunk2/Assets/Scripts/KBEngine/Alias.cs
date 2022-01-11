using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
/// data type collection
/// 
namespace Alias
{
    /// <summary>
    /// see also: alias.xml::BUFF_DATA_TYPE
    /// </summary>
    public class BuffDataType
    {
        public Int32 index = 0;
        public Int32 timerID = 0;
        public Int32 buffID = 0;
        public Int32 casterID = 0;
        public Double endTime = 0.0f;
        public int counter = 0;
        public JsonData misc = null;
        public Dictionary<string, object> localBuffData = new Dictionary<string, object>(); // 用来存储一些本地buff中计算的数据，例如位移坐标等等

        /// <summary>
        /// 用于存储buff自身需要的临时变量数据
        /// </summary>
        public Dictionary<string, object> temp = new Dictionary<string, object>();

        //public static BuffDataType CreateObjFromDict(Dictionary<string, object> dict)
        //{
        //    var obj = new BuffDataType();
        //    obj.index = (sbyte)dict["index"];
        //    obj.timerID = (Int32)dict["timerID"];
        //    obj.buffID = (Int32)dict["buffID"];
        //    obj.casterID = (Int32)dict["casterID"];
        //    obj.endTime = (Double)dict["endTime"];
        //    obj.counter = (sbyte)dict["counter"];
        //    obj.misc = JsonMapper.ToObject((string)dict["misc"]);
        //    return obj;
        //}

        public static Dictionary<string, object> GetDictFromObj(BuffDataType obj)
        {
            var dict = new Dictionary<string, object>();
            dict["index"] = obj.index;
            dict["timerID"] = obj.timerID;
            dict["buffID"] = obj.buffID;
            dict["casterID"] = obj.casterID;
            dict["endTime"] = obj.endTime;
            dict["counter"] = obj.counter;
            dict["misc"] = obj.misc.ToJson();

            return dict;
        }

        public SPELL.SpellEffect GetBuff()
        {
            return SpellLoader.instance.GetEffect(buffID);
        }
    }

    public class CooldownDataType
    {
        public Int32 cooldownID = 0;
        public Double beginTime = 0.0f;
        public Double endTime = 0.0f;
        public float receiveTime = 0.0f;  // cd接收到的时间Time.time

        public CooldownDataType() { }

        public CooldownDataType(Int32 _cooldownID, double _beginTime, double _endTime)
        {
            cooldownID = _cooldownID;
            Set(_beginTime, _endTime);
        }

        public static CooldownDataType CreateObjFromDict(Dictionary<string, object> dict)
        {
            var obj = new CooldownDataType((Int32)dict["cooldownID"], (Double)dict["beginTime"], (Double)dict["endTime"]);
            return obj;
        }

        public void Set(double _beginTime, double _endTime)
        {
            beginTime = _beginTime;
            endTime = _endTime;
            receiveTime = Time.time;
        }
    }

    public class CooldownMgrDataType
    {
        private Dictionary<Int32, CooldownDataType> m_datas = new Dictionary<int, CooldownDataType>();

        public static CooldownMgrDataType CreateObjFromDict(List<object> datas)
        {
            CooldownMgrDataType obj = new CooldownMgrDataType();

            foreach (var d in datas)
            {
                var cd = CooldownDataType.CreateObjFromDict((Dictionary<string, object>)d);
                obj.m_datas[cd.cooldownID] = cd;
            }

            return obj;
        }

        public CooldownDataType Get(Int32 cooldownID)
        {
            if (!m_datas.ContainsKey(cooldownID))
                return null;

            return m_datas[cooldownID];
        }

        public void Set(CooldownDataType cooldown)
        {
            m_datas[cooldown.cooldownID] = cooldown;
        }

        public void Set(Int32 cooldownID, double beginTime, double endTime)
        {
            //Debug.Log(string.Format("set(), cooldownID = {0}, cd s = {1}, cd r = {2}", cooldownID, endTime - beginTime, ServerTime.getCurrentTime() - endTime));
            if (m_datas.ContainsKey(cooldownID))
            {
                var cooldown = m_datas[cooldownID];
                cooldown.Set(beginTime, endTime);
            }
            else
            {
                Set(new Alias.CooldownDataType(cooldownID, beginTime, endTime));
            }
        }

        /// <summary>
        /// 检查指定的cooldown时间是否已经冷却完毕
        /// </summary>
        /// <param name="cooldownID"></param>
        /// <returns></returns>
        public bool IsTimeout(Int32 cooldownID)
        {
            if (!m_datas.ContainsKey(cooldownID))
                return true;

            var cooldown = m_datas[cooldownID];
            //Debug.Log(string.Format("IsTimeout(), cooldownID = {0}, cd s = {1}, cd r = {2}", cooldownID, cooldown.endTime - cooldown.beginTime, ServerTime.getCurrentTime() - cooldown.endTime));
            //return cooldown.endTime <= ServerTime.getCurrentTime();
            return Time.time - cooldown.receiveTime >= cooldown.endTime - cooldown.beginTime;
        }
    }

    public class QUEST
    {
        public QUEST() {}

        public int id;
        public CQuestStatus status;
        public List<int> target = new List<int>();

        public static QUEST CreateObjFromDict(Dictionary<string, object> dict)
        {
            QUEST quest = new QUEST();

            quest.id = (int)dict["id"];
            quest.status = (CQuestStatus)(Int16)dict["status"];
            List<object> data = (List<object>)dict["target"];
            for (int i = 0; i < data.Count; i++)
            {
                quest.target.Add((int)data[i]);
            }

            return quest;
        }
    }

    public class QUESTLIST
    {
        public List<QUEST> items = new List<QUEST>();

        public static QUESTLIST CreateObjFromDict(Dictionary<string, object> dict)
        {
            List<object> values = (List<object>)dict["items"];

            QUESTLIST returnvalue = new QUESTLIST();

            for (int i = 0; i < values.Count; i++)
            {
                returnvalue.items.Add(QUEST.CreateObjFromDict((Dictionary<string, object>)values[i]));
            }

            return returnvalue;

        }
    }


}