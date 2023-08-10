using MidiPlayerTK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Assertions;
using UnityEngine;
using Utility;

namespace Additions.Characters
{
	public abstract class Teacher : CustomNPC
	{
		protected bool canKillByTouch = false;
		private SoundObject? jumpscareSound;
		protected bool DontSpawnNPCS = false;

		protected virtual void OnTriggerEnter(Collider other)
		{
			if (canKillByTouch && other.tag == "Player")
			{
				PlayerManager player = other.GetComponent<PlayerManager>();
				if (!player.invincible)
					Mod.Manager.Kill(other.transform, this, jumpscareSound);
			}
		}
		public void SetKillable(bool enabled, SoundObject? jumpscareSound = null)
		{
			canKillByTouch = enabled;
			if (jumpscareSound != null)
				this.jumpscareSound = jumpscareSound;
		}
		public virtual void PlayerExitedSpawn() { }
		public virtual void NotebookCollected() { }
		public virtual void WrongMathMachineAnswer() { }
		public virtual void GoodMathMachineAnswer() { }
		public virtual void ChalkEraserUsed(Vector3 position) { }
		protected void BeginSpoopMode()
		{
			Singleton<MusicManager>.Instance.StopMidi();
			Singleton<BaseGameManager>.Instance.BeginSpoopMode();
			if (!DontSpawnNPCS)
				this.ec.SpawnNPCs();
			if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free)
				Despawn();
			ec.GetBaldi()?.Despawn();
			this.ec.StartEventTimers();
		}
		
	}
	public enum Teachers
	{
		Baldi,
		Foxo,
		Viktor,
		Alice
	}
}
