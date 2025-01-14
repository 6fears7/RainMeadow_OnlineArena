﻿using Menu;
using System.Collections.Generic;
using UnityEngine;
using RainMeadow;
using System.Linq;

namespace Drown
{
    public class StoreOverlay : Menu.Menu
    {
        public AbstractCreature? foundMe;
        public Vector2 pos;



        public class ItemButton
        {
            public OnlinePhysicalObject player;
            public SimplerButton button;
            public bool mutedPlayer;
            private string clientMuteSymbol;
            public Dictionary<string, int> storeItems;
            public StoreOverlay overlay;
            public int cost;
            public string name;
            public bool didRespawn;
            public ItemButton(StoreOverlay menu, Vector2 pos, RainWorldGame game, ArenaOnlineGameMode arena, KeyValuePair<string, int> itemEntry, int index, bool canBuy = false)
            {
                this.overlay = menu;
                this.name = itemEntry.Key;
                this.cost = itemEntry.Value;
                this.button = new SimplerButton(menu, menu.pages[0], $"{itemEntry.Key}: {itemEntry.Value}", pos, new Vector2(110, 30));

                AbstractCreature me = null;

                this.button.OnClick += (_) =>
                {
                    AbstractPhysicalObject desiredObject = null;
                    for (int i = 0; i < game.GetArenaGameSession.Players.Count; i++)
                    {
                        if (OnlinePhysicalObject.map.TryGetValue(game.GetArenaGameSession.Players[i], out var onlineP) && onlineP.owner == OnlineManager.mePlayer)
                        {
                            me = game.GetArenaGameSession.Players[i];
                        }
                    }

                    switch (index)
                    {
                        case 0:
                            desiredObject = new AbstractSpear(game.world, null, me.pos, game.GetNewID(), false);
                            break;
                        case 1:
                            desiredObject = new AbstractSpear(game.world, null, me.pos, game.GetNewID(), true);
                            break;
                        case 2:
                            desiredObject = new AbstractPhysicalObject(game.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, me.pos, game.GetNewID());
                            break;

                        case 3:

                            didRespawn = false;
                            if (!didRespawn)
                            {
                                RevivePlayer(game.GetArenaGameSession, arena);
                                didRespawn = true;
                            }

                            break;
                        case 4:
                            DrownMode.openedDen = true;
                            DrownMode.iOpenedDen = true;
                            for (int j = 0; j < arena.arenaSittingOnlineOrder.Count; j++)
                            {
                                var currentPlayer = ArenaHelpers.FindOnlinePlayerByFakePlayerNumber(arena, j);
                                if (!currentPlayer.isMe)
                                {
                                    currentPlayer.InvokeOnceRPC(DrownModeRPCs.Arena_OpenDen, DrownMode.openedDen);
                                }
                            }
                            game.cameras[0].hud.PlaySound(SoundID.UI_Multiplayer_Player_Revive);

                            break;
                    }

                    if (desiredObject != null && me != null)
                    {
                        (game.cameras[0].room.abstractRoom).AddEntity(desiredObject);
                        desiredObject.RealizeInRoom();
                    }
                    DrownMode.currentPoints = DrownMode.currentPoints - itemEntry.Value;
                    didRespawn = false;

                };
                this.button.owner.subObjects.Add(button);
            }

            public void Destroy()
            {
                this.button.RemoveSprites();
                this.button.page.RemoveSubObject(this.button);
            }
        }

        public RainWorldGame game;
        public List<ItemButton> storeItemList;
        ItemButton itemButtons;
        public DrownMode drown;

