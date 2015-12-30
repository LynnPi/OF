using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 战斗技能按钮
/// </summary>
public class BattleSkillButton : MonoBehaviour {
    public enum SkillState { Disable, Able, Selected, Cooling}
    public System.Action<int> SelectedSkillCallback = delegate { };
    public System.Action<int> FireSkillCallback = delegate { };
    //five state: Disable, Enable, Selected, CoolDown 

    private const string ANIM_TRIGGER_DISABLE = "disable";
    private const string ANIM_TRIGGER_ABLE = "able";
    private const string ANIM_TRIGGER_SELECTED = "selected";
    private const string ANIM_TRIGGER_COOL_DOWN = "cool";
    private Button FireBtn_;
    private Button EnableBtn_;
    private Animator Anim_;
    private Image CoolDownMask_;
    public bool IsCooling;

    public int SkillID;
    private void Awake() {
        FireBtn_ = transform.FindChild("Root/able").GetComponent<Button>();
        FireBtn_.onClick.AddListener(StepIntoSelectedState);

        EnableBtn_ = transform.FindChild("Root/selected").GetComponent<Button>();
        EnableBtn_.onClick.AddListener(FireSkill);

        Anim_ = GetComponent<Animator>();
        CoolDownMask_ = transform.FindChild("Root/cool/mask").GetComponent<Image>();
    }

    private void StepIntoSelectedState() {
        Anim_.SetTrigger(ANIM_TRIGGER_SELECTED);
        SelectedSkillCallback(int.Parse(gameObject.name));
    }

    private void FireSkill() {
        //Debugger.Log("FireSkill()...");
        FireSkillCallback(int.Parse(gameObject.name));
    }

    public void EnableSkill(bool b) {
        //Debugger.Log("EnableSkill()..." + b);
        Anim_.SetTrigger(b ? ANIM_TRIGGER_ABLE : ANIM_TRIGGER_DISABLE);
    }

    public void ForbidFireSkill(int id) {
        if (id != SkillID) return;
        FireBtn_.interactable = false;
    }

    public void CoolDownSkill(int id) {    
        if (id != SkillID) return;
        //Debugger.Log("CoolDownSkill()...");
        Anim_.SetTrigger(ANIM_TRIGGER_COOL_DOWN);
        float duration = GlobalConfig.GetSkillReference(id).cd/1000f;
        StartCoroutine(FillCoolDownMask(duration));
    }

    /// <summary>
    /// CD动画效果
    /// </summary>
    /// <returns></returns>
    private IEnumerator FillCoolDownMask(float duration) {
        IsCooling = true;
        CoolDownMask_.fillAmount = 1f;
        float animatedTime = 0f;
        while (animatedTime <= duration) {
            animatedTime += Time.deltaTime;
            CoolDownMask_.fillAmount = 1 - animatedTime / duration;
            yield return Time.deltaTime;
        }
        IsCooling = false;

        FireBtn_.interactable = true;

        bool getIsAble = true;//暂时true
        EnableSkill(getIsAble);
    }

    public void ConfigIcon(string disableIconName, string ableIconName) {
        Image icon;
        //配置disable的icon
        icon = transform.FindChild("Root/disable/icon").GetComponent<Image>();
        Global.SetSprite(icon, disableIconName, true);

        //配置able的icon
        icon = transform.FindChild("Root/able/icon").GetComponent<Image>();
        Global.SetSprite(icon, ableIconName, true);
        icon = transform.FindChild("Root/cool/icon").GetComponent<Image>();
        Global.SetSprite(icon, ableIconName, true);
    }
}
