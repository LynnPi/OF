using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using NumberExtension;

public class BattleResultPanel : UIPanelBehaviour {
    private Slider DestroyRatioSlider_;

    private Image BattleResultTitle_;
    private Image GradeImg_;

    private Text ExpText_;
    private Text ExtraExpText_;
    private Text CapitalText_;
    private Text EnergyText_;
    private Text MdealText_;
    private Text DestroyRatioText_;
    private Text NoLostNoteText_;

    private NumberRising ExpRising_;
    private NumberRising ExtraExpRising_;
    private NumberRising CapitalRising_;
    private NumberRising EnergyRising_;
    private NumberRising MdealRising_;
    private NumberRising DestroyRatioRising_;

    private const float ANIM_SHOW_DURATION = 1.5F;
    private const float NUMBER_RISING_DURATION = 2F;

    private Transform LostUnitContentRoot_;
    private void ResetPanelData() {
        DestroyRatioSlider_.value = 0f;
        DestroyRatioText_.text = "0";
        ExpText_.text = "0";
        ExtraExpText_.text = "0";
        CapitalText_.text = "0";
        EnergyText_.text = "0";
        MdealText_.text = "0";

        ExpRising_.Target = 0f;
        ExtraExpRising_.Target = 0f;
        CapitalRising_.Target = 0f;
        EnergyRising_.Target = 0f;
        MdealRising_.Target = 0f;
        DestroyRatioRising_.Target = 0f;

        BattleResultTitle_.enabled = false;
        GradeImg_.enabled = false;
        NoLostNoteText_.enabled = false;

        Utility.DestroyChildren(LostUnitContentRoot_);
    }
    protected override void OnAwake() {
        Button button = transform.FindChild( "ReturnButton" ).GetComponent<Button>();
        button.onClick.AddListener( OnReturnClick );

        BattleResultTitle_ = transform.FindChild("Title/Image_ResultTitle").GetComponent<Image>();
        GradeImg_ = transform.FindChild("Got/Rank/Image_Rank").GetComponent<Image>();

        DestroyRatioSlider_ = transform.FindChild("Got/DestroyRatio").GetComponent<Slider>();
        DestroyRatioText_ = DestroyRatioSlider_.transform.FindChild("Text_Percent").GetComponent<Text>();    
        DestroyRatioRising_ = DestroyRatioText_.gameObject.AddComponent<NumberRising>();
        DestroyRatioRising_.Observer = target => DestroyRatioText_.text = target.ToThousandFormatString() + "%";

        ExpText_ = transform.FindChild("Got/Exp/Text_Exp").GetComponent<Text>();
        ExpRising_ = ExpText_.gameObject.AddComponent<NumberRising>();
        ExpRising_.Observer = target => ExpText_.text = "+" + target.ToThousandFormatString();

        ExtraExpText_ = transform.FindChild("Got/Exp/Text_ExtraExp").GetComponent<Text>();
        ExtraExpRising_ = ExtraExpText_.gameObject.AddComponent<NumberRising>();
        ExtraExpRising_.Observer = target => ExtraExpText_.text = "+" + target.ToThousandFormatString();

        CapitalText_ = transform.FindChild("Got/Capital/Text_Capital").GetComponent<Text>();
        CapitalRising_ = CapitalText_.gameObject.AddComponent<NumberRising>();
        CapitalRising_.Observer = target => CapitalText_.text = target.ToThousandFormatString();

        EnergyText_ = transform.FindChild("Got/Energy/Text_Energy").GetComponent<Text>();
        EnergyRising_ = EnergyText_.gameObject.AddComponent<NumberRising>();
        EnergyRising_.Observer = target => EnergyText_.text = target.ToThousandFormatString();

        MdealText_ = transform.FindChild("Got/Mdeal/Text_Mdeal").GetComponent<Text>();
        MdealRising_ = MdealText_.gameObject.AddComponent<NumberRising>();
        MdealRising_.Observer = target => MdealText_.text = target.ToThousandFormatString();

        LostUnitContentRoot_ = transform.FindChild("LostUnits/Content");

        NoLostNoteText_ = transform.FindChild("LostUnits/Text_NoLost").GetComponent<Text>();
    }

    protected override void OnShow( params object[] args ) {
        OnReShow();
        StartCoroutine( ShowResultSequence( args[0] as proto.S2CFightReport ) );
    }

    protected override void OnReShow() {
        CameraManager.SetTouchActive( false );
        ResetPanelData();
    }

    private void OnReturnClick() {
        SceneManager.EnterScene( SceneManager.SceneType.Login );
    }

    private IEnumerator ShowResultSequence(proto.S2CFightReport result) {
        if( result == null ) {
            Debug.LogError( "ShowResultSequence(), RESULT IS NULL!" );
            yield break;
        }
        Debug.Log( "ShowResultSequence(), result : " + result.fightresult.ToString() );

        yield return new WaitForSeconds( ANIM_SHOW_DURATION );//等待面板动画显示完成

        StartCoroutine( ShowTitle( result.fightresult ) );
        StartCoroutine( ShowDestoryRatio( result.destroyratio ) );
        StartCoroutine( ShowGotEnergy( result.energy ) );
        StartCoroutine( ShowGotCapital( result.capital ) );
        StartCoroutine( ShowExp( result.exp ) );
        StartCoroutine( ShowExtraExp( result.extraexp ) );
        StartCoroutine( ShowMdeal( result.mdeal ) );
        StartCoroutine( ShowGrade(result.grade));
        StartCoroutine( ShowLostUnits( result ) );
    }

