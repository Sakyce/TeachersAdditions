using Additions.Characters;
using Additions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Utility;
using System.Collections;
using UnityEngine.Assertions;

namespace BaldiAdditions.Patches
{
	[HarmonyPatch(typeof(ITM_AlarmClock), nameof(ITM_AlarmClock.Use))]
	class StartSlowdown
	{
		static bool success = false;
		static bool Prefix(ITM_AlarmClock __instance)
		{
			if (Mod.Manager.currentTeacher == Teachers.Foxo)
			{
				if (Mod.Manager.clockModifier != null)
				{
					Singleton<CoreGameManager>.Instance
										.GetHud(0)
										.ShowEventText("Only one clock can be ticking at the same time with Foxo!", 3f);
					success = false;
					return false;
				}
			}
			success = true;
			return true;
		}
		static void Postfix(ITM_AlarmClock __instance)
		{
			if (!success)
				return;
			if (Mod.Manager.currentTeacher == Teachers.Foxo)
			{
				var modifier = new TimeScaleModifier() { npcTimeScale = 0.5f };
				__instance.ec.AddTimeScale(modifier);
				Mod.Manager.clockModifier = modifier;
			}
		}
	}
	[HarmonyPatch(typeof(ITM_AlarmClock), nameof(ITM_AlarmClock.Timer))]
	class StopSlowdown
	{
		static void Postfix(ref IEnumerator __result)
		{
			if (Mod.Manager.currentTeacher == Teachers.Foxo)
			{
				void postfixAction()
				{
					Mod.Manager.ec.RemoveTimeScale(Mod.Manager.clockModifier);
					Mod.Manager.clockModifier = null;
				}
				__result = new SimpleEnumerator() { enumerator = __result, postfixAction = postfixAction }.GetEnumerator();
			}
		}
	}
}
