using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SPELL
{
    public class CooldownManager
    {
        Dictionary<int, float> m_cooldowns = new Dictionary<int, float>();

        /* 检查某个cd的时间是否已过期
	 */
        public bool IsTimeout(int cooldownID)
        {
            float result;
            if (m_cooldowns.TryGetValue(cooldownID, out result))
            {
                return result < Time.time;
            }
            return true;
        }

        /* 设置超时时间。
	 * 如果cooldownID已存在，则判断两者间哪个时间值更大，最最大值
	 * @param cooldownID: cd的唯一编号
	 * @param timeout: 超时时间（单位：秒）；如10秒后过期则设置为10.0f即可
	 */
        public void Set(int cooldownID, float timeout)
        {
            float result;
            timeout += Time.time;
            if (!m_cooldowns.TryGetValue(cooldownID, out result) || result < timeout)
            {
                m_cooldowns[cooldownID] = timeout;
            }
        }

        /* 取一个cooldown当前剩下的时间
	 */
        public float Get(int cooldownID)
        {
            float result;
            if (m_cooldowns.TryGetValue(cooldownID, out result))
            {
                return result - Time.time;
            }
            return 0.0f;
        }

    }

}
