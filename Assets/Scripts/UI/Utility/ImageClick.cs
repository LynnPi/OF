using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// 提供对特定UI区域的点击检测
/// </summary>
[RequireComponent(typeof(UnityEngine.UI.Image))]
public class ImageClick : MonoBehaviour, IPointerClickHandler {
    public System.Action<PointerEventData> OnPointer = delegate { };
    public void OnPointerClick(PointerEventData eventData) {
        OnPointer(eventData);
    }
}
