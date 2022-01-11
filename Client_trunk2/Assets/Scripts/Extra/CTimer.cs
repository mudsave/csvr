public class CTimer
{
    private bool b_Tricking;
    private float f_CurTime;
    private float f_TriggerTime;
    private float f_StartTime = 0.0f;
    private object[] m_arg = null;

    public delegate void EventHandler(object[] arg);
    public event EventHandler tick;

    public CTimer(float start, float second, object[] arg)
    {
        f_StartTime = start;
        f_CurTime = 0.0f;
        f_TriggerTime = second;
		m_arg = arg;
    }
    
	public void Start()
    {
        b_Tricking = true;

        if (f_StartTime == 0.0f)
            Update(f_TriggerTime);
    }
    
    public bool Update(float deltaTime)
    {
        if (b_Tricking)
        {
            f_CurTime += deltaTime;

            if (f_StartTime > 0.0f)
            {
                if (f_CurTime >= f_StartTime)
                {
                    f_CurTime = 0.0f;
                    f_StartTime = 0.0f;
                    tick(m_arg);

                    if (f_TriggerTime == 0.0f)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            else
            {
                if (f_CurTime >= f_TriggerTime)
                {
                    f_CurTime = 0.0f;
                    tick(m_arg);
                    return false;
                }                
            }
        }
        return false;
    }
	
    public void Stop()
    {
        b_Tricking = false;
    }

    public void Continue()
    {
        b_Tricking = true;
    }

    public void Restart()
    {
        b_Tricking = true;
        f_CurTime = 0.0f;
    }

    public void ResetTriggerTime(float second)
    {
        f_TriggerTime = second;
    }
}
