using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class BattleShipInfo : MonoBehaviour {

    // 对应的team
    private TeamDisplay         Team_;
    // 要跟随的Trans
    private Transform           ParentTrans_;
    // 自己的Trans
    private Transform           Trans_;
    // 世界相机
    private Camera              WorldCamera_;
    // UI相机
    private Camera              UICamera_;

    private GameObject          UI_;

    private Animator            PanelAnimator_;  

    private GameValue           Durability_;     // 耐久
    private GameValue           EnergyShield_;   // 能量
    private GameValue           ArmouredShield_; // 装甲

    private Image DurabilityImage_;
    private Image ArmouredImage_;

    private bool                bSelect_ = false;
    private bool                bHide_ = false;

    private void OnEnable() {
        StartCoroutine( UpdatePosition() );
    }

    public static BattleShipInfo Create( Transform parentTrans, TeamDisplay team, string name, float radius, bool isPlayer, bool isAdvancedShip ) {
        GameObject go = new GameObject( "BattleShipInfo_" + (isPlayer ? "Player" : "Enemy") );
        go.transform.parent = UIManager.SceneUICanvas.transform;
        go.transform.localPosition = new Vector3( 99999, 99999, 99999 );
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localScale = Vector3.one;
        string prefabName;
        if( isPlayer ) prefabName = isAdvancedShip ? "battleshipinfo_ad_p" : "battleshipinfo_low_p";
        else           prefabName = isAdvancedShip ? "battleshipinfo_ad_e" : "battleshipinfo_low_e";

        Global.CreateUI( prefabName, go );
        BattleShipInfo uiScript = go.AddComponent<BattleShipInfo>();
        uiScript.Init( parentTrans, team, name, radius );
        return uiScript;
    }

    private void CheckShow() {
        // 选中或残血时才显示UI
        if( bSelect_ ) {
            ShowInfo( true );
        }
        else {
            if( Durability_ == null )
                ShowInfo( false );
            else
                ShowInfo( !Durability_.IsMax() || !EnergyShield_.IsMax() || !ArmouredShield_.IsMax() );
        }

        if( UI_.activeSelf ) {
            string name = "ShipUnderAttack";
            //if( gameObject.name.Contains( "Player" ) && bSelect_ ) 
            //    name = "ShipSelect";
            if( PanelAnimator_.GetCurrentAnimatorStateInfo( 0 ).IsName( name ) )
                return;
            PanelAnimator_.SetTrigger( name );
        }
    }

    public void HideInfo() {
        ShowInfo( false );
        bHide_ = true;
    }

    public void ShowInfo( bool b ) {
        if( bHide_ ) return;
        UI_.SetActive( b );
    }

    public void SetSelect( bool b ) {
        bSelect_ = b;
        CheckShow();
    }

    public void UpdateUIByUnderAttack( int curDurability, int curEnergy, int curArmoured ) {
        Durability_.SetValue( curDurability );
        EnergyShield_.SetValue( curEnergy );
        ArmouredShield_.SetValue( curArmoured );
        CheckShow();
        DurabilityImage_.fillAmount = curDurability / (float)Durability_.Max;
        if( ArmouredShield_.Max == 0 ) {
            ArmouredImage_.fillAmount = 0;
        }
        else {
            ArmouredShield_.SetValue( curArmoured );
            ArmouredImage_.fillAmount = curArmoured / (float)ArmouredShield_.Max;
        }
    }

    public void SyncAttr( int Durability, int Energy, int Armoured ) {
        Durability_ = new GameValue( Durability, Durability );
        
        Durability_.SetToMax();

        EnergyShield_ = new GameValue( Energy, Energy );
        EnergyShield_.SetToMax();

        ArmouredShield_ = new GameValue( Armoured, Armoured );
        ArmouredShield_.SetToMax();
    }

    private void Init( Transform parentTrans, TeamDisplay team, string name, float radius ) {
        Team_ = team;
        ParentTrans_ = parentTrans;
        UI_ = transform.GetChild( 0 ).gameObject;
        Trans_ = transform;
        GameObject go = CameraManager.Instance.MainCamera;
        WorldCamera_ = go.GetComponent<Camera>();
        UICamera_ = CameraManager.Instance.UICamera;

        PanelAnimator_ = UI_.transform.GetComponent<Animator>();
        DurabilityImage_ = UI_.transform.FindChild( "Image_Decor/Image_Hp/Image" ).GetComponent<Image>();
        ArmouredImage_ = UI_.transform.FindChild( "Image_Decor/Image_En/Image" ).GetComponent<Image>();

        Button button = UI_.transform.FindChild( "Image_Circle" ).gameObject.AddComponent<Button>();
        button.onClick.AddListener( OnCircleClick );

        // 初始化血条信息的状态
        PanelAnimator_.SetTrigger( "ShipUnderAttack" );
        // 初始化隐藏
        ShowInfo( false );
    }

    private void OnCircleClick() {
        CameraManager.ClickGameObjectInBattle( Team_.GetHurtShip().gameObject );
    }

    private IEnumerator UpdatePosition() {
        while( true ) {
            if( ParentTrans_ != null && UI_ != null && UI_.activeSelf ) {
                Vector3 pos = WorldCamera_.WorldToViewportPoint( ParentTrans_.position /*+ OriginPosition_*/ );
                Trans_.position = UICamera_.ViewportToWorldPoint( pos );
                pos = Trans_.localPosition;
                pos.z = 0f;
                Trans_.localPosition = pos;
            }
            yield return null;
        }
    }
}