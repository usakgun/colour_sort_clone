using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
        if (TryGetComponent<Image>(out var img))
        {
            img.sprite = ProceduralSpriteGenerator.CreateRoundedRectSprite(256, 96, 44, 0);
            img.type = Image.Type.Sliced;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(_originalScale * 0.9f, 0.1f).SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(_originalScale, 0.2f).SetEase(Ease.OutBack);
    }
}