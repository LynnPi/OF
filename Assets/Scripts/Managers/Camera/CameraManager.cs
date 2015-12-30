using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour {

    /// <summary>
    /// 相机缩放类型
    /// </summary>
    public enum CameraScaleType {
        Min = 0,            // 最近
        Normal,             // 普通
        Max                 // 最远
    }

    /// <summary>
    /// 相机状态类型
    /// </summary>
    public enum CameraStatusType {
        // 战斗场景相机
        NormalLerp = -1,    // 普通状态下插值移动的相机
        Normal,             // 普通状态下跟随相机
        Free,               // 自由状态相机
        Lock,               // 选中状态相机

        // 布阵场景相机
        AutoMoveInDeploy,   // 布阵时自动移动相机
        DeployMoveToBase,   // 布阵时相机移动到基地区
        DeployMoveToDeploy, // 布阵时相机移动到布阵区
        Deploy,             // 布阵时可操作的相机

        SkillSelect,        // 技能选择时的相机

        All,                // 全部
    }

    public class CameraInfo {
        // 位置
        public Vector3 position;

        // 旋转角度
        public Vector3 angles;
    }

    public static CameraManager Instance { get; private set; }

    private GameObject          MainCamera_;

    public GameObject MainCamera {
        get {
            return MainCamera_.transform.GetChild( 0 ).gameObject;
        }
    }

    private Camera MainCameraInstance_;
    public Camera MainCameraInstance {
        get {
            if (MainCameraInstance_ == null) {
                MainCameraInstance_ = MainCamera.GetComponent<Camera>();
                if (!MainCameraInstance_) {
                    Debugger.Log("Not Found Main Camera Component!");
                }
            }
            return MainCameraInstance_;
        }
    }

    private Camera UICamera_;
    public Camera UICamera {
        get {
            if( UICamera_ == null ) {
                UICamera_ = GameObject.Find( "UIManager/UICamera" ).GetComponent<Camera>();
            }
            return UICamera_;
        }
    }

    private Transform           CameraTargetTrans_;

    private float               YMinLimit_ = 245;
    private float               YMaxLimit_ = 359;

    private float               NormalLerpPositionDamping_ = 200;
    private float               NormalPositionDamping_ = 70;
    private float               NormalRotationDamping_ = 60;

    private float               LockPositionDamping_ = 200;
    private float               LockRotationDamping_ = 60;

    private float               OtherPositionDamping_ = 200;
    private float               OtherRotationDamping_ = 40;

    private float               OriginPositionX = 43;
    private float               OriginPositionZ = 50;
    private float               OriginRotationY = 325;

    private float               DeployOriginPositionX = 67.63f;
    private float               DeployOriginPositionY = 163.9f;

    private float               DeployOriginRotationX = 64.65f;
    private float               DeployOriginRotationY = 315.8f;
    private float               DeployOriginRotationZ = 0;

    private float               CameraZdistance = 30;
    private float               NormalCameraZDistance = 0;

    private CameraInfo[]        CameraInfoList_;

    private GameObject          RenderTextureCamera_;

    private RenderTexture       RoleRenderTargetTexture_;

    private CameraStatusType    CameraStatusType_ = CameraStatusType.Normal;

    private CameraScaleType     CameraScaleType_ = CameraScaleType.Normal;

    private Timer               LastScaleTime_ = new Timer();

    //初始位置
    private Vector3             originPosition;
    //初始旋转
    private Vector3             originEulerAngles;
    //强度
    private float               shake_intensity;
    //每秒衰减
    private float               shake_decay;
    //震动时长
    private float               shake_time;


    public static event Action EventOnClickBlankScreenZone = delegate { };
    public static event Action<CameraStatusType> EventOnChangeCameraStatus = delegate { };
    public static event Action<GameObject> EventOnClickShip = delegate { };

    private void Awake() {
        Instance = this;

        int tempNum = (int)CameraStatusType.All;
        CameraInfoList_ = new CameraInfo[tempNum];

        RoleRenderTargetTexture_ = new RenderTexture( 512, 512, 24, RenderTextureFormat.ARGB32 );
        RoleRenderTargetTexture_.name = "3DUIPlayer";
        RoleRenderTargetTexture_.antiAliasing = 1;
        RoleRenderTargetTexture_.wrapMode = TextureWrapMode.Clamp;
        RoleRenderTargetTexture_.filterMode = FilterMode.Bilinear;
        RoleRenderTargetTexture_.generateMips = false;
        RoleRenderTargetTexture_.anisoLevel = 0;

        //EasyTouch.On_TouchStart += EasyTouch_On_TouchStart;
        EasyTouch.On_SimpleTap += EasyTouch_On_SimpleTap;
        EasyTouch.On_Swipe += EasyTouch_On_Swipe;
        //EasyTouch.On_Swipe2Fingers += EasyTouch_On_Swipe2Fingers;
        EasyTouch.On_Twist += EasyTouch_On_Twist;
        EasyTouch.On_PinchIn += EasyTouch_On_PinchIn;
        EasyTouch.On_PinchOut += EasyTouch_On_PinchOut;
    }

    private void OnDestroy() {
        //EasyTouch.On_TouchStart -= EasyTouch_On_TouchStart;
        EasyTouch.On_SimpleTap -= EasyTouch_On_SimpleTap;        
        EasyTouch.On_Swipe -= EasyTouch_On_Swipe;
        //EasyTouch.On_Swipe2Fingers -= EasyTouch_On_Swipe2Fingers;
        EasyTouch.On_Twist -= EasyTouch_On_Twist;
        EasyTouch.On_PinchIn -= EasyTouch_On_PinchIn;
        EasyTouch.On_PinchOut -= EasyTouch_On_PinchOut;
    }

    /// <summary>
    /// 触摸操作开始,改变相机模式
    /// </summary>
    /// <param name="gesture"></param>
    //void EasyTouch_On_TouchStart( Gesture gesture ) {
    //    if( CameraStatusType_ == CameraStatusType.Deploy ) return;
    //    if( CameraStatusType_ == CameraStatusType.Lock ) return;
    //    if( gesture.pickedObject == null ) {
    //        if( CameraStatusType_ == CameraStatusType.Free ) return;
    //        SetCameraType( CameraStatusType.Free );
    //    }
    //}

    /// <summary>
    /// 点击
    /// </summary>
    /// <param name="gesture"></param>
    void EasyTouch_On_SimpleTap( Gesture gesture ) {
        if( CameraStatusType_ < CameraStatusType.Normal ) return;
        if( CameraStatusType_ == CameraStatusType.Deploy ) return;
        if( CameraStatusType_ == CameraStatusType.AutoMoveInDeploy ) return;
        if( CameraStatusType_ == CameraStatusType.DeployMoveToBase ) return;
        if( CameraStatusType_ == CameraStatusType.DeployMoveToDeploy ) return;
        if( CameraStatusType_ == CameraStatusType.SkillSelect ) {
            BattleSceneDisplayManager.SelectSkillTarget( gesture, MainCamera_.transform.position.y );
            return;
        }
        ClickGameObjectInBattle( gesture.pickedObject );
    }

    // 滑动
    void EasyTouch_On_Swipe( Gesture gesture ) {
        if( CameraStatusType_ < CameraStatusType.Normal ) return;
        if( CameraStatusType_ == CameraStatusType.AutoMoveInDeploy ) return;
        if( CameraStatusType_ == CameraStatusType.DeployMoveToBase ) return;
        if( CameraStatusType_ == CameraStatusType.DeployMoveToDeploy ) return;
        if( gesture.touchCount != 1 ) return;
        Transform camTrans = MainCamera_.GetComponent<Transform>();
        CameraInfo cameraInfo = GetCameraInfo( CameraStatusType_ );
        // 自由模式或布阵模式下,滑动是平移
        if( CameraStatusType_ == CameraStatusType.Free || CameraStatusType_ == CameraStatusType.Deploy || CameraStatusType_ == CameraStatusType.SkillSelect ) {
            float x = -gesture.swipeVector.x / 4f;
            float z = -gesture.swipeVector.y / 2f;
            Vector3 temp = (camTrans.rotation * new Vector3( x, 0, z ));
            temp.y = 0;

            cameraInfo.position += temp;
            proto.BattlefieldReference reference = GlobalConfig.GetBattlefieldReferenceByID( BattleSys.NowMapID );
            float width = reference.battlefield_wid * 1.5f;
            cameraInfo.position.x = Mathf.Clamp( cameraInfo.position.x, 0, width );
            int lenth = 0;// reference.deployarea_len + reference.deptharea_len + reference.basearea_len;
            cameraInfo.position.z = cameraInfo.position.z > (lenth - CameraZdistance) ? (lenth - CameraZdistance) : cameraInfo.position.z;
            lenth = reference.deployarea_len + reference.deptharea_len + reference.basearea_len;
            cameraInfo.position.z = cameraInfo.position.z < (-lenth - CameraZdistance-50) ? (-lenth - CameraZdistance-50) : cameraInfo.position.z;
        }
        else if( CameraStatusType_ != CameraStatusType.SkillSelect ) {
            SetCameraType( CameraStatusType.Free );  
            EasyTouch_On_Swipe( gesture );
        }
    }

    // 旋转
    void EasyTouch_On_Twist( Gesture gesture ) {
        if( CameraStatusType_ < CameraStatusType.Normal ) return;
        if( CameraStatusType_ == CameraStatusType.AutoMoveInDeploy ) return;
        if( CameraStatusType_ == CameraStatusType.DeployMoveToBase ) return;
        if( CameraStatusType_ == CameraStatusType.DeployMoveToDeploy ) return;
        //if( CameraStatusType_ == CameraStatusType.Normal ) {
        //    SetCameraType( CameraStatusType.Free );
        //    EasyTouch_On_Twist( gesture );
        //    return;
        //}        
        //CameraInfo cameraInfo = GetCameraInfo( CameraStatusType_ );
        //cameraInfo.angles += new Vector3( 0, gesture.twistAngle, 0 );
        //cameraInfo.angles.y = ClampAngle( cameraInfo.angles.y, YMinLimit_, YMaxLimit_ );
    }

    // 缩小
    private void EasyTouch_On_PinchIn( Gesture gesture ) {
        if( CameraStatusType_ < CameraStatusType.Normal ) return;
        if( CameraStatusType_ == CameraStatusType.Deploy ) return;
        if( CameraStatusType_ == CameraStatusType.AutoMoveInDeploy ) return;
        if( CameraStatusType_ == CameraStatusType.DeployMoveToBase ) return;
        if( CameraStatusType_ == CameraStatusType.DeployMoveToDeploy ) return;

        if( CameraStatusType_ != CameraStatusType.Free      && 
            CameraStatusType_ != CameraStatusType.Lock      &&
            CameraStatusType_ != CameraStatusType.SkillSelect ) {
            SetCameraType( CameraStatusType.Free );
        }
        // 2次缩放之间必须有间隔时间,现设为0.8秒
        if( CheckTime() )
            ModifyCameraScale( true );
    }

    // 放大
    private void EasyTouch_On_PinchOut( Gesture gesture ) {
        if( CameraStatusType_ < CameraStatusType.Normal ) return;
        if( CameraStatusType_ == CameraStatusType.Deploy ) return;
        if( CameraStatusType_ == CameraStatusType.AutoMoveInDeploy ) return;
        if( CameraStatusType_ == CameraStatusType.DeployMoveToBase ) return;
        if( CameraStatusType_ == CameraStatusType.DeployMoveToDeploy ) return;

        if( CameraStatusType_ != CameraStatusType.Free      && 
            CameraStatusType_ != CameraStatusType.Lock      &&
            CameraStatusType_ != CameraStatusType.SkillSelect ) {
            SetCameraType( CameraStatusType.Free );
        }
        // 2次缩放之间必须有间隔时间,现设为0.8秒
        if( CheckTime() )
            ModifyCameraScale( false );
    }

    #region private member func

    private bool CheckTime( float time = 0.8f ) {
        return LastScaleTime_.ToNextTick( time );
    }

    private void SetCameraTarget( Transform trans ) {
        if( trans == null )
            CameraTargetTrans_ = trans;
        else {
            ShipDisplay display = trans.GetComponent<ShipDisplay>();
            if( display == null )
                return;
            CameraTargetTrans_ = display.GetTeam().GetTeamGo().transform;
        }
    }

    private Vector3 GetDeployCameraOriginPostion() {
        ClientShip ship = BattleSys.GetFarestEnemy();
        float vol = ship.Reference.vol;
        Vector3 position = ship.Position + new Vector3( DeployOriginPositionX, 0, 18f + vol / 2 );
        position.y = DeployOriginPositionY;
        return position;
    }

    private Vector3 GetBattleCameraOriginPosition( int shipStrait = Def.ShipTrait.CommanderShip ) {
        bool bFind = false;
        Vector3 targetPos = new Vector3( 0, 0, -5000 );
        List<ClientShip> list = BattleSys.GetPlayerShipList();
        foreach( var iter in list ) {
            if( shipStrait == Def.ShipTrait.Carrier ) {
                if( iter.GetShipStrait() > shipStrait )
                    continue;
            }
            else {
                if( iter.GetShipStrait() != shipStrait )
                    continue;
            }
            if( iter.Position.z <= targetPos.z ) continue;
            targetPos = iter.Position;
            bFind = true;
        }
        if( bFind ) {
            return targetPos;
        }
        else {
            // 没找到指挥舰,则找船
            if( shipStrait == Def.ShipTrait.CommanderShip )
                shipStrait = Def.ShipTrait.Carrier;
            else if( shipStrait == Def.ShipTrait.Carrier )
                shipStrait = Def.ShipTrait.Boat;
            // 没找到船,返回
            else if( shipStrait == Def.ShipTrait.Boat )
                return targetPos;
            return GetBattleCameraOriginPosition( shipStrait );
        }
    }

    private void SetCameraTarget( Dictionary<int, TeamDisplay> teamDisplayList, int shipStrait = Def.ShipTrait.CommanderShip ) {
        //int id = -1;
        Transform targetTrans = null;
        if( teamDisplayList == null ) return;
        foreach( var iter in teamDisplayList ) {
            if( iter.Value == null ) continue;
            if( shipStrait == Def.ShipTrait.Carrier ) {
                if( iter.Value.GetShipStrait() > shipStrait )
                    continue;
            }
            else {
                if( iter.Value.GetShipStrait() != shipStrait )
                    continue;
            }
            if( iter.Value.IsDead )
                continue;
            GameObject go = iter.Value.GetTeamGo();
            if( go == null ) continue;
            Transform trans = go.GetComponent<Transform>();
            if( targetTrans != null ) {
                if( trans.position.z <= targetTrans.position.z ) continue;
            }
            targetTrans = trans;
        }
        if( targetTrans != null ) {
            CameraTargetTrans_ = targetTrans;
        }
        else {
            // 没找到指挥舰,则找船
            if( shipStrait == Def.ShipTrait.CommanderShip )
                shipStrait = Def.ShipTrait.Carrier;
            else if( shipStrait == Def.ShipTrait.Carrier )
                shipStrait = Def.ShipTrait.Boat;
            // 没找到船,返回
            else if( shipStrait == Def.ShipTrait.Boat )
                return;
            SetCameraTarget( teamDisplayList, shipStrait );
        }
    }

    /// <summary>
    /// 抖动相机
    /// </summary>
    /// <returns></returns>
    IEnumerator ShakeCamera() {
        Transform cameraTrans = MainCamera_.transform.GetChild( 0 );
        while( shake_time > 0 ) {
            yield return null;
            shake_time -= Time.deltaTime;
            shake_time = shake_time <= 0 ? 0 : shake_time;

            shake_intensity -= shake_decay * Time.deltaTime;
            shake_intensity = shake_intensity <= 0 ? 0 : shake_intensity;

            cameraTrans.localPosition = originPosition + UnityEngine.Random.insideUnitSphere * shake_intensity;
            cameraTrans.localEulerAngles =
            new Vector3( originEulerAngles.x + UnityEngine.Random.Range( -shake_intensity, shake_intensity ) * .2f,
                originEulerAngles.y + UnityEngine.Random.Range( -shake_intensity, shake_intensity ) * .2f,
                originEulerAngles.z + UnityEngine.Random.Range( -shake_intensity, shake_intensity ) * .2f );
        }
        cameraTrans.localPosition = originPosition;
        cameraTrans.localEulerAngles = originEulerAngles;
    }

    /// <summary>
    /// 通过相机缩放类型得到相机应该处于的Y坐标
    /// </summary>
    /// <param name="scaleType"></param>
    /// <param name="positionY"></param>
    /// <param name="rotationX"></param>
    /// <returns></returns>
    private bool GetPositionY( CameraScaleType scaleType, out float positionY ) {
        positionY = 58;
        if( scaleType == CameraScaleType.Min ) {
            positionY = 58;
            return true;
        }
        else if( scaleType == CameraScaleType.Normal ) {
            positionY = 114.5f;
            return true;
        }
        else if( scaleType == CameraScaleType.Max ) {
            positionY = 176;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 通过相机缩放类型得到相机应该处于的X旋转
    /// </summary>
    /// <param name="scaleType"></param>
    /// <param name="rotationX"></param>
    /// <returns></returns>
    private bool GetRotationX( CameraScaleType scaleType, out float rotationX ) {
        rotationX = 40;
        if( scaleType == CameraScaleType.Min ) {
            rotationX = 40;
            return true;
        }
        else if( scaleType == CameraScaleType.Normal ) {
            rotationX = 60;
            return true;
        }
        else if( scaleType == CameraScaleType.Max ) {
            rotationX = 70;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 通过相机缩放类型得到相机与选中目标的距离
    /// </summary>
    /// <param name="scaleType"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    private bool GetDistance( CameraScaleType scaleType, out float distance ) {

        distance = 98;

        // 只在锁定状态下该值才有效
        if( CameraStatusType_ != CameraStatusType.Lock )
            return false;

        if( scaleType == CameraScaleType.Min ) {
            distance = 98;
            return true;
        }
        else if( scaleType == CameraScaleType.Normal ) {
            distance = 135;
            return true;
        }
        else if( scaleType == CameraScaleType.Max ) {
            distance = 195;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 通过相机状态得到对应的相机信息缓存
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private CameraInfo GetCameraInfo( CameraStatusType type ) {
        if( type == CameraStatusType.NormalLerp )
            type = CameraStatusType.Normal;
        int temp = (int)type;
        if( CameraInfoList_[temp] == null ) {
            CameraInfo tempInfo = new CameraInfo();
            CameraInfoList_[temp] = tempInfo;
            return tempInfo;
        }
        return CameraInfoList_[temp];
    }

    /// <summary>
    /// 修改相机缩放
    /// </summary>
    /// <param name="bAdd"></param>
    private void ModifyCameraScale( bool bAdd ) {
        if( bAdd ) {
            if( CameraScaleType_ >= CameraScaleType.Max )
                return;
        }
        else {
            if( CameraScaleType_ <= CameraScaleType.Min )
                return;
        }
        CameraScaleType_ += (bAdd ? 1 : -1);
        float positionY = 0;
        float rotationX = 0;
        GetPositionY( CameraScaleType_, out positionY );
        GetRotationX( CameraScaleType_, out rotationX );
        for( CameraStatusType type = CameraStatusType.Free; type != CameraStatusType.All; type++ ) {
            CameraInfo cameraInfo = GetCameraInfo( type );
            cameraInfo.position.y = positionY;
            cameraInfo.angles.x = rotationX;
        }
    }

    /// <summary>
    /// 初始化主相机
    /// </summary>
    private void InitMainCamera() {
        if( MainCamera_ ) return;
        MainCamera_ = GameObject.Find( "MainCamera" );
        if( MainCamera_ == null )
            MainCamera_ = new GameObject( "MainCamera" );
        MainCamera_.tag = "MainCamera";
        Camera camera = MainCamera_.GetComponentInChildren<Camera>();
        if( camera == null ) {
            GameObject go = new GameObject( "WorldCamera" );
            go.transform.parent = MainCamera_.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = Vector3.one;
            camera = go.AddComponent<Camera>();
        }
        camera.cullingMask = camera.cullingMask & (~(1 << LayerMask.NameToLayer( "UI" )));
        camera.fieldOfView = 50;        
    }

    /// <summary>
    /// 初始化其他相机,如界面使用的模型相机等
    /// </summary>
    private void InitOtherCamera() {
        if( !RenderTextureCamera_ ) {
            RenderTextureCamera_ = new GameObject();
            RenderTextureCamera_.name = "RenderTextureCamera";
            Camera RenderTargetCamera = RenderTextureCamera_.AddComponent<Camera>();
            RenderTargetCamera.transform.position = new Vector3( 0, 0, 0 );
            RenderTargetCamera.transform.eulerAngles = Vector3.zero;
            RenderTargetCamera.depth = 3;
            RenderTargetCamera.fieldOfView = 30;
            RenderTargetCamera.targetTexture = RoleRenderTargetTexture_;
            RenderTargetCamera.clearFlags = CameraClearFlags.SolidColor;
            RenderTargetCamera.farClipPlane = 10.0f;
            RenderTargetCamera.cullingMask = (1 << LayerMask.NameToLayer( "Model" ));
            RenderTargetCamera.enabled = false;
            RenderTargetCamera.enabled = true;
        }
    }

    /// <summary>
    /// 布阵场景销毁目标指引特效
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private IEnumerator PlayEffectOfMainEnemy(Vector3 pos) {
        //生成
        GameObject prefab = Resources.Load("deploy/operation_target") as GameObject;
        GameObject effectInstance = Instantiate(prefab) as GameObject;
        effectInstance.transform.localPosition = pos;
        effectInstance.transform.localScale = Vector3.one;

        //销毁
        float life_duration = 3f;
        yield return new WaitForSeconds(life_duration);
        Destroy(effectInstance);
    }

    /// <summary>
    /// 布阵相机协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator DeployCameraThread() {        
        // 相机开始移动
        //yield return new WaitForSeconds( 2f );
        MainCamera_.transform.position = Vector3.zero;
        MainCamera_.transform.localEulerAngles = Vector3.zero;

        // 相机要指向的位置缓存
        int firstID = 30000;
        int secondID = 40000;
        int thirdID = 60000;

        Dictionary<int,Vector3> posList = new Dictionary<int,Vector3>();
        int shipCount = BattleSys.GetShipCount( false );
        for( int i=0; i < shipCount; i++ ) {
            ClientShip cs = BattleSys.GetShipByIndex( false, i );
            if( cs.Reference.id == firstID || cs.Reference.id == secondID || cs.Reference.id == thirdID ) {
                if( !posList.ContainsKey( cs.Reference.id ) )
                    posList.Add( cs.Reference.id, cs.Position );
            }
        }
        Animator animator = MainCamera.AddComponent<Animator>();
        animator.runtimeAnimatorController = Resources.Load( "Camera/camera_control" ) as RuntimeAnimatorController;

        Vector3 pos;
        posList.TryGetValue( firstID, out pos );
        MainCamera_.transform.position = pos;
        MainCamera.transform.position = new Vector3( 29.3f, 19.9f, -40.8f );
        MainCamera.transform.localEulerAngles = new Vector3( 15, 315, 0 );
        animator.SetTrigger( "DeployCameraStep01" );
        yield return new WaitForSeconds( 2.3f );

        posList.TryGetValue( secondID, out pos );
        MainCamera_.transform.position = pos;
        MainCamera.transform.position = new Vector3( 38.3f, 4.3f, 49.7f );
        MainCamera.transform.localEulerAngles = new Vector3( -2.3f, 238.54f, 0 );
        animator.SetTrigger( "DeployCameraStep02" );
        yield return new WaitForSeconds( 3.51f );

        posList.TryGetValue( thirdID, out pos );
        MainCamera_.transform.position = pos;
        MainCamera.transform.position = new Vector3(46.8f, 8.1f, -9.7f );
        MainCamera.transform.localEulerAngles = new Vector3( 0, 258, 0 );
        animator.SetTrigger( "DeployCameraStep03" );
        yield return new WaitForSeconds( 3.2f );
        // 显示敌人指挥中心信息
        StartCoroutine( PlayEffectOfMainEnemy(pos) );
        yield return new WaitForSeconds( 2.1f );
        posList.Clear();

        // 得到相机和边线的z值差
        proto.BattlefieldReference reference = GlobalConfig.GetBattlefieldReferenceByID( BattleSys.NowMapID );
        if( reference == null )
            yield break;
        // 战场长度
        float length = reference.basearea_len + reference.deptharea_len+reference.deployarea_len;

        Vector3 position = new Vector3( DeployOriginPositionX, DeployOriginPositionY, -length - 23.8f );
        Vector3 angle = new Vector3( DeployOriginRotationX, DeployOriginRotationY, DeployOriginRotationZ );

        // 初始化基地区域相机属性
        CameraInfo info = GetCameraInfo( CameraStatusType.DeployMoveToBase );
        info.position = position + new Vector3( 0, 0, length - reference.basearea_len / 2 );
        info.angles = angle;
        // 初始化布阵区域相机属性
        info = GetCameraInfo( CameraStatusType.DeployMoveToDeploy );
        info.position = position;
        info.angles = angle;
        // 设置相机位置和旋转
        MainCamera.transform.position = Vector3.zero;
        MainCamera.transform.localEulerAngles = Vector3.zero;
        MainCamera_.transform.position = position;
        MainCamera_.transform.localEulerAngles = angle;
        animator.SetTrigger( "DeployCameraStep04" );
        yield return new WaitForSeconds( 2 );
        Destroy( animator );

        // 移动完毕,修改模式为可操作模式
        SetCameraType( CameraStatusType.Deploy );
        // 显示布阵界面
        UIManager.ShowPanel<DeployPanel>();
        UIManager.ShowPanel<DeployCameraControl>();
        // 开启相机线程
        StartCoroutine( CameraThread( null ) );
    }

    /// <summary>
    /// 相机循环
    /// </summary>
    /// <param name="teamDisplayList"></param>
    /// <returns></returns>
    private IEnumerator CameraThread( Dictionary<int, TeamDisplay> teamDisplayList ) {

        Transform camTrans = MainCamera_.GetComponent<Transform>();
        CameraInfo cameraInfo = GetCameraInfo( CameraStatusType.Normal );
        cameraInfo.position = camTrans.position;

        // 得到场景总长度,以做区域判断
        proto.BattlefieldReference reference = GlobalConfig.GetBattlefieldReferenceByID( BattleSys.NowMapID );
        int lenth = 0;// reference.deployarea_len + reference.deptharea_len + reference.basearea_len;

        while( true ) {

            cameraInfo = GetCameraInfo( CameraStatusType_ );
            Quaternion rotation = Quaternion.Euler( cameraInfo.angles );

            if( CameraStatusType_ == CameraStatusType.NormalLerp ) {
                SetCameraTarget( teamDisplayList );
                if( CameraTargetTrans_ != null ) {
                    bool b1 = false,b2 = false;
                    Vector3 tempTarget = new Vector3( cameraInfo.position.x, cameraInfo.position.y, CameraTargetTrans_.position.z - NormalCameraZDistance );
                    tempTarget = tempTarget.z > (lenth - CameraZdistance) ? new Vector3( tempTarget.x, tempTarget.y, lenth - CameraZdistance ) : tempTarget;                    

                    if( Vector3.Distance( camTrans.localEulerAngles, cameraInfo.angles ) > 0.5f ) {
                        camTrans.rotation = Quaternion.RotateTowards( camTrans.rotation, rotation, Time.deltaTime * NormalRotationDamping_ );
                    }
                    else
                        b1 = true;

                    if( Vector3.Distance( camTrans.position, tempTarget ) > 0.5f ) {
                        camTrans.position = Vector3.MoveTowards( camTrans.position, tempTarget, Time.deltaTime * NormalLerpPositionDamping_ );
                    }
                    else
                        b2 = true;

                    if( b1 && b2 ) {
                        SetCameraType( CameraStatusType.Normal, CameraTargetTrans_ );
                    }
                }
                else
                    SetCameraType( CameraStatusType.Free );
            }
            // 普通
            else if( CameraStatusType_ == CameraStatusType.Normal ) {
                if( CameraTargetTrans_ == null ) {
                    SetCameraType( CameraStatusType.NormalLerp );
                }
                else {
                    if( CameraTargetTrans_ != null ) {
                        Vector3 tempTarget = new Vector3( cameraInfo.position.x, cameraInfo.position.y, CameraTargetTrans_.position.z - NormalCameraZDistance );
                        tempTarget = tempTarget.z > (lenth - CameraZdistance) ? new Vector3( tempTarget.x, tempTarget.y, lenth - CameraZdistance ) : tempTarget;
                        camTrans.rotation = Quaternion.Lerp( camTrans.rotation, rotation, Time.deltaTime * NormalRotationDamping_ );
                        camTrans.position = Vector3.Lerp( camTrans.position, tempTarget, Time.deltaTime * NormalPositionDamping_ );
                    }
                }
            }
            // 自由
            else if( CameraStatusType_ == CameraStatusType.Free || CameraStatusType_ == CameraStatusType.Deploy || CameraStatusType_ == CameraStatusType.SkillSelect ) {
                camTrans.rotation = Quaternion.RotateTowards( camTrans.rotation, rotation, Time.deltaTime * OtherRotationDamping_ );
                camTrans.position = Vector3.MoveTowards( camTrans.position, cameraInfo.position, Time.deltaTime * OtherPositionDamping_ );
            }
            // 锁定舰船
            else if( CameraStatusType_ == CameraStatusType.Lock ) {
                if( CameraTargetTrans_ != null ) {
                    float distance = 0;
                    GetDistance( CameraScaleType_, out distance );
                    Vector3 disVector = new Vector3( 0.0f, 0.0f, -distance );
                    Vector3 position = rotation * disVector + CameraTargetTrans_.position + new Vector3( 0, 0, 40 );
                    if( position.z > (lenth - CameraZdistance) ) {
                        SetCameraType( CameraStatusType.NormalLerp );
                    }
                    else {
                        camTrans.rotation = Quaternion.RotateTowards( camTrans.rotation, rotation, Time.deltaTime * LockRotationDamping_ );
                        camTrans.position = Vector3.MoveTowards( camTrans.position, position, Time.deltaTime * LockPositionDamping_ );
                    }
                }
                else
                    SetCameraType( CameraStatusType.NormalLerp );
            }
            else if( CameraStatusType_ == CameraStatusType.DeployMoveToBase || CameraStatusType_ == CameraStatusType.DeployMoveToDeploy ) {
                bool b1 = false,b2 = false;
                if( Vector3.Distance( camTrans.localEulerAngles, cameraInfo.angles ) > 0.5f ) {
                    camTrans.rotation = Quaternion.RotateTowards( camTrans.rotation, rotation, Time.deltaTime * OtherRotationDamping_ );
                }
                else
                    b1 = true;

                if( Vector3.Distance( camTrans.position, cameraInfo.position ) > 0.5f ) {
                    camTrans.position = Vector3.MoveTowards( camTrans.position, cameraInfo.position, Time.deltaTime * OtherPositionDamping_ );
                }
                else
                    b2 = true;

                if( b1 && b2 ) {
                    camTrans.localEulerAngles = cameraInfo.angles;
                    camTrans.position = cameraInfo.position;
                    SetCameraType( CameraStatusType.Deploy );
                }
            }
            yield return null;
        }
    }

    #endregion

    #region interface
    
    /// <summary>
    /// 战斗中玩家点击
    /// </summary>
    /// <param name="go"></param>
    public static void ClickGameObjectInBattle( GameObject go ) {
        // 点击空白处,重置舰船范围显示
        if( go == null ) {
            Instance.SetCameraType( CameraStatusType.Free );
            EventOnClickBlankScreenZone();
            return;
        }
        // 只有舰船可点击
        if( go.layer != LayerMask.NameToLayer( "Ship" ) && go.layer != LayerMask.NameToLayer( "Planet" ) ) return;
        
        // 点击的是玩家舰船则修改相机模式,并选中该舰船
        if( go.name.Contains( "PlayerShip" ) ) {
            Instance.SetCameraType( CameraStatusType.Lock, go.transform );
        }
        EventOnClickShip( go );
    }

    /// <summary>
    /// 抖动相机
    /// </summary>
    /// <param name="time">震动时间</param>
    /// <param name="intensity">震动强度</param>
    public static void Shake( float time, float intensity ) {
        Instance.StopCoroutine( "ShakeCamera" );
        Instance.originPosition = Vector3.zero;
        Instance.originEulerAngles = Vector3.zero;
        Instance.shake_time = time;        
        Instance.shake_intensity = intensity;
        // 衰减系数 = 强度 / 时间
        Instance.shake_decay = intensity / time;
        Instance.StartCoroutine( "ShakeCamera" );
    }

    /// <summary>
    /// 设置触摸是否可用
    /// </summary>
    /// <param name="b"></param>
    public static void SetTouchActive( bool b ) {
        EasyTouch.SetEnabled( b );
    }

    /// <summary>
    /// 初始化所有相机
    /// </summary>
    public static void InitCamera() {
        if( Instance == null ) return;
        Instance.StopAllCoroutines();
        Instance.InitMainCamera();
    }

    public void InitBattleCamera() {
        if( MainCamera_ == null ) {
            Debug.LogError( "MainCamera is null, InitBattleCamera Failed!" );
            return;
        }
        CameraStatusType_ = CameraStatusType.Normal;
        CameraScaleType_ = CameraScaleType.Max;
        CameraInfo cameraInfo = GetCameraInfo( CameraStatusType_ );
        float positionY = 0;
        float rotationX = 0;
        GetPositionY( CameraScaleType_, out positionY );
        GetRotationX( CameraScaleType_, out rotationX );
        cameraInfo.position = new Vector3( OriginPositionX, positionY, OriginPositionZ );
        cameraInfo.angles = new Vector3( rotationX, OriginRotationY, 0 );
        // 初始化位置和旋转为普通模式,并确认对应坐标
        Vector3 position = GetBattleCameraOriginPosition();
        Quaternion rotation = Quaternion.Euler( cameraInfo.angles );
        Vector3 tempTarget = new Vector3( cameraInfo.position.x, cameraInfo.position.y, position.z - NormalCameraZDistance );
        MainCamera_.transform.rotation = rotation;
        MainCamera_.transform.position = tempTarget;
    }

    /// <summary>
    /// 初始化布阵相机
    /// </summary>
    public bool InitDepolyCamera() {
        bool succeed = false;
        if( MainCamera_ == null ) {
            Debug.LogError( "MainCamera is null, InitDepolyCamera Failed!" );
            return succeed;
        }
        CameraStatusType_ = CameraStatusType.AutoMoveInDeploy;
        CameraScaleType_ = CameraScaleType.Max;
        CameraTargetTrans_ = null;
        Vector3 originPos = GetDeployCameraOriginPostion();
        MainCamera_.transform.position = new Vector3( originPos.x, originPos.y, originPos.z );
        MainCamera_.transform.localEulerAngles = new Vector3( DeployOriginRotationX, DeployOriginRotationY, DeployOriginRotationZ );
        succeed = true;
        return succeed;
    }

    /// <summary>
    /// 设置相机模式
    /// </summary>
    /// <param name="t"></param>
    public void SetCameraType( CameraStatusType t, Transform targetTrans = null ) {
        Transform camTrans = MainCamera_.GetComponent<Transform>();
        CameraStatusType_ = t;
        CameraInfo cameraInfo = GetCameraInfo( CameraStatusType_ );
        SetCameraTarget( targetTrans );
        if( t == CameraStatusType.Free ) {
            cameraInfo.position = camTrans.position;
            cameraInfo.angles = camTrans.eulerAngles;
        }
        else if( t == CameraStatusType.Lock ) {
            cameraInfo.position = camTrans.position;
            cameraInfo.angles = camTrans.eulerAngles;
        }
        else if( t == CameraStatusType.SkillSelect ) {
            cameraInfo.position = camTrans.position;
            cameraInfo.angles = camTrans.eulerAngles;
        }
        else if( t == CameraStatusType.Deploy ) {
            cameraInfo.position = camTrans.position;
            cameraInfo.angles = camTrans.eulerAngles;
        }
        if( t != CameraStatusType.Free )
            EventOnChangeCameraStatus( t );
    }

    public void EnemyCommanderShipDead( GameObject commander ) {
        StopAllCoroutines();
        Animator animator = MainCamera.AddComponent<Animator>();
        animator.runtimeAnimatorController = Resources.Load( "Battle/CameraAnimation/Camera_CommanderShipDieCtrl" ) as RuntimeAnimatorController;
        MainCamera_.transform.position = commander.transform.position;
        MainCamera_.transform.eulerAngles = Vector3.zero;
        MainCamera.transform.localPosition = new Vector3( -17, 16.3f, -42 );
        MainCamera.transform.localEulerAngles = new Vector3( 15, 386, 0 );
        animator.SetTrigger( "CommanderShipDie" );
    }

    public void StartDeployCameraThread() {
        StopAllCoroutines();
        StartCoroutine( DeployCameraThread() );        
    }

    public void StartCameraThread( Dictionary<int, TeamDisplay> teamDisplayList ) {
        StopAllCoroutines();
        StartCoroutine( CameraThread( teamDisplayList ) );
    }

    public RenderTexture GetUIRenderTargetTextexture() {
        return RoleRenderTargetTexture_;
    }

    public GameObject GetRenderTextureCamera() {
        return RenderTextureCamera_;
    }

    #endregion

    private float ClampAngle( float angle, float min, float max ) {
        if( angle > max )
            return max;
        if( angle < min )
            return min;
        return angle;
    }
}