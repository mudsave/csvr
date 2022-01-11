using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPELL;
using UnityEngine.SceneManagement;

public class VRInputSkillManager : MonoBehaviour
{
    [Header("触摸板Up")]
    public PlayerSkillBase UpButtonSkill = null;
    [Header("触摸板Down")]
    public PlayerSkillBase DownButtonSkill = null;
    [Header("触摸板Left")]
    public PlayerSkillBase LeftButtonSkill = null;
    [Header("触摸板Right")]
    public PlayerSkillBase RightButtonSkill = null;

    private PlayerSkillShield ShieldSkill = null;
    private PlayerSkillFlySword FlySwordSkill = null;

    private bool canController = false;

    private bool touchpadGuideActive = false;
    private bool gripGuideActive = false;

    void Awake()
    {
        ShieldSkill = gameObject.GetComponentInChildren<PlayerSkillShield>();
        FlySwordSkill = gameObject.GetComponentInChildren<PlayerSkillFlySword>();

        GlobalEvent.register("Event_RegisterControllerEvents", this, "RegisterControllerEvents");
        GlobalEvent.register("Event_DeregisterControllerEvents", this, "DeregisterControllerEvents");
        GlobalEvent.register("Event_GuideEvent", this, "OnGuideEvent");
        GlobalEvent.register("OnTouchpadPressed", this, "OnTouchpadPressed");
        GlobalEvent.register("OnTriggerPressed", this, "OnPressed");
        GlobalEvent.register("OnTriggerReleased", this, "OnReleased");
        GlobalEvent.register("OnGripPressed", this, "OnGripPressed");
        GlobalEvent.register("OnGripReleased", this, "OnGripReleased");

        GlobalEvent.register("Event_OnPlayerDeath", this, "OnPlayerDeath");
        GlobalEvent.register("Event_OnPlayerResurrection", this, "OnPlayerResurrection");
        RegisterControllerEvents();

        SceneManager.sceneLoaded += OnSceneLoaded;
        if (SceneManager.GetActiveScene().name == "Demo")
        {
            touchpadGuideActive = false;
            gripGuideActive = false;
        }
        else
        {
            touchpadGuideActive = true;
            gripGuideActive = true;
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode model)
    {
        if (scene.name == "Demo")
        {
            touchpadGuideActive = false;
            gripGuideActive = false;
        }
        else
        {
            touchpadGuideActive = true;
            gripGuideActive = true;
        }
    }

    public void RegisterControllerEvents()
    {
        canController = true;
    }

    public void DeregisterControllerEvents()
    {
        canController = false;
    }

    public void OnTouchpadPressed(VRControllerEventArgs e)
    {
        if(!canController)
            return;

        if(!touchpadGuideActive)
            return;

        float angle = e.touchpadAngle;

        if (angle < 45 || angle >= 315)
        {
            if (UpButtonSkill != null && e.hand == UpButtonSkill.castHand)
            {
                UpButtonSkill.Cast();
            }
        }
        else if (45 <= angle && angle < 135)
        {
            if (RightButtonSkill != null && e.hand == RightButtonSkill.castHand)
            {
                RightButtonSkill.Cast();
            }
        }
        else if (135 <= angle && angle < 225)
        {
            if (DownButtonSkill != null && e.hand == DownButtonSkill.castHand)
            {
                DownButtonSkill.Cast();
            }
        }
        else
        {
            if (LeftButtonSkill != null && e.hand == LeftButtonSkill.castHand)
            {
                LeftButtonSkill.Cast();
            }
        }
    }

    public void OnPressed(VRControllerEventArgs e)
    {
        if (!canController)
            return;

        if (FlySwordSkill != null && e.hand == FlySwordSkill.castHand)
        {
            FlySwordSkill.SetFollowHand(true);
        }
    }

    public void OnReleased(VRControllerEventArgs e)
    {
        if (!canController)
            return;

        if (FlySwordSkill != null && e.hand == FlySwordSkill.castHand)
        {
            FlySwordSkill.SetFollowHand(false);
        }
    }

    public void OnGripPressed(VRControllerEventArgs e)
    {
        if (!canController)
            return;

        if(!gripGuideActive)
            return;

        if (FlySwordSkill != null && e.hand == FlySwordSkill.castHand)
        {
            FlySwordSkill.OpenHoldStatus();
        }
    }

    public void OnGripReleased(VRControllerEventArgs e)
    {
        if (!canController)
            return;

        if (FlySwordSkill != null && e.hand == FlySwordSkill.castHand)
        {
            FlySwordSkill.CloseHoldStatus();
        }
    }

    public void OnPlayerDeath()
    {
        DeregisterControllerEvents();
        ShieldSkill.CloseShield();
        FlySwordSkill.CloseFlySword();

        Transform playerTransform = VRInputManager.Instance.playerComponent.gameObject.transform;
        Vector3 position = playerTransform.position + playerTransform.forward * 3;
        position.y += 2;
        Vector3 direction = playerTransform.rotation.eulerAngles;
        GlobalEvent.fire("Event_EndFight", position, direction);

        VRInputManager.Instance.SelectBeam.SetActive(true);
    }

    public void OnPlayerResurrection()
    {
        RegisterControllerEvents();
        ShieldSkill.OpenShield();
        FlySwordSkill.OpenFlySword();
        VRInputManager.Instance.SelectBeam.SetActive(false);
    }

    //键盘模拟施法
    public void KeyboardSimulation()
    {
        if (!canController)
            return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (UpButtonSkill != null)
            {
                UpButtonSkill.Cast();
            }
        }
        else if(Input.GetKeyDown(KeyCode.H))
        {
            if (DownButtonSkill != null)
            {
                DownButtonSkill.Cast();
            }
        }
        else if(Input.GetKeyDown(KeyCode.J))
        {
            if (LeftButtonSkill != null)
            {
                LeftButtonSkill.Cast();
            }
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            if (RightButtonSkill != null)
            {
                RightButtonSkill.Cast();
            }
        }
        else if(Input.GetKeyDown(KeyCode.Q))
        {
            FlySwordSkill.SimulationAttackOlder();
        }
    }

    void Update()
    {
        KeyboardSimulation();
    }

    public void OnGuideEvent(GuideEvent guideEvent)
    {
        if (guideEvent == GuideEvent.HoldFlySword)
        {
            gripGuideActive = true;
        }
        else if(guideEvent == GuideEvent.CastSkill)
        {
            touchpadGuideActive = true;
        }
    }
}
