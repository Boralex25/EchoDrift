using UnityEngine;
using UnityEngine.InputSystem;

public class Checkpoint : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform player;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject checkpointMenu;
    [SerializeField] private GameObject interactionText;
    [SerializeField] private SpriteRenderer checkpointSprite;
    [SerializeField] private Sprite activeCheckpointSprite;

    [SerializeField] private string checkpointName;
    [SerializeField] private string checkpointID;

    [SerializeField] private float interactRadius = 0.5f;

    // СЕРИАЛИЗАЦИЯ ДЛЯ ДЕБАГА! НЕ МЕНЯТЬ В ИНСПЕКТОРЕ!
    [Header("НЕ МЕНЯТЬ! READ ONLY!")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private bool isMenuOpen = false;
    [SerializeField] protected bool isPlayerInRadius = false;

    private PlayerController _playerController;


    private void Awake()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if(player != null) _playerController = player.GetComponent<PlayerController>();

        if (uiManager == null) uiManager = UIManager.Instance;

        if (checkpointSprite != null && isActivated)
        {
            // добавить
        }

        if(checkpointMenu != null) checkpointMenu.SetActive(false);

        Debug.Log("Инициализация чекпоинта");
    }

    private void Update()
    {
        if (!isMenuOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
            return;
        }
        // остальыне взаимодействия (5 разделов)
    }

    void IInteractable.Interact(PlayerController player)
    {
        if (!isPlayerInRadius || isMenuOpen) return;

        if (!isActivated) ActivateCheckpoint();

        OpenMenu();
    }

    private void ActivateCheckpoint()
    {
        isActivated = true;

        if(checkpointSprite != null && activeCheckpointSprite != null)
        {
            checkpointSprite.GetComponent<SpriteRenderer>().sprite = activeCheckpointSprite;
            Debug.Log($"Checkpoint: активация визуала {checkpointName}");
        }

        HintManager.Instance?.RegisterHint(this, $"Активирован чекпоинт {checkpointName}", priotiry: 15f, duration: 3f);

        // сохранение прогресса
        Debug.Log($"Checkpoint: сохранение прогресса у {checkpointID}");
    }

    private void OpenMenu()
    {
        if (isMenuOpen) return;

        isMenuOpen = true;

        Time.timeScale = 0.5f;

        if (checkpointMenu != null) checkpointMenu.SetActive(true);

        HintManager.Instance?.ClearAllHints();

        _playerController?.SetInputBlocked(true);

        Debug.Log($"Checkpoint: открыто меню чекпоинта {checkpointName}");
    }

    private void CloseMenu()
    {
        if (!isMenuOpen) return;
        
        isMenuOpen = false;
        Time.timeScale = 1f;

        if(checkpointMenu != null) checkpointMenu?.SetActive(false);

        _playerController?.SetInputBlocked(false);

        Debug.Log($"Checkpoint: закрыто меню чекпоинта {checkpointName}");

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRadius = true;
            _playerController = other.GetComponent<PlayerController>();

            string hint = isActivated ? $"Нажмите E, чтобы открыть {checkpointName}" : "Нажмите E, чтобы активировать чекпоинт";

            HintManager.Instance?.RegisterHint(this, hint, priotiry: 10f, duration: 0);

            Debug.Log($"Checkpoint: игрок в радиусе чекпоинта {checkpointName}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRadius = false;

            HintManager.Instance?.RemoveHintsFromSource(this);

            if (isMenuOpen) CloseMenu();

            Debug.Log($"Checkpoint: игрок покинул радиус чекпоинта {checkpointName}");
        }
    }
}
