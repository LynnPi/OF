using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FightAnalyzer {


    public static readonly FightAnalyzer Instance = new FightAnalyzer();

    private proto.S2CFightReport ReportMsg_;

    private Dictionary<int, int> LostUnitDict_ = new Dictionary<int, int>();
    private int LostUnitCount_;
    private bool LostFlagShip_;
    private bool DestoryHeadquarters_;
    private FightAnalyzer() {
        ReportMsg_ = new proto.S2CFightReport();
    }


    private int AttackUnitAmount_ { get; set; }
    private int DefenderUnitAmount_ { get; set; }

    /// <summary>
    /// 初始化分析器
    /// </summary>
    /// <param name="attackerUnitAmount"></param>
    /// <param name="DefenderUnitAmount"></param>
    public void Init( int attackerUnitAmount, int DefenderUnitAmount ) {
        AttackUnitAmount_ = attackerUnitAmount;
        DefenderUnitAmount_ = DefenderUnitAmount;

        ReportMsg_.Clear();

        LostUnitDict_.Clear();
        LostUnitCount_ = 0;
        DestoryHeadquarters_ = false;
        LostFlagShip_ = false;
    }

    /// <summary>
    /// 单位被摧毁
    /// </summary>
    public void OnUnitDestory( ShipEntity entity ) {

        if( entity.CampType == FightServiceDef.CampType.Camp_Attacker ) {
            int refID = entity.Ship.Reference.id;
            if( LostUnitDict_.ContainsKey( entity.Ship.Reference.id ) ) {
                LostUnitDict_[refID]++;
            }
            else {
                LostUnitDict_.Add( refID, 1 );
            }

            if( entity.Ship.IsCommanderShip() )
                LostFlagShip_ = true;

            LostUnitCount_++;
        }
        else {
            // 指挥中心的判断，Demo版本先用ID来判断
            if( entity.Ship.Reference.id == 60000 ) {
                DestoryHeadquarters_ = true;
            }
        }

        // TUDO  其他计算暂时不实现
    }

    /// <summary>
    /// 战斗结束检查
    /// </summary>
    /// <returns></returns>
    public bool FightOver() {
        if( EntityManager.Instance.DefenderUnitList.Count == 0 ) {
            // 这种情况是完胜
            CompleteWin();
            return true;
        }
        else if( EntityManager.Instance.AttackUnitList.Count == 0 ) {
            AnalyzeFightResult();
            return true;
        }
        return false;

    }

    /// <summary>
    /// 完胜
    /// </summary>
    private void CompleteWin() {
        ReportMsg_.fightresult = proto.S2CFightReport.Result.Win;
        ReportMsg_.grade = Def.BattleGrade.S;
        ReportMsg_.destroyratio = 100;
        ReportMsg_.exp = GetExpByGrade( ReportMsg_.grade );

        ReportMsg_.energy = 10000;
        ReportMsg_.capital = 20000;
        ReportMsg_.mdeal = 3;
        ReportMsg_.extraexp = 0;

        FillLostUnitInfo();


        BattleSys.OnFightReport( ReportMsg_ );
    }

    /// <summary>
    /// 分析战斗结果
    /// </summary>
    private void  AnalyzeFightResult(){
        // 如果没有打破总部，破坏度也低于50% 在判定这波攻击失败
        int destoryRaio = GetDestoryRatio();

        // 如果击破率低于50%且没有打破敌方指挥中心
        if( destoryRaio < 50 && !DestoryHeadquarters_ )
            ReportMsg_.fightresult = proto.S2CFightReport.Result.Fail;
        else
            ReportMsg_.fightresult = proto.S2CFightReport.Result.Win;

        ReportMsg_.destroyratio = destoryRaio;

        FillLostUnitInfo();

        int grade = GetGrade( destoryRaio );
        ReportMsg_.grade = grade;
        if( ReportMsg_.fightresult == proto.S2CFightReport.Result.Fail ) {
            BattleSys.OnFightReport( ReportMsg_ );
            return;
        }

        int exp = GetExpByGrade( grade );
        ReportMsg_.exp = exp;

        ReportMsg_.energy = 10000;
        ReportMsg_.capital = 20000;
        ReportMsg_.extraexp = 0;
        ReportMsg_.mdeal = 3;

        BattleSys.OnFightReport( ReportMsg_ );
    }

    /// <summary>
    /// 获取击破率
    /// </summary>
    /// <returns></returns>
    private int GetDestoryRatio() {
        int enemyAliveAmount = EntityManager.Instance.DefenderUnitList.Count;
        return (int)(((DefenderUnitAmount_ - enemyAliveAmount) / (float)DefenderUnitAmount_)*100);
    }

    /// <summary>
    /// 填充损失单位信息
    /// </summary>
    private void FillLostUnitInfo() {
        foreach( var lostInfo in LostUnitDict_ ) {
            proto.LostUnitInfo info = new proto.LostUnitInfo();
            info.refid = lostInfo.Key;
            info.amount = lostInfo.Value;
            ReportMsg_.add_lostunit( info );
        }
    }

    private int GetGrade(int destroyRaio) {
        int WinPoint = 0;
        if( DestoryHeadquarters_ )
            WinPoint++;
        if( LostFlagShip_ )
            WinPoint--;

        if( destroyRaio >= 100 ) {
            WinPoint += 5;
        }
        else if( destroyRaio >= 80 ) {
            WinPoint += 4;
        }
        else if( destroyRaio >= 60 ) {
            WinPoint += 3;
        }
        else if( destroyRaio >= 40 )
            WinPoint += 2;
        else if( destroyRaio >= 20 )
            WinPoint += 1;

        switch( WinPoint ) {
            case -1:
            case 0:
            case 1:
                return Def.BattleGrade.D;
            case 2:
                return Def.BattleGrade.C;
            case 3:
                return Def.BattleGrade.B;
            case 4:
                return Def.BattleGrade.A;
            case 5:
            default:
                return Def.BattleGrade.D;
        }
    }

    /// <summary>
    /// 获取经验
    /// </summary>
    /// <param name="grade"></param>
    /// <returns></returns>
    private int GetExpByGrade( int grade ) {
        // TODO: StarChild2 版本把代码写死
        switch( grade ) {
            case Def.BattleGrade.S:
                return 1500;
            case Def.BattleGrade.A:
                return 1000;
            case Def.BattleGrade.B:
                return 650;
            case Def.BattleGrade.C:
                return 400;
            case Def.BattleGrade.D:
                return 200;
            default:
                return 200;
        }
    }
}

