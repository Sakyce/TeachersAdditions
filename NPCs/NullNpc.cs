using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Utility;

namespace Additions.Characters
{
	public class NullTeacher : Teacher
	{
		static readonly float PIXEL_PER_UNIT = 26f;
		private bool isInSpoopMode = false;
		private bool noIndicator;
		private readonly Vector3[] soundLocations = new Vector3[128];
		private int currentSoundVal;
		public static class Sprites
		{
			public static Sprite baseSprite = AssetManager.LoadSprite("null.png", PIXEL_PER_UNIT);
		}
		public static class Sounds
		{
			public static SoundObject bored = AssetManager.LoadSoundObject("null/bored.wav", AudioType.WAV);
			public static SoundObject enough = AssetManager.LoadSoundObject("null/enough.wav", AudioType.WAV);
			public static SoundObject nothing = AssetManager.LoadSoundObject("null/nothing.wav", AudioType.WAV);
			public static SoundObject nowhere = AssetManager.LoadSoundObject("null/nowhereyoucanhide.wav", AudioType.WAV);
			public static SoundObject wherever = AssetManager.LoadSoundObject("null/whereveryouare.wav", AudioType.WAV);

			public static SoundObject scare = AssetManager.LoadSoundObject("foxo/scare.wav", AudioType.WAV);
		}

		public void ClearSoundLocations()
		{
			for (int i = 0; i < soundLocations.Length; i++)
			{
				soundLocations[i] = Vector3.zero;
			}
			currentSoundVal = 0;
		}
		private bool HasSoundLocation()
		{
			for (int i = 0; i < soundLocations.Length; i++)
			{
				if (soundLocations[i] != Vector3.zero)
				{
					return true;
				}
			}
			currentSoundVal = 0;
			return false;
		}

		private IEnumerator Slap()
		{
			navigator.SetSpeed(100f * ec.NpcTimeScale);
			
			var time = 0.3f;
			while (time > 0)
			{
				TargetPlayer(players[0].transform.position);
				time -= Time.deltaTime * ec.NpcTimeScale;
				yield return null;
			}

			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
			StartCoroutine(SlapDelay());
			yield break;
		}
		private IEnumerator SlapDelay()
		{
			var time = 1.5f;
			while (time > 0)
			{
				time -= Time.deltaTime * ec.NpcTimeScale;
				yield return null;
			}
			StartCoroutine(Slap());
			yield break;
		}
		public override void Awake()
		{
			base.Awake();
			audMan.audioDevice.maxDistance *= 6;
			audMan.volumeModifier = 0.8f;
			transform.Find("SpriteBase").localPosition += new Vector3(0, -0.8f + 0.9f, 0);
			SetSprite(Sprites.baseSprite);
			DontSpawnNPCS = true;
			spriteRenderer.color = Color.clear;
		}
		public override void NotebookCollected()
		{
			if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free)
				return;
			if (isInSpoopMode)
				return;
			isInSpoopMode = true;
			spriteRenderer.color = Color.white;
			SetKillable(true, Sounds.scare);
			StopAllCoroutines();
			StartCoroutine(SlapDelay());
		}
	}
}
