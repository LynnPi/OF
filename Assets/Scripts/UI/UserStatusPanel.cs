using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UserStatusPanel : UIPanelBehaviour {
    public const float DEPLOY_ENTER_SCENE_DURATION = 11.11F;
    public static UserStatusPanel Instance { get; private set; }

    private Text MinuteText_;
    private Text SecondText_;

    // 上限时间,秒
    private int MaxTime = 300;

    protected override void OnAwake() {

        Instance = this;

        MinuteText_ = transform.FindChild( "Time_Counter/Text_Time_Minute" ).GetComponent<Text>();
        SecondText_ = transform.FindChild( "Time_Counter/Text_Time_Second" ).GetComponent<Text>();        
    }

    protected override void OnShow( params object[] args ) {
        AudioManager.Instance.PlayMusic = true;
        MinuteText_.text = "00";
        SecondText_.text = "00";
        StartCoroutine( DelayEnterDeploy() );
    }

    public void ShowTime( int nowTime ) {
        // 剩余时间
        int timeLeft = MaxTime - nowTime;

        int minute = timeLeft / 60;
        string minuteStr = minute < 10 ? ("0" + minute) : minute.ToString();

        int second = timeLeft % 60;
        string secondStr = second < 10 ? ("0" + second) : second.ToString();

        MinuteText_.text = minuteStr;
        SecondText_.text = secondStr;
    }

    private IEnumerator DelayEnterDeploy() {
        yield return new WaitForSeconds( 0.1f );
        // 初始化战斗信息
        BattleSys.InitBattleInfo( 1 );
        // 进入布阵场景
        SceneManager.EnterScene( SceneManager.SceneType.Deploy );
        //等相机动画播完了再拨MaskFade动画
        yield return new WaitForSeconds(DEPLOY_ENTER_SCENE_DURATION);
        GameObject go = Global.CreateUI("DeployCameraMaskFade", UIManager.PanelCanvas.gameObject);
        go.transform.SetAsLastSibling();
        yield return new WaitForSeconds(1f);
        Destroy(go);
    }

}