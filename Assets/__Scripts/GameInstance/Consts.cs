using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum eBuilding { None, Road, Settlement, City, Wall, Knight, UpgradeKnight, ActivateKnight, ImproveCity, Knight2, Knight3, Destroy, TakeImprovedCity }
public enum eResources { Brick, Ore, Wheat, Wood, Wool, Desert = 100};
public enum eCommodity { None, Coin=5, Paper, Silk };
public enum eKnightActions { None, TakeAction, Move, MoveRobber, MoveKnight};
public enum GameState { SetupSettlement, SetupCity, Friendly, PreAttack, Playing };
public enum eCardsState { None, Throw, Trade, Robber};

public enum ePorts { p2To1=2, p3To1, p4To1 };

public enum eResponses { None, True, False };

public enum RaiseEventsCode
{
    // GameManager
    SetRandomPlayer = 1,
    PassTurn,
    FinishedThrowing,
    CheckImporveCity,



    // BuildManager
    SendMapData,
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


    // TurnManager
    StartTurn,
    ActivateBarbarians,

    // CardManager
    SevenRolled,
    FinishRollSeven,
    LoseCard,
    TakeCard,
    CompleteTrade,

    // City
    LoseImproveCity,


}
public class Consts
{
    #region Map

    public const int NUM_OF_CARDS = 19;
    public const int NumOfTiles = 19;

    public static List<string> Probabilitiys { get; } = new List<string>() { "5", "2", "6", "3", "8", "10", "9", "12", "11", "4", "8", "10", "9", "4", "5", "6", "3", "11" };

    public static Vector3 PROB_LOCAL_POSITION { get; } = new Vector3(0, 0, -2);

    public static Vector3 RobberLocalPosition { get; } = new Vector3(-2.7f, 0.15f, 0);

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

    public static Color32 CoinDevelopmentColor { get; } = new Color32(1, 39, 255,255);
    public static Color32 PaperDevelopmentColor { get; } = new Color32(54, 166, 0,255);
    public static Color32 SilkDevelopmentColor { get; } = new Color32(255, 250, 8,255);

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

    public static Vector3 ScaleKnight { get; } = new Vector3(1.2f, 1.7f, 1.2f);
    public static Vector3 ScaleKnight3 { get; } = new Vector3(1.4f, 1.7f, 1.2f);

    public static Vector3 KnightRegularScale { get; } = new Vector3(1f, 1.5f, 1f);
    public static Vector3 Knight3RegularScale { get; } = new Vector3(1.2f, 1.5f, 1f);


    public static Vector3 RobberRegularScale { get; } = new Vector3(0.173f, 0.2f, 2f);
    public static Vector3 ScaleRobber { get; } = new Vector3(0.2f, 0.25f, 2.2f);

    public static Vector3 CityRegularScale { get; } = new Vector3(1f, 1f, 1.25f);
    public static Vector3 ScaleCity { get; } = new Vector3(1.3f, 1.3f, 1.5f);

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


    public static Dictionary<eBuilding, Dictionary<eResources, int>> Prices { get; } = new Dictionary<eBuilding, Dictionary<eResources, int>>()
    {
        {eBuilding.Road, new Dictionary<eResources,int>() { { eResources.Brick , 1}, { eResources.Wood, 1 } } },
        {eBuilding.Settlement, new Dictionary<eResources, int>() { { eResources.Brick, 1 }, { eResources.Wood, 1 }, { eResources.Wheat , 1}, { eResources.Wool, 1 } } },
        {eBuilding.City, new Dictionary<eResources,int>() { { eResources.Ore, 3 }, { eResources.Wheat, 2 } } } ,
        {eBuilding.Wall, new Dictionary<eResources,int>() { { eResources.Brick, 2} } } ,
        {eBuilding.Knight, new Dictionary<eResources,int>() { { eResources.Ore, 1 }, { eResources.Wool, 1 } } },
        {eBuilding.UpgradeKnight, new Dictionary<eResources,int>() { { eResources.Ore, 1 }, { eResources.Wool, 1 } } },
        {eBuilding.ActivateKnight, new Dictionary<eResources, int>() { { eResources.Wheat, 1 } } },
    };

    #endregion


    #region Resources Folder Paths
    public const string CardsPath = "Cards/";
    #endregion

    #region Offer Panel

    public const string CancelPanel = "TextPanel/CancelPanel";

    public const string PlayerButtonsPanel = "PlayerButtonsPanel";
    public const string ResponsesPanel = "PlayerButtonsPanel/ResponsesPanel";

    public const string OfferedContent = "OfferedContent";
    
    public const string RequestedContent = "RequestedContent";


    #endregion

}
