#region
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Xml;
#endregion

public class UIDisplayControl {

    public string UIName;
    public bool ClearOpened;
    public string DontClose;
    public bool JustCloseSelfWhenClosing;
}

public static class GlobalConfig {

    private static Dictionary<string, UIDisplayControl> UIDisplayControlList = new Dictionary<string, UIDisplayControl> ();

    private static Dictionary<int,proto.UnitReference> UnitReferenceList = new Dictionary<int, proto.UnitReference>();

    private static Dictionary<int,proto.DefensiveUnitsReference> DefensiveUnitsReferenceList = new Dictionary<int, proto.DefensiveUnitsReference>();

    private static Dictionary<int,proto.BuffReference> BuffReferenceList = new Dictionary<int, proto.BuffReference>();

    private static Dictionary<int,proto.PartsReplaceTeamReference> PartsReplaceTeamReferenceList = new Dictionary<int, proto.PartsReplaceTeamReference>();

    private static Dictionary<int,proto.ShuttleReference> ShuttleReferenceList = new Dictionary<int, proto.ShuttleReference>();

    private static Dictionary<int,proto.ShuttleTeamReference> ShuttleTeamReferenceList = new Dictionary<int, proto.ShuttleTeamReference>();

    private static Dictionary<int,proto.SkillReference> SkillReferenceList = new Dictionary<int, proto.SkillReference>();

    private static Dictionary<int,proto.PartReference> PartReferenceList = new Dictionary<int, proto.PartReference>();

    private static Dictionary<int, string> SysNoticeList = new Dictionary<int, string> ();

    private static Dictionary<int, proto.BattlefieldReference> BattlefieldReferenceList = new Dictionary<int, proto.BattlefieldReference>();

    private static Dictionary<int, List<proto.LevelDeployUnitReference>> LevelDeployReferenceDic = new Dictionary<int, List<proto.LevelDeployUnitReference>>();

    private static List<Vector3[]> PlayerFormationList = new List<Vector3[]>();

    private static List<Vector3[]> EnemyFormationList = new List<Vector3[]>();

#region private member func
    private static int AnalyzeXmlToInt ( XmlNode Node, string Key ) {
        if ( Node.Attributes[ Key ] != null ) {
            string TempValue = Node.Attributes[ Key ].Value;
            if ( TempValue.Length > 0 )
                return int.Parse ( TempValue );
        }
        return 0;
    }

    private static string AnalyzeXmlToString ( XmlNode Node, string Key ) {
        if ( Node.Attributes[ Key ] != null )
            return Node.Attributes[ Key ].Value;
        return "";
    }

    private static bool AnalyzeXmlToBool( XmlNode Node, string Key ) {
        if( Node.Attributes[Key] != null ) {
            string str = Node.Attributes[Key].Value;
            return bool.Parse( str );
        }
        return false;
    }

