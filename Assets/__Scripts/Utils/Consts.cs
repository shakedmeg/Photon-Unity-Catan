﻿using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;


public enum eBuildAction { None, Road, Settlement, City, Wall, Knight, UpgradeKnight, ActivateKnight, ImproveCity, Destroy, TakeImprovedCity }

public enum eBuilding { None, Road, Settlement, City, Wall, Knight, Knight2, Knight3 }

public enum eResources { Brick, Ore, Wheat, Wood, Wool, Desert = 100};
public enum eCommodity { None, Coin=5, Paper, Silk };
public enum eKnightActions { None, TakeAction, Move, MoveRobber, MoveKnight};
public enum GameState { SetupSettlement, SetupCity, Friendly, PreAttack, Playing };
public enum eCardsState { None, Throw, Trade, Give, Exchange, Take };

public enum ePorts { p2To1=2, p3To1, p4To1 };

public enum eResponses { None, True, False };

public enum eLongestRoadState { Game, Tie, Player}

public enum eDevelopmentCardsTypes { Alchemist, Crane, Engineer, Inventor, Irrigation, Medicine, Mining, RoadBuilding, Smith,
                                     Bishop, Deserter, Diplomat, Intrigue, Saboteur, Spy, Warlord, Wedding,
                                     CommercialHarbor, MasterMerchant, Merchant, MerchantFleet, ResourceMonopoly, TradeMonopoly,
                                     Printer, Constitution, None,
}

public enum RaiseEventsCode
{
    // GameManager
    SetRandomPlayer = 1,
    PassTurn,
    CheckImporveCity,
    ActivateRobber,
    SetLongestRoad,
    UpdatePointsForAll,
    GiveDevelopmentCard,
    ReturnDevelopmentCard,
    ReturnDevelopmentCardAfterUsage,
    FinishDevelopmentCardRollHandout,
    WinDevelopmentCard,




    // BuildManager
    SendMapData,            // TurnManager, CardManager, GameManager
    PreSetupSettlement,
    PreSetupCity,
    MatchTilesToDice,
    SendEntitledVertexes,
    UpdateVertexes,
    UpdateEdges,
    AddKnight,
    MoveKnight,
    RemoveKnight,
    DisplaceKnight,
    FinishMoveKnight,
    CheckIfCanLoseCity,
    LoseCity,
    ImproveCity,
    SetImproveCityID,
    TakeImproveCity,
    CheckIfNeedToPick,
    DeactivateAllKnights,
    CheckRoads,
    ChooseKnightToLose,
    BuildDesertedKnight,
    LoseRoad,
    RemoveRoad,
    SwitchProbs,


    // TurnManager
    StartTurn,
    ActivateBarbarians,
    GainTurnControl,
    FinishedThrowing,

    // CardManager
    SevenRolled,            // TurnManager
    FinishRollSeven,
    LoseCard,
    TakeCard,
    CompleteTrade,
    AddCachedCards,
    PickCard,
    Sabotage,
    CountDevCards,
    DevCardsCount,
    Spy,
    Wedding,
    CommercialHarbor,
    CountCommodities,   // this one will count them
    CommoditiesCount,   // this one will return the amout to the player
    CompleteCommercialHarborExchange,
    MasterMerchant,
    CountCards,
    CardsCount,
    CompleteMasterMerchant,
    LoseMerchant,
    ResourceMonopoly,
    TradeMonopoly,
    DeserveDevelopmentCard,
    SendDevelopmentCardFromRoll,
    SendDevelopmentCardFromWin,
    ChooseDevelopmentCard,

    // City
    LoseImproveCity,



    // Dice
    AddGreenPlayer,
    GreenPlayerResponse,
    FinishPickCard,
    SendDiceScore,

    // Barbarians
    FinishDevelopmentCardWinHandout,

    // Diplomat
    FinishDiplomat,

    // Intrigue
    FinishIntrigue,

    // Sabotague
    FinishSabotuer,

    // Wedding
    FinishWedding,

    // CommercialHarbor
    FinishCommercialHarbor,

    // MasterMerchant
    FinishMasterMerchant,

    // ResourceMonopoly
    FinishResourceMonopoly,

    // TradeMonopoly
    FinishTradeMonopoly,

    // PlayerSetup
    SetPlayerPanel,
    AddPoints,
    SetDevelopmentCard,
    SetDisplayCard,
    ActivateSpyPanel,
    ActivateLongestRoad,
    GameOver,


}
public class Consts
{
    #region Map

