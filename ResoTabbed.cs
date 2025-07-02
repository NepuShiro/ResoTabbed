using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using UnityEngine;

namespace ResoTabbed
{
	public class ResoTabbed : ResoniteMod
	{
		public override string Name => "ResoTabbed";
		public override string Author => "NepuShiro";
		public override string Version => "1.1.0";
		public override string Link => "https://github.com/NepuShiro/ResoTabbed/";
		
		private static bool _focused;

		private static ModConfiguration _config;
		
		[AutoRegisterConfigKey]
		private static readonly ModConfigurationKey<string> DynvarInternal = new ModConfigurationKey<string>("DynVar", "The Dynamic Variable to have the expose the focus state as", () => "User/IsFocused");

		private static string DynVar => _config.GetValue(DynvarInternal);
		private static bool _dynVarChanged;

		public override void OnEngineInit()
		{
			_config = GetConfiguration();
			_config!.Save(true);
			
			Harmony harmony = new Harmony("net.NepuShiro.ResoTabbed");
			harmony.PatchAll();
			
			Engine.Current.OnReady += () =>
			{
				Application.focusChanged += focus => _focused = focus;
				DynvarInternal.OnChanged += value => _dynVarChanged = DynamicVariableHelper.IsValidName((string)value);
			};
		}

		[HarmonyPatch(typeof(UserRoot))]
		private static class UserRootPatch
		{
			private static DynamicValueVariable<bool> Variable { get; set; }

			[HarmonyPatch("OnStart")]
			[HarmonyPostfix]
			private static void OnStart(UserRoot __instance)
			{
				if (__instance == null || __instance.ActiveUser != __instance.LocalUser || __instance.World.IsUserspace()) return;
				
				Variable = __instance.Slot.GetComponentOrAttach<DynamicValueVariable<bool>>();
				if (Variable == null) return;
				
				Variable.VariableName.Value = DynVar;
				Variable.Value.Value = true;
			}
			
			[HarmonyPatch("OnCommonUpdate")]
			[HarmonyPostfix]
			private static void OnCommonUpdate(UserRoot __instance)
			{
				if (__instance == null || __instance.ActiveUser != __instance.LocalUser || __instance.World.IsUserspace()) return;

				if (_dynVarChanged && Variable != null)
				{
					Variable.VariableName.Value = DynVar;
					_dynVarChanged = false;
				}
				
				__instance.Slot.WriteDynamicVariable(DynVar, _focused);
			}
		}
	}
}
