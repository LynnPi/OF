using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class BattleRetreatEntrance : UIPanelBehaviour {
    protected override void OnAwake() {
        Button btn = GetComponentInChildren<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick() {
        UIManager.ShowPanel<RetreatPopWindow>();
    }
   
}