    public const int NUM_OF_CARDS = 19;
    public const int NumOfTiles = 19;
    public const int WaterTileNum = 18;

    public static List<string> Probabilitiys { get; } = new List<string>() { "5", "2", "6", "3", "8", "10", "9", "12", "11", "4", "8", "10", "9", "4", "5", "6", "3", "11" };
    public static Vector3 PROB_LOCAL_POSITION { get; } = new Vector3(0, 0, -2);

    public static Vector3 RobberLocalPosition { get; } = new Vector3(-2.7f, 0.15f, 0);
    public static Vector3 MerchantLocalPosition { get; } = new Vector3(0f, 1.6f, 2.7f);

    public static Dictionary<eResources, Color32> TILE_COLOR { get; } = new Dictionary<eResources, Color32>()
        {{eResources.Brick, new Color32(255, 47, 0, 255)},
        {eResources.Wood, new Color32(54, 166, 0, 255)},
        {eResources.Ore, new Color32(194, 205, 191, 255)},
        {eResources.Wheat, new Color32(255, 228, 0, 255)},
        {eResources.Wool, new Color32(40, 255, 16, 255)},
        {eResources.Desert, new Color32(195, 154, 35, 255)}};

    public const string CoinDevelopment = "CoinDevelopment";
    public const string PaperDevelopment = "PaperDevelopment";
    public const string SilkDevelopment = "SilkDevelopment";


    
    //public static Color32 CoinDevelopmentColor { get; } = new Color32(125, 197, 217,255);
    public static Color32 CoinDevelopmentColor { get; } = new Color32(6, 140, 251,255);
    public static Color32 PaperDevelopmentColor { get; } = new Color32(54, 166, 0,255);
    public static Color32 SilkDevelopmentColor { get; } = new Color32(231, 208, 10,255);
    //public static Color32 SilkDevelopmentColor { get; } = new Color32(250, 224, 103,255);

    public static List<Vector3> Quats { get; } = new List<Vector3>() {
        new Vector3(90, 0 , 0), new Vector3(0, 90, 0), new Vector3(0, 0, 0), new Vector3(180, 0, 0), new Vector3(0, 270, 0), new Vector3(270, 0, 0)
    };

    #endregion

    #region Player Room Variables

    public const string PLAYER_READY = "IsPlayerReady";

    public const string PLAYER_COLOR = "c";
    public const string ROOM_COLORS = "c"; // For custom room propeties usage
    public const string COLORS_OWNER = "o";
    public const string YELLOW = "Yellow";
    public const string RED = "Red";
    public const string BLUE = "Blue";
    public const string WHITE = "White";
    public const string BLACK = "Black";

    public static string[] COLORS_NAMES { get; } = new string[] { YELLOW, RED, BLUE, WHITE, BLACK };


    public static List<Dropdown.OptionData> COLORS_DROPDOWN { get; } = new List<Dropdown.OptionData>()
    {
        new ColorOptionData(YELLOW, Color.yellow),
        new ColorOptionData(RED, Color.red),
        new ColorOptionData(BLUE, Color.blue),
        new ColorOptionData(WHITE, Color.white),
        new ColorOptionData(BLACK, Color.black)
    };



    private static Dictionary<string, Color> color_name_to_color = new Dictionary<string, Color>()
    {
        { YELLOW, Color.yellow },
        { RED, Color.red },
        { BLUE, Color.blue },
        { WHITE, Color.white },
        { BLACK, Color.black },
    };

    public static Dictionary<string, Color> COLOR_NAME_TO_COLOR { get { return color_name_to_color; } }

    #endregion

    #region Resources

    public const string Brick = "Brick";
    public const string Wheat = "Wheat";
    public const string Wood = "Wood";
    public const string Ore = "Ore";
    public const string Wool = "Wool";

    #endregion

    #region Commodities

    public const string Paper = "Paper";
    public const string Coin = "Coin";
    public const string Silk = "Silk";

    #endregion

    #region Instantiation Dropdown Values

    public const int DROP_HIGHET = 11;
    public const int DropWall = -5;
    public const float RaiseHighetCity = 0.5f;
    #endregion

    #region Scaling

    public const float ScaleTime = 0.4f;

    public static Vector3 KnightRegularScale { get; } = new Vector3(1f, 1.5f, 1f);
    public static Vector3 Knight3RegularScale { get; } = new Vector3(1.2f, 1.5f, 1f);

    public static Vector3 ScaleKnight { get; } = new Vector3(1.5f, 2f, 1.5f);
    public static Vector3 ScaleKnight3 { get; } = new Vector3(1.7f, 2f, 1.5f);



