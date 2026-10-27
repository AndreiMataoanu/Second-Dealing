using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Single, persistent FMOD audio manager.
/// Usage:
///   AudioManagerFMOD.Play(AudioEvent.sfxHit);
///   AudioManagerFMOD.Play(AudioEvent.sfxDeal, transform.position);
///   AudioManagerFMOD.Stop(AudioEvent.musMainGame);          // fades out
///   AudioManagerFMOD.Stop(AudioEvent.musMainGame, false);   // immediate
///   AudioManagerFMOD.SetParameter(AudioEvent.musMainGame, "Intensity", 0.5f);
/// Assign the EventReferences in the inspector of the one AudioManagerFmod object.
///
/// Adding more events to FMOD? Simply add them as well as a EventReference, to the Register Events and the ENUM
/// Use the TEST event if you are unsure what something does or want to make sure you are looking at the right place
/// The TEST event should have a very noticable sound.
/// </summary>
public class AudioManagerFMOD : MonoBehaviour
{
    public static AudioManagerFMOD Instance { get; private set; }

    // ---------------------------------------------------------------
    // Event references (assign in the inspector)
    // Add to the top of this list so its easy to change in editor :)
    // ---------------------------------------------------------------
    #region Global Parameters
    [Header("Global Parameters")]

    [SerializeField] [ParamRef] private string parGameSpeed;
    [SerializeField] [ParamRef] private string parCoinFlipResult;
    [SerializeField] [ParamRef] private string parElevatorDoorOpen;
    
    #endregion
    
    #region EventRefrences
    [Header("EventRefrences")]
    
