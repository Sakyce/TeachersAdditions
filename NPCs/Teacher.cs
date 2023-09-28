using System;
using System.Collections.Generic;
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
		public virtual new void Awake()
		{
			base.Awake();
			allTilesCache = ec.AllTiles();
		}

		/* shared code between some teachers */
		private List<TileController> allTilesCache;
		public (Door, TileController) TeleportToNearestDoor()
		{
			var playerPos = ec.players[0].transform.position;
			Door? nearestDoor = null;
			var nearest = float.PositiveInfinity;

			// obtenir la porte la plus proche
			foreach (var tile in allTilesCache)
			{
				foreach (var door in tile.doors)
				{
					var distance = (door.transform.position - playerPos).magnitude;
					print("distance");
					if (distance <= nearest)
					{
						nearestDoor = door;
						nearest = distance;
					}
				}
			}

			if (nearestDoor == null)
			{
				throw new Exception("Wtff no nearest door wtf ????");
			}

			// obtenir le côté le plus loin
			Vector3 teleportPosition;
			TileController side;
			if ((nearestDoor.aTile.transform.position - playerPos).magnitude < (nearestDoor.bTile.transform.position - playerPos).magnitude)
			{
				side = nearestDoor.bTile;
				teleportPosition = nearestDoor.bTile.transform.position;
			}
			else
			{
				side = nearestDoor.aTile;
				teleportPosition = nearestDoor.aTile.transform.position;
			}
			transform.position = teleportPosition + Vector3.up * 5f;
			TargetPlayer(players[0].transform.position);
			return (nearestDoor, side);
		}

		public void CreatePoster(Texture2D texture)
		{
			poster = ScriptableObject.CreateInstance<PosterObject>();

            poster.baseTexture = texture;
		}
	}
	public enum Teachers
	{
		Baldi,
		Foxo,
		Viktor,
		Alice,
		Null
	}
}
