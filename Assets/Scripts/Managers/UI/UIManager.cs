using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    private static UIManager                Instance;

    /// <summary>
    /// 已经打开的UI列表
    /// </summary>
    protected List<GameObject>              OpenedUIList = new List<GameObject>();

    /// <summary>
    /// 已经关闭的UI列表
    /// </summary>
    protected List<List<GameObject>>        ClosedUIList = new List<List<GameObject>>();


    void Awake() {
        Instance = this;
    }

    #region Canvas渲染层级相关

    private Canvas PanelCanvas_;
    /// <summary>
    /// 常规界面层
    /// </summary>
    public static Canvas PanelCanvas {
        get {
            if( Instance.PanelCanvas_ == null ) {
                Instance.PanelCanvas_ = Instance.transform.FindChild( "Canvas-UIPanel" ).GetComponent<Canvas>();
            }
            return Instance.PanelCanvas_;
        }
    }

    private Canvas SceneUICanvas_;
    /// <summary>
    /// 场景动态UI层，如血条
    /// </summary>
    public static Canvas SceneUICanvas {
        get {
            if( Instance.SceneUICanvas_ == null ) {
                Instance.SceneUICanvas_ = Instance.transform.FindChild( "Canvas-SceneUI" ).GetComponent<Canvas>();
            }
            return Instance.SceneUICanvas_;
        }
    }

    private Canvas PopWindowCanvas_;
    /// <summary>
    /// 弹窗层
    /// </summary>
    public static Canvas PopWindowCanvas {
        get {
            if( Instance.PopWindowCanvas_ == null ) {
                Instance.PopWindowCanvas_ = Instance.transform.FindChild( "Canvas-PopWindow" ).GetComponent<Canvas>();
            }
            return Instance.PopWindowCanvas_;
        }
    }

    #endregion

    private UIPanelBehaviour GetScript( GameObject go ) {
        return go.GetComponent<UIPanelBehaviour>();
    }

    private IEnumerator ClosePanel( UIPanelBehaviour script, bool bPlaySound, bool bDelay = true ) {
        yield return StartCoroutine( script.Close( bPlaySound, bDelay ) );
    }

    private IEnumerator ClosePanel( GameObject go, bool bPlaySound ) {
        yield return StartCoroutine( ClosePanel( GetScript( go ), bPlaySound ) );
    }

    private void ShowUIListContent() {}

    private IEnumerator OnShowPanel( UIPanelBehaviour script, object[] args ) {
        string uiName = script.name;
        UIDisplayControl ctrl = GlobalConfig.GetUIDisplayControl( uiName );
        ClearAllPanel( ctrl );
        string dontCloseUI = ctrl != null ? ctrl.DontClose : string.Empty;
        if( dontCloseUI != "All" && !string.IsNullOrEmpty( dontCloseUI ) ) {
            CloseAllPanel( dontCloseUI.Split( ',' ) );
        }
        StartCoroutine( script.Show( args ) );
        OpenedUIList.Add( script.gameObject );
        ShowUIListContent();
        yield return null;
    }

    private IEnumerator OnClosePanel( UIPanelBehaviour script, bool bPlaySound ) {
        bool bActive = script.GetActiveSelf();
        if( !bActive ) yield break;
        yield return StartCoroutine( ClosePanel( script, bPlaySound ) );
        OpenedUIList.RemoveAt( OpenedUIList.Count - 1 );
        string uiName = script.name;
        UIDisplayControl ctrl = GlobalConfig.GetUIDisplayControl( uiName );
        if( ctrl != null ) {
            if( ctrl.JustCloseSelfWhenClosing )
                yield break;
        }
        if( ClosedUIList.Count <= 0 ) yield break;
        List<GameObject> lastClosedUIList = ClosedUIList[ClosedUIList.Count - 1];
        for( int i=0; i < lastClosedUIList.Count; i++ ) {
            GameObject panel = lastClosedUIList[i];
            if( !panel ) continue;
            UIPanelBehaviour panelScript = panel.GetComponent<UIPanelBehaviour>();
            StartCoroutine( panelScript.ReShow() );
            OpenedUIList.Add( panel );
        }
        ClosedUIList.RemoveAt( ClosedUIList.Count - 1 );
        ShowUIListContent();
    }

    private T GetPanel<T>() where T : UIPanelBehaviour {
        string uiName = typeof( T ).Name;
        uiName = uiName.ToLower();
        Transform parentTrans = PanelCanvas.transform;
        if( uiName.Contains( "popwindow" ) || uiName.Contains( "dialog" ) )
            parentTrans = PopWindowCanvas.transform;
        else if( uiName.Contains( "sceneui" ) )
            parentTrans = SceneUICanvas.transform;
        Transform child = parentTrans.FindChild( uiName );
        if( !child ) {
            GameObject go = Global.CreateUI( uiName, parentTrans.gameObject );
            if( !go ) {
                Debug.LogError( "CreateUI failed: " + uiName );
                return null;
            }
            child = go.transform;
            go.AddComponent<T>();
        }
        return child.GetComponent<T>();
    }

    private void ClearAllPanel( UIDisplayControl ctrl ) {
        if( ctrl == null ) return;
        if( !ctrl.ClearOpened ) return;
        for( int i=0; i < OpenedUIList.Count; i++ ) {
            GameObject panel = OpenedUIList[i];
            // 不存在的跳过
            if( !panel ) continue;
            UIPanelBehaviour script = panel.GetComponent<UIPanelBehaviour>();
            // 已关闭的跳过
            if( !script.GetActiveSelf() ) continue;
            // 关闭该界面
            StartCoroutine( ClosePanel( script, false ) );
        }
        OpenedUIList.Clear();
        ClosedUIList.Clear();
    }

    private void CloseAllPanel( params string[] dontCloseUIName ) {
        List<GameObject> list = new List<GameObject>();
        for( int i=0; i < OpenedUIList.Count; i++ ) {
            GameObject panel = OpenedUIList[i];
            // 不存在的跳过
            if( !panel ) continue;
            UIPanelBehaviour script = panel.GetComponent<UIPanelBehaviour>();
            // 已关闭的跳过
            if( !script.GetActiveSelf() ) continue;
            // 过滤
            bool b = false;
            for( int k=0; k < dontCloseUIName.Length; k++ ) {
                if( panel.name == dontCloseUIName[k] ) {
                    b = true;
                    break;
                }
            }
            if( b ) continue;
            // 关闭该界面
            StartCoroutine( ClosePanel( script, false ) );
            // 加入到关闭表内
            list.Add( OpenedUIList[i] );
        }
        // 把该阶段的界面加入关闭表
        if( list.Count > 0 )
            ClosedUIList.Add( list );
        // 把关闭的UI从打开表内清除
        for( int i=0; i < list.Count; i++ ) {
            if( OpenedUIList.Contains( list[i] ) )
                OpenedUIList.Remove( list[i] );
        }
    }

    #region interface
    public static void ClosePanel<T>( bool bPlaySound = true ) where T : UIPanelBehaviour {
        if( !Instance ) return;
        T go = Instance.GetPanel<T>();
        if( !go ) return;
        Instance.StartCoroutine( Instance.OnClosePanel( go, bPlaySound ) );
    }

    public static void ShowPanel<T>( params object[] args ) where T : UIPanelBehaviour {
        if( !Instance ) return;
        T go = Instance.GetPanel<T>();
        if( !go ) return;
        Instance.StartCoroutine( Instance.OnShowPanel( go, args ) );
    }
    #endregion
}