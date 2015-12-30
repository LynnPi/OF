using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class DeployPanel : UIPanelBehaviour {
    private List<DeployUnitUIElement> UnitUIElementList_;

	private Button StartBattleBtn_;

	private Button BackHomeBtn_;

    private Button PagingLeftBtn_;

    private Button PagingRightBtn_;

    private Text RemainEnergyText_;

    private Text MaxEnergyText_;

    private Slider EnergySlider_;

    private Transform ScrollViewRoot_;
    private Transform ScrollViewRoot {
        get {
            if( ScrollViewRoot_ == null ) {
                ScrollViewRoot_ = transform.FindChild( "Scroll/ScrollView-H/Content" );
            }
            return ScrollViewRoot_;
        }
    }

    private int MaxEnergy_;
    private int MaxEnergy {
        set {
            MaxEnergy_ = value;
            MaxEnergyText_.text = MaxEnergy_.ToString();
        }
    }

    private int CurrRemainEnergy_;
    private int CurrRemainEnergy {
        set {
            CurrRemainEnergy_ = value;
            RemainEnergyText_.text = CurrRemainEnergy_.ToString();
            RefreshEnergySliderView();
        }
    }

    protected override void OnAwake() {
		StartBattleBtn_ = transform.FindChild ("Button_Start").GetComponent<Button>();
		StartBattleBtn_.onClick.AddListener ( OnClickStartBtn );

		BackHomeBtn_ = transform.FindChild ("Button_Home").GetComponent<Button>();
		BackHomeBtn_.onClick.AddListener ( OnClickHomeBtn );

        PagingLeftBtn_ = transform.FindChild( "Scroll/Button_Paging_Left" ).GetComponent<Button>();
        PagingLeftBtn_.onClick.AddListener( OnPagingLeft );

        PagingRightBtn_ = transform.FindChild( "Scroll/Button_Paging_Right" ).GetComponent<Button>();
        PagingRightBtn_.onClick.AddListener( OnPagingRight );

        EnergySlider_ = transform.FindChild( "EnergyBar/Slider" ).GetComponent<Slider>();
        MaxEnergyText_ = transform.FindChild( "EnergyBar/Text_Max" ).GetComponent<Text>();
        RemainEnergyText_ = transform.FindChild( "EnergyBar/Text_Used" ).GetComponent<Text>();
    }

    protected override void OnRegEvent() {
        BattleSys.EventOnEnterBattle += BattleSys_EventOnEnterBattle;
    }

    protected override void OnUnregEvent() {
        BattleSys.EventOnEnterBattle -= BattleSys_EventOnEnterBattle;
    }

    protected override void OnShow( params object[] args ) {
        InitPanel();
    }

    protected override void OnReShow() {
        InitPanel();
    }

    protected override void OnClose() {
        //单场景下需要销毁生成的UI列表
        Utility.DestroyChildren( ScrollViewRoot );
        UnitUIElementList_.Clear();
    }

	private void OnClickStartBtn(){
		//Debugger.Log ("OnClickStart");
        DeploySceneManager.Instance.FormationRecorder.RecordDeployInfo();

        if (BattleSys.GetShipCount(true) == 0) {
            SampleNotePopWindow.Instance.ShowMessage(1004);
            return;
        }
        BattleSys.EnterBattle();
	}

    private void OnPagingLeft() {
        //Debugger.Log( "OnPagingLeft" );
    }

    private void OnPagingRight() {
        //Debugger.Log( "OnPagingRight" );
    }

    private void BattleSys_EventOnEnterBattle() {
        SceneManager.EnterScene( SceneManager.SceneType.BattleScene );
    }

	private void OnClickHomeBtn(){
		//TODO 返回家园界面
		Debug.Log ("OnClickHome");
        SampleNotePopWindow.Instance.ShowMessage(1005);
	}

    private void InitPanel() {
        BattleSys.ResetPlayerBattleShip();
        InitEnergy();
        GeneratePlayerShipListView();
   
    }

    /// <summary>
    /// 生成玩家可布阵舰船的视图列表
    /// </summary>
    private void GeneratePlayerShipListView() {
        List<ClientShip> dataList = PlayerSys.GetPlayerShipList();
        //哈希结构，键--舰船ID，值--拥有该ID的舰船集合
        Dictionary<int, List<int>> hash = new Dictionary<int, List<int>>();
        for( int index = 0; index < dataList.Count; index++ ) {
            int key = dataList[index].Reference.id;
            if( !hash.ContainsKey( key ) ) {
                hash.Add( key, new List<int>() );
            }
            hash[key].Add( index );
        }
        
        foreach( var id in hash ) {
            string log = "";
            foreach( var item in id.Value ) {
                log += string.Format( "{0}, ", item );
            }
            //Debug.Log( string.Format( "<color=yellow>[ID{0}] : {1}</color>", id.Key, log) );
        }

        UnitUIElementList_ = new List<DeployUnitUIElement>();

        foreach( var item in hash ) {
            GameObject scrollItem = Global.CreateUI( "deployitem", ScrollViewRoot.gameObject );
            scrollItem.name = item.Key.ToString();
            DeployUnitUIElement view = scrollItem.AddComponent<DeployUnitUIElement>();
            view.SyncEnergy += SyncEnergy;
            proto.UnitReference unit = GlobalConfig.GetUnitReference(item.Key);
            view.ShipName = unit.name;
            view.WrapPointCount = unit.warp_cost;
            view.IconName = unit.iconfile;
            view.Level = 10;//舰船成长，暂时没得
            foreach( var v in item.Value ) {
                view.ReadyDeployIndexQueue.Enqueue( v );
            }
            view.UnitCount = item.Value.Count;
            UnitUIElementList_.Add(view);
        }

        foreach (var item in UnitUIElementList_) {
            bool cannotDeployThisType = item.WrapPointCount > CurrRemainEnergy_;
            if (cannotDeployThisType) {
                item.SetDragAbility(false);
            }
            else {
                if (item.ReadyDeployIndexQueue.Count == 0)
                    continue;
                item.SetDragAbility(true);
            }
        }  
    }

    private void InitEnergy() {
        //从数据层获取，暂时给个死的
        MaxEnergy = 20;
        CurrRemainEnergy = MaxEnergy_;
    }

    private void SyncEnergy( int increment ) {
        CurrRemainEnergy = CurrRemainEnergy_ + increment;

        foreach (var item in UnitUIElementList_) {
            bool cannotDeployThisType = item.WrapPointCount > CurrRemainEnergy_;
            if (cannotDeployThisType) {
                item.SetDragAbility(false);
            }
            else {
                if (item.ReadyDeployIndexQueue.Count == 0)
                    continue;
                item.SetDragAbility(true);
            }
        }       
    }

    private void RefreshEnergySliderView() {
        EnergySlider_.value = (float)CurrRemainEnergy_ / MaxEnergy_;
    }

}