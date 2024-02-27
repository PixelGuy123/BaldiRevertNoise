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
		static bool NoPropagatedAudio() => false; // Disable magic

		[HarmonyPatch("VirtualAwake")]
		[HarmonyPrefix]
		static bool FixAudioForBaldi(PropagatedAudioManager __instance, ref AudioSource ___audioDevice, float ___minDistance, float ___maxDistance)
		{
			if (___audioDevice != null) return false;
			var comp = __instance.GetComponent<AudioSource>();
			if (___audioDevice == null && comp != null)
			{
				___audioDevice = comp;
				return false;
			}
			___audioDevice = __instance.gameObject.AddComponent<AudioSource>(); // For some reason, Baldi does not come with a builtin Audio source, so I made this workaround
			___audioDevice.spatialBlend = 1f;
			___audioDevice.rolloffMode = AudioRolloffMode.Linear;
			___audioDevice.minDistance = ___minDistance;
			___audioDevice.maxDistance = ___maxDistance;
			___audioDevice.dopplerLevel = 0f;
			return false;
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
		public const string PLUGIN_VERSION = "1.0.0";
	}


}
