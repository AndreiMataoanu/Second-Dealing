using System;
using System.IO; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Globalization;
using System.Threading;

public class Telemetry : MonoBehaviour
{
    public struct TelemetryData
    {
        public int round;
        public List<Item> itemsInShop;
        public List<Item> itemsPurchased;
        public List<Item> itemsUsed;
        public List<BlackjackEvent> eventsTriggered;
        public int hand;
        public int moneyBet;
        public int totalMoney;
        public bool doubleDown;
        public int split;
        public string winLossTie;
    }

    public const string GoogleFormBaseUrl = "https://docs.google.com/forms/d/e/1FAIpQLScy8rKbkU5HVRV8-eTIpOeKmfiJTACwI1PF1sP8DJJftMc9_w/";

    private const string _gform_id_user = "entry.1053518294";

    private const string _gform_id_run = "entry.1992830053";

    private const string _gform_round = "entry.1215205545";

    private const string _gform_itemsInShop = "entry.2125854374";

    private const string _gform_itemsPurchased = "entry.1360314878";

    private const string _gform_itemsUsed = "entry.1566169386";

    private const string _gform_eventsTriggered = "entry.248122375";

    private const string _gform_hand = "entry.719586169";

    private const string _gform_moneyBet = "entry.359642555";

    private const string _gform_totalMoney = "entry.2020204161";

    private const string _gform_doubleDown = "entry.1095399089";
    private const string _gform_split = "entry.331154695";

    private const string _gform_winLossTie = "entry.309659681";

    private static Guid runId;
    private static Guid userId;
    public static IEnumerator SubmitGoogleForm(TelemetryData data)
    {
        CultureInfo ci = CultureInfo.GetCultureInfo("en-GB");
        Thread.CurrentThread.CurrentCulture = ci;

        string urlGoogleFormResponse = GoogleFormBaseUrl + "formResponse";

        WWWForm form = new WWWForm();

        form.AddField(_gform_id_user,GUIDToShortString(userId));
        form.AddField(_gform_id_run,GUIDToShortString(runId));
        form.AddField(_gform_round,data.round);
        form.AddField(_gform_itemsInShop,ItemlistToString(data.itemsInShop));
        form.AddField(_gform_itemsPurchased,ItemlistToString(data.itemsPurchased));
        form.AddField(_gform_itemsUsed,ItemlistToString(data.itemsUsed));
        form.AddField(_gform_eventsTriggered,EventlistToString(data.eventsTriggered));
        form.AddField(_gform_hand,data.hand);
        form.AddField(_gform_moneyBet,data.moneyBet);
        form.AddField(_gform_totalMoney,data.totalMoney);
        form.AddField(_gform_doubleDown,data.doubleDown.ToString());
        form.AddField(_gform_split,data.split);
        form.AddField(_gform_winLossTie,data.winLossTie);
        using(UnityWebRequest www = UnityWebRequest.Post(urlGoogleFormResponse,form))
        {
            yield return www.SendWebRequest();
              if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + www.error);
                }
                else
                {
                    Debug.Log("Success: " + www.downloadHandler.text);
                }
            //yield return null;
        }
    }
    
    public static void GenerateNewRunID()
    {
        runId = Guid.NewGuid();
    }
    public static void GenerateNewUserID()
    {
        userId = Guid.NewGuid();
    }

    public static string GUIDToShortString(Guid guid)
    {
        var base64Guid = Convert.ToBase64String(guid.ToByteArray());

        // Replace URL unfriendly characters with better ones
        base64Guid = base64Guid.Replace('+', '-').Replace('/', '_');

        // Remove the trailing ==
        return base64Guid.Substring(0, base64Guid.Length - 2);
    }

public static string ItemlistToString(List<Item> list)
{
    string itemString = "";
    foreach (Item item in list) 
    {
        itemString += item.type.ToString() + " "; // append instead of overwrite
    }

    return itemString;
}
public static string EventlistToString(List<BlackjackEvent> list)
{
    string eventString = "";
    foreach (BlackjackEvent blackjackEvent in list) 
    {
        eventString += blackjackEvent.eventName + " "; // append instead of overwrite
    }

    return eventString;
}


}
