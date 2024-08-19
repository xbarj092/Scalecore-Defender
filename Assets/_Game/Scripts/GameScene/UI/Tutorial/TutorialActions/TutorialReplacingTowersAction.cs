using UnityEngine;

public class TutorialReplacingTowersAction : TutorialAction
{
    [SerializeField] private GameObject _clickToContinue;

    [Header("TextTransforms")]
    [SerializeField] private Transform _enemySpawnTransform;
    [SerializeField] private Transform _coreTransform;
    [SerializeField] private Transform _towerPickedUpTransform;

    [Header("Cutouts")]
    [SerializeField] private GameObject _coreZoneCutout;
    [SerializeField] private RectTransform _towerCutout;

    [SerializeField] private GameObject _background;

    private ActionScheduler _actionScheduler;
    private PositionHighlighter _positionHighlighter;
    private Vector2 _spawnPosition;

    private const float PLACE_POSITION_THRESHOLD = 2f;

    private void Awake()
    {
        _positionHighlighter = FindObjectOfType<PositionHighlighter>();
        _actionScheduler = FindObjectOfType<ActionScheduler>();
    }

    private void OnDisable()
    {
        TutorialEvents.OnTowerPickedUp -= OnTowerPickedUp;
        TutorialEvents.OnTowerPlaced -= OnTowerPlaced;
        TutorialEvents.OnEnemyKilled -= TrySpawnNewEnemy;
        TutorialEvents.OnEnemyKilled -= OnEnemyKilled;
    }

    public override void StartAction()
    {
        Vector2 sceneSize;
        sceneSize.y = Camera.main.orthographicSize * 2f * 1.1f;
        sceneSize.x = sceneSize.y * Camera.main.aspect;
        sceneSize /= 2;

        Vector2 towerPosition = TutorialManager.Instance.TowerPosition.normalized;
        _spawnPosition = -towerPosition * sceneSize;
        TutorialEvents.OnEnemySpawnedInvoke(_spawnPosition);
        _tutorialPlayer.SetTextLocalPosition(_enemySpawnTransform.localPosition);
        _tutorialPlayer.MoveToNextNarratorText();
        _clickToContinue.SetActive(true);
        TutorialManager.Instance.CanPlayerMove = false;
        TutorialManager.Instance.CanPlayerPickTowers = false;

        TutorialEvents.OnEnemyKilled += TrySpawnNewEnemy;
        _actionScheduler.ScheduleAction(OnSecondText, () => Input.GetMouseButtonDown(0));
    }

    private void OnSecondText()
    {
        _background.SetActive(true);
        _coreZoneCutout.SetActive(true);
        _tutorialPlayer.SetTextLocalPosition(_coreTransform.localPosition);
        _tutorialPlayer.MoveToNextNarratorText();
        _actionScheduler.ScheduleAction(OnThirdText, () => Input.GetMouseButtonDown(0));
    }

    private void OnThirdText()
    {
        _tutorialPlayer.MoveToNextNarratorText();
        _actionScheduler.ScheduleAction(PickupTower, () => Input.GetMouseButtonDown(0));
    }

    private void TrySpawnNewEnemy(bool coreDeath)
    {
        if (coreDeath)
        {
            TutorialEvents.OnEnemySpawnedInvoke(_spawnPosition);
        }
        else
        {
            OnTowerPlacedCorrectly();
            OnEnemyKilled(coreDeath);
        }
    }

    private void PickupTower()
    {
        _coreZoneCutout.SetActive(false);
        _towerCutout.gameObject.SetActive(true);
        Vector3 worldPosition = FindObjectOfType<TowerShoot>().transform.position;
        _towerCutout.anchorMin = new(0, 0);
        _towerCutout.anchorMax = new(0, 0);

        _towerCutout.transform.position = Camera.main.WorldToScreenPoint(worldPosition);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition + new Vector3(0, 2, 0));

        _tutorialPlayer.GetTextTransform().anchorMin = new(0, 0);
        _tutorialPlayer.GetTextTransform().anchorMax = new(0, 0);

        _tutorialPlayer.SetTextPosition(screenPosition);
        _tutorialPlayer.MoveToNextNarratorText();
        _clickToContinue.SetActive(false);
        TutorialManager.Instance.CanPlayerMove = true;
        TutorialManager.Instance.CanPlayerPickTowers = true;
        TutorialEvents.OnTowerPickedUp += OnTowerPickedUp;
    }

    private void OnTowerPickedUp()
    {
        _towerCutout.gameObject.SetActive(false);
        _background.SetActive(false);
        TutorialEvents.OnTowerPickedUp -= OnTowerPickedUp;
        _tutorialPlayer.SetTextLocalPosition(_towerPickedUpTransform.localPosition);
        _tutorialPlayer.MoveToNextNarratorText();
        TutorialManager.Instance.PlacePosition = -TutorialManager.Instance.TowerPosition.normalized * (FindObjectOfType<SizeIncrease>().transform.localScale.x - 1);
        _positionHighlighter.HighlightPosition(TutorialManager.Instance.PlacePosition, PLACE_POSITION_THRESHOLD);
        TutorialEvents.OnPlayerMoved += OnPlayerMoved;
    }

    private void OnPlayerMoved()
    {
        TutorialEvents.OnPlayerMoved -= OnPlayerMoved;
        _tutorialPlayer.MoveToNextNarratorText();
        Vector3 worldPosition = FindObjectOfType<PositionHighlighter>().transform.position + new Vector3(0, 2, 0);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        _tutorialPlayer.GetTextTransform().anchorMin = new(0, 0);
        _tutorialPlayer.GetTextTransform().anchorMax = new(0, 0);

        _tutorialPlayer.SetTextPosition(screenPosition);
        TutorialEvents.OnTowerPlaced += OnTowerPlaced;
    }

    private void OnTowerPlaced()
    {
        if (Mathf.Abs(TutorialManager.Instance.TowerPosition.x - TutorialManager.Instance.PlacePosition.x) < PLACE_POSITION_THRESHOLD &&
            Mathf.Abs(TutorialManager.Instance.TowerPosition.y - TutorialManager.Instance.PlacePosition.y) < PLACE_POSITION_THRESHOLD)
        {
            OnTowerPlacedCorrectly();
        }
    }

    private void OnTowerPlacedCorrectly()
    {
        TutorialManager.Instance.CanPlayerPickTowers = false;
        TutorialEvents.OnEnemyKilled -= TrySpawnNewEnemy;
        TutorialEvents.OnTowerPlaced -= OnTowerPlaced;
        _positionHighlighter.LowlightPosition();
        _tutorialPlayer.TextFadeAway();
        TutorialEvents.OnEnemyKilled += OnEnemyKilled;
    }

    private void OnEnemyKilled(bool coreDeath)
    {
        TutorialEvents.OnEnemyKilled -= OnEnemyKilled;
        TutorialManager.Instance.CanPlayerPickTowers = true;
        OnActionFinishedInvoke();
    }

    public override void Exit()
    {
        TutorialManager.Instance.InstantiateTutorial(TutorialID.Upgrades);
    }
}