    private static void InitFormationList() {
        Vector3[] tempList;

        // 1
        tempList = new Vector3[5] { new Vector3( 0, -3, 7 ), new Vector3( 6, 0, 0 ), new Vector3( -6, 0, 0 ), new Vector3( 11, 3, -7 ), new Vector3( -11, 3, -7 ) };
        PlayerFormationList.Add( tempList );
        tempList = new Vector3[5] { new Vector3( 0, -3, -7 ), new Vector3( 6, 0, 0 ), new Vector3( -6, 0, 0 ), new Vector3( 11, 3, 7 ), new Vector3( -11, 3, 7 ) };
        EnemyFormationList.Add( tempList );

        // 2
        tempList = new Vector3[5] { new Vector3( 0, 3, -7 ), new Vector3( 6, 0, 0 ), new Vector3( -6, 0, 0 ), new Vector3( 11, -3, 7 ), new Vector3( -11, -3, 7 ) };
        PlayerFormationList.Add( tempList );
        tempList = new Vector3[5] { new Vector3( 0, 3, 7 ), new Vector3( 6, 0, 0 ), new Vector3( -6, 0, 0 ), new Vector3( 11, -3, -7 ), new Vector3( -11, -3, -7 ) };
        EnemyFormationList.Add( tempList );

        // 3
        tempList = new Vector3[5] { new Vector3( 0, 0, 0 ), new Vector3( 10, -1, 5 ), new Vector3( -10, -1, 5 ), new Vector3( 0, 2, 12 ), new Vector3( 0, -3, -9 ) };
        PlayerFormationList.Add( tempList );
        tempList = new Vector3[5] { new Vector3( 0, 0, 0 ), new Vector3( 10, -1, -5 ), new Vector3( -10, -1, -5 ), new Vector3( 0, 2, -12 ), new Vector3( 0, -3, 9 ) };
        EnemyFormationList.Add( tempList );

        // 4
        tempList = new Vector3[5] { new Vector3( 0, 4, -10 ), new Vector3( -10, 0, 2 ), new Vector3( 10, 0, 2 ), new Vector3( -5, -4, 10 ), new Vector3( 5, -4, 10 ) };
        PlayerFormationList.Add( tempList );
        tempList = new Vector3[5] { new Vector3( 0, 4, 10 ), new Vector3( -10, 0, -2 ), new Vector3( 10, 0, -2 ), new Vector3( -5, -4, -10 ), new Vector3( 5, -4, -10 ) };
        EnemyFormationList.Add( tempList );

        // 5
        tempList = new Vector3[5] { new Vector3( 0, 2, -11 ), new Vector3( -6, 8, 0 ), new Vector3( 6, 8, 0 ), new Vector3( -11, -4, 11 ), new Vector3( 11, -4, 11 ) };
        PlayerFormationList.Add( tempList );
        tempList = new Vector3[5] { new Vector3( 0, 2, 11 ), new Vector3( -6, 8, 0 ), new Vector3( 6, 8, 0 ), new Vector3( -11, -4, -11 ), new Vector3( 11, -4, -11 ) };
        EnemyFormationList.Add( tempList );
    }
#endregion

#region interface

    public static bool LoadXml () {
        try {
            // 读配置文件
            TextAsset textAsset = ( TextAsset )Resources.Load ( "Config/Global", typeof ( TextAsset ) );
            if ( textAsset == null ) {
                Debug.Log ( "can not load resource Config/Global.xml" );
                return false;
            }
            XmlDocument XmlDoc = new XmlDocument ();
            XmlDoc.LoadXml ( textAsset.text );

            XmlNodeList NodeList = XmlDoc.SelectSingleNode ( "Config" ).ChildNodes;
            foreach ( XmlNode Node in NodeList ) {
                if ( Node.Name == "SysNotice" ) {
                    int ID = AnalyzeXmlToInt ( Node, "ID" );
                    string Content = AnalyzeXmlToString ( Node, "Content" );
                    if ( Content == "" ) continue;
                    if ( !SysNoticeList.ContainsKey ( ID ) )
                        SysNoticeList.Add ( ID, Content );
                }
                else if ( Node.Name == "UIDisplayControl" ) {
                    UIDisplayControl ctrl = new UIDisplayControl ();
                    ctrl.UIName = AnalyzeXmlToString ( Node, "UIName" );
                    ctrl.DontClose = AnalyzeXmlToString( Node, "DontClose" );
                    ctrl.ClearOpened = AnalyzeXmlToBool( Node, "ClearOpened" );
                    ctrl.JustCloseSelfWhenClosing = AnalyzeXmlToBool( Node, "JustCloseSelfWhenClosing" );
                    UIDisplayControlList.Add( ctrl.UIName, ctrl );
                }
            }

            InitFormationList();
            return true;
        }
        catch ( Exception err ) {
            Debug.Log ( err.Message );
            return false;
        }
    }

    public static void InitBin () {
        UnitReferenceList.Clear();
        DefensiveUnitsReferenceList.Clear();
        BuffReferenceList.Clear();
        PartsReplaceTeamReferenceList.Clear();
        ShuttleReferenceList.Clear();
        ShuttleTeamReferenceList.Clear();
        SkillReferenceList.Clear();
        PartReferenceList.Clear();
        BattlefieldReferenceList.Clear();
        LevelDeployReferenceDic.Clear();
    }