    [SerializeField] private EventReference TEST;
    [SerializeField] private EventReference sfxBlackJackHand;
    [SerializeField] private EventReference sfxCardAddToDeck;
    [SerializeField] private EventReference sfxJokerCard;
    [SerializeField] private EventReference sfxDoubleDoubleCard;
    [SerializeField] private EventReference sfxCutCutCard;
    [SerializeField] private EventReference sfxTarotCard;
    [SerializeField] private EventReference sfxFlip;
    [SerializeField] private EventReference sfxKeepsakeUnlock;
    [SerializeField] private EventReference uiSettingsDialTurn;
    [SerializeField] private EventReference sfxRoundStart;
    [SerializeField] private EventReference sfxNegativeCard;
    [SerializeField] private EventReference sfxDoubledCard;
    [SerializeField] private EventReference sfxCutCard;
    [SerializeField] private EventReference sfxDeal;
    [SerializeField] private EventReference sfxHit;
    [SerializeField] private EventReference sfxStand;
    [SerializeField] private EventReference sfxDoubleDown;
    [SerializeField] private EventReference sfxSplit;
    [SerializeField] private EventReference sfxShuffle;
    [SerializeField] private EventReference sfxPlayerBored;
    [SerializeField] private EventReference sfxPlayerWin;
    [SerializeField] private EventReference sfxPlayerLose;
    [SerializeField] private EventReference sfxPlayerSelect;
    [SerializeField] private EventReference sfxPlayerDeselect;
    [SerializeField] private EventReference sfxElevatorDoorOpen;
    [SerializeField] private EventReference sfxElevatorDoorClose;
    [SerializeField] private EventReference sfxElevatorDrive;
    [SerializeField] private EventReference sfxElevatorButton;
    [SerializeField] private EventReference sfxFootstepsPlayer;
    [SerializeField] private EventReference sfxCantUseItem;
    [SerializeField] private EventReference sfxEventSelect;
    [SerializeField] private EventReference sfxEventRerolled;
    [SerializeField] private EventReference sfxIncreasingBet;
    [SerializeField] private EventReference sfxDecreasingBet;
    [SerializeField] private EventReference sfxMaximumBet;
    [SerializeField] private EventReference sfxMinimumBet;
    [SerializeField] private EventReference sfxShopOpen;
    [SerializeField] private EventReference sfxShopClose;
    [SerializeField] private EventReference sfxShopBuy;
    [SerializeField] private EventReference sfxShopSell;
    [SerializeField] private EventReference sfxCantBuy;
    [SerializeField] private EventReference sfxCardSelectorPress;
    [SerializeField] private EventReference sfxCardSelectorPrint;
    [SerializeField] private EventReference sfxCashOut;
    [SerializeField] private EventReference sfxCashOutChampagneSpawn;
    [SerializeField] private EventReference sfxCashOutChampagnePop;
    [SerializeField] private EventReference sfxDealerTalk;
    [SerializeField] private EventReference sfxGenerelEvent;
    [SerializeField] private EventReference sfxDartRemoveEvent;
    [SerializeField] private EventReference sfxDartAddEvent;
    [SerializeField] private EventReference sfxDartEventHit;
    [SerializeField] private EventReference sfxDartEventBurn;
    [SerializeField] private EventReference sfxPowerBall;
    [SerializeField] private EventReference sfxPowerBallHit;
    [SerializeField] private EventReference sfxAceSetEventOne;
    [SerializeField] private EventReference sfxAceSetEventEleven;
    [SerializeField] private EventReference sfxNegativeSuitEvent;
    [SerializeField] private EventReference sfxRageEventStart;
    [SerializeField] private EventReference sfxRageEventIncrease;
    [SerializeField] private EventReference sfxRageEventThreshold;
    [SerializeField] private EventReference sfxJokerEvent;
    [SerializeField] private EventReference sfxRemoveSuitEvent;
    [SerializeField] private EventReference sfxRemoveValueEvent;
    [SerializeField] private EventReference sfxRouletteEvent;
    [SerializeField] private EventReference sfxRouletteEventNewTarget;
    [SerializeField] private EventReference sfxDoubleFiveBelowEvent;
    [SerializeField] private EventReference sfxHalveSixAboveEvent;
    [SerializeField] private EventReference sfxGenerelItem;
    [SerializeField] private EventReference sfxKnifeItem;
    [SerializeField] private EventReference sfxCrossItem;
    [SerializeField] private EventReference sfxCrossItemEffect;
    [SerializeField] private EventReference sfxScissorsItem;
    [SerializeField] private EventReference sfxScissorsItemPickup;
    [SerializeField] private EventReference sfxFanItem;
    [SerializeField] private EventReference sfxCigarettesItem;
    [SerializeField] private EventReference sfxNFTItem;
    [SerializeField] private EventReference sfxNFTItemEmitter;
    [SerializeField] private EventReference sfxOrganItem;
    [SerializeField] private EventReference sfxOrganItemExpire;
    [SerializeField] private EventReference sfxCoinItem;
    [SerializeField] private EventReference sfxAlcoholItem;
    [SerializeField] private EventReference sfxAlcoholItemEffect;
    [SerializeField] private EventReference sfxAcidItem;
    [SerializeField] private EventReference sfxAcidItemEmitter;
    [SerializeField] private EventReference sfxAcidItemPickup;
    [SerializeField] private EventReference sfxSunglassesItem;
    [SerializeField] private EventReference sfxKeepsakeGenerel;
    [SerializeField] private EventReference sfxBlueBallsKeepsake;
    [SerializeField] private EventReference sfxSnowGlobeKeepsake;
    [SerializeField] private EventReference sfx777Keepsake;
    [SerializeField] private EventReference sfxTrustFundKeepsake;
    [SerializeField] private EventReference sfxInheritanceKeepsake;
    [SerializeField] private EventReference sfxHattricKeepsake;
    [SerializeField] private EventReference sfxBloodPressureKeepsake;
    [SerializeField] private EventReference sfxGoFishKeepsake;
    [SerializeField] private EventReference sfxBloodRoseKeepsake;
    [SerializeField] private EventReference sfxOnyxRoseKeepsake;
    [SerializeField] private EventReference sfxGlassEyeKeepsake;
    [SerializeField] private EventReference sfxLoyalyCardKeepsake;
    [SerializeField] private EventReference sfxPyroKeepsake;
    [SerializeField] private EventReference sfxPrinterKeepsake;
    [SerializeField] private EventReference sfxSecondDealingKeepsake;
    [SerializeField] private EventReference sfxTarotKeepsake;
    [SerializeField] private EventReference sfxDivorcePaperKeepsake;
    [SerializeField] private EventReference sfxWeddingRingKeepsake;
    [SerializeField] private EventReference sfxHiddenAceKeepsake;
    [SerializeField] private EventReference sfxSprayCanKeepsake;
    [SerializeField] private EventReference sfxJointAccountKeepsake;
    [SerializeField] private EventReference sfxAntiMatterKeepsake;
    [SerializeField] private EventReference ambCasinoMain;
    [SerializeField] private EventReference ambElevatorMain;
    [SerializeField] private EventReference uiMenuButtonHover;
    [SerializeField] private EventReference uiMenuButtonPress;
    [SerializeField] private EventReference uiMenuStartGame;
    [SerializeField] private EventReference uiPauseGame;
    [SerializeField] private EventReference uiPauseResumeGame;
    [SerializeField] private EventReference uiPauseQuitGame;
    [SerializeField] private EventReference uiHoverIngameButton;
    [SerializeField] private EventReference musMainMenu;
    [SerializeField] private EventReference musElevator1;
    [SerializeField] private EventReference musMainGame;
    [SerializeField] private EventReference musWIn;
    #endregion
    
