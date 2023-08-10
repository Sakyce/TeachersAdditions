using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Utility;

namespace Additions.Characters
{
	public class Viktor : Teacher
	{
		static float minimumSlapDelay = 0.2f;
		static float decreasePerNotebook = 1f;
		static float slapTime = 0.7f;
		static float slapSpeed = 30f;
		private float extraAnger = 0f;
		private bool angry = false;
		private bool firstJacketTriggered = false;
		private Vector3 closetLocation;
		private bool isJacketDirty = false;
		public static class Sprites
		{
			static readonly float PIXEL_PER_UNIT = 16f;
			public static Sprite normal = AssetManager.LoadSprite("viktor/default.png", PIXEL_PER_UNIT);
			public static Sprite evil = AssetManager.LoadSprite("viktor/evil.png", PIXEL_PER_UNIT);
		}
		public static class Sounds
		{
			public static SoundObject intro = AssetManager.LoadSoundObject("viktor/intro.ogg", AudioType.OGGVORBIS, soundType:SoundType.Voice);
			public static SoundObject school = AssetManager.LoadSoundObject("viktor/school.ogg", AudioType.OGGVORBIS, soundType: SoundType.Music);
			public static SoundObject walk = AssetManager.LoadSoundObject("viktor/walk.ogg", AudioType.OGGVORBIS, "*footsteps*");
			public static SoundObject scream = AssetManager.LoadSoundObject("viktor/scream.ogg", AudioType.OGGVORBIS);
			public static SoundObject youshouldnt = AssetManager.LoadSoundObject("viktor/youshouldnt.ogg", AudioType.OGGVORBIS, soundType: SoundType.Voice);
			public static SoundObject firstjacket = AssetManager.LoadSoundObject("viktor/jacket0.ogg", AudioType.OGGVORBIS, soundType: SoundType.Voice);
			public static SoundObject[] jackets = new SoundObject[]
			{
				AssetManager.LoadSoundObject("viktor/jacket1.ogg", AudioType.OGGVORBIS, soundType: SoundType.Voice),
				AssetManager.LoadSoundObject("viktor/jacket2.ogg", AudioType.OGGVORBIS, soundType: SoundType.Voice),
				AssetManager.LoadSoundObject("viktor/jacket3.ogg", AudioType.OGGVORBIS, soundType: SoundType.Voice),
				AssetManager.LoadSoundObject("viktor/jacket4.ogg", AudioType.OGGVORBIS, soundType: SoundType.Voice),
			};
		}
		private IEnumerator Slap()
		{
			audMan.PlaySingle(Sounds.walk);
			navigator.SetSpeed(slapSpeed);

			var time = slapTime;
			while (time > 0) {
				time -= Time.deltaTime * ec.NpcTimeScale;
				TargetPlayer(players[0].transform.position);
				yield return null;
			}

			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
			StartCoroutine(SlapLoop());
			yield break;
		}
		private IEnumerator SlapLoop()
		{
			var time = minimumSlapDelay + decreasePerNotebook * ec.notebookTotal - Mod.Manager.manager.FoundNotebooks * decreasePerNotebook - extraAnger;
			Console.WriteLine(time);
			while (time > 0)
			{
				time -= Time.deltaTime * ec.NpcTimeScale;
				yield return null;
			}
			StartCoroutine(Slap());
			yield break;
		}
		public new void Awake()
		{
			base.Awake();
			transform.Find("SpriteBase").localPosition += new Vector3(0, -0.8f + 1.4f, 0);
			SetSprite(Sprites.normal);
			Singleton<MusicManager>.Instance.StopMidi();
			audMan.PlaySingle(Sounds.intro);
			ec.audMan.PlaySingle(Sounds.school);
			audMan.audioDevice.maxDistance *= 4;
			audMan.volumeModifier = 2f;
			Vector3? loc = null;
			foreach (var tile in ec.tiles)
			{
				if (tile.room.category == RoomCategory.Closet)
				{
					loc = tile.transform.position;
					break;
				}
			}
			if (loc == null)
				loc = ec.RealRoomMid(ec.offices[0]);
			if (loc != null)
				closetLocation = (Vector3)loc;
		}
		protected override void OnTriggerEnter(Collider other)
		{
			base.OnTriggerEnter(other);
			var chalkeraser = other.gameObject.GetComponent<ChalkEraser>();
			Console.WriteLine(other.gameObject.name);
			if (chalkeraser)
				SmearJacket();
		}
		public void SmearJacket()
		{
			if (angry && !isJacketDirty)
			{
				StopAllCoroutines();
				StartCoroutine(DirtyJacketSequence());
			}
		}
		private IEnumerator DirtyJacketSequence()
		{
			isJacketDirty = true;
			SetKillable(false);
			if (firstJacketTriggered)
				audMan.PlaySingle(Sounds.jackets.Choice());
			else
				audMan.PlaySingle(Sounds.firstjacket);
			firstJacketTriggered = true;
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
			yield return new WaitForSeconds(3f);
			navigator.SetSpeed(23f);

			navigator.ClearDestination();
			TargetPosition(closetLocation);
			while (navigator.HasDestination)
				yield return null;
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0;
			isJacketDirty = false;
			StartCoroutine(SlapLoop());
			yield return null;
			SetKillable(true);
			yield break;
		}
		private IEnumerator GetMad()
		{
			SetSprite(Sprites.evil);
			ec.audMan.FlushQueue(true);
			audMan.FlushQueue(true);
			ec.audMan.PlaySingle(Sounds.youshouldnt);

			yield return new WaitForSeconds(3f);

			BeginSpoopMode();
			SetKillable(true, Sounds.scream);
			StartCoroutine(SlapLoop());
			yield break;
		}
		public override void ChalkEraserUsed(Vector3 position)
		{
			if (Vector3.Distance(position, transform.position) <= 30f)
			{
				SmearJacket();
			}
		}
		public override void WrongMathMachineAnswer()
		{
			//extraAnger += 0.25f;
		}
		public override void NotebookCollected()
		{
			if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free)
				return;
			if (angry)
				return;
			angry = true;
			StopAllCoroutines();
			StartCoroutine(GetMad());
		}
		public override void PlayerExitedSpawn()
		{
			Singleton<MusicManager>.Instance.StopMidi();
		}
	}
}
