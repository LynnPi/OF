using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SampleNotePopWindow : MonoBehaviour {
    public static SampleNotePopWindow Instance_;
    public static SampleNotePopWindow Instance { 
        get{
            if(Instance_ == null){
                GameObject go = new GameObject("SampleNotePopWindow".ToLower());
                go.transform.SetParent(UIManager.PopWindowCanvas.transform);
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = Vector3.zero;
                go.AddComponent<RectTransform>();
                Instance_ = go.AddComponent<SampleNotePopWindow>();
            }
            return Instance_;
        }
    }

    private bool Busy_;
    private GameObject Window_;
    
    public void ShowMessage(int msgID) {
        if (Busy_) {
            StopCoroutine(PlayAnimation());
            Destroy(Window_);
            Window_ = null;
        }

        if (Window_ == null) {
            Window_ = Global.CreateUI("SampleNotePopWindow", gameObject);
        }

        string msg = ReadNewMessage(msgID);

        WriteNewMessage(msg);

        StartCoroutine(PlayAnimation());
    }

    private string ReadNewMessage(int id) {
        string content = string.Empty;
        //暂时做个假的
        switch (id) {
            case 1001:
                content = "Location overlap！Tap on empty area";
                break;
            case 1002:
                content = "Invalid Location！Tap on the deploy zone";
                break;
            case 1003:
                content = "Warp Point is not enough";
                break;
            case 1004:
                content = "Please deploy your army";
                break;
            case 1005:
                content = "Release in next version";
                break;
            case 1006:
                content = "choose an attack target ship";
                break;
            case 1007:
                content = "choose an attack target position";
                break;
            default:
                break;
        }
        return content;
    }

    private void WriteNewMessage(string msg) {
        if (!Window_) return;
        Text contentLabel = Window_.GetComponentInChildren<Text>();
        contentLabel.text = msg;
    }

    private IEnumerator PlayAnimation() {
        if (!Window_) 
            yield break;
        Busy_ = true;

        Animator anim = Window_.GetComponentInChildren<Animator>();

        anim.SetTrigger("show");
        
        yield return new WaitForSeconds(2.5f);
        Busy_ = false;
    }

}
