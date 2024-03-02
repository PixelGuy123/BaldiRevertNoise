using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace BaldiRevertNoise
{
    [BepInPlugin(ModInfo.PLUGIN_GUID, ModInfo.PLUGIN_NAME, ModInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
			Harmony h = new(PluginInfo.PLUGIN_GUID);
			h.PatchAll();
        }
    }

	[HarmonyPatch(typeof(PropagatedAudioManager))]
	internal class RevertAudioPatch
	{
		[HarmonyPatch("VirtualLateUpdate")]
		[HarmonyPatch("VirtualUpdate")]
		[HarmonyPrefix]
		static bool NoPropagatedAudio(PropagatedAudioManager __instance)
		{
			__instance.audioDevice.transform.position = __instance.transform.position;
			return false;
		}

		[HarmonyPatch("VirtualAwake")]
		[HarmonyPostfix]
		static void FullVolume(PropagatedAudioManager __instance, float ___minDistance, float ___maxDistance)
		{
			__instance.audioDevice.volume = 100;
			__instance.audioDevice.minDistance = ___minDistance;
			__instance.audioDevice.maxDistance = ___maxDistance;
		}

		[HarmonyPatch("GetSubtitleScale")]
		[HarmonyPrefix]
		static bool NormalSubSize(ref float __result, Transform cameraTransform, PropagatedAudioManager __instance, AudioSource ___audioDevice) // The subtitle still needs to be updated, duh
		{
			__result = Mathf.Max(1f - Vector3.Distance(cameraTransform.position, __instance.transform.position) / ___audioDevice.maxDistance, 0f);
			return false;
		}
	}

	internal static class ModInfo
	{
		public const string PLUGIN_GUID = "pixelguy.pixelmodding.baldiplus.revertnoise";
		public const string PLUGIN_NAME = "Baldi\'s 0.3.8 Noise";
		public const string PLUGIN_VERSION = "1.0.1";
	}


}
