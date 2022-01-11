using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
 * 播放音效有两种方案
  	1.在每个角色或NPC身上增加一个AudioSource的组件，播放的时候
 	  	AudioSource soundSource = XXX;
 		soundSource.clip = xxxx;
		soundSource.Play();

	2.使用静态函数AudioSource.PlayClipAtPoint(AudioClip, vector3);
		此函数会动态创建一个AudioSource来播放此声音片段，可以支持同时播放N个音效。
		缺点就是不支持循环播放

	当前就是使用第二种方式来播放技能音效
 * 
 * 
 * 
 */

public class SoundFXManager : MonoBehaviour
{

    public static SoundFXManager Instance; //this script

    public AudioSource backgroundMusic;
    private ComplexBool m_isPlayBackGroudMusic = new ComplexBool(true, 1);

    [System.Serializable]
    public class SoundFxMeta
    {
        public string soundName = "";			//KEY name
        public int soundFxType = 1; //type 
        public Object soundFX = null;           //AudioClip
        public float volumeValue = 1.0f;    //音量大小
        public bool loop = false;       //是否循环播放


    }

    public List<SoundFxMeta> m_soundFXMetas = new List<SoundFxMeta>();
    public List<SoundFxMeta> m_uiSoundFXMetas = new List<SoundFxMeta>(); //ui界面音乐

    void Awake()
    {
        if (Instance != null)
            throw new System.Exception("SoundFXManager::Awake(), reuplicate instance!");

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        m_soundFXMetas.AddRange(m_uiSoundFXMetas); //合并
    }

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public SoundFxMeta GetSoundMetaData(string name)
    {
        if (name != "")
        {
            //不用foreach,据说这个foreach效率比较低，未测试
            int size = m_soundFXMetas.Count;
            for (int i = 0; i < size; i++)
            {
                if (m_soundFXMetas[i].soundName == name)
                {
                    return m_soundFXMetas[i];
                }

            }
        }

        return null;

    }

    public void PlaySound(string soundName, Transform transform)
    {

        //TODO 播放音效
        SoundFxMeta meta = GetSoundMetaData(soundName);
        if (meta != null)
        {
            AudioSource.PlayClipAtPoint((AudioClip)meta.soundFX, transform.position);
        }
    }

    ////播放UI声音
    //public AudioSource PlayUISound(string soundName)
    //{
    //    if (!CSystemSettingDataClass.instance.hasSysSettingFlag(eSystemSetting.sound))
    //    {
    //        return null;
    //    }

    //    SoundFXManager.SoundFxMeta sound = SoundFXManager.Instance.GetSoundMetaData(soundName);
    //    AudioSource source = NGUITools.PlaySound((AudioClip)sound.soundFX, sound.volumeValue, 1.0f);
    //    return source;
    //}

    ////播放UI声音
    //public AudioSource PlayUISound(AudioClip sound)
    //{
    //    if (!CSystemSettingDataClass.instance.hasSysSettingFlag(eSystemSetting.sound))
    //    {
    //        return null;
    //    }

    //    AudioSource source = NGUITools.PlaySound((AudioClip)sound, NGUITools.soundVolume, 1.0f);
    //    return source;
    //}

    ////增加一个UI的audioSource
    //public AudioSource AddUIAudioSource(string soundName, bool loop = false)
    //{
    //    if (!CSystemSettingDataClass.instance.hasSysSettingFlag(eSystemSetting.sound))
    //    {
    //        return null;
    //    }

    //    SoundFXManager.SoundFxMeta sound = SoundFXManager.Instance.GetSoundMetaData(soundName);
    //    float volume = sound.volumeValue;
    //    volume *= NGUITools.soundVolume;

    //    AudioSource source = CGameObject.instance.gameObject.AddComponent<AudioSource>();
    //    source.clip = (AudioClip)sound.soundFX;
    //    source.priority = 50;
    //    source.pitch = 1;
    //    source.volume = volume;
    //    source.loop = loop;
    //    source.spatialBlend = 0;
    //    return source;
    //}

    ////增加一个UI的audioSource
    //public AudioSource AddUIAudioSource(AudioClip sound, bool loop = false)
    //{
    //    if (!CSystemSettingDataClass.instance.hasSysSettingFlag(eSystemSetting.sound))
    //    {
    //        return null;
    //    }

    //    float volume = NGUITools.soundVolume;

    //    AudioSource source = CGameObject.instance.gameObject.AddComponent<AudioSource>();
    //    source.clip = (AudioClip)sound;
    //    source.priority = 50;
    //    source.pitch = 1;
    //    source.volume = volume;
    //    source.loop = loop;
    //    source.spatialBlend = 0;
    //    return source;
    //}

    //public AudioSource AddBackgroundAudioSource(AudioClip sound, bool loop = false)
    //{
    //    float volume = NGUITools.soundVolume;

    //    AudioSource source = CGameObject.instance.gameObject.AddComponent<AudioSource>();
    //    source.clip = (AudioClip)sound;
    //    source.priority = 50;
    //    source.pitch = 1;
    //    source.volume = volume;
    //    source.loop = loop;
    //    source.spatialBlend = 0;
    //    return source;
    //}

    ////移除一个UI的audioSource
    //public void RemoveUIAudioSource(AudioSource audio)
    //{
    //    Object.Destroy(audio);
    //}

    //public void AddSoundMeta(string soundName, Object soundFX, int type = 0)
    //{
    //    //TODO
    //}

    ///// <summary>
    ///// 播放背景音乐
    ///// </summary>
    //public void PlayBackGroundMusic(string tag = null)
    //{
    //    if (backgroundMusic == null)
    //        return;

    //    bool oldValue = m_isPlayBackGroudMusic.Value();

    //    if (tag != null)
    //    {
    //        m_isPlayBackGroudMusic.subComplex(tag);
    //    }
    //    else
    //    {
    //        m_isPlayBackGroudMusic.subComplex();
    //    }

    //    if (oldValue == m_isPlayBackGroudMusic.Value())
    //    {
    //        return;
    //    }

    //    backgroundMusic.Play();
    //}

    ///// <summary>
    ///// 停止背景音乐
    ///// </summary>
    //public void StopBackGroundMusic(string tag = null)
    //{
    //    if (backgroundMusic == null)
    //        return;

    //    bool oldValue = m_isPlayBackGroudMusic.Value();

    //    if (tag != null)
    //    {
    //        m_isPlayBackGroudMusic.addComplex(tag);
    //    }
    //    else
    //    {
    //        m_isPlayBackGroudMusic.addComplex();
    //    }

    //    if (oldValue == m_isPlayBackGroudMusic.Value())
    //    {
    //        return;
    //    }

    //    backgroundMusic.Stop();
    //}

    ///// <summary>
    ///// 销毁背景音乐
    ///// </summary>
    //public void DestroyBackGroundSound()
    //{
    //    if (backgroundMusic == null)
    //        return;

    //    RemoveUIAudioSource(backgroundMusic);
    //    backgroundMusic = null;
    //}

    ////重置背景音乐
    //public void OnClearBackGroundMusic()
    //{
    //    backgroundMusic = null;
    //    m_isPlayBackGroudMusic = new ComplexBool(true, 1);
    //}
}
