using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CTimerManager
{
    private ulong timeid = 1;
	Dictionary<ulong, CTimer> timerlist = new Dictionary<ulong, CTimer>();

	public void Update () {

        var enumerator = timerlist.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var element = enumerator.Current;
            if (element.Value.Update(Time.deltaTime))
                removeTimer(element.Key);
        }
	}

	public ulong addTimer(CTimer.EventHandler func, float start, float second, object[] arg)
	{
		if (timeid > 1.8e+19)
			timeid = 1;
		timeid++;

        CTimer t = new CTimer(start, second, arg);
		t.tick += func;
		t.Start();
		timerlist.Add(timeid, t);
		return timeid;
	}

    public CTimer getTimer(ulong timeid)
    {
        if (timerlist.ContainsKey(timeid))
            return timerlist[timeid];
        else
            return null;
    }

	public void removeTimer(ulong timeid)
	{
        if (timerlist.ContainsKey(timeid))
		    timerlist.Remove(timeid);
	}

    public void removeAll()
    {
        timerlist.Clear();
    }
}