    public static void AnalyzeBin( byte[] bytes ){
        var tempManager = new proto.ReferenceManager();
        tempManager.Parse( bytes, 0, bytes.Length );

        for( int i=0; i < tempManager.units_res_size(); i++ ) {
            proto.UnitReference unit = tempManager.units_res( i );
            UnitReferenceList.Add( unit.id, unit );
        }

        for( int i=0; i < tempManager.defensive_units_res_size(); i++ ) {
            proto.DefensiveUnitsReference unit = tempManager.defensive_units_res( i );
            DefensiveUnitsReferenceList.Add( unit.id, unit );
        }

        for( int i=0; i < tempManager.buff_res_size(); i++ ) {
            proto.BuffReference unit = tempManager.buff_res( i );
            BuffReferenceList.Add( unit.id, unit );
        }

        for( int i=0; i < tempManager.parts_replace_team_res_size(); i++ ) {
            proto.PartsReplaceTeamReference unit = tempManager.parts_replace_team_res( i );
            PartsReplaceTeamReferenceList.Add( unit.id, unit );
        }

        for( int i=0; i < tempManager.shuttle_res_size(); i++ ) {
            proto.ShuttleReference unit = tempManager.shuttle_res( i );
            ShuttleReferenceList.Add( unit.id, unit );
        }

        for( int i=0; i < tempManager.shuttle_team_res_size(); i++ ) {
            proto.ShuttleTeamReference unit = tempManager.shuttle_team_res( i );
            ShuttleTeamReferenceList.Add( unit.id, unit );
        }

        for( int i=0; i < tempManager.skill_res_size(); i++ ) {
            proto.SkillReference unit = tempManager.skill_res( i );
            SkillReferenceList.Add( unit.id, unit );
        }

        for( int i=0; i < tempManager.parts_res_size(); i++ ) {
            proto.PartReference unit = tempManager.parts_res( i );
            PartReferenceList.Add( unit.id, unit );
        }

        for( int i = 0; i < tempManager.battlefield_res_size(); i++ ) {
            proto.BattlefieldReference battle = tempManager.battlefield_res( i );
            BattlefieldReferenceList.Add( battle.id, battle );
        }

        List<proto.LevelDeployUnitReference> levelDelpoyUnitList = null;
        for( int i = 0; i < tempManager.leveldeployunit_res_size(); i++ ) {
            proto.LevelDeployUnitReference levelDeployUnit = tempManager.leveldeployunit_res( i );

            if( LevelDeployReferenceDic.ContainsKey(levelDeployUnit.battlefield_id) ) {
                levelDelpoyUnitList = LevelDeployReferenceDic[levelDeployUnit.battlefield_id];
            }
            else {
                levelDelpoyUnitList = new List<proto.LevelDeployUnitReference>();
                LevelDeployReferenceDic.Add( levelDeployUnit.battlefield_id, levelDelpoyUnitList );
            }

            levelDelpoyUnitList.Add( levelDeployUnit );
        }
    }

    public static string GetSysNotice ( int ID ) {
        string result = "";
        if ( SysNoticeList == null )
            return result;
        SysNoticeList.TryGetValue ( ID, out result );
        return result;
    }

    public static UIDisplayControl GetUIDisplayControl ( string name ) {
        UIDisplayControl ctrl = null;
        UIDisplayControlList.TryGetValue ( name, out ctrl );
        return ctrl;
    }

    public static proto.UnitReference GetUnitReference( int id ) {
        proto.UnitReference unit = null;
        UnitReferenceList.TryGetValue( id, out unit );
        return unit;
    }

    public static proto.PartReference GetPartReference( int id ) {
        proto.PartReference parts = null;
        PartReferenceList.TryGetValue( id, out parts );
        return parts;
    }

    public static proto.SkillReference GetSkillReference( int id ) {
        if( SkillReferenceList.ContainsKey( id ) )
            return SkillReferenceList[id];

        return null;
    }

    public static proto.BuffReference GetBuffReference( int id ) {
        if( BuffReferenceList.ContainsKey( id ) )
            return BuffReferenceList[id];
        return null;
    }

    public static List<proto.LevelDeployUnitReference> GetUnitLisByBattlefieldByID( int id ) {
        if( LevelDeployReferenceDic.ContainsKey(id) )
            return LevelDeployReferenceDic[id];

        return null;
    }

    public static proto.BattlefieldReference GetBattlefieldReferenceByID( int id ) {
        if( BattlefieldReferenceList.ContainsKey( id ) )
            return BattlefieldReferenceList[id];

        return null;
    }

    public static Vector3[] GetFormationList( bool bPlayer ) {
        List<Vector3[]> list = bPlayer ? PlayerFormationList : EnemyFormationList;
        int index = UnityEngine.Random.Range( 0, list.Count - 1 );
        return list[index];
    }

#endregion
}