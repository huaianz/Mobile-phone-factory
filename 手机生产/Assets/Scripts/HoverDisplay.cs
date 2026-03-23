using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class HoverDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("鼠标悬停时显示的对象")]
    public GameObject[] targets;

    [Tooltip("鼠标离开后延迟隐藏的时间（秒）")]
    public float hideDelay = 1f;

    private bool isMouseOver = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        SetTargetsActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        if (!isMouseOver)
        {
            SetTargetsActive(false);
        }
    }

    private void SetTargetsActive(bool active)
    {
        foreach (GameObject target in targets)
        {
            if (target != null)
                target.SetActive(active);
        }
    }
}