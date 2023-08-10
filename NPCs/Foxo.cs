using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Utility;

namespace Additions.Characters
{
	public class Foxo : Teacher
	{
		static float minimumSlapDelay = 0.4f;
		static float decreasePerNotebook = 0.25f;
		private float extraAnger = 0f;
		private bool fireExtinguisherUsed = false;
		private bool isAngry = false;
		public static class Sprites
		{
			static readonly float PIXEL_PER_UNIT = 30f;
			public static Sprite stare = AssetManager.LoadSprite("foxo/stare.png", PIXEL_PER_UNIT);
			public static Sprite jump = AssetManager.LoadSprite("foxo/jump.png", PIXEL_PER_UNIT);
			public static Sprite sprayed = AssetManager.LoadSprite("foxo/sprayed.png", PIXEL_PER_UNIT);
			public static Sprite[] wave = new Sprite[5] {
				AssetManager.LoadSprite("foxo/wave1.png", PIXEL_PER_UNIT),
				AssetManager.LoadSprite("foxo/wave2.png", PIXEL_PER_UNIT),
				AssetManager.LoadSprite("foxo/wave3.png", PIXEL_PER_UNIT),
				AssetManager.LoadSprite("foxo/wave4.png", PIXEL_PER_UNIT),
				AssetManager.LoadSprite("foxo/wave5.png", PIXEL_PER_UNIT),
			};
			public static Sprite[] slap = new Sprite[4] {
				AssetManager.LoadSprite("foxo/slap1.png", PIXEL_PER_UNIT),
				AssetManager.LoadSprite("foxo/slap2.png", PIXEL_PER_UNIT),
				AssetManager.LoadSprite("foxo/slap3.png", PIXEL_PER_UNIT),
				AssetManager.LoadSprite("foxo/slap4.png", PIXEL_PER_UNIT),
			};
		}
		public static class Sounds
		{
			public static SoundObject fear = AssetManager.LoadSoundObject("foxo/fear.wav", AudioType.WAV);
			public static SoundObject ding = AssetManager.LoadSoundObject("foxo/ding.wav", AudioType.WAV);
			public static SoundObject school = AssetManager.LoadSoundObject("foxo/school.wav", AudioType.WAV, soundType: SoundType.Music);
			public static SoundObject hellothere = AssetManager.LoadSoundObject("foxo/hellothere.wav", AudioType.WAV, soundType: SoundType.Voice);
			public static SoundObject slap = AssetManager.LoadSoundObject("foxo/slap.wav", AudioType.WAV, "SLAP!");
			public static SoundObject scare = AssetManager.LoadSoundObject("foxo/scare.wav", AudioType.WAV);
			public static SoundObject jump = AssetManager.LoadSoundObject("foxo/boing.wav", AudioType.WAV);
			public static SoundObject scream = AssetManager.LoadSoundObject("foxo/scream.wav", AudioType.WAV);
			public static SoundObject[] praises = new SoundObject[] {
				AssetManager.LoadSoundObject("foxo/praise1.wav", AudioType.WAV),
				AssetManager.LoadSoundObject("foxo/praise2.wav", AudioType.WAV)
		};
		}
		public void Update()
		{
			PlayerManager player = players[0];
			if (!player.plm.running)
				player.plm.AddStamina(player.plm.staminaDrop * 0.8f * Time.deltaTime * player.PlayerTimeScale, true);
		}
		private IEnumerator PlayWave()
		{
			for (int i = 0; i < 5; i++)
			{
				SetSprite(Sprites.wave[i]);
				yield return new WaitForSeconds(0.15f);
			}
			for (int i = 5 - 1; i >= 0; i--)
			{
				SetSprite(Sprites.wave[i]);
				yield return new WaitForSeconds(0.15f);
			}
			SetSprite(Sprites.wave[0]);
			yield break;
		}
		private IEnumerator PlaySlap()
		{
			StopCoroutine("PlaySlap");
			audMan.PlaySingle(Sounds.slap);
			for (int i = 0; i < 4; i++)
			{
				SetSprite(Sprites.slap[i]);
				var time = 0.1f;
				while (time > 0)
				{
					time -= Time.deltaTime * ec.NpcTimeScale;
					yield return null;
				}
			}
			yield break;
		}
		private IEnumerator Slap()
		{
			StartCoroutine(PlaySlap());
			navigator.SetSpeed(100f * ec.NpcTimeScale);
			TargetPlayer(players[0].transform.position);
			var time = 0.13f;
			while (time >0)
			{
				time -= Time.deltaTime * ec.NpcTimeScale;
				yield return null;
			}
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
			yield break;
		}
		private IEnumerator GetMad()
		{
			SetSprite(Sprites.stare);
			Singleton<MusicManager>.Instance.StopMidi();
			ec.audMan.FlushQueue(true);
			ec.audMan.PlaySingle(Sounds.fear);
			yield return new WaitForSeconds(13f);
			BeginSpoopMode();
			ec.audMan.PlaySingle(Sounds.ding);
			SetKillable(true, Sounds.scare);
			isAngry = true;
			StartCoroutine(SlapLoop());
			yield break;
		}
		private IEnumerator SlapLoop()
		{
			var time = minimumSlapDelay + decreasePerNotebook * ec.notebookTotal - Mod.Manager.manager.FoundNotebooks * decreasePerNotebook - extraAnger;
			while (time > 0)
			{
				time -= Time.deltaTime * ec.NpcTimeScale;
				yield return null;
			}
			if (UnityEngine.Random.Range(0, 30) == 0)
				StartCoroutine(Jump());
			else
				StartCoroutine(Slap());
			StartCoroutine(SlapLoop());
			yield break;
		}
		private IEnumerator PraisePlayer()
		{
			audMan.FlushQueue(true);
			audMan.PlaySingle(Sounds.praises.Choice());
			SetSprite(Sprites.wave[0]);
			SetKillable(false);
			navigator.ClearDestination();
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
			yield return new WaitForSeconds(6);
			StartCoroutine(SlapLoop());
			yield return null;
			SetKillable(true);
			yield break;
		}
		private IEnumerator Jump()
		{
			SetSprite(Sprites.jump);
			audMan.PlaySingle(Sounds.jump);
			navigator.SetSpeed(26f * ec.NpcTimeScale);
			var time = 6f;
			while (time > 0)
			{
				time -= Time.deltaTime * ec.NpcTimeScale;
				TargetPlayer(players[0].transform.position);
				yield return null;
			}
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
			SetSprite(Sprites.slap[0]);
			yield break;
		}
		public new void Awake()
		{
			base.Awake();
			audMan.audioDevice.maxDistance *= 13;
			audMan.volumeModifier = 0.9f;
			transform.Find("SpriteBase").localPosition += new Vector3(0, -0.8f + 2.4f, 0);
			SetSprite(Sprites.wave[0]);
			Singleton<MusicManager>.Instance.StopMidi();
			audMan.PlaySingle(Sounds.hellothere);
			ec.audMan.PlaySingle(Sounds.school);
			DontSpawnNPCS = true;
			StartCoroutine(PlayWave());
		}
		private IEnumerator FireExtinguisherSequence()
		{
			audMan.FlushQueue(true);
			audMan.PlaySingle(Sounds.scream);
			SetSprite(Sprites.sprayed);
			yield return new WaitForSeconds(16);
			StartCoroutine(SlapLoop());
			yield break;
		}
		private void FireExtinguisher()
		{
			StopAllCoroutines();
			StartCoroutine(FireExtinguisherSequence());
		}
		public override void WrongMathMachineAnswer()
		{
			extraAnger += 0.0075f;
		}
		public override void GoodMathMachineAnswer()
		{
			if (!isAngry) return;
			StopAllCoroutines();
			StartCoroutine(PraisePlayer());
		}
		public override void PlayerExitedSpawn()
		{
			if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free)
				return;
			StopAllCoroutines();
			StartCoroutine(GetMad());
		}
		public override void ChalkEraserUsed(Vector3 position)
		{
			if (Vector3.Distance(position, transform.position) <= 60f)
			{
				FireExtinguisher();
			}
		}
	}
}
