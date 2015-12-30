using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 消息派发器
/// </summary>
public class MessageDispatcher {

    // 消息排序
    private class TelegramCompare : IComparer<Telegram> {
        public int Compare(Telegram lhs, Telegram rhs){
            if( lhs.TriggerFrame < rhs.TriggerFrame )
                return -1;
            else if( lhs.TriggerFrame < rhs.TriggerFrame )
                return 1;
            else {
                if( lhs.GUID < rhs.GUID )
                    return -1;
            }
            return 0;
        }
    }

    public readonly static MessageDispatcher Instance = new MessageDispatcher();

    private PriorityQueue<Telegram> TelegramQueue_; 
    private MessageDispatcher() {
        TelegramQueue_ = new PriorityQueue<Telegram>(100, new TelegramCompare());
    }

    public void DispatchMessage( int msgType, int sender, int receiver, int delayFrame, int val1 = 0, int val2 = 0, float val3 = 0, float val4 = 0, float val5 = 0 ) {
        Telegram msg = new Telegram();
        msg.MsgType = msgType;
        msg.SenderID = sender;
        msg.ReceiverID = receiver;
        msg.TriggerFrame = delayFrame;
        msg.Val1 = val1;
        msg.Val2 = val2;
        msg.Val3 = val3;
        msg.Val4 = val4;
        msg.Val5 = val5;
        if( delayFrame > 0 ) {
            msg.TriggerFrame += FightTicker.FrameCount;
            TelegramQueue_.Push( msg );
        }
        else {
            Entity entity = EntityManager.Instance.GetEntityByID( msg.ReceiverID );
            if( entity != null ) {
                Discharge( entity, msg );
            }
        }
    }

    public void ClearAll() {
        TelegramQueue_.Clear();
    }

    /// <summary>
    /// 派发延时消息
    /// </summary>
    public void DispatchDelayedMessages() {
        while( TelegramQueue_.Count > 0 && TelegramQueue_.Top.TriggerFrame <= FightTicker.FrameCount ) {
            Telegram msg = TelegramQueue_.Top;
            Entity entity = EntityManager.Instance.GetEntityByID( msg.ReceiverID );
            if( entity != null) {
                Discharge( entity, msg );
            }
            TelegramQueue_.Pop();
        }
    }

    private void Discharge(Entity receiver, Telegram telegram){
        if( !receiver.HandleMessage( telegram ) ) {
            Debug.Log( "No Receiver with ID of" + receiver.ID.ToString() + "found" );
        }
    } 
}
