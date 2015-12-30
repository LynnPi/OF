using UnityEngine;

public class UGUIMessageBox : MonoBehaviour{
    public static UGUIMessageBox Instance { private set; get; }
    private Rect WindowRect_ = new Rect( 20, 20, 400, 200 );

    public static void Show( string title, string msg, string buttonText, System.Action callBack ){
        if( !Instance ){
            GameObject go = new GameObject( "UGUIMessageBox" );
            Instance = go.AddComponent<UGUIMessageBox>();
        }
        Instance.Title_ = title;
        Instance.Message_ = msg;
        Instance.ButtonText_ = buttonText;
        Instance.CallBack_ = callBack;
    }

    private string Title_ = "";
    private string Message_ = "";
    private string ButtonText_= "";
    private System.Action CallBack_ = null;
    public GUISkin LoadingSkin_;

    void Awake(){        
        LoadingSkin_ = Resources.Load< GUISkin >( "LoadingGUISkin" );
        Debug.Log( LoadingSkin_ );
    }

    void OnGUI(){        
        GUI.skin = LoadingSkin_;
        WindowRect_.x = (int)( Screen.width * 0.5f - WindowRect_.width * 0.5f );
        WindowRect_.y = (int)( Screen.height * 0.5f - WindowRect_.height * 0.5f );
        GUILayout.Window( 1, WindowRect_, DoWindow, Title_ );


    }

    void DoWindow( int windowID ){
        GUILayout.Space(10);
        GUILayout.TextArea( Message_, GUILayout.Width( 380 ), GUILayout.Height( 100 ) );
        //if ( GUILayout.Button( ButtonText_, GUILayout.Width( 100 ), GUILayout.Height( 50 ) ) ) {
        if ( GUI.Button( new Rect(150, 140, 100, 50), ButtonText_ ) ) {
            if( CallBack_ != null ){
                CallBack_();
            }
            Destroy( this.gameObject );          
        }

    }


}