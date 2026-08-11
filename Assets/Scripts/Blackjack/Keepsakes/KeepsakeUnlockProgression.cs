using System;
using System.Collections.Generic;
using UnityEngine;

public class KeepsakeUnlockProgression : MonoBehaviour
{
    public static KeepsakeUnlockProgression instance;

    private Dictionary<ChallengeType, int> roundStats = new Dictionary<ChallengeType, int>();
    private Dictionary<ChallengeType, int> playthroughStats = new Dictionary<ChallengeType, int>();

    public event Action OnProgressChanged;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadPlaythroughStats();
    }

    //temp
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            UnlockAllProgress();
        }

        if(Input.GetKeyDown(KeyCode.F12))
        {
            ResetAllProgress();
        }
    }

    //temp
    public void UnlockAllProgress()
    {
        foreach(ChallengeType type in Enum.GetValues(typeof(ChallengeType)))
        {
            playthroughStats[type] = 999;

            PlayerPrefs.SetInt(type.ToString(), 999);
        }

        PlayerPrefs.Save();

        OnProgressChanged?.Invoke();
    }

    //temp
    public void ResetAllProgress()
    {
        roundStats.Clear();
        playthroughStats.Clear();

        foreach(ChallengeType type in Enum.GetValues(typeof(ChallengeType)))
        {
            PlayerPrefs.DeleteKey(type.ToString());
        }

        PlayerPrefs.Save();

        OnProgressChanged?.Invoke();
    }

    public void AddStat(ChallengeType type, int amount = 1)
    {
        if(!roundStats.ContainsKey(type))
        {
            roundStats[type] = 0;
        }

        roundStats[type] += amount;

        if(!playthroughStats.ContainsKey(type))
        {
            playthroughStats[type] = 0;
        }

        if(IsRoundChallenge(type))
        {
            if(roundStats[type] > playthroughStats[type])
            {
                playthroughStats[type] = roundStats[type];
            }
        }
        else
        {
            playthroughStats[type] += amount;
        }

        if(type == ChallengeType.DoubleDown || type == ChallengeType.Split)
        {
            int dd = roundStats.ContainsKey(ChallengeType.DoubleDown) ? roundStats[ChallengeType.DoubleDown] : 0;
            int split = roundStats.ContainsKey(ChallengeType.Split) ? roundStats[ChallengeType.Split] : 0;

            if(dd >= 5 && split >= 5)
            {
                int combo = roundStats.ContainsKey(ChallengeType.DoubleDownAndSplit) ? roundStats[ChallengeType.DoubleDownAndSplit] : 0;

                if(combo == 0)
                {
                    AddStat(ChallengeType.DoubleDownAndSplit, 1);
                }
            }
        }

        SavePlaythroughStats();

        OnProgressChanged?.Invoke();
    }

    public bool HasMetRequirement(Keepsake keepsake)
    {
        playthroughStats.TryGetValue(keepsake.requiredChallenge, out int progress);

        return progress >= keepsake.requiredTarget;
    }

    public int GetProgress(ChallengeType type)
    {
        int progress = 0;

        if(IsRoundChallenge(type))
        {
            roundStats.TryGetValue(type, out progress);
        }
        else
        {
            playthroughStats.TryGetValue(type, out progress);
        }

        return progress;
    }

    private bool IsRoundChallenge(ChallengeType type)
    {
        return type == ChallengeType.ItemAfterStand || type == ChallengeType.AlterDealerHand || type == ChallengeType.DoubleDownAndSplit;
    }

    public void EndRun()
    {
        AddStat(ChallengeType.CompleteRound);

        roundStats.Clear();
    }

    private void SavePlaythroughStats()
    {
        foreach(ChallengeType type in Enum.GetValues(typeof(ChallengeType)))
        {
            if(playthroughStats.ContainsKey(type))
            {
                PlayerPrefs.SetInt(type.ToString(), playthroughStats[type]);
            }
        }

        PlayerPrefs.Save();
    }

    private void LoadPlaythroughStats()
    {
        foreach(ChallengeType type in Enum.GetValues(typeof(ChallengeType)))
        {
            playthroughStats[type] = PlayerPrefs.GetInt(type.ToString(), 0);
        }
    }

    #region Check Progress

    public void CheckSuitWinCondition(List<List<CardInstance>> allHands)
    {
        bool allRed = true;
        bool allBlack = true;

        foreach(var hand in allHands)
        {
            foreach(var card in hand)
            {
                if(card.cardData.IsRedSuit())
                {
                    allBlack = false;
                }
                else if(card.cardData.IsBlackSuit())
                {
                    allRed = false;
                }
            }
        }

        if(allRed)
        {
            AddStat(ChallengeType.WinRedSuits);
        }

        if(allBlack)
        {
            AddStat(ChallengeType.WinBlackSuits);
        }
    }

    public void CheckThreeOfAKind(List<List<CardInstance>> allHands)
    {
        foreach(var hand in allHands)
        {
            Dictionary<int, int> valueCounts = new Dictionary<int, int>();

            foreach(var card in hand)
            {
                int val = card.cardData.GetValue();

                if(!valueCounts.ContainsKey(val)) valueCounts[val] = 0;

                valueCounts[val]++;

                if(valueCounts[val] >= 3)
                {
                    AddStat(ChallengeType.ThreeOfAKind);

                    return;
                }
            }
        }
    }

    #endregion
}
