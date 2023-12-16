using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace DiscordRichPresence
{
    [BepInPlugin("tairasoul.vaproxy.richpresence", "RichPresence", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private void Awake()
        {
            Log = Logger;
            Log.LogInfo("RichPresence awake.");
        }

        private void OnDestroy()
        {
            GameObject Presence = new GameObject("RichPresence");
            DontDestroyOnLoad(Presence);
            Presence.AddComponent<Presence>();
            Log.LogInfo("RichPresence setup.");
        }
    }
}
