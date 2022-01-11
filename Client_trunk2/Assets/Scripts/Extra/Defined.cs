using System.Collections.Generic;
using UnityEngine;

/* 通用定义
 */

public class SystemClass
{
    //下载资源路径
    public static readonly string DownServerURL =
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
 "file://" + Application.dataPath + "/AssetBundle/Windows32";

#elif UNITY_ANDROID
	"http://192.168.1.254";
#elif UNITY_IPHONE
    "http://192.168.1.254";
#endif

    //本地配置路径
    public static readonly string ConfigURL =
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
 "file://" + Application.dataPath + "/StreamingAssets";

#elif UNITY_ANDROID
	"jar:file://" + Application.dataPath + "!/assets";
#elif UNITY_IPHONE
    "file://" +Application.dataPath + "/Raw";
#endif

    //直接读取配置
    public static readonly string ConfigPath =
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
 Application.dataPath + "/StreamingAssets/config";

#elif UNITY_ANDROID
	 Application.dataPath + "!/assets/config";
#elif UNITY_IPHONE
    Application.dataPath + "/Raw/config";
#endif
}

public class CPrefabPaths
{
    static public string LoadingWinRoot = "UI/LoadingWinRoot";
    static public string Loading = "UI/Loading/Loading";
    static public string Login = "UI/Login/Login";
    static public string GM = "UI/Extra/GM";
    static public string Error = "UI/Error/ErrorWin";

    static public string SystemWinRoot = "UI/SystemWinRoot";
    static public string ErrorTip = "UI/Error/ErrorTip";
    static public string ErrorMessage = "UI/Error/ErrorMessage";
    static public string GuideSkillGesture = "UI/Guide/GuideSkillGesture";
    static public string UIGateSystem = "UI/UIGateSystem/UIGateSystem";
    static public string Resurrection = "UI/Resurrection/Resurrection";

    static public string MonsterHurtMessage = "UI/Hurt/MonsterHurtMessage";
    static public string PlayerHurtMessage = "UI/Hurt/PlayerHurtMessage";

    static public string UpdateMessage = "UI/Update/Update";

    static public string PlayerStatus = "UI/PlayerStatus";

    static public string MainMenu = "UI/MainMenu/MainMenu";
    static public string UILoginModeList = "UI/MainMenu/ModeList";
    static public string UILoginPlotList = "UI/MainMenu/PlotList";
    static public string UIFightResult = "UI/FightResult/FightResult";

    //static public string UIMainWin = "UI/UIRoot/UIMainWin";
    //static public string UIPlayerInfo = "UI/UIRoot/UIPlayerInfo";
    //static public string UISystemWin = "UI/UIRoot/UISystemWin";

    //登陆系统资源
    //static public string LoginByNamePwd = "UI/Login/LoginByNamePwd";

    //UI系统资源路径
    //static public string UIHoleWin = "UI/Hole/HoleWindow";
}

//entity类型
public enum CEntityType
{
    None = 0, //无类型
    Player = 1,
    Monster = 2,
    NPC = 3,
    Gear = 4,		//游戏机关
    RangeTrap = 5,	//范围陷阱
    Space = 7,		//空间

    TargetPoint = 1001, //传送目标点
};

/// <summary>
/// 死亡表现类型
/// </summary>
public enum CDeadType : int
{
    None = -1,
    Normal = 0,      //标准
    Dissolution = 1, //溶解
    Cutting = 2,     //切割
    Fracture = 3,    //破碎
}

/// <summary>
/// 玩家职业定义。
/// 注意：这个必须与服务器的定义保持一致
/// </summary>
public enum eProfession : sbyte
{
    ZhanShi = 0,  // 战士
    JianKe = 1,   // 剑客
    SheShou = 2,  // 射手
    FaShi = 3,    // 法师
}

/// <summary>
/// 游戏对象之间的关系，主要是技能用来确定对象是否符合作用条件///
/// </summary>
public enum eCampRelationship : int
{
    Irrelative = -1,  // unknow
    Friendly = 0,
    Hostile = 1,
    neutrality = 2,
};

public enum eTargetRelationship : int
{
    None = -1, //其他关系
    All = 0,	//所有类型目标
    HostilePlayers = 1, 	//敌对玩家
    FriendlyPlayers = 2,	//友方玩家（不包括自己）
    Own = 3,	//自己
    Teammates = 4,	//同队玩家（不包括自己）
    HostileMonster = 5,	//敌对怪物
    FriendlyMonster = 6,	//友方怪物
    NeutralityPlayers = 7, //中立玩家
    NeutralityNPC = 8, //中立NPC
};

