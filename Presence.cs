using UnityEngine;
using DiscordRPC;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using Devdog.General.UI;
using HarmonyLib;

namespace DiscordRichPresence
{
    internal class Presence : MonoBehaviour
    {
        internal string Area = "Intro";
        internal Locations AreaText;
        internal string RichPresenceMessage = "State.Menu";
        internal DiscordRpcClient Client;
        internal DateTime StartTime = DateTime.UtcNow;
        internal static bool InUI = false;
        internal static string UIName = "INVENTORY";

        internal void Awake()
        {
            Client = new DiscordRpcClient("1185413607165530142");
            Client.Initialize();
            Client.OnReady += Client_OnReady;
            Client.OnConnectionEstablished += Client_OnConnectionEstablished;
            Application.quitting += DisableRichPresence;
            Harmony harmony = new Harmony("vaproxy.richpresence");
            harmony.PatchAll();
        }

        private void Client_OnConnectionEstablished(object sender, DiscordRPC.Message.ConnectionEstablishedMessage args)
        {
            Plugin.Log.LogInfo("Connection established.");
        }

        private void Client_OnReady(object sender, DiscordRPC.Message.ReadyMessage args)
        {
            Plugin.Log.LogInfo("RichPresence ready.");
            StartCoroutine(RichPresenceUpdate());
        }

        [HarmonyPatch(typeof(UIWindowPage))]
        static class WindowPagePatches
        {
            [HarmonyPatch("DoShow")]
            [HarmonyPostfix]
            static void Postfix(ref UIWindowPage __instance)
            {
                UIName = __instance.name.Replace(" ", "_").Replace("'", "").Replace("\"", "").ToUpper();
            }
        }

        internal IEnumerator RichPresenceUpdate()
        {
            float timer = 0f;
            const float updateInterval = 0.5f; // Update interval in seconds
            while (true)
            {
                // Manually track time
                timer += Time.unscaledDeltaTime;
                // Check if it's time to update the presence based on the interval
                if (timer >= updateInterval)
                {
                    // Reset the timer
                    timer = 0f;
                    //Plugin.Log.LogInfo($"Setting presence to Playing VA Proxy - {RichPresenceMessage}");
                    RichPresence Presence = new RichPresence();
                    Presence.WithDetails(RichPresenceMessage);
                    TimeSpan dur = DateTime.UtcNow - StartTime;
                    string hours = "";
                    if (dur.Hours != 0)
                    {
                        hours = $"{dur.Hours}h ";
                    }
                    string minutes = "";
                    if (dur.Minutes != 0)
                    {
                        minutes = $"{dur.Minutes}m ";
                    }
                    string seconds = "";
                    if (dur.Seconds != 0)
                    {
                        seconds = $"{dur.Seconds}s ";
                    }
                    Presence.WithState($"{hours}{minutes}{seconds} elapsed");
                    Assets assets = new Assets
                    {
                        LargeImageKey = "va_proxy",
                        LargeImageText = "VA Proxy"
                    };
                    Presence.WithAssets(assets);
                    Client.SetPresence(Presence);
                }
                yield return null;
            }
        }

        internal void Update()
        {
            GameObject Pages = GameObject.Find("MAINMENU/Canvas/Pages");
            UIWindow window = Pages.GetComponent<UIWindow>();
            InUI = window.isVisible;
            if (!InUI)
            {
                if (!AreaText || !AreaText.transform.parent)
                {
                    AreaText = GameObject.Find("UI/ui/Area").GetComponent<Locations>();
                }
                if (AreaText)
                {
                    Area = AreaText.ID;
                }
                Scene current = SceneManager.GetActiveScene();
                if (current.name == "Menu")
                {
                    RichPresenceMessage = "State.Menu";
                }
                else
                {
                    RichPresenceMessage = $"Location.{Area.Replace(" ", "_").Replace("'", "").Replace("\"", "").ToUpper()}";
                }
            }
            else
            {
                RichPresenceMessage = $"State.{UIName}";
            }
        }

        internal void DisableRichPresence()
        {
            Client.ClearPresence();
            Client.Dispose();
        }
    }
}
