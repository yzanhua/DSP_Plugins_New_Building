using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using HarmonyLib;

namespace TestDspPlugin
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInProcess(GAME_PROCESS)]
    public class PluginSJTUPassword : BaseUnityPlugin
    {
        public const string GUID = "DSP_plugin_zh_PluginSJTUPassword";
        public const string NAME = "PluginSJTUPassword";
        public const string VERSION = "1.0";
        private const string GAME_PROCESS = "DSPGAME.exe";

        void Start()
        {
            new Harmony("DSP_plugin_zh_PluginSJTUPassword").PatchAll(typeof(PatchSJTUPassword));
        }

        [HarmonyPatch(typeof(XConsole), "Update")]
        private class PatchSJTUPassword
        {
            [HarmonyPrefix]
            private static void ChangePassword(XConsole __instance)
            {
                __instance.password = 0;
            }
        }
    }
}
