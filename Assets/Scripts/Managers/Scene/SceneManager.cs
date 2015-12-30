using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour {
    public static SceneManager Instance { get; private set; }

    public enum SceneType {
        Launcher = 0,
        Loading,
        Login,
        MainScene,
        BattleScene,
        Deploy
    }

    private static SceneType        SceneType_ = SceneType.Launcher;

    private static GameObject       SceneCtrl_;

    private void Awake() {
        Instance = this;
        //DontDestroyOnLoad( gameObject );   
    }

    private void Start() {
        EnterScene( SceneType.Launcher );
    }

    private static void InitScene() {
        if( SceneCtrl_ ) {
            Destroy( SceneCtrl_ );
        }

        SceneCtrl_ = new GameObject( SceneType_.ToString() );
        SceneCtrl_.transform.parent = Instance.transform;
        switch(SceneType_){
            case SceneType.Launcher:
                SceneCtrl_.AddComponent<Launcher>();
                break;
            case SceneType.Loading:
                UIManager.ShowPanel<LoadingPanel>();
                break;
            case SceneType.Login:
                // 重新初始化数据
                CameraManager.Instance.MainCamera.transform.localPosition = Vector3.zero;
                CameraManager.Instance.MainCamera.transform.localEulerAngles = Vector3.zero;
                Animator animator = CameraManager.Instance.MainCamera.GetComponent<Animator>();
                if( animator != null )
                    Destroy( animator );
                UIManager.ShowPanel<LoginPanel>();
                break;
            case SceneType.MainScene:
                UIManager.ShowPanel<UserStatusPanel>();
                break;
            case SceneType.BattleScene:
                SceneCtrl_.AddComponent<BattleSceneDisplayManager>();
                UIManager.ShowPanel<BattleSkillPanel>();
                UIManager.ShowPanel<BattleCameraControlPanel>();
                UIManager.ShowPanel<BattleMiniMapPanel>();
                UIManager.ShowPanel<BattleRetreatEntrance>();
                break;
            case SceneType.Deploy:
                SceneCtrl_.AddComponent<DeploySceneManager>();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 得到场景类型
    /// </summary>
    /// <returns></returns>
    public static SceneType GetSceneType() {
        return SceneType_;
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="sceneType"></param>
    public static void EnterScene( SceneType sceneType ) {
        // 如果上一个场景是战斗,释放缓存
        if( SceneType_ == SceneType.BattleScene ) {
            BattleSceneDisplayManager.Instance.Reset();
            AssetLoader.ClearAllBattleCaches();
            AssetLoader.ReleaseCaches();
        }
        SceneType_ = sceneType;
        CameraManager.InitCamera();
        CameraManager.SetTouchActive( sceneType >= SceneType.BattleScene );
        InitScene();
        AudioManager.InitMusic( sceneType );
    }


 /*
 *                                     Game World Area Sample Map
 *                           ---------------------*----------------------
 *                           -       GameWorldOriginPosition(0,0,0)     -
 *                           -                                          -
 *                           -               Base Area                  -
 *                           -                                          -
 *                           -                                          - 
 *                           -------------------------------------------- 
                             -               Interval                   -
 *                           --------------------------------------------   
 *                           -                                          -
 *                           -               Depth Area                 -
 *                           -                                          -
 *                           --------------------------------------------  
 *                           -               Interval                   -
 *                           --------------------------------------------                               
 *                           -                                          -
 *                           -               Deploy Area                -
 *                           -                                          -
 *                           --------------------------------------------    
 *                           -               Other Area                 -
 *                           -                                          -
 * 
 */
}