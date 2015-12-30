using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BattleMiniMapPanel : UIPanelBehaviour {
    private Button MiniBtn_;
   
    protected override void OnAwake() {
        MiniBtn_ = transform.FindChild("Button_Mini").GetComponent<Button>();
        MiniBtn_.onClick.AddListener(OnMiniClick);
    }

    private void OnMiniClick(){
        SampleNotePopWindow.Instance.ShowMessage(1005);
    }
}
