using BepInEx;
using IL;
using RainMeadow;
using System;
using System.Security.Cryptography;
using System.Security.Permissions;
using UnityEngine;

//#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace Drown
{
    [BepInPlugin("henpemaz.tag", "Tag", "0.1.0")]
    public partial class Drown : BaseUnityPlugin
    {
        public static Drown instance;
        private bool init;
        private bool fullyInit;

        public static RainMeadow.OnlineGameMode.OnlineGameModeType tagGameMode = new("Tag", true);
        public static ProcessManager.ProcessID TagMenu = new("TagMenu", true);

        public static HideTimer hideTimer;

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

                On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;


                fullyInit = true;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                fullyInit = false;
                //throw;
            }
        }

        private void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig(self, cam);
            if (OnlineManager.lobby != null && OnlineManager.lobby.gameMode is TagGameMode tgm)
            {
                hideTimer = new HideTimer(self, self.fContainers[0], tgm);
                self.AddPart(hideTimer);  // Add timer to HUD system
            }
        }
    }
}