using System;
using System.Collections.Generic;

using Timer = System.Timers.Timer;

/// <summary>
/// 召唤师信息
/// </summary>
public class PlayerInfo {

    /// <summary>
    /// SN标识
    /// </summary>
    public int                              Sn;

    public List<ClientShip>                 ClientShipList_ = new List<ClientShip>();

    public void ResetInfo () {
        ClientShipList_.Clear();        
    }
}