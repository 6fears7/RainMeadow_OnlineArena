using Menu.Remix.MixedUI;
using System;
using UnityEngine;
public class DrownOptions : OptionInterface
{

    public readonly Configurable<int> MaxCreatureCount;
    public readonly Configurable<int> PointsForSpear;
    public readonly Configurable<int> PointsForExplSpear;
    public readonly Configurable<int> PointsForBomb;
    public readonly Configurable<int> PointsForRespawn;
    public readonly Configurable<int> PointsForDenOpen;

    public readonly Configurable<KeyCode> StoreItem1;
    public readonly Configurable<KeyCode> StoreItem2;
    public readonly Configurable<KeyCode> StoreItem3;
    public readonly Configurable<KeyCode> StoreItem4;
    public readonly Configurable<KeyCode> StoreItem5;


    private UIelement[] OnlineArenaSettings;


    public DrownOptions(global::Drown.DrownMod instance)
    {

        MaxCreatureCount = config.Bind("DrownMaxCreatures", 10);
        PointsForSpear = config.Bind("DrownPointsForSpear", 1);
        PointsForExplSpear = config.Bind("DrownPointsForExplSpear", 10);
        PointsForBomb = config.Bind("DrownPointsForBomb", 10);
        PointsForRespawn= config.Bind("DrownPointsForRespawn", 25);
        PointsForDenOpen = config.Bind("DrownPointsForDenOpen", 100);

        StoreItem1 = config.Bind("DrownStoreItem1", KeyCode.Alpha1);
        StoreItem2 = config.Bind("DrownStoreItem2", KeyCode.Alpha2);
        StoreItem3 = config.Bind("DrownStoreItem3", KeyCode.Alpha3);
        StoreItem4 = config.Bind("DrownStoreItem4", KeyCode.Alpha4);
        StoreItem5 = config.Bind("DrownStoreItem5", KeyCode.Alpha5);
    }

    public override void Initialize()
    {
        try
        {
            OpTab drownTab = new OpTab(this, "DROWN");
            Tabs = new OpTab[1] { drownTab };
            OnlineArenaSettings = new UIelement[23]
            {
                new OpLabel(10f, 550f, "DROWN", bigText: true),
                new OpLabel(10f, 505, "Max creatures in level", bigText: false),
                new OpTextBox(MaxCreatureCount, new Vector2(10, 480), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },

                new OpLabel(10f, 460, "Points required to buy a spear", bigText: false),
                new OpTextBox(PointsForSpear, new Vector2(10, 435), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },
                new OpLabel(10f, 410, "Points required to buy an explosive spear", bigText: false),
                new OpTextBox(PointsForExplSpear, new Vector2(10, 385), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },
               new OpLabel(10f, 365, "Points required to buy a scav bomb", bigText: false),
                new OpTextBox(PointsForBomb, new Vector2(10, 340), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },
                new OpLabel(10f, 315, "Points required to buy a respawn", bigText: false),
                new OpTextBox(PointsForRespawn, new Vector2(10, 290), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },
                new OpLabel(10f, 265, "Points required to open dens", bigText: false),
                new OpTextBox(PointsForDenOpen, new Vector2(10, 240), 160f)
                {
                    accept = OpTextBox.Accept.Int
                },

                new OpLabel(260, 505, "Key used to activate store item #1", bigText: false),
                new OpKeyBinder(StoreItem1, new Vector2(260, 475), new Vector2(150f, 30f)),

                new OpLabel(260, 460, "Key used to activate store item #2", bigText: false),
                new OpKeyBinder(StoreItem2, new Vector2(260, 430), new Vector2(150f, 30f)),

                new OpLabel(260, 410, "Key used to activate store item #3", bigText: false),
                new OpKeyBinder(StoreItem3, new Vector2(260, 380), new Vector2(150f, 30f)),

                new OpLabel(260, 365, "Key used to activate store item #4", bigText: false),
                new OpKeyBinder(StoreItem4, new Vector2(260, 340), new Vector2(150f, 30f)),

                new OpLabel(260, 315, "Key used to activate store item #5", bigText: false),
                new OpKeyBinder(StoreItem5, new Vector2(260, 290), new Vector2(150f, 30f)),


        };
            drownTab.AddItems(OnlineArenaSettings);

        }
        catch (Exception ex)
        {
            RainMeadow.RainMeadow.Error("Error opening Drown Options Menu" + ex);
        }
    }

    public override void Update()
    {
    }
}
