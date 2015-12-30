using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
public class RetreatPopWindow : UIPanelBehaviour {
    private Button OkBtn_;
    private Button CancelBtn_;
    private float AnimDuration_ = 0.167f;
    private Animator Anim_;
    protected override void OnAwake() {
        OkBtn_ = transform.FindChild("Panel/Button/Button_OK").GetComponent<Button>();
        OkBtn_.onClick.AddListener(OnClickOk);
        CancelBtn_ = transform.FindChild("Panel/Button/Button_Cancel").GetComponent<Button>();
        CancelBtn_.onClick.AddListener(OnClickCancel);
        Anim_ = GetComponentInChildren<Animator>();
    }

    private void OnClickCancel() {
        StartCoroutine(WaitToClose(delegate { UIManager.ClosePanel<RetreatPopWindow>(); }));
    }

    private void OnClickOk() {
        StartCoroutine(WaitToClose(delegate { SceneManager.EnterScene(SceneManager.SceneType.Login); }));
    }

    private IEnumerator WaitToClose(System.Action call) {
        Anim_.SetTrigger("hide");
        yield return new WaitForSeconds(AnimDuration_);
        if(call != null) call();
    }
}
