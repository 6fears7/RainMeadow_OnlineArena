using RainMeadow;

namespace Drown
{
    public static class DrownModeRPCs
    {
        [RainMeadow.RPCMethod]
        public static void Arena_IncrementPlayerScore(RPCEvent rpcEvent, int score)
        {

            ++score;

        }

        [RainMeadow.RPCMethod]
        public static void Arena_OpenDen(RPCEvent rpcEvent, bool denOpen)
        {

           DrownMode.openedDen = denOpen;

        }
    }
}