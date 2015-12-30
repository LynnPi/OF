using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 富文本处理类
/// 
/// 可处理动态字的富文本,ngui自带的富文本太坑了,不能处理动态字,汉字那么多,怎么做图集,坑爹!!
/// ngui自带的超链接,如果换行,碰撞盒相当悲剧...所以还是自己写吧
/// 本类已经处理了文字,图片,超链接的混排.并实现了显示时的打字机效果,可传入参数决定是否调用打字机,基本来说做游戏已经足够了.
/// 
/// 实现原理:根据总宽度分割字符串,把每一行内容归类,显示时,根据该行最大高度的组件来定位该行的起始Y坐标.
/// 最终显示结果为,图片和文字底对齐
/// 
/// 要求挂接在需要富文本的label上,该label必须为左上对齐,pivot为topleft,或左对齐,pivot为left,并有固定的width
/// 
/// 富文本以
/// *#开始,#*结束

/// \n为换行
/// 
/// 如*#img=Button#* 
/// 此为图片,Button为资源名
/// 
/// 如*#select=text,type,value#* 
/// 此为超链接
/// 超链接里,text是要显示的内容,type为超链接类型,value为对应消息,如果是网站,该值为网站url.
/// 比如要显示一个物品的超链接:
/// 
/// *#select=头盔,0,3002#*就是label里显示为头盔,ID为3002的物品

/// By Aklan,2015/3/13 QQ:346193
/// </summary>

public class RichTextProcessing : MonoBehaviour {

    public static event Action<HyperlinksType,string> EventOnClickHyperlinks;

    public enum RichTextInfoType {
        Invalid = -1,
        Text,               // 纯文本
        Select,             // 超链接
        Sprite              // 图片
    }

    public enum HyperlinksType {
        item = 0,
        hero,
        url,
    }

    public class RichTextStruct {
        public bool             bNewLine;
        public string           info;
        public RichTextInfoType type;
        public string           hyperlinksType;
        public string           data;
    }

    public class RichGo {
        public float            xPosition;
        public GameObject       go;
        public RichTextInfoType type;

        public void Set( float tempXPos, GameObject tempGo ) {
            xPosition = tempXPos;
            go = tempGo;
        }
    }

    public class RichLine {
        public int              maxHeight;
        public List<RichGo>     goList = new List<RichGo>();
    }

    private GameObject              TextLabelTemplate_;

    private float                   PositionX_ = 0;
    private List<RichTextStruct>    RichTextlist_ = new List<RichTextStruct>();
    private List<RichLine>          RichLineList = new List<RichLine>();

    private int                     Depth_;
    private int                     LabelHeight_;
    private int                     LabelSpacingX_;
    private int                     LabelSpacingY_;
    private int                     LineHeight_;
    private float                   LineWidth_;

    private int                     StartIndex_ =0;
    private int                     EndIndex_ = 0;

    private string                  Command_ ="";

    private string                  Command_first_="";
    private string                  Command_last_="";
    private int                     EqualIndex_ =-1;

    private void Awake() {
        /*
        Text label = gameObject.GetComponent<Text>();
        Depth_ = label.depth + 1;
        LabelHeight_ = label.fontSize;
        LabelSpacingX_ = label.spacingX;
        LabelSpacingY_ = label.spacingY;
        LineWidth_ = label.width;
         */
    }

    private void AddGameObjectToLine( bool bNewLine, float tempXPos, GameObject tempGo, RichTextInfoType type = RichTextInfoType.Invalid ) {
        if( bNewLine ) {
            RichLine newRichLine = new RichLine();
            // 这里处理,第一行的特殊情况
            newRichLine.maxHeight = RichLineList.Count <= 0 ? 0 : LabelHeight_;
            RichLineList.Add( newRichLine );
        }
        if( tempGo == null )
            return;
        RichLine line = RichLineList[RichLineList.Count - 1];
        RichGo go = new RichGo();
        go.type = type;
        go.Set( tempXPos, tempGo );
        line.goList.Add( go );

        //Image img = go.go.GetComponent<Image>();
        /*
        if( img != null ) {
            if( line.maxHeight < img.height )
                line.maxHeight = img.height;
        }
        */
    }