        public StoreOverlay(ProcessManager manager, RainWorldGame game, DrownMode drown, ArenaOnlineGameMode arena) : base(manager, RainMeadow.RainMeadow.Ext_ProcessID.SpectatorMode)
        {
            this.game = game;
            this.drown = drown;
            this.pages.Add(new Page(this, null, "store", 0));
            this.selectedObject = null;
            this.storeItemList = new();
            this.pos = new Vector2(180, 553);
            this.pages[0].subObjects.Add(new Menu.MenuLabel(this, this.pages[0], this.Translate("STORE"), new Vector2(pos.x, pos.y + 30f), new Vector2(110, 30), true));
            var storeItems = new Dictionary<string, int> {
            { "Spear", DrownMode.spearCost },
            { "Explosive Spear", DrownMode.spearExplCost },
            { "Scavenger Bomb", DrownMode.bombCost },
            { "Respawn", DrownMode.respCost },
            { "Open Dens", DrownMode.denCost},


        };
            int index = 0; // Initialize an index variable

            foreach (var item in storeItems)
            {
                // Format the message for the button, for example: "Spear: 1"
                string buttonMessage = $"{item.Key}: {item.Value}";

                // Create a new ItemButton for each dictionary entry
                this.itemButtons = new ItemButton(this, pos, game, arena, item, index, true);
                this.storeItemList.Add(itemButtons);


                pos.y -= 40; // Move the button 40 units down for the next one
                index++;
            }

        }

        public override void Update()
        {
            base.Update();
            for (int p = 0; p < game.Players.Count; p++)
            {
                if (OnlinePhysicalObject.map.TryGetValue(game.Players[p], out var onlineC))
                {

                    if (onlineC.owner == OnlineManager.mePlayer)
                    {
                        foundMe = game.Players[p];
                    }

                }
                else
                {
                    foundMe = null;
                }
            }
            if (storeItemList != null)
            {
                for (int i = 0; i < storeItemList.Count; i++)
                {
                    storeItemList[i].button.buttonBehav.greyedOut = true;

                    if (storeItemList[i].name == "Respawn")
                    {
                        if (foundMe is not null && foundMe.state.alive || DrownMode.openedDen)
                        {
                            storeItemList[i].button.buttonBehav.greyedOut = true;

                        }
                        else
                        {
                            storeItemList[i].button.buttonBehav.greyedOut = DrownMode.currentPoints < storeItemList[i].cost;
                        }
                    }

                    if (storeItemList[i].name == "Open Dens" && !DrownMode.openedDen)
                    {
                        storeItemList[i].button.buttonBehav.greyedOut = DrownMode.currentPoints < storeItemList[i].cost;
                    }

                    if (foundMe != null && !DrownMode.openedDen && (storeItemList[i].name != "Respawn" && storeItemList[i].name != "Open Dens"))
                    {
                        storeItemList[i].button.buttonBehav.greyedOut = DrownMode.currentPoints < storeItemList[i].cost;
                    }


                }
                if (foundMe != null)
                {
                    if (Input.GetKeyDown(DrownMod.drownOptions.StoreItem1.Value) && DrownMode.currentPoints >= storeItemList[0].cost && !DrownMode.openedDen)
                    {
                        storeItemList[0].button.Clicked();
                    }
                    if (Input.GetKeyDown(DrownMod.drownOptions.StoreItem2.Value) && DrownMode.currentPoints >= storeItemList[1].cost && !DrownMode.openedDen)
                    {
                        storeItemList[1].button.Clicked();
                    }
                    if (Input.GetKeyDown(DrownMod.drownOptions.StoreItem3.Value) && DrownMode.currentPoints >= storeItemList[2].cost && !DrownMode.openedDen)
                    {
                        storeItemList[2].button.Clicked();
                    }
                }
                if (Input.GetKeyDown(DrownMod.drownOptions.StoreItem4.Value) && DrownMode.currentPoints >= storeItemList[3].cost && !DrownMode.openedDen && (foundMe == null || foundMe.state.dead))
                {
                    storeItemList[3].button.Clicked();
                }
                if (Input.GetKeyDown(DrownMod.drownOptions.StoreItem5.Value) && DrownMode.currentPoints >= storeItemList[4].cost && !DrownMode.openedDen)
                {
                    storeItemList[4].button.Clicked();
                }
            }
        }

        private static void RevivePlayer(ArenaGameSession game, ArenaOnlineGameMode arena)
        {


            List<int> exitList = new List<int>();

            for (int i = 0; i < game.room.world.GetAbstractRoom(0).exits; i++)
            {
                exitList.Add(i);
            }

            arena.avatars.Clear();
            arena.onlineArenaGameMode.SpawnPlayer(arena, game, game.room, exitList);


        }

    }
}