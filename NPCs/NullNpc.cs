using System;
using System.Collections;
using System.Collections.Generic;
using TeachersAdditions.Components;
using UnityEngine;
using Utility;

namespace Additions.Characters
{
	public class NullTeacher : Teacher
	{
		static readonly float PIXEL_PER_UNIT = 26f;
		private bool isInSpoopMode = false;
		private BaldiHearing hearing;

		private List<NullPhrase> genericPhrases = new List<NullPhrase>();

		private bool enoughPlayed = false;
        private bool hahaPlayed = false;
		private bool hidePlayed = false;
		private bool nothingPlayed = false;
		private bool scaryPlayed = false;
		private bool stopPlayed = false;
		private bool wherePlayed = false;
		private bool boredPlayed = false;

		public static class Sounds
		{
            public static SoundObject audEnough = AssetManager.LoadSoundObject("null/enough.wav", AudioType.WAV);
            public static SoundObject audHaha;
            public static SoundObject audHide = AssetManager.LoadSoundObject("null/nowhereyoucanhide.wav", AudioType.WAV);
            public static SoundObject audNothing = AssetManager.LoadSoundObject("null/nothing.wav", AudioType.WAV);
            public static SoundObject audScary;
            public static SoundObject audStop;
            public static SoundObject audWhere = AssetManager.LoadSoundObject("null/whereveryouare.wav", AudioType.WAV);
            public static SoundObject audBored = AssetManager.LoadSoundObject("null/bored.wav", AudioType.WAV);
        }