    public static Vector3 RobberRegularScale { get; } = new Vector3(0.173f, 0.2f, 2f);
    public static Vector3 ScaleRobber { get; } = new Vector3(0.2f, 0.25f, 2.2f);

    public static Vector3 MerchantRegularScale { get; } = new Vector3(0.173f, 0.2f, 30f);

    public static Vector3 RoadRegularScale { get; } = new Vector3(1, 1, 3);
    public static Vector3 ScaleRoad { get; } = new Vector3(2, 1.1f, 5);
    public static Vector3 SettlementRegularScale { get; } = Vector3.one;
    public static Vector3 ScaleSettlement { get; } = new Vector3(1.4f, 1.4f, 1.4f);    
    public static Vector3 CityRegularScale { get; } = new Vector3(1f, 1f, 1.25f);
    public static Vector3 ScaleCity { get; } = new Vector3(1.3f, 1.3f, 1.5f);

    public static Vector3 DiceRegularScale { get; } = new Vector3(32.4f, 32.4f, 32.4f);
    public static Vector3 ScaleDice { get; } = new Vector3(38, 38, 38);


    #endregion

    #region Knight Activation
    public static Color KnightHeadActivated { get { return Color.gray; } }
    #endregion

    #region Buttons

    public const string BuildRoad = "BuildRoad";
    public const string BuildSettlement = "BuildSettlement";
    public const string BuildCity = "BuildCity";
    public const string BuildWall = "BuildWall";
    public const string BuildKnight = "BuildKnight";
    public const string UpgradeKnight = "UpgradeKnight";
    public const string ActivateKnight = "ActivateKnight";
    public const string UpgradeCity = "UpgradeCity";

    public const string RoadCleanUp = "RoadCleanUp";
    public const string SettlementCleanUp = "SettlementCleanUp";
    public const string CityCleanUp = "CityCleanUp";
    public const string WallCleanUp = "WallCleanUp";
    public const string KnightCleanUp = "KnightCleanUp";
    public const string UpgradeKnightCleanUp = "UpgradeKnightCleanUp";
    public const string ActivateKnightCleanUp = "ActivateKnightCleanUp";
    public const string UpgradeCityCleanUp = "UpgradeCityCleanUp";


    public static Dictionary<eBuildAction, Dictionary<eResources, int>> Prices { get; } = new Dictionary<eBuildAction, Dictionary<eResources, int>>()
    {
        {eBuildAction.Road, new Dictionary<eResources,int>() { { eResources.Brick , 1}, { eResources.Wood, 1 } } },
        {eBuildAction.Settlement, new Dictionary<eResources, int>() { { eResources.Brick, 1 }, { eResources.Wood, 1 }, { eResources.Wheat , 1}, { eResources.Wool, 1 } } },
        {eBuildAction.City, new Dictionary<eResources,int>() { { eResources.Ore, 3 }, { eResources.Wheat, 2 } } } ,
        {eBuildAction.Wall, new Dictionary<eResources,int>() { { eResources.Brick, 2} } } ,
        {eBuildAction.Knight, new Dictionary<eResources,int>() { { eResources.Ore, 1 }, { eResources.Wool, 1 } } },
        {eBuildAction.UpgradeKnight, new Dictionary<eResources,int>() { { eResources.Ore, 1 }, { eResources.Wool, 1 } } },
        {eBuildAction.ActivateKnight, new Dictionary<eResources, int>() { { eResources.Wheat, 1 } } },
    };

    #endregion

    #region Resources Folder Paths
    public const string CardsPath = "Cards/";
    public const string DisplayCardsPath = "Development Cards/Backgrounds And Points/";
    #endregion

    #region Offer Panel

    public const string CancelPanel = "TextPanel/CancelPanel";

    public const string PlayerButtonsPanel = "PlayerButtonsPanel";
    public const string ResponsesPanel = "PlayerButtonsPanel/ResponsesPanel";

    public const string OfferedContent = "OfferedContent";
    
    public const string RequestedContent = "RequestedContent";


    #endregion

    #region Ports

    public const string p2to1 = "2 : 1";
    public const string p3to1 = "3 : 1";
    public const string p4to1 = "4 : 1";

    public const string PortsFolder = "Ports/";

    #endregion

    #region PlayerPanelUI