    public void Reset() {

        if( TextLabelTemplate_ == null ) {
            TextLabelTemplate_ = GameObject.Instantiate( gameObject ) as GameObject;
            TextLabelTemplate_.name = "RichTextProcessingTemplate";
            Destroy( TextLabelTemplate_.GetComponent<RichTextProcessing>() );
            TextLabelTemplate_.SetActive( false );
            TextLabelTemplate_.transform.parent = gameObject.transform.parent;
        }

        PositionX_ = 0;
        RichTextlist_.Clear();
        RichLineList.Clear();
        AddGameObjectToLine( true, 0, null );
        Global.ClearChild( gameObject );
        StartIndex_ =0;
        EndIndex_ = 0;
        Command_ ="";
        Command_first_="";
        Command_last_="";
        EqualIndex_ =-1;
    }

    /// <summary>
    /// 解析
    /// </summary>
    /// <param name="str"></param>
    private void Analyze( string str ){
        // 把换行替换
        //str = str.Replace( "\\n", "*#%%%#*" );
        str = str.Replace( "\n", "*#%%%#*" );
        // 遍历字符串
        while( str.Length > 0 ) {
            // 如果有换行或富文本
            StartIndex_ = str.IndexOf( "*#" );
            if( StartIndex_ >= 0 ) {
                // 如果不是第0个开始,说明前面有纯文本
                if( StartIndex_ > 0 ) {
                    RichTextStruct temp = new RichTextStruct();
                    temp.bNewLine = false;
                    string vText = str.Substring( 0, StartIndex_ );
                    temp.info = vText;
                    temp.type = RichTextInfoType.Text;
                    RichTextlist_.Add( temp );
                    str = str.Remove( 0, StartIndex_ );
                }

                // 获得结尾字符
                EndIndex_ = str.IndexOf( "#*" );
                // 有的配对的结尾符
                if( EndIndex_ > 0 ) {
                    // 得到内容
                    Command_ = str.Substring( 2, EndIndex_ - 2 );
                    // 移除结尾符
                    str = str.Remove( 0, EndIndex_ + 2 );
                }
                // 没有配对的
                else {
                    // 直接入表,是纯文本
                    RichTextStruct temp = new RichTextStruct();
                    temp.bNewLine = false;
                    temp.info = str;
                    temp.type = RichTextInfoType.Text;
                    RichTextlist_.Add( temp );
                    str = str.Remove( 0, str.Length );
                }

                // 有内容,得到是否有换行
                if( !string.IsNullOrEmpty( Command_ ) ) {
                    EqualIndex_ = Command_.IndexOf( "%%%" );
                }
                else {
                    // 没有内容,继续循环
                    continue;
                }

                // 如果有换行的
                if( EqualIndex_ >= 0 ) {
                    RichTextStruct temp  = new RichTextStruct();
                    temp.bNewLine = true;
                    RichTextlist_.Add( temp );
                }
                else {
                    // 其他富文本
                    if( Command_.IndexOf( "=" ) > 0 ) {
                        CutString( Command_, "=", out Command_first_, out Command_last_ );
                    }
                    RichTextStruct temp  = new RichTextStruct();
                    temp.bNewLine = false;
                    switch( Command_first_ ) {
                        case "img":
                                temp.info = Command_last_;
                                temp.type = RichTextInfoType.Sprite;
                                break;
                        case "select":
                                string vText="";
                                string vEventFunc="";
                                string vValue="";
                                if( Command_last_.IndexOf( "," ) > -1 ) {
                                    // 取得点击参数
                                    CutString( Command_last_, ",", out vText, out vEventFunc );
                                }
                                if( vEventFunc.IndexOf( "," ) > -1 ) {
                                    CutString( vEventFunc, ",", out vEventFunc, out vValue );
                                }
                                if( vText.Length <= 0 ) {
                                    temp.info = Command_last_;
                                    temp.type = RichTextInfoType.Text;
                                }
                                else {
                                    temp.info = vText;
                                    temp.type = RichTextInfoType.Select;
                                    temp.hyperlinksType = vEventFunc;
                                    temp.data = vValue;
                                }
                                break;
                        default:
                                temp.info = Command_;
                                temp.type = RichTextInfoType.Text;
                                break;
                    }
                    RichTextlist_.Add( temp );
                }
            }
            else {
                RichTextStruct temp  = new RichTextStruct();
                temp.bNewLine = false;
                temp.info = str;
                temp.type = RichTextInfoType.Text;
                RichTextlist_.Add( temp );
                str = str.Remove( 0, str.Length );
            }
        }
    }

