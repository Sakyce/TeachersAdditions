using Additions.Characters;
using BepInEx;
using HarmonyLib;
using System;
using UnityEngine;

namespace Additions
{
	[BepInPlugin("sakyce.baldiplus.teachersadditions", "Baldi's Basics Plus Teachers Additions ", "1.0.0.0")]
	unsafe public class Mod : BaseUnityPlugin
	{
		public void Awake()
		{
			Harmony harmony = new Harmony("sakyce.baldiplus.teachersadditions");
			harmony.PatchAll();
		}
		public static int GetCurrentFloor()
		{
			return 1;
		}
		public static Manager Manager;
		public static Teachers NextTeacher;
	}
}