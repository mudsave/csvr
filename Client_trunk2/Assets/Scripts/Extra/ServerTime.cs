using UnityEngine;
using System.Collections;
using System;

public class ServerTime
{

	static double m_serverTime = 0;
	static double m_unityTime = 0;

	public static double serverTime
    {
		get { return m_serverTime; }
		set { m_serverTime = value; }
    }
	
	public static double unityTime
    {
		get { return m_unityTime; }
        set { m_unityTime = value; }
    }

    public static double getCurrentTime()
    {
		return (m_serverTime + Time.time - m_unityTime);
    }

	public static double systemTimeStamp()
	{   
       TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
	   return ts.TotalSeconds;
	}

	public static void initStart()
	{
		m_unityTime = Time.time;
		//Debug.Log(string.Format("ServerTime::initStart(), {0}", m_unityTime));
	}

	public static void initEnd(double systemTime)
	{
		var now = Time.time;
		//Debug.Log(string.Format("ServerTime::initEnd(), old {0}, new {1}, - {2}", m_unityTime, now, now - m_unityTime));
		//Debug.Log(string.Format("ServerTime::initEnd(), server time = '{0}', local time = '{1}', - {2}", systemTime, now, now - systemTime));
		m_serverTime = systemTime;
		m_unityTime = now - (now - m_unityTime) / 2;
	}
}


