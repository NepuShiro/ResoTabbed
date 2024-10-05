using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using UnityEngine;

namespace ResoTabbed
{
	public class ResoTabbed : ResoniteMod
	{
		internal const string VERSION_CONSTANT = "1.0.0";
		public override string Name => "ResoTabbed";
		public override string Author => "NepuShiro";
		public override string Version => VERSION_CONSTANT;
		public override string Link => "https://git.nepu.men/NepuShiro/ResoTabbed/";
		
		private static bool focused;
		private static bool? previousFocused = null;

		public override void OnEngineInit()
		{
			Harmony harmony = new Harmony("com.NepuShiro.ResoTabbed");
			harmony.PatchAll();
			
			Engine.Current.OnReady += () =>
			{
				Application.focusChanged += (bool focus) =>
				{
					focused = focus;
				};
			};
		}

		[HarmonyPatch(typeof(UserRoot))]
		static class UserRootPatch
		{
			[HarmonyPatch("OnStart")]
			[HarmonyPostfix]
			static void OnStart(UserRoot __instance)
			{
				if (__instance == null || __instance.ActiveUser != __instance.LocalUser || __instance.World.IsUserspace()) return;
				
				var apple = __instance.Slot.GetComponentOrAttach<DynamicValueVariable<bool>>();
				if (apple.VariableName.Value != "User/IsFocused") apple.VariableName.Value = "User/IsFocused";
				if (apple.Value.Value == false) apple.Value.Value = true;
			}
			
			[HarmonyPatch("OnCommonUpdate")]
			[HarmonyPostfix]
			static void OnCommonUpdate(UserRoot __instance)
			{
				if (__instance == null || __instance.ActiveUser != __instance.LocalUser || __instance.World.IsUserspace()) return;
				
				if (previousFocused != focused)
				{
					__instance.Slot.WriteDynamicVariable("User/IsFocused", focused);
					previousFocused = focused;
				}
			}
		}
	}
}
