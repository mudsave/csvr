using UnityEngine;
using System.Collections;

/// <summary>
/// 音效管理系统
/// </summary>
public class AudioManager : MonoBehaviour {

    public static AudioManager Instance = null;
    private AudioSource m_BGMAudioSource = null;
    private const string ResourcePath = "Sounds/";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            if (Instance != this)
            {
                throw new System.Exception("AudioManager::Awake(), reuplicate instance!");
            }
        }
    }

    void Start()
    {
    /// if (m_BGMAudioSource == null)
    /// {
    ///     BGMPlay("Background/BetrayalVoices", 0.2f, true);
    /// }
    }

    public AudioSource BGMusic
    {
        get { return m_BGMAudioSource; }
    }

    /// <summary>
    /// 播放声音
    /// </summary>
    /// <param name="audioname"></param>
    /// <param name="volume"></param>
    /// <param name="delay"></param>
    /// <param name="isLoop"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public AudioSource SoundPlay(string audioname, float volume = 1, float delay = 0.0f, bool isLoop = false, GameObject obj = null)
    {
        AudioClip sound = GetResAudioClip(audioname);
        return PlayClip(sound, volume, delay, isLoop, obj);
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="bgmname"></param>
    /// <param name="volume"></param>
    /// <param name="isloop"></param>
    public void BGMPlay(string audioname, float volume, bool isloop)
    {
        BGMStop();    //无论怎么样，先停止背景音乐
     
        if (audioname != null)
        {
            AudioClip bgmClip = this.GetResAudioClip(audioname);
            if (bgmClip != null)
            {
                PlayBGMClip(bgmClip, volume, isloop);
            }
        }
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void BGMPause()
    {
        if (m_BGMAudioSource != null)
        {
            m_BGMAudioSource.Pause();
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void BGMStop()
    {
        if (m_BGMAudioSource != null && m_BGMAudioSource.gameObject)
        {
            Destroy(m_BGMAudioSource.gameObject);
            m_BGMAudioSource = null;
        }
    }

    /// <summary>
    /// 设置背景音乐声音大小
    /// </summary>
    /// <param name="volume"></param>
    public void BGMSetVolume(float volume)
    {
        if (m_BGMAudioSource != null)
        {
            m_BGMAudioSource.volume = volume;
        }
    }

    private AudioClip GetResAudioClip(string aduioName)
    {
        return Resources.Load(ResourcePath + aduioName) as AudioClip;
    }

    private AudioSource PlayClip(AudioClip audioClip, float volume = 1f, float delay = 0.0f, bool isLoop = false, GameObject obj = null, string name = null)
    {
        if (audioClip == null)
        {
            return null;
        }
        else
        {
            GameObject gameObj = new GameObject(name == null ? "SoundClip" : name);
            AudioSource source = gameObj.AddComponent<AudioSource>();

            if (obj)
                gameObj.transform.parent = obj.transform;
            else
                gameObj.transform.parent = gameObject.transform;
            
            if (!isLoop)
                StartCoroutine(PlayClipEndDestroy(audioClip, gameObj, delay));

            source.loop = isLoop;
            source.pitch = 1f;
            source.volume = volume;
            source.clip = audioClip;

            if (delay > 0.0f)
                StartCoroutine(PlayClipDelay(source, delay));
            else
                source.Play();

            return source;
        }
    }

    private IEnumerator PlayClipEndDestroy(AudioClip audioclip, GameObject soundobj, float delay)
    {
        if (soundobj == null || audioclip == null)
        {
            yield break;
        }
        else
        {
            yield return new WaitForSeconds((audioclip.length + delay) * Time.timeScale);
            Destroy(soundobj);
        }
    }

    private IEnumerator PlayClipDelay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (source != null)
        {
            source.Play();
        }
    }


    private void PlayBGMClip(AudioClip audioClip, float volume, bool isloop)
    {
        if (audioClip == null)
        {
            return;
        }
        else
        {
            GameObject obj = new GameObject("BGMusic");
            obj.transform.parent = gameObject.transform;
            AudioSource audioSource = obj.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.loop = isloop;
            audioSource.pitch = 1f;
            audioSource.Play();
            m_BGMAudioSource = audioSource;
        }
    }
}
