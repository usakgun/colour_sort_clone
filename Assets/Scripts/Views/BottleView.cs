using System;
using UnityEngine;
using DG.Tweening;

public class BottleView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] layerRenderers;
    [SerializeField] private SpriteRenderer bottleRenderer;
    [SerializeField] private GameObject capObject;
    [SerializeField] private LineRenderer streamLine;
    [SerializeField] private ParticleSystem completeParticles;

    public BottleModel Model { get; private set; }
    public event Action<BottleView> OnBottleClicked;

    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Vector3 _capOriginalScale;
    private Vector3[] _layerBasePositions;
    private Vector3[] _layerBaseScales;
    private SpriteRenderer _meniscusRenderer;
    private ParticleSystem _bubbleParticles;
    private bool _isSelected;
    private bool _wasCompleted;

    private void Awake()
    {
        if (bottleRenderer == null)
        {
            bottleRenderer = GetComponent<SpriteRenderer>();
        }
        if (streamLine == null)
        {
            streamLine = GetComponent<LineRenderer>();
        }
        if (streamLine != null)
        {
            streamLine.enabled = false;
            streamLine.positionCount = 2;
        }

        _layerBasePositions = new Vector3[layerRenderers.Length];
        _layerBaseScales = new Vector3[layerRenderers.Length];

        Sprite tubeFill = ProceduralSpriteGenerator.CreateTestTubeSprite(true);
        Sprite tubeBorder = ProceduralSpriteGenerator.CreateTestTubeSprite(false);
        bottleRenderer.sprite = tubeBorder;
        bottleRenderer.color = new Color(0.88f, 0.94f, 1f, 0.95f);
        if (TryGetComponent<SpriteMask>(out var mask)) mask.sprite = tubeFill;

        transform.localScale = new Vector3(1.15f, 1.15f, 1f);
        _originalScale = transform.localScale;

        if (TryGetComponent<BoxCollider2D>(out var col))
        {
            col.size = new Vector2(0.9f, 3.0f);
        }

        float[] layerHeights = { 0.72f, 0.68f, 0.68f, 0.70f };
        float[] layerYPos = { -1.08f, -0.39f, 0.28f, 0.96f };
        for (int i = 0; i < layerRenderers.Length; i++)
        {
            layerRenderers[i].maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            _layerBaseScales[i] = new Vector3(0.85f, layerHeights[i], 1f);
            _layerBasePositions[i] = new Vector3(0f, layerYPos[i], 0f);
            layerRenderers[i].transform.localScale = _layerBaseScales[i];
            layerRenderers[i].transform.localPosition = _layerBasePositions[i];
        }

        if (capObject != null)
        {
            if (capObject.TryGetComponent<SpriteRenderer>(out var capSr))
            {
                capSr.sprite = ProceduralSpriteGenerator.CreateRoundedRectSprite(128, 96, 20, 0, 128f);
                capSr.color = new Color(0.82f, 0.62f, 0.42f, 1f);
                capSr.sortingOrder = 5;
            }
            capObject.transform.localPosition = new Vector3(0f, 1.42f, 0f);
            _capOriginalScale = new Vector3(0.68f, 0.32f, 1f);
            capObject.transform.localScale = _capOriginalScale;
            capObject.SetActive(false);
        }

        CreateMeniscus();
        CreateBubbleParticles();
    }

    private void CreateMeniscus()
    {
        GameObject meniscusObj = new GameObject("Meniscus");
        meniscusObj.transform.SetParent(transform, false);
        _meniscusRenderer = meniscusObj.AddComponent<SpriteRenderer>();
        _meniscusRenderer.sprite = ProceduralSpriteGenerator.CreateCircleSprite(64, 64f);
        _meniscusRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        _meniscusRenderer.sortingOrder = 2;
        meniscusObj.transform.localScale = new Vector3(0.74f, 0.12f, 1f);
        meniscusObj.SetActive(false);
    }

    private void CreateBubbleParticles()
    {
        GameObject bubbleObj = new GameObject("PourBubbles");
        bubbleObj.transform.SetParent(transform, false);
        _bubbleParticles = bubbleObj.AddComponent<ParticleSystem>();
        _bubbleParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = _bubbleParticles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.3f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
        main.startColor = new Color(1f, 1f, 1f, 0.75f);
        main.gravityModifier = -0.4f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = _bubbleParticles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12, 16) });

        var shape = _bubbleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.55f, 0.1f, 0.1f);

        var renderer = bubbleObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 4;
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (capObject != null)
        {
            capObject.transform.DOKill();
        }
        if (_meniscusRenderer != null)
        {
            _meniscusRenderer.transform.DOKill();
        }
        foreach (var sr in layerRenderers)
        {
            if (sr != null) sr.transform.DOKill();
        }
    }

    public void Initialize(BottleModel model, Vector3 startPosition)
    {
        Model = model;
        _wasCompleted = model.IsCompleted;
        if (capObject != null)
        {
            capObject.SetActive(_wasCompleted);
        }
        SetPosition(startPosition);
        UpdateVisualsImmediate();
    }

    public void SetPosition(Vector3 newPosition)
    {
        _isSelected = false;
        transform.DOKill();
        _originalPosition = newPosition;
        transform.position = newPosition;
        transform.localScale = _originalScale;
    }

    private void OnMouseDown()
    {
        OnBottleClicked?.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        if (_isSelected == selected) return;
        _isSelected = selected;

        transform.DOKill();
        float targetY = _isSelected ? _originalPosition.y + 0.35f : _originalPosition.y;
        transform.DOMoveY(targetY, 0.2f).SetEase(Ease.OutBack);
    }

    public void PlayErrorShake()
    {
        _isSelected = false;
        transform.DOKill();
        transform.position = _originalPosition;
        transform.DOShakePosition(0.3f, new Vector3(0.18f, 0f, 0f), 25, 90f, false, true)
            .OnComplete(() => transform.position = _originalPosition);
    }

    public void PlayReceiveBubbles()
    {
        if (_bubbleParticles == null || Model.IsEmpty) return;
        int topIndex = Mathf.Clamp(Model.Colors.Count - 1, 0, layerRenderers.Length - 1);
        float topY = _layerBasePositions[topIndex].y + (_layerBaseScales[topIndex].y * 0.5f);
        _bubbleParticles.transform.localPosition = new Vector3(0f, topY - 0.1f, 0f);
        _bubbleParticles.Play();
    }

    public void PlayPourAnimation(BottleView targetBottle, Action onPourMoment, Action onComplete)
    {
        _isSelected = false;
        transform.DOKill();

        SetSortingOrder(10);

        float direction = transform.position.x < targetBottle.transform.position.x ? -1f : 1f;
        Vector3 pourPosition = targetBottle.transform.position + new Vector3(direction * 0.75f, 1.9f, 0f);
        float tiltAngle = direction < 0 ? -65f : 65f;

        Color pourColor = Color.white;
        for (int i = layerRenderers.Length - 1; i >= 0; i--)
        {
            if (layerRenderers[i].gameObject.activeSelf)
            {
                pourColor = layerRenderers[i].color;
                break;
            }
        }

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(pourPosition, 0.3f).SetEase(Ease.OutQuad));
        seq.Append(transform.DORotate(new Vector3(0f, 0f, tiltAngle), 0.2f).SetEase(Ease.OutQuad));
        seq.AppendCallback(() =>
        {
            if (streamLine != null)
            {
                Vector3 mouthPos = transform.TransformPoint(new Vector3(0f, 1.35f, 0f));
                Vector3 targetBottom = targetBottle.transform.position + new Vector3(0f, -0.8f, 0f);
                streamLine.startColor = pourColor;
                streamLine.endColor = pourColor;
                streamLine.SetPosition(0, mouthPos);
                streamLine.SetPosition(1, targetBottom);
                streamLine.enabled = true;
            }
            onPourMoment?.Invoke();
            targetBottle.PlayReceiveBubbles();
        });
        seq.AppendInterval(0.32f);
        seq.AppendCallback(() =>
        {
            if (streamLine != null)
            {
                streamLine.enabled = false;
            }
        });
        seq.Append(transform.DORotate(Vector3.zero, 0.2f));
        seq.Append(transform.DOMove(_originalPosition, 0.3f).SetEase(Ease.InOutQuad));
        seq.OnComplete(() =>
        {
            SetSortingOrder(0);
            onComplete?.Invoke();
        });
    }

    private void SetSortingOrder(int baseOrder)
    {
        if (bottleRenderer != null)
        {
            bottleRenderer.sortingOrder = baseOrder + 3;
        }

        for (int i = 0; i < layerRenderers.Length; i++)
        {
            layerRenderers[i].sortingOrder = baseOrder + 1;
        }

        if (_meniscusRenderer != null)
        {
            _meniscusRenderer.sortingOrder = baseOrder + 2;
        }
    }

    public void UpdateVisualsImmediate()
    {
        ColorType[] colors = Model.Colors.ToArray();
        Array.Reverse(colors);

        for (int i = 0; i < layerRenderers.Length; i++)
        {
            layerRenderers[i].transform.DOKill();
            if (i < colors.Length && colors[i] != ColorType.None)
            {
                layerRenderers[i].gameObject.SetActive(true);
                layerRenderers[i].color = GetUnityColor(colors[i]);
                layerRenderers[i].transform.localScale = _layerBaseScales[i];
                layerRenderers[i].transform.localPosition = _layerBasePositions[i];
            }
            else
            {
                layerRenderers[i].gameObject.SetActive(false);
            }
        }

        UpdateMeniscus(colors, false);
    }

    public void UpdateVisuals()
    {
        ColorType[] colors = Model.Colors.ToArray();
        Array.Reverse(colors);

        for (int i = 0; i < layerRenderers.Length; i++)
        {
            Transform layerTr = layerRenderers[i].transform;
            layerTr.DOKill();

            bool shouldBeActive = i < colors.Length && colors[i] != ColorType.None;
            bool isCurrentlyActive = layerRenderers[i].gameObject.activeSelf;

            if (shouldBeActive)
            {
                layerRenderers[i].color = GetUnityColor(colors[i]);
                if (!isCurrentlyActive)
                {
                    layerRenderers[i].gameObject.SetActive(true);
                    float bottomY = _layerBasePositions[i].y - (_layerBaseScales[i].y * 0.5f);
                    layerTr.localScale = new Vector3(_layerBaseScales[i].x, 0f, 1f);
                    layerTr.localPosition = new Vector3(_layerBasePositions[i].x, bottomY, 0f);

                    layerTr.DOScaleY(_layerBaseScales[i].y, 0.28f).SetEase(Ease.OutQuad);
                    layerTr.DOLocalMoveY(_layerBasePositions[i].y, 0.28f).SetEase(Ease.OutQuad);
                }
                else
                {
                    layerTr.localScale = _layerBaseScales[i];
                    layerTr.localPosition = _layerBasePositions[i];
                }
            }
            else if (isCurrentlyActive)
            {
                int index = i;
                float bottomY = _layerBasePositions[i].y - (_layerBaseScales[i].y * 0.5f);
                layerTr.DOScaleY(0f, 0.25f).SetEase(Ease.InQuad);
                layerTr.DOLocalMoveY(bottomY, 0.25f).SetEase(Ease.InQuad)
                    .OnComplete(() => layerRenderers[index].gameObject.SetActive(false));
            }
        }

        UpdateMeniscus(colors, true);

        if (!_wasCompleted && Model.IsCompleted)
        {
            _wasCompleted = true;
            DOVirtual.DelayedCall(0.28f, () =>
            {
                transform.DOPunchScale(new Vector3(0.15f, -0.2f, 0f), 0.35f, 8, 0.8f);

                if (completeParticles != null)
                {
                    completeParticles.Play();
                }

                if (capObject != null)
                {
                    capObject.SetActive(true);
                    capObject.transform.DOKill();
                    capObject.transform.localScale = Vector3.zero;
                    capObject.transform.DOScale(_capOriginalScale, 0.25f).SetEase(Ease.OutBack);
                }
            });
        }
        else if (!Model.IsCompleted)
        {
            _wasCompleted = false;
            if (capObject != null)
            {
                capObject.SetActive(false);
            }
        }
    }

    private void UpdateMeniscus(ColorType[] bottomToTopColors, bool animate)
    {
        if (_meniscusRenderer == null) return;
        _meniscusRenderer.transform.DOKill();

        int count = bottomToTopColors.Length;
        if (count == 0)
        {
            _meniscusRenderer.gameObject.SetActive(false);
            return;
        }

        int topIdx = Mathf.Clamp(count - 1, 0, layerRenderers.Length - 1);
        Color baseColor = GetUnityColor(bottomToTopColors[topIdx]);
        _meniscusRenderer.color = Color.Lerp(baseColor, Color.white, 0.38f);

        float targetY = _layerBasePositions[topIdx].y + (_layerBaseScales[topIdx].y * 0.5f);
        _meniscusRenderer.gameObject.SetActive(true);

        if (animate)
        {
            _meniscusRenderer.transform.DOLocalMoveY(targetY, 0.28f).SetEase(Ease.OutQuad);
        }
        else
        {
            _meniscusRenderer.transform.localPosition = new Vector3(0f, targetY, 0f);
        }
    }

    private Color GetUnityColor(ColorType type)
    {
        return type switch
        {
            ColorType.Red => new Color(1f, 0.28f, 0.34f),
            ColorType.Blue => new Color(0.12f, 0.56f, 1f),
            ColorType.Green => new Color(0.18f, 0.83f, 0.45f),
            ColorType.Yellow => new Color(1f, 0.8f, 0.1f),
            ColorType.Purple => new Color(0.65f, 0.35f, 0.95f),
            ColorType.Orange => new Color(1f, 0.5f, 0.15f),
            _ => Color.clear
        };
    }
}