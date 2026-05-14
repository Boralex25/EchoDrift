using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance {  get; private set; }

    [SerializeField] private int[] levelThreshods = { 100, 250, 450, 700 };

    private int currentXP;
    private int currentEcho;
    private int currentLevel;
    int skillPoints;

    public int CurrentXP => currentXP;
    public int CurrentEcho => currentEcho;
    public int CurrentLevel => currentLevel;
    public int SkillPoints => skillPoints;

    public UnityEvent<int> OnEchoChanged;
    public UnityEvent<int, int> OnXPChanged;
    public UnityEvent<int> OnSkillPointsChanged;

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
        currentXP = 0;
        currentEcho = 0;
        currentLevel = 1;
    }

    private void AddEcho(int amount)
    {
        currentEcho += amount;
        OnEchoChanged?.Invoke(currentEcho);
    }

    private void SpendEcho(int amount)
    {
        if(currentEcho >= amount)
        {
            currentEcho -= amount;
            OnEchoChanged?.Invoke(currentEcho);
        }
    }

    private void AddXP(int amount)
    {
        currentXP += amount;
        OnXPChanged?.Invoke(currentXP, currentLevel);
    }

    private void CheckLevelUp()
    {
        foreach(int threshold in levelThreshods)
        {
            if(currentXP >= threshold)
            {
                currentLevel++;
                skillPoints++;
            }
        }
    }

}