		public static class Sprites
		{
			public static Sprite baseSprite = AssetManager.LoadSprite("null.png", PIXEL_PER_UNIT);
		}
		private IEnumerator Slap()
		{
			navigator.SetSpeed(100f * ec.NpcTimeScale);

			var time = 0.2f;
			while (time > 0)
			{
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
			var time = 1.3f;
			while (time > 0)
			{
				time -= Time.deltaTime * ec.NpcTimeScale;
				yield return null;
			}
			StartCoroutine(Slap());
			yield break;
		}
		public void Update()
		{
            if (!isInSpoopMode) return;
            if (!controlOverride && !navigator.HasDestination)
				if (hearing.HasSoundLocation())
				{
					hearing.UpdateSoundTarget();
				}
				else
					WanderRandom();
		}
		public override void DestinationEmpty()
		{
            if (!isInSpoopMode) return;
            base.DestinationEmpty();
			if (navigator.passableObstacles.Contains(PassableObstacle.Window))
				navigator.passableObstacles.Clear();

			if (!controlOverride && !returningFromDetour)
			{
				if (hearing.HasSoundLocation())
				{
					hearing.UpdateSoundTarget();
					return;
				}
				WanderRandom();
			}
		}
		protected override void OnTriggerEnter(Collider other)
		{
			base.OnTriggerEnter(other);

			if (navigator.passableObstacles.Contains(PassableObstacle.Window) && other.CompareTag("Window"))
			{
				other.GetComponent<Window>().Break(false);
				SpeechCheck(NullPhrase.Hide, 0.04f);
			}
		}

		private void SpeechCheck(NullPhrase phrase, float chance)
		{
            if (!isInSpoopMode) return;
            if (!controlOverride)
			{
				switch (phrase)
				{
					case NullPhrase.Enough:
						if (!enoughPlayed && !audMan.IsPlaying && UnityEngine.Random.Range(0f, 1f) <= chance)
						{
							audMan.QueueAudio(Sounds.audEnough);
							enoughPlayed = true;
						}
						break;
					/*case NullPhrase.Haha:
						if (!hahaPlayed && !audMan.IsPlaying && UnityEngine.Random.Range(0f, 1f) <= chance)
						{
							audMan.QueueAudio(audHaha);
							hahaPlayed = true;
							return;
						}
						break;*/
					case NullPhrase.Hide:
						if (!hidePlayed && !audMan.IsPlaying && UnityEngine.Random.Range(0f, 1f) <= chance)
						{
							audMan.QueueAudio(Sounds.audHide);
							hidePlayed = true;
							return;
						}
						break;
					case NullPhrase.Nothing:
						if (!nothingPlayed && !audMan.IsPlaying && UnityEngine.Random.Range(0f, 1f) <= chance)
						{
							audMan.QueueAudio(Sounds.audNothing);
							nothingPlayed = true;
							return;
						}
						break;
					case NullPhrase.Scary:
					case NullPhrase.Stop:
					case NullPhrase.Where:
						break;
					case NullPhrase.Generic:
						if (UnityEngine.Random.Range(0f, 1f) <= chance)
						{
							List<NullPhrase> list = new List<NullPhrase>(genericPhrases);
							for (int i = 0; i < list.Count; i++)
							{
								NullPhrase value = list[i];
								int index = UnityEngine.Random.Range(i, list.Count);
								list[i] = list[index];
								list[index] = value;
							}
							while (list.Count > 0)
							{
								NullPhrase nullPhrase = list[UnityEngine.Random.Range(0, list.Count)];
								if (nullPhrase != NullPhrase.Bored)
								{
									switch (nullPhrase)
									{
										/*case NullPhrase.Scary:
											if (!scaryPlayed /*&& gameTime >= 240f * /&& !looker.PlayerInSight)
											{
												audMan.QueueAudio(audScary);
												scaryPlayed = true;
												genericPhrases.Remove(NullPhrase.Scary);
												return;
											}
											break;*/
										/*case NullPhrase.Stop:
											if (!stopPlayed / *&& NullNPC.attempts >= 5 && gameTime >= 60f * /)
											{
												audMan.QueueAudio(audStop);
												stopPlayed = true;
												genericPhrases.Remove(NullPhrase.Stop);
												return;
											}
											break;*/
										case NullPhrase.Where:
											if (!wherePlayed /*&& hadTargetTime >= 30f && gameTime >= 60f*/)
											{
												audMan.QueueAudio(Sounds.audWhere);
												wherePlayed = true;
												genericPhrases.Remove(NullPhrase.Where);
												return;
											}
											break;
									}
								}
								else if (!boredPlayed /*&& gameTime >= 300f && NullNPC.timeSinceExcitingThing >= 60f*/)
								{
									audMan.QueueAudio(Sounds.audBored);
									boredPlayed = true;
									genericPhrases.Remove(NullPhrase.Bored);
									return;
								}
								list.Remove(nullPhrase);
							}
							return;
						}
						break;
					default:
						return;
				}
			}
		}
        public override void PlayerInSight(PlayerManager player)
        {
            if (!isInSpoopMode) return;
            if (hearing == null) return;
            hearing.ClearSoundLocations();
            hearing.noIndicator = true;
            hearing.Hear(player.transform.position, 127);
            hearing.noIndicator = false;
        }
        public override void PlayerSighted(PlayerManager player)
		{
            if (!isInSpoopMode)
                return;
            base.PlayerSighted(player);
			if (!navigator.passableObstacles.Contains(PassableObstacle.Window))
			{
				navigator.passableObstacles.Add(PassableObstacle.Window);
				navigator.CheckPath();
			}
		}
		public override void Awake()
		{
			base.Awake();
            hearing = BaldiHearing.Create(gameObject, this);
            hearing.noIndicator = true;
            hearing.SoundTargetChanged += (BaldiHearing sender, Vector3 pos) => { TargetPlayer(pos);  };
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
			hearing.noIndicator = false;
            isInSpoopMode = true;
			spriteRenderer.color = Color.white;
			SetKillable(true, Foxo.Sounds.scare);
			StopAllCoroutines();
			StartCoroutine(SlapDelay());
		}

        public override void Hear(Vector3 position, int value)
        {
            base.Hear(position, value);
			hearing.Hear(position, value);
        }
    }

	public enum NullPhrase
	{
		Bored,
		Enough,
		Haha,
		Hide,
		Nothing,
		Scary,
		Stop,
		Where,
		Generic
	}

}
