using RainMeadow;
using System.Linq;

namespace Drown
{
    public class DrownMode : ExternalArenaGameMode
    {

        public static ArenaSetup.GameTypeID Drown = new ArenaSetup.GameTypeID("Drown", register: true);


        public bool isInStore = false;

        public static int currentPoints;
        public static bool openedDen = false;

        private int _timerDuration;
        private int waveStart = 1200;
        private int currentWaveTimer = 1200;
        private int currentWave = 0;
        private int lastCleanupWave = 0;

        public override bool IsExitsOpen(ArenaOnlineGameMode arena, On.ArenaBehaviors.ExitManager.orig_ExitsOpen orig, ArenaBehaviors.ExitManager self)
        {
            return openedDen;

        }

        public override bool SpawnBatflies(FliesWorldAI self, int spawnRoom)
        {
            return true;
        }

        public override void ArenaSessionCtor(ArenaOnlineGameMode arena, On.ArenaGameSession.orig_ctor orig, ArenaGameSession self, RainWorldGame game)
        {
            DrownMode.openedDen = false;
            currentWave = 1;
            currentPoints = 5;
            lastCleanupWave = 0;
            foreach (var player in self.arenaSitting.players)
            {
                player.score = currentPoints;
            }
        }

        public override void InitAsCustomGameType(ArenaSetup.GameTypeSetup self)
        {

            self.foodScore = 1;
            self.survivalScore = 0;
            self.spearHitScore = 1;
            self.repeatSingleLevelForever = false;
            self.denEntryRule = ArenaSetup.GameTypeSetup.DenEntryRule.Standard;
            self.rainWhenOnePlayerLeft = false;
            self.levelItems = true;
            self.fliesSpawn = true;
            self.saveCreatures = false;

        }

        public override string TimerText()
        {
            var waveTimer = ArenaPrepTimer.FormatTime(currentWaveTimer);
            return $": Current points: {currentPoints}. Current Wave: {currentWave}. Next wave: {waveTimer}";
        }

        public override int SetTimer(ArenaOnlineGameMode arena)
        {
            return arena.setupTime = 1;
        }

        public override void ResetGameTimer()
        {
            _timerDuration = 1;

        }

        public override int TimerDuration
        {
            get { return _timerDuration; }
            set { _timerDuration = value; }
        }

        public override int TimerDirection(ArenaOnlineGameMode arena, int timer)
        {
            if (!openedDen)
            {
                return ++timer;
            }
            else
            {
                return timer;
            }
        }

        public override void LandSpear(ArenaOnlineGameMode arena, ArenaGameSession self, Player player, Creature target, ArenaSitting.ArenaPlayer aPlayer)
        {

        }

        public override void HUD_InitMultiplayerHud(ArenaOnlineGameMode arena, HUD.HUD self, ArenaGameSession session)
        {
            base.HUD_InitMultiplayerHud(arena, self, session);
            self.AddPart(new StoreHUD(self, session.game.cameras[0], this));
        }

        public override bool HoldFireWhileTimerIsActive(ArenaOnlineGameMode arena)
        {
            return arena.countdownInitiatedHoldFire = false;
        }
        public override string AddCustomIcon(ArenaOnlineGameMode arena, PlayerSpecificOnlineHud hud)
        {
            if (isInStore)
            {
                return "spearSymbol";

            }
            else
            {
                return base.AddCustomIcon(arena, hud);
            }
        }

        public override void ArenaSessionUpdate(ArenaOnlineGameMode arena, ArenaGameSession session)
        {

            if (!session.sessionEnded)
            {
                for (int i = 0; i < session.Players.Count; i++)
                {
                    if (!OnlinePhysicalObject.map.TryGetValue(session.Players[i], out _))
                    {
                        if (session.Players[i].state.alive) // alive and without an owner? Die
                        {
                            session.Players[i].Die();
                        }
                        session.Players.RemoveAt(i);
                    }

                    if (session.GameTypeSetup.spearsHitPlayers)
                    {
                        if (session.exitManager.IsPlayerInDen(session.Players[i]))
                        {
                            session.EndSession();
                        }
                    }
                }
            }

            if (!openedDen)
            {
                currentWaveTimer--;

                if (currentWaveTimer == 0)
                {
                    currentWaveTimer = waveStart;
                }
                if (currentWaveTimer % waveStart == 0)
                {
                    session.SpawnCreatures();
                    currentWave++;
                }
                if (currentWave % 3 == 0 && currentWave > lastCleanupWave)
                {
                    lastCleanupWave = currentWave;

                    CreatureCleanup(arena, session);
                }
            }

        }

        private void CreatureCleanup(ArenaOnlineGameMode arena, ArenaGameSession session)
        {
            if (RoomSession.map.TryGetValue(session.room.abstractRoom, out var roomSession))
            {
                var entities = session.room.abstractRoom.entities;
                for (int i = entities.Count - 1; i >= 0; i--)
                {
                    if (entities[i] is AbstractPhysicalObject apo && apo is AbstractCreature ac && ac.state.dead && ac.creatureTemplate.type != CreatureTemplate.Type.Slugcat && OnlinePhysicalObject.map.TryGetValue(apo, out var oe))
                    {
                        oe.apo.LoseAllStuckObjects();
                        if (!oe.isMine)
                        {
                            oe.beingMoved = true;

                            if (oe.apo.realizedObject is Creature c && c.inShortcut)
                            {
                                if (c.RemoveFromShortcuts()) c.inShortcut = false;
                            }

                            entities.Remove(oe.apo);

                            session.room.abstractRoom.creatures.Remove(oe.apo as AbstractCreature);

                            session.room.RemoveObject(oe.apo.realizedObject);
                            session.room.CleanOutObjectNotInThisRoom(oe.apo.realizedObject);
                            oe.beingMoved = false;
                        }
                        else
                        {
                            oe.apo.realizedObject.RemoveFromRoom();
                            oe.ExitResource(roomSession);
                            oe.ExitResource(roomSession.worldSession);
                        }


                    }
                }
            }
        }

    }
}