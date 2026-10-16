using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private const string LevelSaveKey = "SavedLevelIndex";

    [SerializeField] private BottleView bottlePrefab;
    [SerializeField] private List<LevelDataSO> levels;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Button addBottleButton;
    [SerializeField] private float bottleSpacingX = 1.6f;
    [SerializeField] private float bottleSpacingY = 3.8f;

    private readonly List<BottleView> _spawnedBottles = new List<BottleView>();
    private readonly List<BottleModel> _initialLevelState = new List<BottleModel>();
    private GameController _gameController;
    private BottleView _selectedBottle;
    private bool _isAnimating;
    private bool _extraBottleUsed;
    private int _currentLevelIndex;

    private void Start()
    {
        _currentLevelIndex = PlayerPrefs.GetInt(LevelSaveKey, 0);
        LoadLevel(_currentLevelIndex);
    }

    [ContextMenu("Reset Saved Level")]
    public void ResetSavedProgress()
    {
        PlayerPrefs.DeleteKey(LevelSaveKey);
        _currentLevelIndex = 0;
        LoadLevel(_currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        ClearCurrentLevel();

        if (uiManager != null)
        {
            uiManager.HideWinPanel();
            uiManager.HideFailPanel();
            uiManager.UpdateLevelText(index + 1);
        }

        List<BottleModel> models;

        if (levels != null && index < levels.Count)
        {
            models = CreateModelsFromSO(levels[index]);
        }
        else
        {
            int colorCount = Mathf.Clamp(2 + (index / 2), 3, 6);
            int emptyBottles = 2;
            models = LevelGenerator.GenerateLevel(colorCount, emptyBottles, 4);
        }

        SaveInitialState(models);
        SpawnBottleViews(models);
        _gameController = new GameController(models);
    }

    private void SaveInitialState(List<BottleModel> models)
    {
        _initialLevelState.Clear();
        foreach (var model in models)
        {
            BottleModel copy = new BottleModel(model.Id, model.Capacity);
            ColorType[] colors = model.Colors.ToArray();
            System.Array.Reverse(colors);
            foreach (var c in colors)
            {
                copy.ForceAddColor(c);
            }
            _initialLevelState.Add(copy);
        }
    }

    public void RestartLevel()
    {
        if (_isAnimating) return;

        ClearCurrentLevel();

        if (uiManager != null)
        {
            uiManager.HideFailPanel();
        }

        List<BottleModel> restoredModels = new List<BottleModel>();
        foreach (var savedModel in _initialLevelState)
        {
            BottleModel copy = new BottleModel(savedModel.Id, savedModel.Capacity);
            ColorType[] colors = savedModel.Colors.ToArray();
            System.Array.Reverse(colors);
            foreach (var c in colors)
            {
                copy.ForceAddColor(c);
            }
            restoredModels.Add(copy);
        }

        SpawnBottleViews(restoredModels);
        _gameController = new GameController(restoredModels);
    }

    public void AddExtraBottle()
    {
        if (_isAnimating || _extraBottleUsed) return;

        if (_selectedBottle != null)
        {
            _selectedBottle.SetSelected(false);
            _selectedBottle = null;
        }

        _extraBottleUsed = true;
        if (addBottleButton != null)
        {
            addBottleButton.interactable = false;
        }

        int newId = _spawnedBottles.Count;
        int capacity = _spawnedBottles.Count > 0 ? _spawnedBottles[0].Model.Capacity : 4;
        BottleModel newModel = new BottleModel(newId, capacity);

        _gameController.AddExtraBottle(newModel);

        BottleView view = Instantiate(bottlePrefab, transform);
        view.Initialize(newModel, Vector3.zero);
        view.OnBottleClicked += HandleBottleClicked;
        _spawnedBottles.Add(view);

        UpdateBottlePositions();
    }

    private List<BottleModel> CreateModelsFromSO(LevelDataSO levelData)
    {
        List<BottleModel> models = new List<BottleModel>();
        for (int i = 0; i < levelData.bottles.Count; i++)
        {
            BottleModel model = new BottleModel(i, levelData.bottleCapacity);
            foreach (ColorType color in levelData.bottles[i].initialColors)
            {
                model.ForceAddColor(color);
            }
            models.Add(model);
        }
        return models;
    }

    private void SpawnBottleViews(List<BottleModel> models)
    {
        for (int i = 0; i < models.Count; i++)
        {
            BottleView view = Instantiate(bottlePrefab, transform);
            view.Initialize(models[i], Vector3.zero);
            view.OnBottleClicked += HandleBottleClicked;
            _spawnedBottles.Add(view);
        }

        UpdateBottlePositions();
    }

    private void UpdateBottlePositions()
    {
        int count = _spawnedBottles.Count;
        int maxPerRow = 5;

        for (int i = 0; i < count; i++)
        {
            Vector3 targetPos;
            if (count <= maxPerRow)
            {
                float spacing = count == 5 ? 1.3f : bottleSpacingX;
                float startX = -(count - 1) * spacing * 0.5f;
                targetPos = new Vector3(startX + (i * spacing), 0f, 0f);
            }
            else
            {
                int topRowCount = Mathf.CeilToInt(count / 2f);
                int bottomRowCount = count - topRowCount;

                if (i < topRowCount)
                {
                    float spacing = topRowCount >= 5 ? 1.3f : bottleSpacingX;
                    float startX = -(topRowCount - 1) * spacing * 0.5f;
                    targetPos = new Vector3(startX + (i * spacing), bottleSpacingY * 0.5f, 0f);
                }
                else
                {
                    int bottomIndex = i - topRowCount;
                    float spacing = bottomRowCount >= 5 ? 1.3f : bottleSpacingX;
                    float startX = -(bottomRowCount - 1) * spacing * 0.5f;
                    targetPos = new Vector3(startX + (bottomIndex * spacing), -bottleSpacingY * 0.5f, 0f);
                }
            }

            _spawnedBottles[i].SetPosition(targetPos);
        }
    }

    private void ClearCurrentLevel()
    {
        foreach (var bottle in _spawnedBottles)
        {
            if (bottle != null)
            {
                bottle.OnBottleClicked -= HandleBottleClicked;
                Destroy(bottle.gameObject);
            }
        }
        _spawnedBottles.Clear();
        _selectedBottle = null;
        _isAnimating = false;
        _extraBottleUsed = false;

        if (addBottleButton != null)
        {
            addBottleButton.interactable = true;
        }
    }

    private void HandleBottleClicked(BottleView clickedBottle)
    {
        if (_isAnimating) return;

        if (_selectedBottle == null)
        {
            if (!clickedBottle.Model.IsEmpty && !clickedBottle.Model.IsCompleted)
            {
                _selectedBottle = clickedBottle;
                _selectedBottle.SetSelected(true);
            }
            else
            {
                clickedBottle.PlayErrorShake();
            }
        }
        else if (_selectedBottle == clickedBottle)
        {
            _selectedBottle.SetSelected(false);
            _selectedBottle = null;
        }
        else
        {
            bool poured = _gameController.TryPour(_selectedBottle.Model, clickedBottle.Model);
            if (poured)
            {
                _isAnimating = true;
                BottleView sourceBottle = _selectedBottle;
                _selectedBottle = null;

                sourceBottle.PlayPourAnimation(
                    clickedBottle,
                    () =>
                    {
                        sourceBottle.UpdateVisuals();
                        clickedBottle.UpdateVisuals();
                    },
                    () =>
                    {
                        _isAnimating = false;
                        if (_gameController.CheckWinCondition())
                        {
                            _currentLevelIndex++;
                            PlayerPrefs.SetInt(LevelSaveKey, _currentLevelIndex);
                            PlayerPrefs.Save();

                            if (uiManager != null)
                            {
                                uiManager.ShowWinPanel();
                            }
                            else
                            {
                                LoadLevel(_currentLevelIndex);
                            }
                        }
                        else if (!_gameController.HasAnyValidMove())
                        {
                            if (uiManager != null)
                            {
                                uiManager.ShowFailPanel(!_extraBottleUsed);
                            }
                        }
                    }
                );
            }
            else
            {
                _selectedBottle.SetSelected(false);
                _selectedBottle = null;
                clickedBottle.PlayErrorShake();
            }
        }
    }

    public void LoadNextLevel()
    {
        LoadLevel(_currentLevelIndex);
    }

    public void UndoMove()
    {
        if (_isAnimating) return;

        if (_selectedBottle != null)
        {
            _selectedBottle.SetSelected(false);
            _selectedBottle = null;
        }

        if (_gameController.TryUndo())
        {
            if (uiManager != null)
            {
                uiManager.HideFailPanel();
            }
            foreach (var view in _spawnedBottles)
            {
                view.UpdateVisuals();
            }
        }
    }
}