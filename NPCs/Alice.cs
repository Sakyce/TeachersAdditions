using HarmonyLib;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Utility;

namespace Additions.Characters
{
	public class Alice : Teacher
	{
		private bool isAngry = false;
		private bool isInSpoopMode = false;
		private bool isStunned = false;
		private bool isWandering = false;
		private float animationStep = 1f;
		private int walkAnimationFrame = 0;
		private float walkSpeed = 16f;
		private AnimState currentAnimation = AnimState.normal;
		public enum AnimState
		{
			normal,
			evil,
			walk
		}
		public static class Sounds
		{
			public static SoundObject intro = AssetManager.LoadSoundObject("alice/hello.ogg", AudioType.OGGVORBIS, soundType: SoundType.Voice);
			public static SoundObject school = AssetManager.LoadSoundObject("alice/school.mp3", AudioType.MPEG, soundType: SoundType.Music);
			public static SoundObject transform1 = AssetManager.LoadSoundObject("alice/transform1.ogg", AudioType.OGGVORBIS, soundType: SoundType.Voice);
			public static SoundObject transform2 = AssetManager.LoadSoundObject("alice/transform2.ogg", AudioType.OGGVORBIS, soundType: SoundType.Voice);
			public static SoundObject scare = AssetManager.LoadSoundObject("alice/jumpscare.ogg", AudioType.OGGVORBIS, soundType: SoundType.Voice);
			public static SoundObject[] quotes = new SoundObject[13];
			private static bool quotesLoaded = false;
			public static void LoadQuotes()
			{
				if (quotesLoaded) return;
				quotesLoaded = true;
				for (int i = 1; i < 13; i++)
				{
					Console.WriteLine(i);
					Console.WriteLine(quotes.Length);
					var asset = AssetManager.LoadSoundObject($"alice/quote{i}.ogg", AudioType.OGGVORBIS, soundType: SoundType.Voice);
					quotes[i] = asset;
				}
			}
		}
		public static class Sprites
		{
			static readonly float PIXEL_PER_UNIT = 25f;
			public static Sprite normal = AssetManager.LoadSprite("alice/default.png", PIXEL_PER_UNIT);
			public static Sprite evil = AssetManager.LoadSprite("alice/evil.png", PIXEL_PER_UNIT);
			public static Sprite[] walks = new Sprite[] {
				AssetManager.LoadSprite("alice/walk1.png", PIXEL_PER_UNIT),
				AssetManager.LoadSprite("alice/walk2.png", PIXEL_PER_UNIT),
				AssetManager.LoadSprite("alice/walk3.png", PIXEL_PER_UNIT),
				AssetManager.LoadSprite("alice/walk4.png", PIXEL_PER_UNIT)
			};
		}
		public new void Awake()
		{
			base.Awake();
			transform.Find("SpriteBase").localPosition += new Vector3(0, -0.8f + 1.4f, 0);
			Singleton<MusicManager>.Instance.StopMidi();

			currentAnimation = AnimState.normal;
			Sounds.LoadQuotes();

			audMan.PlaySingle(Sounds.intro);
			ec.audMan.PlaySingle(Sounds.school);

			audMan.audioDevice.maxDistance *= 4;
			audMan.volumeModifier = 2f;
		}
		public void Update()
		{

			if (isAngry && !isStunned)
			{
				if (looker.PlayerInSight)
				{
					isWandering = false;
					TargetPlayer(players[0].transform.position);
				}
				else if (!navigator.HasDestination)
				{
					isWandering = true;
					WanderRandom();
				}
			}
			
			switch (currentAnimation)
			{
				case (AnimState.walk):
					animationStep -= Time.deltaTime;
					if (animationStep <= 0)
					{
						animationStep = 0.1f;
						walkAnimationFrame += 1;
						if (walkAnimationFrame >= 4)
							walkAnimationFrame = 0;
						SetSprite(Sprites.walks[walkAnimationFrame]);
					}
					break;
				case (AnimState.normal):
					SetSprite(Sprites.normal);
					break;
				case (AnimState.evil):
					SetSprite(Sprites.evil);
					break;
			}
		}
		private IEnumerator StunSequence()
		{
			isStunned = true;
			SetKillable(false);
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
			currentAnimation = AnimState.evil;
			isWandering = false;
			yield return new WaitForSeconds(3f);

			navigator.SetSpeed(walkSpeed);
			currentAnimation = AnimState.walk;
			navigator.ClearDestination();

			var time = 10f;
			while (time>0)
			{
				time -= Time.deltaTime * ec.NpcTimeScale;
				navigator.RunFrom(players[0].transform.position);
				yield return null;
			}

			isStunned = false;
			SetKillable(true);
			yield break;
		}
		public void Stun()
		{
			if (isAngry && !isStunned)
			{
				StopAllCoroutines();
				StartCoroutine(StunSequence());
			}
		}
		private IEnumerator GetMad()
		{
			ec.audMan.FlushQueue(true);
			audMan.FlushQueue(true);

			ec.audMan.PlaySingle(Sounds.transform1);
			yield return new WaitForSeconds(3.8f);
			ec.audMan.PlaySingle(Sounds.transform2);
			currentAnimation = AnimState.walk;

			navigator.SetSpeed(walkSpeed);
			BeginSpoopMode();
			SetKillable(true, Sounds.scare);
			isAngry = true;
			yield break;
		}
		public override void NotebookCollected()
		{
			if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free)
				return;
			if (isInSpoopMode)
			{
				if (isAngry && !isStunned && isWandering)
				{
					TargetPlayer(players[0].transform.position);
					if (!audMan.IsPlaying)
						audMan.PlaySingle(Sounds.quotes.Choice());
				}
				return;
			}
			isInSpoopMode = true;
			StopAllCoroutines();
			StartCoroutine(GetMad());
		}
		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			if (isAngry && !isStunned && isWandering && !audMan.IsPlaying)
				audMan.PlaySingle(Sounds.quotes.Choice());
		}
		public override void WrongMathMachineAnswer()
		{
			if (isAngry && !isStunned && isWandering)
			{
				Console.WriteLine("Wrong answer");
				TargetPlayer(players[0].transform.position);
				if (!audMan.IsPlaying)
					audMan.PlaySingle(Sounds.quotes.Choice());
			}
			
		}
		public override void ChalkEraserUsed(Vector3 position)
		{
			if (Vector3.Distance(position, transform.position) <= 30f)
				Stun();
		}
	}
}
