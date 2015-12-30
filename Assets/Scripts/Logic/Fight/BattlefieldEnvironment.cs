using UnityEngine;
using System.Collections;

/// <summary>
/// 战场的各种环境变量管理
/// </summary>
public class BattlefieldEnvironment {
    public static readonly BattlefieldEnvironment Instance = new BattlefieldEnvironment();


    /// TODO 还需要基地区，纵深区，基地区，战场宽度的变量控制

    /// <summary>
    /// 脱离边界，Z值
    /// </summary>
    public int BattlefieldZBoundary {
        get;
        private set;
    }

    public void Init() {
        // Demo版本写死，后面要根据玩家的成长和数据配置来填充具体的数值
        BattlefieldZBoundary = 0;
    }

}
