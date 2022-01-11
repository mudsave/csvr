using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class CElemGroupTrigger : CTriggerBase
{
    [System.Serializable]
    public class EventInfo
    {
        public string eventName;
        public int value;
    }

    public List<EventInfo> eventlist = new List<EventInfo>();
    public double delay = 0;

    // Use this for initialization
    public override void Start()
    {
        base.Start();
    }

    public override JsonData WriteJson()
    {
        JsonData jsonData = new JsonData();
        jsonData["type"] = this.GetType().FullName;
        jsonData["delay"] = delay;
        jsonData["event"] = new JsonData();
        jsonData["event"].SetJsonType(JsonType.Array);
        for (int i = 0; i < eventlist.Count; i++)
        {
            JsonData data = new JsonData();
            data["eventName"] = eventlist[i].eventName;
            data["value"] = eventlist[i].value;
            
            jsonData["event"].Add(data);
        }

        jsonData["birthPoint"] = new JsonData();
        jsonData["birthPoint"].SetJsonType(JsonType.Array);

        exportChildEntitiyConfig(jsonData["birthPoint"]);

        return jsonData;
    }
}
