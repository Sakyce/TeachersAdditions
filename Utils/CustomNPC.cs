using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Utility
{
	public class CustomNPC : NPC
	{
		public static NPC[] Debug_NPCOfType;
		public static T GetNPCObjectOfType<T>() where T : NPC
		{
			T[] npcs = Resources.FindObjectsOfTypeAll<T>();

			// no warnings have been throwo so far
			if (npcs.Length <= 0)
				Debug.LogWarning($"No NPC of type {typeof(T).Name} have been found in the hidden scene for some reasons");

			foreach (var item in npcs)
			{
				Debug.Log(item.name);
				if (!item.name.StartsWith("Custom_"))
				{
					Debug.Log("Got");
					Debug.Log(item.name);
					return item;
				}
			}
			if (npcs.Length > 1)
				Debug.LogWarning("Multiple baseNPCs have been found and a choice have been made!!!");
			return npcs[0];
		}

		public static Dictionary<string, Sprite> PreloadedSprites = new Dictionary<string, Sprite>();

		public new SpriteRenderer spriteRenderer;
		public AudioManager audMan;

		public new void Awake()
		{
			base.Awake();
			spriteRenderer = transform.Find("SpriteBase").Find("Sprite").gameObject.GetComponent<SpriteRenderer>();
			Destroy(GetComponent<Animator>());
			audMan = GetComponent<AudioManager>();
		}

		public void SetSprite(string filename)
		{
			spriteRenderer.sprite = PreloadedSprites[filename];
		}
		public void SetSprite(Sprite sprite)
		{
			spriteRenderer.sprite = sprite;
		}
	}

	public class NPCBuilder<N, B> where B : NPC where N : NPC
	{
		public N Make()
		{
			B clone = GameObject.Instantiate(
					CustomNPC.GetNPCObjectOfType<B>()
			);
			GameObject.DontDestroyOnLoad(clone);
			GameObject.Destroy(clone.GetComponent<B>());
			clone.gameObject.SetActive(false);
			N behavior = clone.gameObject.AddComponent<N>();

			clone.name = string.Concat("Custom_", typeof(N).Name);
			return behavior;
		}
	}
	//[HarmonyPatch(typeof(EnvironmentController), nameof(EnvironmentController.SpawnNPC))]
	class SpawnNPCFix
	{
		static bool Prefix(EnvironmentController __instance, ref NPC npc, ref IntVector2 position)
		{
			NPC npcClone = GameObject.Instantiate<NPC>(npc, __instance.transform);

			npcClone.navigator = npcClone.GetComponent<Navigator>();
			npcClone.navigator.ec = __instance;
			npcClone.navigator.npc = npcClone;
			npcClone.navigator.useHeatMap = false;

			npcClone.looker = npcClone.GetComponent<Looker>();
			npcClone.looker.npc = npcClone;

			npcClone.ec = __instance;

			npcClone.transform.localPosition =
					new UnityEngine.Vector3((float)position.x * 10f + 5f, 5f, (float)position.z * 10f + 5f);

			__instance.npcs.Add(npcClone);

			npcClone.Initialize();
			npcClone.gameObject.SetActive(true);


			for (int i = 0; i < Singleton<CoreGameManager>.Instance.setPlayers; i++)
			{
				npcClone.players.Add(Singleton<CoreGameManager>.Instance.GetPlayer(i));
			}
			return false;
		}
	}
}
