﻿using BepInEx;
using RainMeadow;
using System;
using System.Linq;
using System.Security.Permissions;

//#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace Drown
{
    [BepInPlugin("uo.drown", "Drown", "0.1.0")]
    public partial class DrownMod : BaseUnityPlugin
    {
        public static DrownOptions drownOptions;
        public static DrownMod instance;
        private bool init;
        private bool fullyInit;
        private bool addedMod = false;
        private bool showedUserStoreMessage = false;
        public void OnEnable()
        {
            instance = this;
            drownOptions = new DrownOptions(this);


            On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (init) return;
            init = true;

            try
            {
                MachineConnector.SetRegisteredOI("uo_drown", drownOptions);

                On.Menu.MultiplayerMenu.ctor += MultiplayerMenu_ctor;
                On.HUD.TextPrompt.AddMessage_string_int_int_bool_bool += TextPrompt_AddMessage_string_int_int_bool_bool;
                On.Creature.Violence += Creature_Violence;
                On.Lizard.Violence += Lizard_Violence;
                On.CompetitiveGameSession.ShouldSessionEnd += CompetitiveGameSession_ShouldSessionEnd;


                fullyInit = true;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                fullyInit = false;
            }
        }



        private bool CompetitiveGameSession_ShouldSessionEnd(On.CompetitiveGameSession.orig_ShouldSessionEnd orig, CompetitiveGameSession self)
        {
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && arena.onlineArenaGameMode == arena.registeredGameModes.FirstOrDefault(kvp => kvp.Value == DrownMode.Drown.value).Key)
            {
                if (self.GameTypeSetup.spearsHitPlayers) // Competitive
                {
                    if (DrownMode.currentPoints >= DrownMode.respCost && !DrownMode.openedDen) // We can still respawn
                    {
                        return false;
                    }

                    var alivePlayers = self.Players.Where(player => player.state.alive);
                    var myPlayer = self.Players.Find(player => player.state.alive && OnlinePhysicalObject.map.TryGetValue(player, out var onlineP) && onlineP.owner == OnlineManager.mePlayer);

                    if (DrownMode.openedDen && !DrownMode.iOpenedDen && myPlayer != null && myPlayer.realizedCreature != null)
                    {
                        myPlayer.realizedCreature.Die();
                    }
                    return alivePlayers.Any(player => self.exitManager.playersInDens.Any(denPlayer => denPlayer.creature.abstractCreature == player)) || !self.Players.Any(player => player.state.alive); // Dens are opened and we have no money. Did anyone beat us there or is everyone dead?

                }

                if (!self.GameTypeSetup.spearsHitPlayers) // Cooperative
                {
                    if (DrownMode.currentPoints >= DrownMode.respCost && !DrownMode.openedDen) // We can still respawn
                    {
                        return false;
                    }
                    var alivePlayers = self.Players.Where(player => player.state.alive);
                    return alivePlayers.All(player => self.exitManager.playersInDens.Any(denPlayer => denPlayer.creature.abstractCreature == player)) || !self.Players.Any(player => player.state.alive);
                }
            }
            return orig(self);


        }

        private void Lizard_Violence(On.Lizard.orig_Violence orig, Lizard self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos onAppendagePos, Creature.DamageType type, float damage, float stunBonus)
        {
            orig(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
            if (self.State.dead)
            {
                return;
            }
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && arena.onlineArenaGameMode == arena.registeredGameModes.FirstOrDefault(kvp => kvp.Value == DrownMode.Drown.value).Key)
            {

                var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
                if (game.manager.upcomingProcess != null)
                {
                    return;
                }
                foreach (var abs in game.GetArenaGameSession.Players)
                {
                    if (abs == self.killTag && OnlinePhysicalObject.map.TryGetValue(abs, out var onlinePlayer) && onlinePlayer.owner == OnlineManager.mePlayer) //  Me. I killed them.
                    {
                        DrownMode.currentPoints++;
                    }
                }
                for (int i = 0; i < arena.arenaSittingOnlineOrder.Count; i++)
                {
                    var currentPlayer = ArenaHelpers.FindOnlinePlayerByFakePlayerNumber(arena, i);
                    if (!currentPlayer.isMe)
                    {
                        currentPlayer.InvokeOnceRPC(DrownModeRPCs.Arena_IncrementPlayerScore, DrownMode.currentPoints, OnlineManager.mePlayer.inLobbyId);
                    }
                    else
                    {

                        var playerWhoScored = ArenaHelpers.FindOnlinePlayerNumber(arena, currentPlayer);
                        game.GetArenaGameSession.arenaSitting.players[playerWhoScored].score = DrownMode.currentPoints;
                    }
                }

            }
        }

        private void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
            if (self.State.dead)
            {
                return;
            }
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && arena.onlineArenaGameMode == arena.registeredGameModes.FirstOrDefault(kvp => kvp.Value == DrownMode.Drown.value).Key)
            {
                var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
                if (game.manager.upcomingProcess != null)
                {
                    return;
                }
                foreach (var abs in game.GetArenaGameSession.Players)
                {
                    if (abs == self.killTag && OnlinePhysicalObject.map.TryGetValue(abs, out var onlinePlayer) && onlinePlayer.owner == OnlineManager.mePlayer) //  Me. I killed them.
                    {
                        DrownMode.currentPoints++;
                    }
                }
                for (int i = 0; i < arena.arenaSittingOnlineOrder.Count; i++)
                {
                    var currentPlayer = ArenaHelpers.FindOnlinePlayerByFakePlayerNumber(arena, i);
                    if (!currentPlayer.isMe)
                    {
                        currentPlayer.InvokeOnceRPC(DrownModeRPCs.Arena_IncrementPlayerScore, DrownMode.currentPoints, OnlineManager.mePlayer.inLobbyId);
                    }
                    else
                    {

                        var playerWhoScored = ArenaHelpers.FindOnlinePlayerNumber(arena, currentPlayer);
                        game.GetArenaGameSession.arenaSitting.players[playerWhoScored].score = DrownMode.currentPoints;
                    }
                }

            }


        }

        private void TextPrompt_AddMessage_string_int_int_bool_bool(On.HUD.TextPrompt.orig_AddMessage_string_int_int_bool_bool orig, HUD.TextPrompt self, string text, int wait, int time, bool darken, bool hideHud)
        {
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && arena.onlineArenaGameMode == arena.registeredGameModes.FirstOrDefault(kvp => kvp.Value == DrownMode.Drown.value).Key)
            {
                text = text + $" - Press {drownOptions.OpenStore.Value} to access the store";
                orig(self, text, wait, time, darken, hideHud);
            }
            else
            {
                orig(self, text, wait, time, darken, hideHud);
            }

        }

        private void MultiplayerMenu_ctor(On.Menu.MultiplayerMenu.orig_ctor orig, Menu.MultiplayerMenu self, ProcessManager manager)
        {
            if (RainMeadow.RainMeadow.isArenaMode(out var arena))
            {
                var drown = new DrownMode();
                if (!arena.registeredGameModes.ContainsKey(drown))
                {
                    arena.registeredGameModes.Add(new DrownMode(), DrownMode.Drown.value);
                }
            }
            orig(self, manager);


        }
    }
}