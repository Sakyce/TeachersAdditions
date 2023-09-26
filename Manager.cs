using Additions.Characters;
using UnityEngine;
using System;
using System.Collections;
using Random = System.Random;
using Utility;
using UnityEngine.UIElements;

namespace Additions
{
	public class Manager : MonoBehaviour
	{
		public BaseGameManager gameManager;
		public CoreGameManager coremanager;
		public EnvironmentController ec;
		public bool initialized;
		public Random rng;
		public Teachers currentTeacher;
		public Teacher teacher;
		public TimeScaleModifier? clockModifier;
		public virtual void Initialize(BaseGameManager __manager)
		{
			gameManager = __manager;
			coremanager = Singleton<CoreGameManager>.Instance;
			ec = __manager.Ec;
			initialized = true;
			rng = new Random(coremanager.Seed());

			currentTeacher = Mod.NextTeacher;
			switch (Mod.NextTeacher)
			{
				case Teachers.Baldi:
					break;
				case Teachers.Foxo:
					SpawnTeacher(new NPCBuilder<Foxo, Beans>().Make());
					break;
				case Teachers.Viktor:
					SpawnTeacher(new NPCBuilder<Viktor, Beans>().Make());
					break;
				case Teachers.Alice:
					SpawnTeacher(new NPCBuilder<Alice, Beans>().Make());
					break;
				case Teachers.Null:
					SpawnTeacher(new NPCBuilder<NullTeacher, Beans>().Make());
					break;
			}
		}
		public Teacher SpawnTeacher(Teacher teacherbase)
		{
			Teacher teacher = GameObject.Instantiate<Teacher>(teacherbase, ec.transform);
			this.teacher = teacher;

			teacher.navigator = teacher.GetComponent<Navigator>();
			teacher.navigator.ec = ec;
			teacher.navigator.npc = teacher;
			teacher.navigator.useHeatMap = false;

			teacher.looker = teacher.GetComponent<Looker>();
			teacher.looker.npc = teacher;

			teacher.ec = ec;

			teacher.transform.localPosition = this.ec.spawnPoint + Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.forward * 20f;

			ec.npcs.Add(teacher);

			teacher.Initialize();
			teacher.gameObject.SetActive(true);

			for (int i = 0; i < Singleton<CoreGameManager>.Instance.setPlayers; i++)
			{
				teacher.players.Add(Singleton<CoreGameManager>.Instance.GetPlayer(i));
			}
			return teacher;
		}
		public void PlayerExitedSpawn()
		{
			if (teacher)
				teacher.PlayerExitedSpawn();
		}
		public NPC SpawnNPC(NPC npc, IntVector2 position)
		{
			NPC npcClone = GameObject.Instantiate<NPC>(npc, ec.transform);

			npcClone.navigator = npcClone.GetComponent<Navigator>();
			npcClone.navigator.ec = ec;
			npcClone.navigator.npc = npcClone;
			npcClone.navigator.useHeatMap = false;

			npcClone.looker = npcClone.GetComponent<Looker>();
			npcClone.looker.npc = npcClone;

			npcClone.ec = ec;

			npcClone.transform.localPosition =
					new UnityEngine.Vector3((float)position.x * 10f + 5f, 5f, (float)position.z * 10f + 5f);

			ec.npcs.Add(npcClone);

			npcClone.Initialize();
			npcClone.gameObject.SetActive(true);

			for (int i = 0; i < Singleton<CoreGameManager>.Instance.setPlayers; i++)
			{
				npcClone.players.Add(Singleton<CoreGameManager>.Instance.GetPlayer(i));
			}
			return npcClone;
		}
		public void Kill(Transform player, NPC baldi, SoundObject? scareSound, Vector3 offset = new Vector3())
		{
			CoreGameManager coregame = Singleton<CoreGameManager>.Instance;
			GameCamera cam = coregame.GetCamera(0);

			Time.timeScale = 0f;
			Singleton<MusicManager>.Instance.StopMidi();
			cam.UpdateTargets(baldi.transform, 0);
			cam.offestPos = (player.position - baldi.transform.position).normalized * 2f + Vector3.up + offset;
			cam.controllable = false;
			cam.matchTargetRotation = false;

			coregame.disablePause = true;
			coregame.audMan.volumeModifier = 0.6f;
			if (scareSound)
			{
				coregame.audMan.PlaySingle(scareSound);
			}

			StartCoroutine(KillEndSequence());
			Singleton<InputManager>.Instance.Rumble(1f, 2f);
		}
		private IEnumerator KillEndSequence()
		{
			CoreGameManager coregame = Singleton<CoreGameManager>.Instance;
			GameCamera cam = coregame.GetCamera(0);

			float time = 1f;
			while (time > 0f)
			{
				time -= Time.unscaledDeltaTime;
				cam.camCom.farClipPlane = 500f * time;
				cam.billboardCam.farClipPlane = 500f * time;
				yield return null;
			}

			cam.camCom.farClipPlane = 1000f;
			cam.billboardCam.farClipPlane = 1000f;
			cam.StopRendering(true);
			coregame.audMan.FlushQueue(true);
			AudioListener.pause = true;

			time = 2f;
			while (time > 0f)
			{
				time -= Time.unscaledDeltaTime;
				yield return null;
			}
			Singleton<GlobalCam>.Instance.SetListener(true);
			if (coregame.lives < 1 && coregame.extraLives < 1)
			{
				Singleton<GlobalCam>.Instance.SetListener(true);
				coregame.ReturnToMenu();
			}
			else
			{
				if (coregame.lives > 0)
				{
					coregame.lives--;
				}
				else
				{
					coregame.extraLives--;
				}
				Singleton<BaseGameManager>.Instance.RestartLevel();
			}
			yield break;
		}
	}
}
