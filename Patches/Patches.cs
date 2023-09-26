using Additions.Characters;
using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;
using Utility;

namespace Additions.Patches
{
	[HarmonyPatch(typeof(MainGameManager), nameof(MainGameManager.CreateHappyBaldi))]
	public class CreateManagerByPatchingHappyBaldi
	{
		public static bool Prefix(MainGameManager __instance)
		{
			var manager = __instance.Ec.gameObject.AddComponent<Manager>();
			manager.Initialize(__instance);
			Mod.Manager = manager;
			if (manager.currentTeacher == Teachers.Baldi)
				return true;
			return false;
		}
	}
	[HarmonyPatch(typeof(EndlessGameManager), nameof(EndlessGameManager.CreateHappyBaldi))]
	public class CreateManagerByPatchingHappyBaldiEndless
	{
		public static bool Prefix(MainGameManager __instance)
		{
			var manager = __instance.Ec.gameObject.AddComponent<Manager>();
			manager.Initialize(__instance);
			Mod.Manager = manager;
			if (manager.currentTeacher == Teachers.Baldi)
				return true;
			return false;
		}
	}
	[HarmonyPatch(typeof(LevelGenerator), nameof(LevelGenerator.Generate))]
	public class ChooseNewTeacher
	{
		static void Postfix(LevelGenerator __instance, ref IEnumerator __result)
		{
			void postfixAction()
			{
				switch (1/*__instance.controlledRNG.Next(1, 4)*/) // Baldi will be included later uwu don't worry
				{
					case 0:
						Mod.NextTeacher = Teachers.Baldi;
						break;
					case 1:
						Mod.NextTeacher = Teachers.Foxo;
						break;
					case 2:
						Mod.NextTeacher = Teachers.Viktor;
						break;
					case 3:
						Mod.NextTeacher = Teachers.Alice;
						break;
					case 4:
						Mod.NextTeacher = Teachers.Null;
						break;
				}
			}
			__result = new SimpleEnumerator() { enumerator = __result, postfixAction = postfixAction }.GetEnumerator();
		}
	}
	[HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.ExitedSpawn))]
	class ExitedSpawnPatch
	{
		static bool Prefix(MainGameManager __instance)
		{
			if (Mod.Manager == null)
				return true;
			__instance.Ec.SetElevators(false);
			if (Mod.Manager.currentTeacher != Teachers.Baldi)
				Mod.Manager.teacher.PlayerExitedSpawn();
			return false;
		}
	}
	[HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.CollectNotebook))]
	class NotebookCollectedPatch
	{
		static void Postfix()
		{
			if (Mod.Manager.currentTeacher != Teachers.Baldi)
				Mod.Manager.teacher.NotebookCollected();
		}
	}
	[HarmonyPatch(typeof(ChalkEraser), nameof(ChalkEraser.Use))]
	class AddTagToChalkCloudBoxCollider
	{
		static void Postfix(ChalkEraser __instance)
		{
			var collider = __instance.gameObject.GetComponent<BoxCollider>();
			collider.isTrigger = true;
			var chalkpos = __instance.transform.position;
			Console.WriteLine(chalkpos.ToString());
			if (Mod.Manager.currentTeacher != Teachers.Baldi)
				Mod.Manager.teacher.ChalkEraserUsed(chalkpos);
		}
	}
	[HarmonyPatch(typeof(MainGameManager), nameof(MainGameManager.BeginPlay))]
	class NoSchoolMusicPatch
	{
		static void Postfix(ChalkEraser __instance)
		{
			if (Mod.Manager.currentTeacher != Teachers.Baldi)
				Singleton<MusicManager>.Instance.StopMidi();
		}
	}
}