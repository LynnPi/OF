using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class BattleSkillPanelCtrlButton : MonoBehaviour {
    public System.Action ClickEntranceCallback = delegate { };
    private int AnimHashActive_;
    private Animator Anim_;
    private Button EntranceBtn_;
    void Awake() {
        Anim_ = GetComponent<Animator>();
        AnimHashActive_ = Animator.StringToHash("active");
        EntranceBtn_ = transform.FindChild("Button").GetComponent<Button>();
        EntranceBtn_.onClick.AddListener(OnClickBtn);
    }

    private void OnClickBtn() {
        Debugger.Log("BattleSkillPanelEntrance.OnClickBtn()...");
        ClickEntranceCallback();
    }
    public void EnableEntrance(bool b) {
        Anim_.SetBool(AnimHashActive_, b);
    }  
}
