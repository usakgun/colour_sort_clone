using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Transform winPopupTransform;

    private RectTransform _popupCard;
    private GameObject _mainMenuPanel;
    private TextMeshProUGUI _menuLevelText;
    private Transform _playButtonTransform;

    private GameObject _failPanel;
    private RectTransform _failPopupCard;
    private Button _failExtraBottleBtn;

    private void Awake()
    {
        SetupGradientBackground();
        SetupWinPopupCard();
        SetupFailPopupCard();
        SetupMainMenu();
    }

    private void SetupGradientBackground()
    {
        GameObject bgObj = new GameObject("GradientBackground2D");
        SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSpriteGenerator.CreateGradientBackgroundSprite(
            256, 512,
            new Color(0.16f, 0.24f, 0.32f, 1f),
            new Color(0.06f, 0.09f, 0.13f, 1f)
        );
        sr.sortingOrder = -20;
        bgObj.transform.position = Vector3.zero;
        bgObj.transform.localScale = new Vector3(5f, 5f, 1f);
    }

    private void SetupMainMenu()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        _mainMenuPanel = new GameObject("MainMenuPanel");
        _mainMenuPanel.transform.SetParent(canvas.transform, false);
        _mainMenuPanel.transform.SetAsLastSibling();

        Image bgImg = _mainMenuPanel.AddComponent<Image>();
        bgImg.sprite = ProceduralSpriteGenerator.CreateGradientBackgroundSprite(
            256, 512,
            new Color(0.14f, 0.22f, 0.30f, 1f),
            new Color(0.05f, 0.08f, 0.12f, 1f)
        );

        RectTransform panelRt = _mainMenuPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        TMP_FontAsset defaultFont = levelText != null ? levelText.font : null;

        GameObject titleObj = new GameObject("GameTitle");
        titleObj.transform.SetParent(_mainMenuPanel.transform, false);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) titleTmp.font = defaultFont;
        titleTmp.text = "COLOUR\nSORT";
        titleTmp.fontSize = 110f;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = new Color(1f, 0.85f, 0.2f, 1f);
        titleTmp.alignment = TextAlignmentOptions.Center;

        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        titleShadow.effectDistance = new Vector2(0f, -10f);

        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.sizeDelta = new Vector2(800f, 320f);
        titleRt.anchoredPosition = new Vector2(0f, 260f);

        GameObject subObj = new GameObject("MenuLevelInfo");
        subObj.transform.SetParent(_mainMenuPanel.transform, false);
        _menuLevelText = subObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) _menuLevelText.font = defaultFont;
        int savedLvl = PlayerPrefs.GetInt("SavedLevelIndex", 0) + 1;
        _menuLevelText.text = $"CURRENT LEVEL: {savedLvl}";
        _menuLevelText.fontSize = 42f;
        _menuLevelText.fontStyle = FontStyles.Bold;
        _menuLevelText.color = new Color(0.75f, 0.88f, 1f, 0.9f);
        _menuLevelText.alignment = TextAlignmentOptions.Center;

        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.sizeDelta = new Vector2(600f, 80f);
        subRt.anchoredPosition = new Vector2(0f, 40f);

        GameObject playBtnObj = new GameObject("PlayButton");
        playBtnObj.transform.SetParent(_mainMenuPanel.transform, false);

        Image btnImg = playBtnObj.AddComponent<Image>();
        btnImg.sprite = ProceduralSpriteGenerator.CreateRoundedRectSprite(256, 110, 50, 0, 100f);
        btnImg.type = Image.Type.Sliced;
        btnImg.color = new Color(0.18f, 0.83f, 0.45f, 1f);

        Shadow btnShadow = playBtnObj.AddComponent<Shadow>();
        btnShadow.effectColor = new Color(0.08f, 0.45f, 0.22f, 1f);
        btnShadow.effectDistance = new Vector2(0f, -12f);

        Button btn = playBtnObj.AddComponent<Button>();
        playBtnObj.AddComponent<UIButtonAnimation>();

        RectTransform btnRt = playBtnObj.GetComponent<RectTransform>();
        btnRt.sizeDelta = new Vector2(460f, 140f);
        btnRt.anchoredPosition = new Vector2(0f, -180f);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(playBtnObj.transform, false);
        TextMeshProUGUI btnTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) btnTmp.font = defaultFont;
        btnTmp.text = "PLAY";
        btnTmp.fontSize = 58f;
        btnTmp.fontStyle = FontStyles.Bold;
        btnTmp.color = Color.white;
        btnTmp.alignment = TextAlignmentOptions.Center;

        RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.offsetMin = Vector2.zero;
        btnTextRt.offsetMax = Vector2.zero;

        _playButtonTransform = playBtnObj.transform;
        _playButtonTransform.DOScale(1.06f, 0.75f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

        btn.onClick.AddListener(HideMainMenu);
    }

    public void HideMainMenu()
    {
        if (_mainMenuPanel == null) return;
        if (_playButtonTransform != null) _playButtonTransform.DOKill();

        _mainMenuPanel.transform.DOScale(1.1f, 0.2f).SetEase(Ease.InQuad);
        CanvasGroup cg = _mainMenuPanel.AddComponent<CanvasGroup>();
        cg.DOFade(0f, 0.2f).OnComplete(() => _mainMenuPanel.SetActive(false));
    }

    private void SetupWinPopupCard()
    {
        if (winPanel == null) return;

        if (winPanel.TryGetComponent<Image>(out var overlayImg))
        {
            overlayImg.color = new Color(0.03f, 0.05f, 0.08f, 0.85f);
        }

        GameObject cardObj = new GameObject("PopupCard");
        cardObj.transform.SetParent(winPanel.transform, false);

        Image cardImg = cardObj.AddComponent<Image>();
        cardImg.sprite = ProceduralSpriteGenerator.CreateRoundedRectSprite(256, 256, 42, 0, 100f);
        cardImg.type = Image.Type.Sliced;
        cardImg.color = new Color(0.14f, 0.20f, 0.27f, 1f);

        Shadow shadow = cardObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(0f, -15f);

        Outline outline = cardObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.28f, 0.38f, 0.50f, 1f);
        outline.effectDistance = new Vector2(4f, -4f);

        _popupCard = cardObj.GetComponent<RectTransform>();
        _popupCard.sizeDelta = new Vector2(780f, 520f);
        _popupCard.anchoredPosition = Vector2.zero;

        for (int i = winPanel.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = winPanel.transform.GetChild(i);
            if (child != _popupCard.transform)
            {
                child.SetParent(_popupCard.transform, true);
                if (child.TryGetComponent<TextMeshProUGUI>(out _))
                {
                    child.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 110f);
                }
                else if (child.TryGetComponent<Button>(out _))
                {
                    child.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -90f);
                }
            }
        }
    }

    private void SetupFailPopupCard()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        _failPanel = new GameObject("FailPanel");
        _failPanel.transform.SetParent(canvas.transform, false);

        Image overlayImg = _failPanel.AddComponent<Image>();
        overlayImg.color = new Color(0.05f, 0.02f, 0.03f, 0.85f);

        RectTransform panelRt = _failPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        GameObject cardObj = new GameObject("FailPopupCard");
        cardObj.transform.SetParent(_failPanel.transform, false);

        Image cardImg = cardObj.AddComponent<Image>();
        cardImg.sprite = ProceduralSpriteGenerator.CreateRoundedRectSprite(256, 256, 42, 0, 100f);
        cardImg.type = Image.Type.Sliced;
        cardImg.color = new Color(0.18f, 0.14f, 0.20f, 1f);

        Shadow shadow = cardObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(0f, -15f);

        Outline outline = cardObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.85f, 0.25f, 0.30f, 1f);
        outline.effectDistance = new Vector2(4f, -4f);

        _failPopupCard = cardObj.GetComponent<RectTransform>();
        _failPopupCard.sizeDelta = new Vector2(820f, 540f);
        _failPopupCard.anchoredPosition = Vector2.zero;

        TMP_FontAsset defaultFont = levelText != null ? levelText.font : null;

        GameObject titleObj = new GameObject("FailTitle");
        titleObj.transform.SetParent(_failPopupCard, false);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) titleTmp.font = defaultFont;
        titleTmp.text = "NO MOVES LEFT!";
        titleTmp.fontSize = 62f;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = new Color(1f, 0.35f, 0.4f, 1f);
        titleTmp.alignment = TextAlignmentOptions.Center;

        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.sizeDelta = new Vector2(750f, 120f);
        titleRt.anchoredPosition = new Vector2(0f, 130f);

        LevelManager lvlMgr = FindAnyObjectByType<LevelManager>();

        Button retryBtn = CreatePopupButton(_failPopupCard, "TRY AGAIN", new Color(0.95f, 0.55f, 0.15f, 1f), new Vector2(-190f, -90f), defaultFont);
        retryBtn.onClick.AddListener(() =>
        {
            HideFailPanel();
            if (lvlMgr != null) lvlMgr.RestartLevel();
        });

        _failExtraBottleBtn = CreatePopupButton(_failPopupCard, "+1 BOTTLE", new Color(0.12f, 0.72f, 0.68f, 1f), new Vector2(190f, -90f), defaultFont);
        _failExtraBottleBtn.onClick.AddListener(() =>
        {
            HideFailPanel();
            if (lvlMgr != null) lvlMgr.AddExtraBottle();
        });

        _failPanel.SetActive(false);
    }

    private Button CreatePopupButton(Transform parent, string label, Color bgColor, Vector2 pos, TMP_FontAsset font)
    {
        GameObject btnObj = new GameObject(label + "_Button");
        btnObj.transform.SetParent(parent, false);

        Image img = btnObj.AddComponent<Image>();
        img.sprite = ProceduralSpriteGenerator.CreateRoundedRectSprite(256, 96, 44, 0, 100f);
        img.type = Image.Type.Sliced;
        img.color = bgColor;

        Shadow sh = btnObj.AddComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, 0.5f);
        sh.effectDistance = new Vector2(0f, -8f);

        Button btn = btnObj.AddComponent<Button>();
        btnObj.AddComponent<UIButtonAnimation>();

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(340f, 110f);
        rt.anchoredPosition = pos;

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = label;
        tmp.fontSize = 36f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;

        return btn;
    }

    public void UpdateLevelText(int levelNumber)
    {
        if (levelText != null)
        {
            levelText.text = $"LEVEL {levelNumber}";
        }
        if (_menuLevelText != null)
        {
            _menuLevelText.text = $"CURRENT LEVEL: {levelNumber}";
        }
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Transform target = _popupCard != null ? _popupCard : winPopupTransform;
            if (target != null)
            {
                target.DOKill();
                target.localScale = Vector3.zero;
                target.DOScale(Vector3.one, 0.38f).SetEase(Ease.OutBack);
            }
        }
    }

    public void HideWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    public void ShowFailPanel(bool canUseExtraBottle)
    {
        if (_failPanel != null)
        {
            _failPanel.SetActive(true);
            if (_failExtraBottleBtn != null)
            {
                _failExtraBottleBtn.interactable = canUseExtraBottle;
            }
            if (_failPopupCard != null)
            {
                _failPopupCard.DOKill();
                _failPopupCard.localScale = Vector3.zero;
                _failPopupCard.DOScale(Vector3.one, 0.38f).SetEase(Ease.OutBack);
            }
        }
    }

    public void HideFailPanel()
    {
        if (_failPanel != null)
        {
            _failPanel.SetActive(false);
        }
    }
}