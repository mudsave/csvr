using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FightResultWin : UIWindow
{
    public float m_showInterval = 1.0f;

    [Header("Sounds name")]
    public string m_audioButtonClick;
    public string m_audioButtonHover;

    private GameObject m_sword;
    private Text m_swordText;

    private GameObject m_counter;
    private Text m_counterText;

    private GameObject m_immunity;
    private Text m_immunityText;

    private GameObject m_HP;
    private Text m_HPText;

    private GameObject m_crystal;
    private Text m_crystalText;

    private GameObject m_destroy;
    private Text m_destroyText;

    private GameObject m_collection;
    private Text m_collectionText;

    private GameObject m_reward;
    private Text m_rewardText;

    private GameObject m_nextButton;
    private GameObject m_backButton;

    private bool m_showImmediately = false;

    private List<GameObject> m_contents = new List<GameObject>(8);

    protected override void OnInit()
    {
        InitItem();
        InstallGlobalEvents();
    }

    private void InitItem()
    {
        m_sword = transform.Find("Content/Sword/Value").gameObject;
        m_swordText = m_sword.GetComponent<Text>();
        m_contents.Add(m_sword);

        m_counter = transform.Find("Content/Counter/Value").gameObject;
        m_counterText = m_counter.GetComponent<Text>();
        m_contents.Add(m_counter);

        m_immunity = transform.Find("Content/Immunity/Value").gameObject;
        m_immunityText = m_immunity.GetComponent<Text>();
        m_contents.Add(m_immunity);

        m_HP = transform.Find("Content/HP/Value").gameObject;
        m_HPText = m_HP.GetComponent<Text>();
        m_contents.Add(m_HP);

        m_crystal = transform.Find("Content/Crystal/Value").gameObject;
        m_crystalText = m_crystal.GetComponent<Text>();
        m_contents.Add(m_crystal);

        m_destroy = transform.Find("Content/Destroy/Value").gameObject;
        m_destroyText = m_destroy.GetComponent<Text>();
        m_contents.Add(m_destroy);

        m_collection = transform.Find("Content/Collection/Value").gameObject;
        m_collectionText = m_collection.GetComponent<Text>();
        m_contents.Add(m_collection);

        m_reward = transform.Find("Content/Reward").gameObject;
        m_rewardText = m_reward.GetComponent<Text>();
        m_contents.Add(m_reward);

        m_nextButton = transform.Find("Next").gameObject;
        m_backButton = transform.Find("Back").gameObject;
    }

    private void InstallGlobalEvents()
    {
        GlobalEvent.register("Event_fightingEnd", this, "InitFightResultData");
    }

    public void InitFightResultData()
    {
        // @TODO: 去统计模块取数据填充界面
        Debug.Log("FightResultWin::fightingEnd:init data.");
    }

    public void OnClickNext()
    {
        PlayerSound(m_audioButtonClick);
        m_showImmediately = true;
        m_nextButton.SetActive(false);
        m_backButton.SetActive(true);
    }

    public void OnClickBack()
    {
        PlayerSound(m_audioButtonClick);
        UIManager.Instance.CloseUI(CPrefabPaths.UIFightResult);
        UIManager.Instance.OpenUI(CPrefabPaths.UILoginPlotList, transform.position, transform.rotation.eulerAngles, true);
        UIManager.Instance.OpenUI(CPrefabPaths.MainMenu, transform.position, transform.rotation.eulerAngles, false);
        UIManager.Instance.OpenUI(CPrefabPaths.UILoginModeList, transform.position, transform.rotation.eulerAngles, false);
        //CGameState.Instance().ChangeState(GameStateEnum.LoadingLoginState);
    }

    private IEnumerator ShowIssue(GameObject m_gameObject)
    {
        ActiveObject(m_gameObject);
        
        Animator animator = m_gameObject.GetComponent<Animator>();
        if(animator == null)
        {
            Debug.LogError("FightResultWin::ShowIssue:some value has not animator component, please check them.");
            yield return new WaitForSeconds(m_showInterval);
        }
        else
        {
            animator.enabled = true;
            yield return new WaitForSeconds(m_showInterval);
            animator.enabled = false;
        }
    }

    private void ActiveObject(GameObject m_gameObject)
    {
        m_gameObject.SetActive(true);
    }

    public void BackSelectLevel()
    {
        UIManager.Instance.CloseUI(CPrefabPaths.UIFightResult); // 临时代码，实际功能应该是要返回关卡选择界面
        //CGameState.Instance().ChangeState(GameStateEnum.LoadingLoginState);
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        InitFightResultData();
        StartCoroutine(ShowContent());
    }

    private IEnumerator ShowContent()
    {
        for (int i = 0; i < m_contents.Count; ++i)
        {
            if (m_showImmediately)
                ActiveObject(m_contents[i]);
            else
                yield return ShowIssue(m_contents[i]);
        }
    }

    protected override void OnClose()
    {
        m_showImmediately = false;
        m_nextButton.SetActive(true);
        m_backButton.SetActive(false);

        m_sword.SetActive(false);
        m_counter.SetActive(false);
        m_immunity.SetActive(false);
        m_HP.SetActive(false);
        m_crystal.SetActive(false);
        m_destroy.SetActive(false);
        m_collection.SetActive(false);
        m_reward.SetActive(false);

        base.OnClose();
    }

    private void PlayerSound(string p_soundName)
    {
        if (p_soundName != "")
            AudioManager.Instance.SoundPlay(p_soundName);
    }

    public void Button_hoverBegin()
    {
        PlayerSound(m_audioButtonHover);
    }
}
