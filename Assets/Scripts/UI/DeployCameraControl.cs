using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class DeployCameraControl : UIPanelBehaviour{
    public enum CameraState { OnAreaOfDeploy, OnAreaOfBase}

    private float SWITCH_CAM_ANIM_DURATION = 0.67F;
    private Button SwitchBtn_;

    private Animator Anim_;
    private Animator Anim {
        get {
            if (Anim_ == null) {
                Anim_ = transform.FindChild("Controller").GetComponent<Animator>();
            }
            return Anim_;
        }
    }

    private CameraState State_;
    private CameraState State {
        get {
            return State_;
        }
        set {
            if (State_.Equals(value)) return;
            State_ = value;
            string triggerName = State_ == CameraState.OnAreaOfBase ? "switch_to_base" : "switch_to_deploy";
            StartCoroutine(PlayAnimation(triggerName));
        }
    }
    protected override void OnAwake() {
        SwitchBtn_ = transform.FindChild("Controller/Button_Switch").GetComponent<Button>();
        SwitchBtn_.onClick.AddListener(OnCameraClick);
    }

    private void OnCameraClick() {
        if (State == CameraState.OnAreaOfDeploy) {
            State = CameraState.OnAreaOfBase;
            CameraManager.Instance.SetCameraType( CameraManager.CameraStatusType.DeployMoveToBase );
        }
        else if (State == CameraState.OnAreaOfBase) {
            State = CameraState.OnAreaOfDeploy;
            CameraManager.Instance.SetCameraType( CameraManager.CameraStatusType.DeployMoveToDeploy );
        }
        else {}   
    }

    private IEnumerator PlayAnimation(string triggerName) {
        SwitchBtn_.interactable = false;
        Anim.SetTrigger(triggerName);
        yield return new WaitForSeconds(SWITCH_CAM_ANIM_DURATION);
        SwitchBtn_.interactable = true;
    }
}