    // ---------------------------------------------------------------
    // Lookup + live instance tracking
    // ---------------------------------------------------------------
    private readonly Dictionary<AudioEvent, EventReference> events = new Dictionary<AudioEvent, EventReference>();
    private readonly Dictionary<AudioEvent, List<EventInstance>> liveInstances = new Dictionary<AudioEvent, List<EventInstance>>();
    
    
    //Matches Event String Names with Audio Events, Add here if more events are added to game. 
    // --UPDATE IF BLACKJACKEVENT.SO ARE CHANGED--
    
    private static readonly Dictionary<string, AudioEvent> eventLookup = new Dictionary<string, AudioEvent>
    {
        { "Adding Jokers",         AudioEvent.sfxJokerEvent },
        { "Adding card copies",    AudioEvent.sfxDartAddEvent },
        { "Remove cards",          AudioEvent.sfxDartRemoveEvent },
        { "Powerball!",            AudioEvent.sfxPowerBall },
        { "Ace is always 1",       AudioEvent.sfxAceSetEventOne },
        { "Ace is always 11",      AudioEvent.sfxAceSetEventEleven },
        { "Negative Spades",       AudioEvent.sfxNegativeSuitEvent },
        { "Negative Clubs",        AudioEvent.sfxNegativeSuitEvent },
        { "Negative Hearts",       AudioEvent.sfxNegativeSuitEvent },
        { "Negative Diamonds",     AudioEvent.sfxNegativeSuitEvent },
        { "Dealer got ragebaited", AudioEvent.sfxRageEventStart },
        { "Roulette Blackjack",    AudioEvent.sfxRouletteEvent },
        { "Low values doubled",    AudioEvent.sfxDoubleFiveBelowEvent },
        { "High values halved",    AudioEvent.sfxHalveSixAboveEvent },
        { "Removed Spades",        AudioEvent.sfxRemoveSuitEvent },
        { "Removed Clubs",         AudioEvent.sfxRemoveSuitEvent },
        { "Removed Hearts",        AudioEvent.sfxRemoveSuitEvent },
        { "Removed Diamonds",      AudioEvent.sfxRemoveSuitEvent },
        
        { "Removed Value: 2",      AudioEvent.sfxRemoveValueEvent },
        { "Removed Value: 3",      AudioEvent.sfxRemoveValueEvent },
        { "Removed Value: 4",      AudioEvent.sfxRemoveValueEvent },
        { "Removed Value: 5",      AudioEvent.sfxRemoveValueEvent },
        { "Removed Value: 6",      AudioEvent.sfxRemoveValueEvent },
        { "Removed Value: 7",      AudioEvent.sfxRemoveValueEvent },
        { "Removed Value: 8",      AudioEvent.sfxRemoveValueEvent },
        { "Removed Value: 9",      AudioEvent.sfxRemoveValueEvent },
        { "Removed Value: 10",     AudioEvent.sfxRemoveValueEvent },
        { "Removed Value: Ace",    AudioEvent.sfxRemoveValueEvent },
    };

