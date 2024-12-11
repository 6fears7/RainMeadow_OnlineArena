using BepInEx;
using IL;
using RainMeadow;
using System;
using System.Linq;
using System.Security.Permissions;
using UnityEngine;

//#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = false)]
namespace Drown
{
    [BepInPlugin("uo.drown", "Drown", "0.1.0")]
    public partial class DrownMod : BaseUnityPlugin
    {
        public static DrownMod instance;
        private bool init;
        private bool fullyInit;
        private bool addedMod = false;
        private bool showedUserStoreMessage = false;
        public void OnEnable()
        {
            instance = this;

            On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (init) return;
            init = true;

            try
            {

                On.Menu.MultiplayerMenu.ctor += MultiplayerMenu_ctor;
                On.HUD.TextPrompt.AddMessage_string_int_int_bool_bool += TextPrompt_AddMessage_string_int_int_bool_bool;

                fullyInit = true;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                fullyInit = false;
            }
        }

        private void TextPrompt_AddMessage_string_int_int_bool_bool(On.HUD.TextPrompt.orig_AddMessage_string_int_int_bool_bool orig, HUD.TextPrompt self, string text, int wait, int time, bool darken, bool hideHud)
        {
            if (RainMeadow.RainMeadow.isArenaMode(out var arena) && arena.onlineArenaGameMode == arena.registeredGameModes.FirstOrDefault(kvp => kvp.Value == DrownMode.Drown.value).Key)
            {
                text = text + $" - Press {RainMeadow.RainMeadow.rainMeadowOptions.SpectatorKey.Value} to access the store";
                orig(self, text, wait, time, darken, hideHud);
            } else
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