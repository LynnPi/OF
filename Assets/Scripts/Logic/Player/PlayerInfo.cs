using System;
using System.Collections.Generic;

using Timer = System.Timers.Timer;

/// <summary>
/// �ٻ�ʦ��Ϣ
/// </summary>
public class PlayerInfo {

    /// <summary>
    /// SN��ʶ
    /// </summary>
    public int                              Sn;

    public List<ClientShip>                 ClientShipList_ = new List<ClientShip>();

    public void ResetInfo () {
        ClientShipList_.Clear();        
    }
}