    private void Awake()
    {
        // Only one AudioManagerFmod may exist.
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[AudioManagerFmod] Duplicate found, destroying it.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        RegisterEvents();
    }
    
        //For event name matching, looks at the string of text played on monitor and matches it to sound event.
    public static bool TryPlay(string eventName)
    {
        if (eventLookup.TryGetValue(eventName, out var audioEvent))
        {
            Play(audioEvent);
            return true;
        }

        Debug.LogWarning($"[AudioManagerFmod] No sound mapped for event '{eventName}'.");
        return false;
    }

    private void RegisterEvents()
    {
        #region RegisteredEvents
        events[AudioEvent.TEST] = TEST;
        events[AudioEvent.sfxBlackJackHand] = sfxBlackJackHand;
        events[AudioEvent.sfxCardAddToDeck] = sfxCardAddToDeck;
        events[AudioEvent.sfxJokerCard] = sfxJokerCard;
        events[AudioEvent.sfxDoubleDoubleCard] = sfxDoubleDoubleCard;
        events[AudioEvent.sfxCutCutCard] = sfxCutCutCard;
        events[AudioEvent.sfxTarotCard] = sfxTarotCard;
        events[AudioEvent.sfxKeepsakeUnlock] = sfxKeepsakeUnlock;
        events[AudioEvent.uiSettingsDialTurn] = uiSettingsDialTurn;
        events[AudioEvent.sfxFlip] = sfxFlip;
        events[AudioEvent.sfxRoundStart] = sfxRoundStart;
        events[AudioEvent.sfxNegativeCard] = sfxNegativeCard;
        events[AudioEvent.sfxDoubledCard] = sfxDoubledCard;
        events[AudioEvent.sfxCutCard] = sfxCutCard;
        events[AudioEvent.sfxDeal] = sfxDeal;
        events[AudioEvent.sfxHit] = sfxHit;
        events[AudioEvent.sfxStand] = sfxStand;
        events[AudioEvent.sfxDoubleDown] = sfxDoubleDown;
        events[AudioEvent.sfxSplit] = sfxSplit;
        events[AudioEvent.sfxShuffle] = sfxShuffle;
        events[AudioEvent.sfxPlayerBored] = sfxPlayerBored;
        events[AudioEvent.sfxPlayerWin] = sfxPlayerWin;
        events[AudioEvent.sfxPlayerLose] = sfxPlayerLose;
        events[AudioEvent.sfxPlayerSelect] = sfxPlayerSelect;
        events[AudioEvent.sfxPlayerDeselect] = sfxPlayerDeselect;
        events[AudioEvent.sfxElevatorDoorOpen] = sfxElevatorDoorOpen;
        events[AudioEvent.sfxElevatorDoorClose] = sfxElevatorDoorClose;
        events[AudioEvent.sfxElevatorDrive] = sfxElevatorDrive;
        events[AudioEvent.sfxElevatorButton] = sfxElevatorButton;
        events[AudioEvent.sfxFootstepsPlayer] = sfxFootstepsPlayer;
        events[AudioEvent.sfxCantUseItem] = sfxCantUseItem;
        events[AudioEvent.sfxEventSelect] = sfxEventSelect;
        events[AudioEvent.sfxEventRerolled] = sfxEventRerolled;
        events[AudioEvent.sfxIncreasingBet] = sfxIncreasingBet;
        events[AudioEvent.sfxDecreasingBet] = sfxDecreasingBet;
        events[AudioEvent.sfxMaximumBet] = sfxMaximumBet;
        events[AudioEvent.sfxMinimumBet] = sfxMinimumBet;
        events[AudioEvent.sfxShopOpen] = sfxShopOpen;
        events[AudioEvent.sfxShopClose] = sfxShopClose;
        events[AudioEvent.sfxShopBuy] = sfxShopBuy;
        events[AudioEvent.sfxShopSell] = sfxShopSell;
        events[AudioEvent.sfxCantBuy] = sfxCantBuy;
        events[AudioEvent.sfxCardSelectorPress] = sfxCardSelectorPress;
        events[AudioEvent.sfxCardSelectorPrint] = sfxCardSelectorPrint;
        events[AudioEvent.sfxCashOut] = sfxCashOut;
        events[AudioEvent.sfxCashOutChampagneSpawn] = sfxCashOutChampagneSpawn;
        events[AudioEvent.sfxCashOutChampagnePop] = sfxCashOutChampagnePop;
        events[AudioEvent.sfxDealerTalk] = sfxDealerTalk;
        events[AudioEvent.sfxGenerelEvent] = sfxGenerelEvent;
        events[AudioEvent.sfxDartRemoveEvent] = sfxDartRemoveEvent;
        events[AudioEvent.sfxDartAddEvent] = sfxDartAddEvent;
        events[AudioEvent.sfxDartEventHit] = sfxDartEventHit;
        events[AudioEvent.sfxDartEventBurn] = sfxDartEventBurn;
        events[AudioEvent.sfxPowerBall] = sfxPowerBall;
        events[AudioEvent.sfxPowerBallHit] = sfxPowerBallHit;
        events[AudioEvent.sfxAceSetEventOne] = sfxAceSetEventOne;
        events[AudioEvent.sfxAceSetEventEleven] = sfxAceSetEventEleven;
        events[AudioEvent.sfxNegativeSuitEvent] = sfxNegativeSuitEvent;
        events[AudioEvent.sfxRageEventStart] = sfxRageEventStart;
        events[AudioEvent.sfxRageEventIncrease] = sfxRageEventIncrease;
        events[AudioEvent.sfxRageEventThreshold] = sfxRageEventThreshold;
        events[AudioEvent.sfxJokerEvent] = sfxJokerEvent;
        events[AudioEvent.sfxRouletteEvent] = sfxRouletteEvent;
        events[AudioEvent.sfxRouletteEventNewTarget] = sfxRouletteEventNewTarget;
        events[AudioEvent.sfxRemoveSuitEvent] = sfxRemoveSuitEvent;
        events[AudioEvent.sfxRemoveValueEvent] = sfxRemoveValueEvent;
        events[AudioEvent.sfxDoubleFiveBelowEvent] = sfxDoubleFiveBelowEvent;
        events[AudioEvent.sfxHalveSixAboveEvent] = sfxHalveSixAboveEvent;
        events[AudioEvent.sfxGenerelItem] = sfxGenerelItem;
        events[AudioEvent.sfxKnifeItem] = sfxKnifeItem;
        events[AudioEvent.sfxCrossItem] = sfxCrossItem;
        events[AudioEvent.sfxCrossItemEffect] = sfxCrossItemEffect;
        events[AudioEvent.sfxScissorsItem] = sfxScissorsItem;
        events[AudioEvent.sfxScissorsItemPickup] = sfxScissorsItemPickup;
        events[AudioEvent.sfxFanItem] = sfxFanItem;
        events[AudioEvent.sfxCigarettesItem] = sfxCigarettesItem;
        events[AudioEvent.sfxNFTItem] = sfxNFTItem;
        events[AudioEvent.sfxNFTItemEmitter] = sfxNFTItemEmitter;
        events[AudioEvent.sfxOrganItem] = sfxOrganItem;
        events[AudioEvent.sfxOrganItemExpire] = sfxOrganItemExpire;
        events[AudioEvent.sfxCoinItem] = sfxCoinItem;
        events[AudioEvent.sfxAlcoholItem] = sfxAlcoholItem;
        events[AudioEvent.sfxAlcoholItemEffect] = sfxAlcoholItemEffect;
        events[AudioEvent.sfxAcidItem] = sfxAcidItem;
        events[AudioEvent.sfxAcidItemEmitter] = sfxAcidItemEmitter;
        events[AudioEvent.sfxAcidItemPickup] = sfxAcidItemPickup;
        events[AudioEvent.sfxSunglassesItem] = sfxSunglassesItem;
        events[AudioEvent.sfxKeepsakeGenerel] = sfxKeepsakeGenerel;
        events[AudioEvent.sfxBlueBallsKeepsake] = sfxBlueBallsKeepsake;
        events[AudioEvent.sfxSnowGlobeKeepsake] = sfxSnowGlobeKeepsake;
        events[AudioEvent.sfx777Keepsake] = sfx777Keepsake;
        events[AudioEvent.sfxTrustFundKeepsake] = sfxTrustFundKeepsake;
        events[AudioEvent.sfxInheritanceKeepsake] = sfxInheritanceKeepsake;
        events[AudioEvent.sfxHattricKeepsake] = sfxHattricKeepsake;
        events[AudioEvent.sfxBloodPressureKeepsake] = sfxBloodPressureKeepsake;
        events[AudioEvent.sfxGoFishKeepsake] = sfxGoFishKeepsake;
        events[AudioEvent.sfxBloodRoseKeepsake] = sfxBloodRoseKeepsake;
        events[AudioEvent.sfxOnyxRoseKeepsake] = sfxOnyxRoseKeepsake;
        events[AudioEvent.sfxGlassEyeKeepsake] = sfxGlassEyeKeepsake;
        events[AudioEvent.sfxLoyalyCardKeepsake] = sfxLoyalyCardKeepsake;
        events[AudioEvent.sfxPyroKeepsake] = sfxPyroKeepsake;
        events[AudioEvent.sfxPrinterKeepsake] = sfxPrinterKeepsake;
        events[AudioEvent.sfxSecondDealingKeepsake] = sfxSecondDealingKeepsake;
        events[AudioEvent.sfxTarotKeepsake] = sfxTarotKeepsake;
        events[AudioEvent.sfxDivorcePaperKeepsake] = sfxDivorcePaperKeepsake;
        events[AudioEvent.sfxWeddingRingKeepsake] = sfxWeddingRingKeepsake;
        events[AudioEvent.sfxHiddenAceKeepsake] = sfxHiddenAceKeepsake;
        events[AudioEvent.sfxSprayCanKeepsake] = sfxSprayCanKeepsake;
        events[AudioEvent.sfxJointAccountKeepsake] = sfxJointAccountKeepsake;
        events[AudioEvent.sfxAntiMatterKeepsake] = sfxAntiMatterKeepsake;
        events[AudioEvent.ambCasinoMain] = ambCasinoMain;
        events[AudioEvent.ambElevatorMain] = ambElevatorMain;
        events[AudioEvent.uiMenuButtonHover] = uiMenuButtonHover;
        events[AudioEvent.uiMenuButtonPress] = uiMenuButtonPress;
        events[AudioEvent.uiMenuStartGame] = uiMenuStartGame;
        events[AudioEvent.uiPauseGame] = uiPauseGame;
        events[AudioEvent.uiPauseResumeGame] = uiPauseResumeGame;
        events[AudioEvent.uiPauseQuitGame] = uiPauseQuitGame;
        events[AudioEvent.uiHoverIngameButton] = uiHoverIngameButton;
        events[AudioEvent.musMainMenu] = musMainMenu;
        events[AudioEvent.musElevator1] = musElevator1;
        events[AudioEvent.musMainGame] = musMainGame;
        events[AudioEvent.musWIn] = musWIn;
        #endregion
    }