    private void CreateAll( bool bTypewriter ) {
        for( int i = 0; i < RichTextlist_.Count; i++ ) {
            if( RichTextlist_[i].bNewLine ) {
                PositionX_ = 0;
                AddGameObjectToLine( true, 0, null );
            }
            switch( RichTextlist_[i].type ) {
                case RichTextInfoType.Text:
                    if( !string.IsNullOrEmpty( RichTextlist_[i].info ) )
                        CreateTextLabel( RichTextlist_[i].info, bTypewriter );
                    break;
                case RichTextInfoType.Sprite:
                    if( !string.IsNullOrEmpty( RichTextlist_[i].info ) )
                        CreateSprite( RichTextlist_[i].info, bTypewriter );
                    break;
                case RichTextInfoType.Select:
                    CreateEventLabel( RichTextlist_[i], bTypewriter );
                    break;
                default:
                    break;
            }
        }
    }

    private void Sort() {
        /*
        float y = 0;
        Image label = TextLabelTemplate_.GetComponent<Image>();
        // 如果是居中的,那么根据行数要往上调起始位置
        if( label.pivot == UIWidget.Pivot.Left ) {
            if( RichLineList.Count > 1 ) {
                if( RichLineList.Count % 2 == 0 ) {
                    y += (LabelHeight_ + LabelSpacingY_) * 0.5f * RichLineList.Count / 2;
                }
                else {
                    y += (LabelHeight_ + LabelSpacingY_) * (RichLineList.Count - 1) / 2;
                }
            }            
        }

        for( int i=0; i < RichLineList.Count; i++ ) {

            if( i != 0 ) {
                // 其他行,直接下降最大高度,以及Y间隔
                y -= RichLineList[i].maxHeight;
                y -= LabelSpacingY_;
            }
            else {
                // 第一行特殊处理,如果不为0,说明有图片,那么下降图片高度-字体高度
                if( RichLineList[i].maxHeight > 0 && RichLineList[i].maxHeight > LabelHeight_ ) {
                    if( label.pivot == UIWidget.Pivot.TopLeft )
                        y -= (RichLineList[i].maxHeight - LabelHeight_);
                }
            }
            foreach( RichGo iter in RichLineList[i].goList ) {
                bool isSprite = iter.go.GetComponent<UISprite>() != null;
                // 如果是图片,再下降字体高度的Y坐标
                float tempY = y;
                if( isSprite ) {
                    if( label.pivot == UIWidget.Pivot.TopLeft )
                        tempY -= LabelHeight_;
                    else
                        tempY -= LabelHeight_ / 2;
                }
                iter.go.transform.localPosition = new Vector3( iter.xPosition, tempY );
            }
        }
         */
    }

    private IEnumerator Typewriter() {
        for( int i=0; i < RichLineList.Count; i++ ) {
            List<RichGo> list = RichLineList[i].goList;
            for( int k=0; k < list.Count; k++ ) {
                yield return null;
                RichGo iter = list[k];                
                iter.go.SetActive( true );
                // 只在纯文本时才使用打字机效果
                if( iter.type != RichTextInfoType.Text ) 
                    continue;
                Text label = iter.go.GetComponent<Text>();
                string content = label.text;
                yield return StartCoroutine( Global.TypewriterDialog( label, content ) );
            }
            yield return null;
        }
    }

    /// <summary>
    /// 创建普通文本
    /// </summary>
    /// <param name="str"></param>
    private void CreateTextLabel( string str, bool bTypewriter ) {
        /*
        GameObject go = CreateLabel();
        Text ul = go.GetComponent<Text>();
        ul.width = (int)(LineWidth_ - PositionX_);
        ul.text = str;
        string sbstr = ul.processedText;
        ul.text = sbstr;

        // 处理万一在拖动表里,需要拖动,处理碰撞区域和大小
        BoxCollider box = go.GetComponent<BoxCollider>();
        if( box != null ) {
            box.center = new Vector3( ul.printedSize.x / 2, -ul.printedSize.y / 2 );
            box.size = new Vector3( ul.printedSize.x, ul.printedSize.y );
        }

        AddGameObjectToLine( false, PositionX_, go, RichTextInfoType.Text );
        PositionX_ += ul.printedSize.x + LabelSpacingX_;

        if( bTypewriter )
            go.SetActive( false );

        str = str.Remove( 0, sbstr.Length );
        if( str.Length >= 1 ) {
            AddGameObjectToLine( true, 0, null, RichTextInfoType.Text );
            PositionX_ = 0;
            CreateTextLabel( str, bTypewriter );
        }
         */
    }

