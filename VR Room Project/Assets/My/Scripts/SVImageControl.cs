using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SVImageControl : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField] private Image pickerImage;
    private RawImage SVImage;
    private ColorPickerControl CC;
    private RectTransform rectTransform, pickerTransform;

    private void Awake()
    {
        SVImage = GetComponent<RawImage>();
        CC = FindObjectOfType<ColorPickerControl>();
        rectTransform = GetComponent<RectTransform>();
        pickerTransform = pickerImage.GetComponent<RectTransform>();
        pickerTransform.localPosition = new Vector2(-(rectTransform.sizeDelta.x * 0.5f), -(rectTransform.sizeDelta.y * 0.5f)); // .position?
    }

    private void UpdateColor(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPos))
        {
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;
            float deltaX = width * 0.5f;
            float deltaY = height * 0.5f;
            localPos.x = Mathf.Clamp(localPos.x, -deltaX, deltaX);
            localPos.y = Mathf.Clamp(localPos.y, -deltaY, deltaY);
            float xNorm = (localPos.x + deltaX) / width;
            float yNorm = (localPos.y + deltaY) / height;
            pickerTransform.localPosition = localPos;
            pickerImage.color = Color.HSVToRGB(0, 0, 1 - yNorm);
            if (CC != null) CC.SetSV(xNorm, yNorm);
        }
        //Vector3 pos = rectTransform.InverseTransformPoint(eventData.position);
        ////float deltaX = rectTransform.sizeDelta.x * 0.5f;
        ////float deltaY = rectTransform.sizeDelta.y * 0.5f;
        ////pos.x = Mathf.Clamp(pos.x, -deltaX, deltaX);
        ////pos.y = Mathf.Clamp(pos.y, -deltaY, deltaY);
        ////float xNorm = (pos.x + deltaX) / rectTransform.sizeDelta.x;
        ////float yNorm = (pos.y + deltaY) / rectTransform.sizeDelta.y;
        //if (pos.x < -deltaX)
        //{
        //    pos.x = -deltaX;
        //}
        //else if (pos.x > deltaX)
        //{
        //    pos.x = deltaX;
        //}
        //if (pos.y < -deltaY)
        //{
        //    pos.y = -deltaY;
        //}
        //else if (pos.y  > deltaY)
        //{
        //    pos.y = deltaY;
        //}
        //float x = pos.x + deltaX;
        //float y = pos.y + deltaY;
        //float xNorm = x / rectTransform.sizeDelta.x;
        //float yNorm = y / rectTransform.sizeDelta.y;
        ////pickerTransform.localPosition = pos;
        ////pickerImage.color = Color.HSVToRGB(0, 0, 1 - yNorm);
        ////CC.SetSV(xNorm, yNorm);
    }

    public void OnDrag(PointerEventData eventData) { UpdateColor(eventData); }

    public void OnPointerClick(PointerEventData eventData) { UpdateColor(eventData); }

    public void SetCursorPosition(float s, float v)
    {
        if (rectTransform == null || pickerTransform == null) return;
        float deltaX = rectTransform.sizeDelta.x * 0.5f;
        float deltaY = rectTransform.sizeDelta.y * 0.5f;
        float x = (s * rectTransform.sizeDelta.x) - deltaX;
        float y = (v * rectTransform.sizeDelta.y) - deltaY;
        pickerTransform.localPosition = new Vector2(x, y);
        pickerImage.color = Color.HSVToRGB(0, 0, 1 - v);
    }
}