    private void Update()
    {
        CleanupFinishedInstances();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        StopAllInternal(false);
        Instance = null;
    }

    // ---------------------------------------------------------------
    // Public static API
    // ---------------------------------------------------------------

    /// <summary>Plays a new instance of the event. Returns the instance so you can control it further.</summary>
    public static EventInstance Play(AudioEvent audioEvent)
    {
        return Require() ? Instance.PlayInternal(audioEvent, null) : default;
    }

    /// <summary>Plays a new instance of the event at a world position (3D events).</summary>
    public static EventInstance Play(AudioEvent audioEvent, Vector3 position)
    {
        return Require() ? Instance.PlayInternal(audioEvent, position) : default;
    }

    /// <summary>Stops and releases every live instance of this event.</summary>
    public static void Stop(AudioEvent audioEvent, bool allowFadeOut = true)
    {
        if (Require()) Instance.StopInternal(audioEvent, allowFadeOut);
    }

    /// <summary>Stops and releases every live instance of every event.</summary>
    public static void StopAll(bool allowFadeOut = true)
    {
        if (Require()) Instance.StopAllInternal(allowFadeOut);
    }

    /// <summary>Sets a parameter on all live instances of this event.</summary>
    public static void SetParameter(AudioEvent audioEvent, string parameterName, float value)
    {
        if (!Require()) return;
        if (!Instance.liveInstances.TryGetValue(audioEvent, out var list)) return;
        foreach (var inst in list) inst.setParameterByName(parameterName, value);
    }