    /// <summary>
    /// 创建超链接
    /// </summary>
    /// <param name="data"></param>
    private void CreateEventLabel( RichTextStruct data, bool bTypewriter, string lastStr = "" ) {
        /*
        GameObject go = CreateLabel();
        Text ul = go.GetComponent<Text>();

        ul.width = (int)(LineWidth_ - PositionX_);

        string tempStr = data.info;
        if( !string.IsNullOrEmpty( lastStr ) )
            tempStr = lastStr;

        ul.text = tempStr;
        string sbstr = ul.processedText;
        ul.text = "[u][url=" + data.hyperlinksType + "/" + data.data + "]" + sbstr + "[/url][/u]";

        // 处理碰撞的区域和大小
        BoxCollider box = go.GetComponent<BoxCollider>();
        if( box == null )
            box = go.AddComponent<BoxCollider>();
        box.center = new Vector3( ul.printedSize.x / 2, -ul.printedSize.y / 2 );
        box.size = new Vector3( ul.printedSize.x, ul.printedSize.y );
        go.AddComponent<RichTextClickEvent>();

        AddGameObjectToLine( false, PositionX_, go, RichTextInfoType.Select );
        PositionX_ += ul.printedSize.x + LabelSpacingX_;

        if( bTypewriter )
            go.SetActive( false );

        tempStr = tempStr.Remove( 0, sbstr.Length );
        if( tempStr.Length >= 1 ) {
            AddGameObjectToLine( true, 0, null );
            PositionX_ = 0;
            CreateEventLabel( data, bTypewriter, tempStr );
        }   
         */
    }

    /// <summary>
    /// 创建图片
    /// </summary>
    /// <param name="str"></param>
    private void CreateSprite( string str, bool bTypewriter ) {
        /*
        GameObject go = NGUITools.AddChild( gameObject );
        go.name = "Sprite";
        UISprite sprite = go.AddComponent<UISprite>();
        sprite.depth = Depth_;
        sprite.pivot = UIWidget.Pivot.BottomLeft;
        
        // 设置图片
        Global.SetSprite( sprite, str, true );
        // 如果超过宽度,先换行
        if( PositionX_ + sprite.width > LineWidth_ ) {
            PositionX_ = 0;
            AddGameObjectToLine( true, 0, null );
        }
        AddGameObjectToLine( false, PositionX_, go, RichTextInfoType.Sprite );
        PositionX_ += sprite.width + LabelSpacingX_;
        if( bTypewriter )
            go.SetActive( false );
         */
    }

    private GameObject CreateLabel() {
        /*
        GameObject go = NGUITools.AddChild( gameObject, TextLabelTemplate_ );
        Destroy( go.GetComponent<RichTextProcessing>() );
        go.name = "Label";
        Text label = go.GetComponent<Text>();
        label.depth = Depth_;
        label.overflowMethod = UILabel.Overflow.ResizeHeight;
        label.maxLineCount = 1;
        go.SetActive( true );
        return go;
         */
        //TODO temp
        return null;
    }

    private void CutString( string iString, string iCommand, out string oFirstStr, out string oLastStr ) {
        int EqualIndex_ =  iString.IndexOf( iCommand );
        oFirstStr = iString.Substring( 0, EqualIndex_ );
        oLastStr = iString.Substring( EqualIndex_ + iCommand.Length, iString.Length - (EqualIndex_ + iCommand.Length) );
    }

    /// <summary>
    /// 触发超链接点击
    /// </summary>
    /// <param name="data"></param>
    public void ClickHyperLinks( string data ) {
        string[] list = data.Split( '/' );
        if( list.Length < 2 ) return;
        HyperlinksType tempType = (HyperlinksType)int.Parse( list[0] );
        string tempValue = list[1];
        Debug.Log( tempType + "," + tempValue );

        // 需要触发超链接的类挂接该回调
        if( EventOnClickHyperlinks != null )
            EventOnClickHyperlinks( tempType, tempValue );
    }

    /// <summary>
    /// 解析字符串并显示排版
    /// </summary>
    /// <param name="str">传入的字符串</param>
    /// <param name="bTypewriter">是否显示打字机效果(一个一个字的显示)</param>
    /// <returns></returns>
    public IEnumerator ShowStr( string str, bool bTypewriter = false ) {
        // 如果空,返回
        if( string.IsNullOrEmpty( str ) )
            yield break;

        Reset();

        Analyze( str );

        CreateAll( bTypewriter );

        Sort();

        if( bTypewriter )
            yield return StartCoroutine( Typewriter() );
    }
}