    public const string Good = "Green";
    public const string Bad = "Bad";
    public const string LoseCity = "LoseCity";
    public const string LoseKnight = "LoseKnight";
    public const string DisplaceKnightIntrigue = "DisplaceKnightIntrigue";
    public const string DisplaceKnight = "DisplaceKnight";
    public const string Saboteur = "Saboteur";
    public const string Wedding = "Wedding";
    public const string CommercialHarbor = "CommercialHarbor";
    public const string MasterMerchantTaker = "MasterMerchantTaker";
    public const string MasterMerchantVictim = "MasterMerchantVictim";
    public const string Spy = "Spy";
    public const string SpyVictim = "SpyVictim";
    public const string ChooseDevCard = "Choosing Development Card";
    public const string ThrowDevCard = "Throwing Development Card";

    public const string Default = "Default";


    #endregion


    #region Development Cards
    public static Dictionary<eDevelopmentCardsTypes, string> DevelopmentCardsDesc { get; } = new Dictionary<eDevelopmentCardsTypes, string>()
    {
        { eDevelopmentCardsTypes.Alchemist, "This is the only progress card you can play before you roll the dice. It allows you to choose the results of both production dice. Then, roll the event die as normal and resolve the event." },
        { eDevelopmentCardsTypes.Crane, "You can build a city improvement (abbey, town hall, etc). for 1 commodity less than normal."},
        { eDevelopmentCardsTypes.Engineer, "You may build one city wall for free."},
        { eDevelopmentCardsTypes.Inventor, "Switch two number tokens of your choice, but not 2, 12, 6, or 8."},
        { eDevelopmentCardsTypes.Irrigation, "Collect 2 grain cards for each fields hex adjacent to at least one of your settlements or cities."},
        { eDevelopmentCardsTypes.Medicine, "You may upgrade a settlement to a city for 2 ore and 1 grain. When you play this card, you save 1 ore and 1 grain. You may not combine two of these cards for the same city."},
        { eDevelopmentCardsTypes.Mining, "Collect 2 ore cards for each mountains hex adjacent to at least one of your settlements or cities."},
        { eDevelopmentCardsTypes.RoadBuilding, "This card allows you to build 2 roads for free."},
        { eDevelopmentCardsTypes.Smith, "You may promote 2 of your knights 1 level each for free."},
        { eDevelopmentCardsTypes.Bishop, "Move the robber, following the normal rules. Draw 1 random resource/commodity card from each player who has a settlement or city next to the robber's new hex."},
        { eDevelopmentCardsTypes.Deserter, "Choose an opponent. He must remove 1 of his knights (his choice) from the board. You may then place 1 of your own knights, on the board. Its strength must equal to the knight removed (the normal rules for placing knights apply)."},
        { eDevelopmentCardsTypes.Diplomat, "You may remove an \"open\" road (without another road or other piece at one end). If you remove your own road, you may immediately place it somewhere else on the island (following all the normal building rules) for free."},
        { eDevelopmentCardsTypes.Intrigue, "You may displace an opponent’s knight. Theknight must be on an intersection connected to at least one of your roads."},
        { eDevelopmentCardsTypes.Saboteur, "When you play this card, each player who has as many or more victory points than you must discard half (round down) of his cards to the bank (resource and/or commodity cards)."},
        { eDevelopmentCardsTypes.Spy, "Look at another player's hand of progress cards. You may choose 1 card to take and add to your hand."},
        { eDevelopmentCardsTypes.Warlord, "You may activate all of your knights for free."},
        { eDevelopmentCardsTypes.Wedding, "Each of your opponents who has more victory points than you must give you 2 resource/commodity cards of his choice."},
        { eDevelopmentCardsTypes.CommercialHarbor, "You may force each of the other players to make a special trade. You may offer each opponent any 1 resource card from your hand. He must exchange it for any 1 commodity card of his choice from his hand, if he has any."},
        { eDevelopmentCardsTypes.MasterMerchant, "Choose another player who has more victory points than you do. Look at the player's hand of resource and commodity cards and choose 2 cards to take and add to your hand."},
        { eDevelopmentCardsTypes.Merchant, "Place the Merchant on any land hex next to 1 of your settlements or cities. You may exchange the resources produced by this type of hex with the supply at a 2:1 rate, as long as the merchant remains on that hex."},
        { eDevelopmentCardsTypes.MerchantFleet, "You may use one resource or commodity of your choice to make any number of 2:1 trades with the supply during the turn that you play this card."},
        { eDevelopmentCardsTypes.ResourceMonopoly, "Name a resource. Each player must give you 2 of that type of resource if they have them."},
        { eDevelopmentCardsTypes.TradeMonopoly, "Name a commodity. Each player must give you 1 commodity of that type if they have them."},
    };



    #endregion
}
