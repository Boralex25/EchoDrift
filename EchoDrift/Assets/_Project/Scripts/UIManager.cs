using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Health")]
    [SerializeField] private Transform healthContainer;
    [SerializeField] private GameObject heartPrefab;

    [Header("Heart sprites")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartThreeQuarters;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartQuarter;
    [SerializeField] private Sprite heartEmpty;

    [Header("Local mechanic")]
    [SerializeField] private Image torchFillImg;
    [SerializeField] private TextMeshProUGUI torchTimerText;

    [Header("Hint")]
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private float hintDuration;

    private List<Image> heartImages = new List<Image>();

    private Coroutine hintCoroutine;

    

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        hintText.text = "";
    }

    public void InitializeHealth(int maxHealthInUnits)
    {
        foreach(Transform child in healthContainer) 
            Destroy(child.gameObject);

        heartImages.Clear();

        int heartCount = maxHealthInUnits / 4;

        for(int i = 0; i < heartCount; i++)
        {
            GameObject heartObj = Instantiate(heartPrefab, healthContainer);
            Image heartImage = heartObj.GetComponent<Image>();
            heartImages.Add(heartImage);
        }
    }

    public void UpdateHealth(int currentHealthInUnits, int maxHealthInUnits)
    {
        Debug.Log($"UpdateHealth вызван: {currentHealthInUnits} / {maxHealthInUnits}");
        int heartCount = maxHealthInUnits / 4;
        int remainingUnits = currentHealthInUnits;

        for( int i = 0;i < heartCount; i++)
        {
            int heartUnits = Mathf.Min(remainingUnits, 4);

            SetHeartSprite(heartImages[i], heartUnits);

            remainingUnits -= heartUnits;

            if (remainingUnits <= 0) remainingUnits = 0;
        }

        Debug.Log($"Количество сердец в списке: {heartImages.Count}");
    }

    private void SetHeartSprite(Image heartImage, int units)
    {
        Debug.Log($"SetHeartSprite: units = {units}");

        Sprite selectedSprite = units switch
        {
            4 => heartFull,
            3 => heartThreeQuarters,
            2 => heartHalf,
            1 => heartQuarter,
            0 => heartEmpty,
            _ => heartEmpty
        };

        heartImage.sprite = selectedSprite;
    }

    public void UpdateTorchTimer(float currentTime, float maxTime)
    {
        int timeForText = Mathf.CeilToInt(currentTime);

        torchTimerText.text = $"{timeForText} сек";
        //Debug.Log($"{timeForText} сек");

        if(torchFillImg != null && torchFillImg.type == Image.Type.Filled) torchFillImg.fillAmount = currentTime / maxTime;

        if (currentTime <= 5) torchTimerText.color = Color.red;
        else torchTimerText.color = Color.white;
    }

    public void ShowHint(string message, float duration = -1)
    {
        if (duration < 0) duration = hintDuration;

        if(duration == 0)
        {
            if (hintCoroutine != null) StopCoroutine(hintCoroutine);

            hintText.text = message;
            hintText.gameObject.SetActive(true);
            hintCoroutine = null;
            return;
        }

        if (hintCoroutine != null) StopCoroutine(hintCoroutine);

        hintText.text = message;
        hintText.gameObject.SetActive(false);
        hintCoroutine = StartCoroutine(HideHintAfterDuration(duration));
        
    }

    public void HideHint()
    {
        if(hintCoroutine != null) StopCoroutine(hintCoroutine);
        hintText?.gameObject.SetActive(false);
        hintCoroutine = null;
    }

    private IEnumerator HideHintAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        hintText.gameObject.SetActive(false);
        hintCoroutine = null;
    }
}
