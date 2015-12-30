using System;
using UnityEngine;

public class Timer {

    public Timer () {

    }

    // 启动定时器
    public void Startup ( float Secs ) {
        m_interval = Secs;
        Update ();
    }

    // 是否超时
    public bool IsTimeOut () {
        return UnityEngine.Time.realtimeSinceStartup >= m_updateTime + m_interval;
    }

    // 是否到达下一个时间变
    public bool ToNextTime () {
        if ( IsTimeOut () ) {
            Update ();

            return true;
        }
        else {
            return false;
        }
    }

    // 是否时间结束
    public bool TimeOver () {
        if ( IsEnable () && IsTimeOut () ) {
            Disable ();

            return true;
        }

        return false;
    }

    // 使定时器无效化
    public void Disable () {
        m_interval = m_updateTime = 0;
    }

    // 定时器是否处于激活
    public bool IsEnable () {
        return m_updateTime != 0;
    }

    // 延长定时器的时间片
    public void IncInterval ( float Secs, float Limit ) {
        m_interval = Math.Max ( m_interval + Secs, Limit );
    }

    // 减少定时器的时间片
    public void DecInterval ( float Secs, float Limit ) {
        m_interval = Math.Min ( m_interval - Secs, Limit );
    }

    // 是否超时---指定时间
    public bool IsTimeOut ( float Secs ) {
        return UnityEngine.Time.realtimeSinceStartup >= m_updateTime + Secs;
    }

    // 是否到达下个时间--指定时间
    public bool ToNextTime ( float Secs ) {
        if ( IsTimeOut ( Secs ) ) {
            Update ();

            return true;
        }

        return false;
    }

    public bool ToNextTick ( float Secs ) {
        if ( IsTimeOut ( Secs ) ) {
            if ( UnityEngine.Time.realtimeSinceStartup >= m_updateTime + Secs * 2 ) {
                Update ();

                return true;
            }
            else {
                m_updateTime += Secs;

                return true;
            }
        }

        return false;
    }

    // 剩余时间
    public float GetRemain () {
        float interval = UnityEngine.Time.realtimeSinceStartup - m_updateTime;

        return Math.Min ( Math.Max ( m_interval - interval, 0 ), m_interval );
    }

    public float IntervalTime {
        get {
            return m_interval;
        }
        set {
            m_interval = value;
        }
    }

    // 用于将引擎的Tick时间周期赋值给定时器
    private void Update () {
        m_updateTime = UnityEngine.Time.realtimeSinceStartup;
    }

    private float m_interval = 0;
    private float m_updateTime = 0;
}
