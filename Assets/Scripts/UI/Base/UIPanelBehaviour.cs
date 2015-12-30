using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public abstract class UIPanelBehaviour : MonoBehaviour {
    public const string ANIM_TRIGGER_SHOW = "show";
    public const string ANIM_TRIGGER_HIDE = "hide";

    /// <summary>
    /// 初始化各控件
    /// </summary>
    protected virtual void OnAwake() { }

    /// <summary>
    /// 每帧调用
    /// </summary>
    protected virtual void OnUpdate() { }

    /// <summary>
    /// 第一次渲染前调用
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator OnStart() { yield break; }

    /// <summary>
    /// 销毁
    /// </summary>
    protected virtual void OnBeginDestroy() { }

    /// <summary>
    /// 给各控件设置好数据,以准备显示
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnShow( params object[] args ) { }

    /// <summary>
    /// 显示动画完毕后
    /// 基本用于指引调用
    /// 因为界面显示调用onshow时,界面可能还在屏幕外
    /// 在onshow里处理会有坐标差异问题,故增加此函数,在界面移动动画完毕后调用
    /// </summary>
    protected virtual void OnActiveAnimationFinished() { }

    /// <summary>
    /// 重新打开,在数据不变的情况下再次打开该面板,比如从下一级界面回到上一级界面时
    /// </summary>
    protected virtual void OnReShow() { }

    /// <summary>
    /// 同OnActiveAnimationFinished,在隐藏动画完毕后调用
    /// </summary>
    protected virtual void OnInActiveAnimationFinished() { }

    /// <summary>
    /// 准备关闭界面
    /// </summary>
    protected virtual void OnClose() { }

    /// <summary>
    /// 注册消息
    /// </summary>
    protected virtual void OnRegEvent() { }
    /// <summary>
    /// 注销消息
    /// </summary>
    protected virtual void OnUnregEvent() { }

    /// <summary>
    /// 渲染模型的相机
    /// </summary>
    private static GameObject                   RenderTextureCamera_ = null;

    /// <summary>
    /// 界面显示或关闭时所用的tweener动画集合
    /// </summary>
    //public UITweener[]                          Tweeners;   //remove
    public Animator[]                             UIAnimators;//add by Lynn, for uGui

    /// <summary>
    /// 事件接收
    /// </summary>
    protected GameObject                        EventReceiver_;

    protected GameObject                        Background_;

    protected GameObject                        CloseBtn_;
    void Awake() {
        Transform t = gameObject.transform.FindChild( "EventReceiver" );
        EventReceiver_ = t ? t.gameObject : this.gameObject;

        t = gameObject.transform.FindChild( "Background" );
        Background_ = t ? t.gameObject : null;
         
        //SetActive( false );

        t = EventReceiver_.transform.FindChild ( "CloseButton" );
        CloseBtn_ = t ? t.gameObject : null;
        
        UIAnimators = EventReceiver_.GetComponentsInChildren<Animator>();

        // 让子类初始化相关控件
        OnAwake();
    }

    void OnDestroy() {
        OnBeginDestroy();
    }

    void OnEnable(){
        OnRegEvent();
    }

    void OnDisable(){
        OnUnregEvent();
    }

    void Update () {
        OnUpdate ();
    }

    IEnumerator Start () {
        yield return StartCoroutine ( OnStart () );
    }

    /// <summary>
    /// 获得模型相机
    /// </summary>
    /// <returns></returns>
    private static GameObject GetRenderTextureCamera () {
        if ( RenderTextureCamera_ == null )
            RenderTextureCamera_ = GameObject.Find ( "RenderTextureCamera" );
        return RenderTextureCamera_;
    }

    /// <summary>
    /// 根据状态以及界面播放相关音效
    /// </summary>
    private void PlaySound (bool threeD = false) {
        string soundName = "UI";
        if ( gameObject.name == "DragPackagePanel" )
            soundName = "bagbutton";
        else if ( gameObject.name == "MailPanel" )
            soundName = "mail";
        AudioManager.PlaySoundByName(GetActiveSelf() ? "close" : soundName, gameObject.transform.position, threeD);
    }

    /// <summary>
    /// 播放界面打开或关闭时所需要的动画
    /// </summary>
    /// <param name="bForward"></param>
    /// <returns></returns>
    private IEnumerator PlayTweeners ( bool bForward ) {
        if (UIAnimators == null) {
            Debug.Log(transform.name + " : UI Animator == null, do nothing");
            yield break;
        }
        float duration = 0f;
        for( int i=0; i < UIAnimators.Length; i++ ) {
            if( UIAnimators[i] == null ) continue;
            //判断是否有show、hide触发器，如果没，默认自动播放
            if( bForward ) {
                bool hasTrigger = CheckAnimatorHasTrigger( UIAnimators[i] , ANIM_TRIGGER_SHOW);
                if (hasTrigger) {
                    //duration = UIAnimators[i].GetCurrentAnimatorStateInfo(0).length;
                    UIAnimators[i].SetTrigger(ANIM_TRIGGER_SHOW);
                }
            }
            else {
                bool hasTrigger = CheckAnimatorHasTrigger( UIAnimators[i], ANIM_TRIGGER_HIDE );
                if (hasTrigger) {
                    //duration = UIAnimators[i].GetCurrentAnimatorStateInfo(0).length;
                    UIAnimators[i].SetTrigger(ANIM_TRIGGER_HIDE);
                }
            }
            //int length = UIAnimators[i].GetCurrentAnimatorClipInfo( 0 ).Length;
            //if( duration < length )
            //    duration = length;
        }
        yield return new WaitForSeconds( duration );
    }

    /// <summary>
    /// 设置显示或隐藏
    /// </summary>
    /// <param name="active"></param>
    private void SetActive ( bool active ) {
        if ( Background_ )
            Background_.SetActive( active );
        if( CloseBtn_ )
            CloseBtn_.SetActive( active );
        if( EventReceiver_ ) {
            EventReceiver_.SetActive( active );
        }
        this.enabled = active;
    }

    /// <summary>
    /// 重新打开该界面
    /// </summary>
    /// <returns></returns>
    public IEnumerator ReShow () {
        PlaySound ();
        ClearModel ();
        SetActive ( true );
        yield return null;
        OnReShow();
        yield return new WaitForSeconds( 0.1f );
        yield return StartCoroutine( PlayTweeners( true ) );
        OnActiveAnimationFinished();
    }

    /// <summary>
    /// 显示界面
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public IEnumerator Show( params object[] args ) {        
        PlaySound ();
        ClearModel();
        SetActive ( true );
        yield return null;
        OnShow( args );
        yield return new WaitForSeconds( 0.1f );
        yield return StartCoroutine( PlayTweeners( true ) );
        OnActiveAnimationFinished();
    }

    /// <summary>
    /// 关闭界面
    /// </summary>
    /// <param name="bPlaySound">是否播放关闭音效</param>
    /// <returns></returns>
    public IEnumerator Close ( bool bPlaySound, bool bDelay = true ) {
        if ( bPlaySound )
            PlaySound ();
        OnClose ();
        if ( bDelay )
            yield return StartCoroutine( PlayTweeners( false ) );
        SetActive( false );
        OnInActiveAnimationFinished();
    }

    /// <summary>
    /// 获得当前显示状态
    /// </summary>
    /// <returns></returns>
    public bool GetActiveSelf () {
        return EventReceiver_.activeSelf;
    }

    /// <summary>
    /// 显示角色模型
    /// </summary>
    /// <param name="chr">角色配置</param>
    /// <param name="position">显示位置</param>
    /// <param name="scale">缩放</param>
    /// <param name="angle">角度</param>
    /// <returns></returns>
    public IEnumerator ShowModel ( Vector3 position, Vector3 scale, Vector3 angle ) {
        yield break;        
    }

    /// <summary>
    /// 清除角色模型
    /// </summary>
    public static void ClearModel () {
        Global.ClearChild ( GetRenderTextureCamera () );
    }

    private bool CheckAnimatorHasTrigger(Animator animator, string triggerName ) {
        if( animator.runtimeAnimatorController == null ) return false;
        foreach( var p in animator.parameters ) {
            if( p.name == triggerName ) return true;
        }
        return false;
    }
}