//模型类型
public enum CModelType
{
    Avatar = 1,
    Item,      //物品模型
    Gate,      //传送门模型
    Equipment, //装备
    CSAvatar,  //创世人物模型
};

//Layer层
public enum eLayers
{
    Default = 0,

    UI = 5,
    Entity = 9,
    Trap = 10,
    Bullet = 11,
    Broken = 12,
    Diban = 15,
    Shield = 16,
    Ice = 18,
    Fire = 19,
};

//武器放置点
public enum CWeaponNode
{
    rightHand = 1,
    leftHand = 2,
    bothHand = 3,
    bothHandTurn = 4,
};

public struct CModelNodePath
{
    static public string r_h = "Model/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Neck/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Bip001 R Hand/r_h";
    static public string l_h = "Model/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Neck/Bip001 L Clavicle/Bip001 L UpperArm/Bip001 L Forearm/Bip001 L Hand/l_h";
    static public string bip001_Prop3_hs = "Model/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Prop3";
    static public string vr_l_h = "[CameraRig]/Controller (left)/LeftController/Weapon";
    static public string vr_r_h = "[CameraRig]/Controller (right)/RightController/Weapon";
    static public string cs_r_h = "Model/root/hips/spine/spine1/spine2/spine3/rightShoulder/rightArm/rightForeArm/rightForeArmRoll/rightHand/right_hand";
    static public string cs_l_h = "Model/root/hips/spine/spine1/spine2/spine3/leftShoulder/leftArm/leftForeArm/leftForeArmRoll/leftHand/left_hand";
}

public enum ePlayerControlStatus
{
    avatar = 0, //玩家状态
    talisman = 1, //法宝状态
    riding = 2, //骑乘状态
};

/// <summary>
/// 动作位移同步标志，用于标识使用什么方式来同步来自动作的位移
/// </summary>
public enum eAnimatPosSyncFlag : int
{
    NotSync = 0,              // 不同步（即动作位移不会影响GameObject的位置）
    SyncAny,                  // 无条件同步
    NotSyncOnCollideAny,      // 碰到任何东西（Collider)时停下来
    NotSyncOnCollideStoper,   // 碰到墙一类的会停下来
    NotSyncOnCollideAvatar,   // 碰到“可战斗单位”会停下来
    NotSyncOnCollideEnemy,    // 碰到敌人会停下来
}

/// <summary>
/// 状态定义，每个状态之间都是互斥的，也就是同一时间仅存在一个状态
/// </summary>
public enum eEntityStatus : int
{
    /// <summary>
    /// 未决状态表示当前处于混沌中，不应该做任何事情
    /// </summary>
    Pending = 0,

    Idle,         // 待机
    Fight,        // 战斗
    Death,        // 死亡
}

/// <summary>
/// 游戏对象标志集合，用于给相一些相同的事情做互斥，例如正在被击退……
/// 注意：定义与取值必须与服务器端保持一致
/// </summary>
public enum eEffectStatus : int
{
    Moving = 0,       // 移动中
    SpellCasting,     // 施法中
    HitBy,            // 受击中
    Invincible,       // 无敌状态
    Dizziness,        // 眩晕状态
    SuperBody,        // 霸体状态
    Fixed,            // 固定状态
    Frozen,           // 冰冻状态
    Max,              // can't great than 31
};

/// <summary>
/// 用于标志影响行为的设置，即当被眩晕时，不能做啥动作等。
/// 这里每一行名称都应该与上面的eEffectStatus对应
/// </summary>
public class Status2Action
{
    public static eActionRestrict[][] map = new eActionRestrict[][] {
        new eActionRestrict[] { }, //for Moving
        new eActionRestrict[] { eActionRestrict.ForbidMove, }, //for SpellCasting
        new eActionRestrict[] { eActionRestrict.ForbidMove, eActionRestrict.ForbidSpell, eActionRestrict.ForbidThink }, //for HitBy
        new eActionRestrict[] { },//for Invincible
        new eActionRestrict[] { eActionRestrict.ForbidMove, eActionRestrict.ForbidSpell, eActionRestrict.ForbidThink },//for Dizziness
        new eActionRestrict[] { eActionRestrict.ForbidHitBy }, //for SuperBody
        new eActionRestrict[] { eActionRestrict.ForbidMove }, //for Fixed
        new eActionRestrict[] { eActionRestrict.ForbidMove, eActionRestrict.ForbidSpell, eActionRestrict.ForbidThink}, //for Frozen
    };
};

