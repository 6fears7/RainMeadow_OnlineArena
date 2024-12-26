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

    private UIelement[] OnlineArenaSettings;


    public DrownOptions(global::Drown.DrownMod instance)
    {

        MaxCreatureCount = config.Bind("DrownMaxCreatures", 10);
        PointsForSpear = config.Bind("DrownPointsForSpear", 1);
        PointsForExplSpear = config.Bind("DrownPointsForExplSpear", 10);
        PointsForBomb = config.Bind("DrownPointsForBomb", 10);
        PointsForRespawn= config.Bind("DrownPointsForRespawn", 25);
        PointsForDenOpen = config.Bind("DrownPointsForDenOpen", 100);

    }

    public override void Initialize()
    {
        try
        {
            OpTab drownTab = new OpTab(this, "DROWN");
            Tabs = new OpTab[1] { drownTab };
            OnlineArenaSettings = new UIelement[13]
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