    /// <summary>
    /// 显示标题（胜利、失败or完成）
    /// </summary>
    /// <param name="resultType"></param>
    /// <returns></returns>
    private IEnumerator ShowTitle( proto.S2CFightReport.Result resultType ) {
        Debug.Log( "ShowTitle()" );
        BattleResultTitle_.enabled = true;
        switch( resultType ) {
            case proto.S2CFightReport.Result.Win:
                Global.SetSprite(BattleResultTitle_, "art_v", true);
                break;
            case proto.S2CFightReport.Result.Fail:
                Global.SetSprite(BattleResultTitle_, "art_f", true);
                break;
            case proto.S2CFightReport.Result.COMPLETE:
                break;
            default:
                break;
        }
        yield return null;
    }

    /// <summary>
    /// 显示战斗评级：S、A、B、C、D
    /// </summary>
    /// <param name="grade"></param>
    /// <returns></returns>
    private IEnumerator ShowGrade(int grade) {
        yield return new WaitForSeconds(NUMBER_RISING_DURATION);
        Debug.Log("showGreade : " + grade);
        GradeImg_.enabled = true;

        switch( grade ) {  
            case Def.BattleGrade.A:
                Global.SetSprite(GradeImg_, "art_a", true);
                break;
            case Def.BattleGrade.B:
                Global.SetSprite(GradeImg_, "art_b", true);
                break;
            case Def.BattleGrade.C:
                Global.SetSprite(GradeImg_, "art_c", true);
                break;
            case Def.BattleGrade.D:
                Global.SetSprite(GradeImg_, "art_d", true);
                break;
            case Def.BattleGrade.S:
                Global.SetSprite(GradeImg_, "art_s", true);
                break;
            default:
                Debug.LogError("Exceptional Grade: " + grade);
                break;
        }
    }

    /// <summary>
    /// 显示击破率
    /// </summary>
    /// <param name="ratio"></param>
    /// <returns></returns>
    private IEnumerator ShowDestoryRatio(int ratio) {
        Debugger.Log("ShowDestoryRatio(), ratio: " + ratio);
        float percent = ratio / 100f;
        StartCoroutine(SlideDestroyRatio(NUMBER_RISING_DURATION, percent));
        DestroyRatioRising_.Play(NUMBER_RISING_DURATION, (ulong)ratio);
        yield return null;
    }

    /// <summary>
    /// 显示勋章
    /// </summary>
    /// <param name="mdeal"></param>
    /// <returns></returns>
    private IEnumerator ShowMdeal(int value) {
        MdealRising_.Play( NUMBER_RISING_DURATION, (ulong)value );
        yield return null;
    }

    /// <summary>
    /// 显示获得能量
    /// </summary>
    /// <param name="energy"></param>
    /// <returns></returns>
    private IEnumerator ShowGotEnergy(int value) {
        EnergyRising_.Play( NUMBER_RISING_DURATION, (ulong)value );
        yield return null;
    }

    /// <summary>
    /// 显示获得金钱
    /// </summary>
    /// <param name="capital"></param>
    /// <returns></returns>
    private IEnumerator ShowGotCapital(int value) {
        CapitalRising_.Play( NUMBER_RISING_DURATION, (ulong)value );
        yield return null;
    }

    /// <summary>
    /// 显示战损单位信息
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private IEnumerator ShowLostUnits( proto.S2CFightReport result ) {
        int lostUnitSize = result.lostunit_size();
       
        if( lostUnitSize == 0 ) {
            NoLostNoteText_.enabled = true;
            NoLostNoteText_.text = "No unit loss, congratulation!";
            yield break;
        }
        for( int i = 0; i < lostUnitSize; i++ ) {
            proto.LostUnitInfo info = result.lostunit( i );
            string iconName = GlobalConfig.GetUnitReference(info.refid).iconfile;
            Debug.Log("lost iocnName : " + iconName);
            GameObject obj = Global.CreateUI("lostunit", LostUnitContentRoot_.gameObject);
            obj.name = string.Format("_lost_id_{0}", info.refid);
            Image iconImg = obj.GetComponent<Image>();
            Text amountText = obj.GetComponentInChildren<Text>();
            Global.SetSprite(iconImg, iconName, true);
            amountText.text = string.Format("x{0}", info.amount);

            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }

    /// <summary>
    /// 显示获得经验
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    private IEnumerator ShowExp(int value) {
        ExpRising_.Play( NUMBER_RISING_DURATION, (ulong)value );
        yield return null;
    }

    /// <summary>
    /// 显示获得额外经验
    /// </summary>
    /// <param name="extraExp"></param>
    /// <returns></returns>
    private IEnumerator ShowExtraExp(int value) {
        ExtraExpRising_.Play(NUMBER_RISING_DURATION, (ulong)value);
        yield return null;
    }

    private IEnumerator SlideDestroyRatio(float duration, float to) {
        if( to <= 0f || to > 1f ) {
            yield break;
        }

        float current = 0f;
        while( current <= to ) {
            current += Time.deltaTime / duration;
            DestroyRatioSlider_.value = current;
            yield return null;
        }
    }
}
