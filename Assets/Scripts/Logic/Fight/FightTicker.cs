using UnityEngine;
using System.Collections;


/// <summary>
/// 战场计时器
/// 仅限于
/// </summary>
public static class FightTicker {

    // 当前tick
    public static int Ticker;
    public static int FrameCount;

    public static void InitTick() {
        Ticker = 0;
    }

    public static void Tick() {
        Ticker += FightServiceDef.FRAME_INTERVAL_TIME;
        FrameCount++;
    }
}
