using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 事件系统
/// </summary>
public class EntityEvent
{
    public struct Pair
	{
		public object obj;
		public string funcname;
		public System.Reflection.MethodInfo method;
	};
		
	public struct EventObj
	{
		public Pair info;
		public object[] args;
	};
		
    Dictionary<string, List<Pair>> events = new Dictionary<string, List<Pair>>();
		
	public void clear()
	{
        events.Clear();
	}

    public bool hasRegister(string eventname)
    {
        return _hasRegister(events, eventname);
    }

    private bool _hasRegister(Dictionary<string, List<Pair>> events, string eventname)
    {
        bool has = false;
        has = events.ContainsKey(eventname);
        return has;
    }

    public bool register(string eventname, object obj, string funcname)
    {
        return _register(events, eventname, obj, funcname);
    }

    private bool _register(Dictionary<string, List<Pair>> events, string eventname, object obj, string funcname)
    {
        _deregister(events, eventname, obj, funcname);
        List<Pair> lst = null;

        Pair pair = new Pair();
        pair.obj = obj;
        pair.funcname = funcname;
        pair.method = obj.GetType().GetMethod(funcname);
        if (pair.method == null)
        {
            Debug.LogError("EntityEvent::_register: " + obj + "not found method[" + funcname + "]");
            return false;
        }

        if (!events.TryGetValue(eventname, out lst))
        {
            lst = new List<Pair>();
            lst.Add(pair);
            events.Add(eventname, lst);
            return true;
        }

        lst.Add(pair);
        return true;
    }

    public bool deregister(string eventname, object obj, string funcname)
    {
        return _deregister(events, eventname, obj, funcname);
    }

    private bool _deregister(Dictionary<string, List<Pair>> events, string eventname, object obj, string funcname)
    {
        List<Pair> lst = null;

        if (!events.TryGetValue(eventname, out lst))
        {
            return false;
        }

        for (int i = 0; i < lst.Count; i++)
        {
            if (obj == lst[i].obj && lst[i].funcname == funcname)
            {
                lst.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public bool deregister(object obj)
    {
        return _deregister(events, obj);
    }

    private bool _deregister(Dictionary<string, List<Pair>> events, object obj)
    {
        var ide = events.GetEnumerator();
        while (ide.MoveNext())
        {
            KeyValuePair<string, List<Pair>> e = ide.Current;

            List<Pair> lst = e.Value;
            for (int i = lst.Count - 1; i >= 0; i--)
            {
                if (obj == lst[i].obj)
                    lst.RemoveAt(i);
            }
        }

        return true;
    }

    public void fire(string eventname, params object[] args)
    {
        fire_(events, eventname, args);
    }

    private void fire_(Dictionary<string, List<Pair>> events, string eventname, object[] args)
    {
        List<Pair> lst = null;

        if (!events.TryGetValue(eventname, out lst))
        {
            //Debug.LogWarning("EntityEvent::fire_: event(" + eventname + ") not found!");
            return;
        }

        for (int i = lst.Count-1; i >= 0; i--)
        {
            EventObj eobj = new EventObj();
            eobj.info = lst[i];
            eobj.args = args;

            try
            {
                eobj.info.method.Invoke(eobj.info.obj, eobj.args);
            }
            catch (Exception e)
            {
                Debug.LogError("EntityEvent::fire_: event=" + eobj.info.funcname + "\n" + e.ToString());
            }
        }
    }		
}
