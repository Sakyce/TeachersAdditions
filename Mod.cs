using Additions.Characters;
using BepInEx;
using HarmonyLib;
using MidiPlayerTK;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Additions
{
	[BepInPlugin("sakyce.baldiplus.teachersadditions", "Baldi's Basics Plus Teachers Additions ", "1.0.0.0")]
	unsafe public class Mod : BaseUnityPlugin
	{
		public void Awake()
		{
			instance = this;
			Harmony harmony = new Harmony("sakyce.baldiplus.teachersadditions");
			harmony.PatchAll();
		}
		public static int GetCurrentFloor()
		{
			return 1;
		}
		public void BuildPosterAtAndWaitForTeacher(TileController tile, Direction dir)
		{
			StartCoroutine(WaitForTeacherAsync(tile, dir));
		}
		public IEnumerator WaitForTeacherAsync(TileController tile, Direction dir)
		{
			while (Mod.Manager == null) yield return null;
			while (Mod.Manager.teacher == null) yield return null;
			var teacher = Mod.Manager.teacher;
			if (teacher.Poster == null) yield break;
            teacher.ec.BuildPoster(teacher.Poster, tile, dir);
            yield break;
        }
        public static Mod instance;
        public static Manager Manager;
        public static Teachers NextTeacher;
		
	}
}