using UnityEditor;
using UnityEngine;

public class SvnUtility{
    [MenuItem( "PalmPioneer/Svn/Update Project" )]
    private static void  SvnUpdateProject(){
        Debug.Log( Application.dataPath );      
        string strCmdText = "/C tortoiseproc /command:update /path:" + Application.dataPath;
        System.Diagnostics.Process.Start( "CMD.exe", strCmdText );
    }

    [MenuItem( "PalmPioneer/Svn/Commit Scripts" )]
    private static void SvnCommitScripts() {
        Debug.Log( Application.dataPath );
        string strCmdText = "/C tortoiseproc /command:commit /path:" + Application.dataPath + "/Scripts";
        System.Diagnostics.Process.Start( "CMD.exe", strCmdText );

        strCmdText = "/C tortoiseproc /command:commit /path:" + Application.dataPath + "/resources";
        System.Diagnostics.Process.Start( "CMD.exe", strCmdText );
    }
}