    /// <summary>True if at least one instance of this event is currently playing.</summary>
    public static bool IsPlaying(AudioEvent audioEvent)
    {
        if (!Require()) return false;
        if (!Instance.liveInstances.TryGetValue(audioEvent, out var list)) return false;
        foreach (var inst in list)
        {
            inst.getPlaybackState(out var state);
            if (state != PLAYBACK_STATE.STOPPED) return true;
        }
        return false;
    }

    // ---------------------------------------------------------------
    // Internals
    // ---------------------------------------------------------------
    private static bool Require()
    {
        if (Instance != null) return true;
        Debug.LogError("[AudioManagerFmod] No AudioManagerFmod in the scene.");
        return false;
    }

    private EventInstance PlayInternal(AudioEvent audioEvent, Vector3? position)
    {
        if (!events.TryGetValue(audioEvent, out var reference) || reference.IsNull)
        {
            Debug.LogWarning($"[AudioManagerFmod] Event '{audioEvent}' has no EventReference assigned.");
            return default;
        }

        EventInstance instance = RuntimeManager.CreateInstance(reference);
        if (position.HasValue)
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));

        instance.start();

        if (!liveInstances.TryGetValue(audioEvent, out var list))
        {
            list = new List<EventInstance>();
            liveInstances[audioEvent] = list;
        }
        list.Add(instance);
        return instance;
    }

    private void StopInternal(AudioEvent audioEvent, bool allowFadeOut)
    {
        if (!liveInstances.TryGetValue(audioEvent, out var list)) return;

        foreach (var inst in list)
        {
            inst.stop(allowFadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            inst.release(); // freed by FMOD once it has actually stopped
        }
        list.Clear();
    }

    private void StopAllInternal(bool allowFadeOut)
    {
        foreach (var kv in liveInstances)
        {
            foreach (var inst in kv.Value)
            {
                inst.stop(allowFadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
                inst.release();
            }
            kv.Value.Clear();
        }
    }

    private void CleanupFinishedInstances()
    {
        foreach (var kv in liveInstances)
        {
            var list = kv.Value;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                list[i].getPlaybackState(out var state);
                if (state == PLAYBACK_STATE.STOPPED)
                {
                    list[i].release();
                    list.RemoveAt(i);
                }
            }
        }
    }
    public static void SetGlobalParameter(string parameterName, float value)
    {
        if (!Require()) return;

        RuntimeManager.StudioSystem.setParameterByName(parameterName, value);
    }
}