public class EntityStatus2Action
{
    public static eActionRestrict[][] map = new eActionRestrict[][] {
        new eActionRestrict[] { eActionRestrict.ForbidMove, eActionRestrict.ForbidSpell, eActionRestrict.ForbidHitBy, eActionRestrict.ForbidUseProp }, //Pending
        new eActionRestrict[] { }, //Idle
        new eActionRestrict[] { }, //Fight
        new eActionRestrict[] { eActionRestrict.ForbidMove, eActionRestrict.ForbidSpell, eActionRestrict.ForbidUseProp, eActionRestrict.ForbidThink },//Death
    };
};

/// <summary>
/// 行为动作禁止标志
///	注意：定义与取值必须与服务器端保持一致
/// </summary>
public enum eActionRestrict : int
{
    ForbidMove = 0,    // 禁止移动
    ForbidSpell,       // 禁止施法（攻击）
    ForbidHitBy,       // 禁止受击（不被受击，即不会设置“受击”状态）
    ForbidThink,       // 禁止思考（NPCAI）
    ForbidUseProp,     // 禁止使用道具
    Max,  // can't great than 31
};

public enum AnimatorLayer : int
{
    Default = 0,
    Combat,  // for combat action
    // more...
};

public enum eUIShowType
{
    common = 0x01, //普通
    reChained = 0x02, //重新链式,清空以前的链式内容，重新开始
    addChained = 0x04, //增加链式
    endChained = 0x08, //结束链式
}

public enum eUIHideType
{
    common = 0x01, //普通（如果之前增加链式，并且在链式顶层，默认SubChained）
    endChained = 0x02, //结束链式
    subChained = 0x04, //减少链式
    disChained = 0x08, //不影响链式的隐藏
}

public enum eUIActivateType
{
    none, //无意义
    show, //显示
    hide, //隐藏
}

public enum eFightResultType  //伤害结果类型
{
    Hit = 0x01,   // 命中
    Crit = 0x02,  // 暴击
}

public enum SkillType
{
    Shoot,  //射击型
    Point,  //定点型
    Direction,  //方向型
    Target, //目标型
}

public enum CQuestStatus
{
    receive = 0,    //已接受
    doing = 1,		//进行中
    complete = 2,	//已完成
    lose = 3,		//已失败
    reward = 4,		//已奖励
    action = 5, 	//已执行

    missed = 100, //未接取状态
}

public enum CQuestType
{
    mainQuest = 1,	//主线任务
    branchQuest = 2, //支线任务
    dayQuest = 3, //日常任务
    guildQuest = 4,//帮会任务
}

//任务品质
public enum CQuestQualityType
{
    white = 1, //白
    green = 2, //绿
    blue = 3, //蓝
    violet = 4, //紫
    orange = 5, //橙
}

public class eVirtualItemID
{
    public const int money = 60001;
    public const int exp = 60002;
    public const int potential = 60003;
}

public class CQuestConst
{
    static public string inlayCrystalDescribe = "晶石镶嵌";
    static public string petFightOutDescribe = "幻兽出战";
    static public string petAbsorbDescribe = "幻兽吸收";
    static public string equipUpgradeDescribe = "装备强化";
    static public string equipTalismanDescribe = "装备法宝";
    static public string wieldTalismanDescribe = "祭起法宝";
    static public string itemSynthesisDescribe = "物品合成";
    static public string buildBuildingDescribe = "建造建筑";
    static public string upgradeBuildingDescribe = "升级建筑";
    static public string useItemDescribe = "使用物品";

    static public Dictionary<CQuestQualityType, string> qualityColour = new Dictionary<CQuestQualityType, string>()
    {
        {CQuestQualityType.white, "ffffff"},
        {CQuestQualityType.green,  "0dd533"},
        {CQuestQualityType.blue,  "00aeff"},
        {CQuestQualityType.violet,  "A020F0"},
        {CQuestQualityType.orange,  "CD950C"},
    };
}

