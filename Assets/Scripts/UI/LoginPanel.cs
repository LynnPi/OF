using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoginPanel : UIPanelBehaviour {

    protected override void OnAwake() {
        Button button = transform.FindChild( "Button_Play" ).GetComponent<Button>();
        button.onClick.AddListener( OnPlayClick );


	}

    protected override void OnRegEvent() {
        PlayerSys.EventOnLogin += PlayerSys_EventOnLogin;
    }

    protected override void OnUnregEvent() {
        PlayerSys.EventOnLogin -= PlayerSys_EventOnLogin;
    }

    protected override void OnShow( params object[] args ) {
        AudioManager.Instance.PlayMusic = true;
        PlayerSys.ResetPlayerInfo();
        EnableLoginCameraAnimation();
        EnableShipAnimation();
        StartCoroutine(ShakeCameraAtFocusTime());
    }

    protected override void OnClose() {
        DisableLoginCameraAnimation();
        DisableableShipAnimation();
    }
    void PlayerSys_EventOnLogin() {
        SceneManager.EnterScene( SceneManager.SceneType.MainScene );
    }

    void OnPlayClick() {
        PlayerSys.Login();
    }
    Animator LoginCameraAnimator_;

    private void EnableLoginCameraAnimation() {
        RuntimeAnimatorController res = Resources.Load("Login/camera_anim/Main_Camera_login") as RuntimeAnimatorController;

        if (res == null) {
            Debugger.LogError("Get Login Animation Failed!");
            return;
        }
        LoginCameraAnimator_ = CameraManager.Instance.MainCamera.transform.parent.gameObject.GetComponent<Animator>();
        if( LoginCameraAnimator_ == null )
            LoginCameraAnimator_ = CameraManager.Instance.MainCamera.transform.parent.gameObject.AddComponent<Animator>();
        LoginCameraAnimator_.runtimeAnimatorController = res;
        CameraManager.Instance.MainCamera.transform.parent.position = new Vector3(70f, 180f, 100f);
        CameraManager.Instance.MainCamera.transform.parent.eulerAngles = new Vector3(356f, 267f, 0f);
    }

    private void DisableLoginCameraAnimation() {
        Destroy(LoginCameraAnimator_);
    }

    private GameObject LoginShipEffect_;
    private void EnableShipAnimation() {
        GameObject prefab = Resources.Load("Login/ship_anim/scenes/login_ship") as GameObject;
  
        LoginShipEffect_ = Instantiate(prefab) as GameObject;
        LoginShipEffect_.transform.localScale = Vector3.one;
        LoginShipEffect_.transform.eulerAngles = Vector3.zero;
    }

    private void DisableableShipAnimation() {
        Destroy(LoginShipEffect_);
        LoginShipEffect_ = null;
    }

    private IEnumerator ShakeCameraAtFocusTime() {
        yield return new WaitForSeconds(17.5f);
        CameraManager.Shake(1.15f, 1.2f);
    }

}
