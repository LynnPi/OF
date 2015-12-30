using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class BattleCameraControlPanel : UIPanelBehaviour {
    private float SWITCH_CAM_ANIM_DURATION = 0.75F;
    private Button SwitchBtn_;
    private Animator Anim_;

    protected override void OnAwake() {
        SwitchBtn_ = transform.FindChild("Button_Switch").GetComponent<Button>();
        SwitchBtn_.onClick.AddListener(OnCameraClick);
        Anim_ = transform.FindChild("Background").GetComponentInChildren<Animator>();
    }
    private void OnCameraClick() {
        CameraManager.Instance.SetCameraType( CameraManager.CameraStatusType.NormalLerp );
        BattleSkillPanel.Instance.FoldSkillPanel();
        StartCoroutine( PlayAnimation() );
    }
    private IEnumerator PlayAnimation() {
        SwitchBtn_.interactable = false;
        Anim_.SetTrigger("pressed");
        yield return new WaitForSeconds(SWITCH_CAM_ANIM_DURATION);      
        SwitchBtn_.interactable = true;
    }
}
