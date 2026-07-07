using System;
using System.Collections.Generic;
using UnityEngine;

public class KeepsakeUnlockProgression : MonoBehaviour
{
    public static KeepsakeUnlockProgression instance;

    private Dictionary<ChallengeType, int> roundStats = new Dictionary<ChallengeType, int>();
    private Dictionary<ChallengeType, int> playthroughStats = new Dictionary<ChallengeType, int>();

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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F12))
        {
            ResetAllProgress();
        }
    }

    public void ResetAllProgress()
    {
        roundStats.Clear();
        playthroughStats.Clear();

        foreach(ChallengeType type in Enum.GetValues(typeof(ChallengeType)))
        {
            PlayerPrefs.DeleteKey(type.ToString());
        }

        PlayerPrefs.Save();
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

        playthroughStats[type] += amount;

        SavePlaythroughStats();
    }

    public bool HasMetRequirement(Keepsake keepsake)
    {
        int progress = 0;
        bool isRoundSpecific = IsRoundChallenge(keepsake.requiredChallenge);

        if(isRoundSpecific)
        {
            roundStats.TryGetValue(keepsake.requiredChallenge, out progress);
        }
        else
        {
            playthroughStats.TryGetValue(keepsake.requiredChallenge, out progress);
        }

        return progress >= keepsake.requiredTarget;
    }

    public int GetProgress(ChallengeType type)
    {
        int progress = 0;
        bool isRoundSpecific = IsRoundChallenge(type);

        if(isRoundSpecific)
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
        return type == ChallengeType.ItemAfterStand || type == ChallengeType.AlterDealerHand;
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
}