/// <summary>One entry per event in the sound sheet (column E).</summary>
public enum AudioEvent
{
        TEST,
        sfxBlackJackHand,
        sfxCardAddToDeck,
        sfxJokerCard,
        sfxDoubleDoubleCard,
        sfxCutCutCard,
        sfxTarotCard,
        sfxFlip,
        sfxKeepsakeUnlock,
        uiSettingsDialTurn,
        sfxRoundStart,
        sfxNegativeCard,
        sfxDoubledCard,
        sfxCutCard,
        sfxDeal,
        sfxHit,
        sfxStand,
        sfxDoubleDown,
        sfxSplit,
        sfxShuffle,
        sfxPlayerBored,
        sfxPlayerWin,
        sfxPlayerLose,
        sfxPlayerSelect,
        sfxPlayerDeselect,
        sfxElevatorDoorOpen,
        sfxElevatorDoorClose,
        sfxElevatorDrive,
        sfxElevatorButton,
        sfxFootstepsPlayer,
        sfxCantUseItem,
        sfxEventSelect,
        sfxEventRerolled,
        sfxIncreasingBet,
        sfxDecreasingBet,
        sfxMaximumBet,
        sfxMinimumBet,
        sfxShopOpen,
        sfxShopClose,
        sfxShopBuy,
        sfxShopSell,
        sfxCantBuy,
        sfxCardSelectorPress,
        sfxCardSelectorPrint,
        sfxCashOut,
        sfxCashOutChampagneSpawn,
        sfxCashOutChampagnePop,
        sfxDealerTalk,
        sfxGenerelEvent,
        sfxDartRemoveEvent,
        sfxDartAddEvent,
        sfxDartEventHit,
        sfxDartEventBurn,
        sfxPowerBall,
        sfxPowerBallHit,
        sfxAceSetEventOne,
        sfxAceSetEventEleven,
        sfxNegativeSuitEvent,
        sfxRageEventStart,
        sfxRageEventIncrease,
        sfxRageEventThreshold,
        sfxJokerEvent,
        sfxRemoveSuitEvent,
        sfxRemoveValueEvent,
        sfxRouletteEvent,
        sfxRouletteEventNewTarget,
        sfxDoubleFiveBelowEvent,
        sfxHalveSixAboveEvent,
        sfxGenerelItem,
        sfxKnifeItem,
        sfxCrossItem,
        sfxCrossItemEffect,
        sfxScissorsItem,
        sfxScissorsItemPickup,
        sfxFanItem,
        sfxCigarettesItem,
        sfxNFTItem,
        sfxNFTItemEmitter,
        sfxOrganItem,
        sfxOrganItemExpire,
        sfxCoinItem,
        sfxAlcoholItem,
        sfxAlcoholItemEffect,
        sfxAcidItem,
        sfxAcidItemEmitter,
        sfxAcidItemPickup,
        sfxSunglassesItem,
        sfxKeepsakeGenerel,
        sfxBlueBallsKeepsake,
        sfxSnowGlobeKeepsake,
        sfx777Keepsake,
        sfxTrustFundKeepsake,
        sfxInheritanceKeepsake,
        sfxHattricKeepsake,
        sfxBloodPressureKeepsake,
        sfxGoFishKeepsake,
        sfxBloodRoseKeepsake,
        sfxOnyxRoseKeepsake,
        sfxGlassEyeKeepsake,
        sfxLoyalyCardKeepsake,
        sfxPyroKeepsake,
        sfxPrinterKeepsake,
        sfxSecondDealingKeepsake,
        sfxTarotKeepsake,
        sfxDivorcePaperKeepsake,
        sfxWeddingRingKeepsake,
        sfxHiddenAceKeepsake,
        sfxSprayCanKeepsake,
        sfxJointAccountKeepsake,
        sfxAntiMatterKeepsake,
        ambCasinoMain,
        ambElevatorMain,
        uiMenuButtonHover,
        uiMenuButtonPress,
        uiMenuStartGame,
        uiPauseGame,
        uiPauseResumeGame,
        uiPauseQuitGame,
        uiHoverIngameButton,
        musMainMenu,
        musElevator1,
        musMainGame,
        musWIn,
}

