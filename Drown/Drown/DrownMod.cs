using BepInEx;
using IL;
using RainMeadow;
using System;
using System.Security.Permissions;
using UnityEngine;

//#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace Drown
{
    [BepInPlugin("uo.drown", "Drown", "0.1.0")]
    public partial class DrownMod : BaseUnityPlugin
    {
        public static DrownMod instance;
        private bool init;
        private bool fullyInit;
        private bool addedMod = false;
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

                fullyInit = true;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                fullyInit = false;
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