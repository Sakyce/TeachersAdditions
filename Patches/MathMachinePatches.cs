using Additions.Characters;
using Additions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaldiAdditions.Patches
{
	[HarmonyPatch(typeof(MathMachine), nameof(MathMachine.Clicked))]
	class WrongMathMachineAnswerPatch
	{
		static void Postfix(MathMachine __instance)
		{
			if (Mod.Manager.currentTeacher != Teachers.Baldi && __instance.baldiAngered)
				__instance.baldiAngered = false;
				Mod.Manager.teacher.WrongMathMachineAnswer();
		}
	}
	[HarmonyPatch(typeof(MathMachine), nameof(MathMachine.Completed))]
	class CorrectMathMachineAnswerPatch
	{
		static void Postfix(MathMachine __instance)
		{
			if (Mod.Manager.currentTeacher != Teachers.Baldi)
				Mod.Manager.teacher.GoodMathMachineAnswer();
		}
	}
	//[HarmonyPatch(typeof(MathMachine), nameof(MathMachine.Start))]
	class MoreQuestionsInMathMachinePatch
	{
		static bool Prefix(MathMachine __instance)
		{
			// Seems to only count for the teacher from the last level :(
			if (Mod.Manager.currentTeacher == Teachers.Viktor)
				__instance.totalProblems = 3;
			else
				__instance.totalProblems = 1;
			return true;
		}
	}
}