/// <summary>
/// 任务刷新消耗
/// </summary>
public class QuestRefreshCoinCost
{
    public static int refreshAllDailyTaskCost = 15;//刷新全部日常任务元宝消耗
}

/// <summary>
/// 对话消息类型
/// </summary>
public enum eDialogMsgType : int
{
    None = -1,
    Dialog = 0,     //任务对话
    Gossip = 1,     //闲聊
    ReceiveQuest = 2,   //接任务对话
    DeliveryQuest = 3,  //交付任务对话
}

/// <summary>
/// 功能NPC任务状态显示
/// </summary>
public enum CQuestNpcStatus
{
    NULL = 0,       //无状态
    receive = 1,    //有接取的任务
    doing = 2,		//任务进行中
    complete = 3,	//任务完成
}

/// <summary>
/// 错误信息
/// </summary>
public class CErrorStartID
{
    static public int guild = 11000;
    static public int team = 12000;
    static public int fight = 13000;
    static public int player = 14000;
    static public int chat = 15000;
    static public int spell = 16000;
    static public int kigbag = 17000;
    static public int quest = 18000;
    static public int shop = 20000;
    static public int pet = 21000;
    static public int friends = 22000;
    static public int talisman = 23000;

    static public int mail = 25000;

    static public int reward = 27000;
}

/// <summary>
/// 临时技能配置表
/// </summary>
public class SkillConfig
{
    public SkillConfig(int skillID, SkillType skillType, string gestureName, string weaponEffectName)
    {
        this.skillID = skillID;
        this.skillType = skillType;
        this.gestureName = gestureName;
        this.weaponEffectName = weaponEffectName;
    }

    public int skillID;
    public SkillType skillType;
    public string gestureName;
    public string weaponEffectName;
}

/// <summary>
/// 临时传送门配置表
/// </summary>
public class GateConfig
{
    public GateConfig(string id, string gateName, string resPath)
    {
        this.id = id;
        this.gateName = gateName;
        this.resPath = resPath;
    }

    public string id;
    public string gateName;
    public string resPath;
}

public class ClientConst
{
    #region 临时技能配置表

    public static Dictionary<int, SkillConfig> skillConfigs = new Dictionary<int, SkillConfig>() {
        { 33001, new SkillConfig( 33001,SkillType.Shoot,"fire","huoweapon")},
        { 12, new SkillConfig( 12,SkillType.Point,"leidian","dianweapon")},
        { 13, new SkillConfig( 13,SkillType.Point,"taiji","lvseweapon")},
        { 14, new SkillConfig( 14,SkillType.Shoot,"wave3","bingweapon")},
        { 15, new SkillConfig( 15,SkillType.Point,"xingxing","zaohuanweapon")},
        { 17, new SkillConfig( 17,SkillType.Direction,"zhenkai","")},
        { 18, new SkillConfig( 18,SkillType.Direction,"shanxian","")},
        { 1001, new SkillConfig( 1001,SkillType.Direction,"first","")},
        { 1002, new SkillConfig( 1002,SkillType.Direction,"second","")},
        { 1003, new SkillConfig( 1003,SkillType.Direction,"third","")},
        { 1000001, new SkillConfig( 1000001,SkillType.Shoot,"dun","")},
        { 1000002, new SkillConfig( 1000002,SkillType.Shoot,"dunCancel","")},
        { 30, new SkillConfig( 30,SkillType.Target,"dian","")},
        { 31, new SkillConfig( 31,SkillType.Point,"blackhole","")},
    };

    public static SkillConfig GetSkillConfigByGestureName(string gestureName)
    {
        if (skillConfigs != null && skillConfigs.Count > 0)
        {
            Dictionary<int, SkillConfig>.Enumerator enumerator = skillConfigs.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (gestureName == enumerator.Current.Value.gestureName)
                    return enumerator.Current.Value;
            }
        }
        return null;
    }

    #endregion 临时技能配置表

    #region 临时传送门配置表

    public static Dictionary<string, GateConfig> gateConfigs = new Dictionary<string, GateConfig>() {
        { "99999999", new GateConfig( "99999999","锁灵塔","UI/UIGateSystem/GateSoulingta")},
        { "30200002", new GateConfig( "30200002","祭剑台","UI/UIGateSystem/GateJijiantai")},
    };

    #endregion 临时传送门配置表
}