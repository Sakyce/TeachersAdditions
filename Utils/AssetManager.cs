using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Utility
{
	public static class AssetManager
	{
		public static string GetAssetsPath()
		{
			return Path.Combine(Application.streamingAssetsPath, "sakyce.TeachersAdditionAssets");
		}
		[Obsolete]
		public static void playMusic(LoopingSoundObject aud)

		{
			aud.mixer = Singleton<PlayerFileManager>.Instance.mixer[(int)SoundType.Music];
			Singleton<MusicManager>.Instance.QueueFile(aud, true);
			Singleton<MusicManager>.Instance.StartFileQueue();
		}

		public static AudioClip LoadAudioClip(string filename, AudioType audiotype)
		{
			string path = Path.Combine("File:///", GetAssetsPath(), "audio", filename);
			UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, audiotype);
			request.SendWebRequest();
			while (!request.isDone) { }
			return DownloadHandlerAudioClip.GetContent(request);
		}

		public static SoundObject LoadSoundObject(string filename, AudioType audiotype, string? soundKey = null, SoundType soundType = SoundType.Effect)
		{
			SoundObject soundObject = ScriptableObject.CreateInstance<SoundObject>();
			soundObject.soundClip = LoadAudioClip(filename, audiotype);
			soundObject.subDuration = soundObject.soundClip.length + 1f;
			soundObject.soundType = soundType;
			soundObject.soundKey = soundKey;
			if (soundKey == null)
			{
				soundObject.subtitle = false;
			}
			soundObject.color = Color.white;
			return soundObject;
		}
		public static Texture2D LoadTexture(string filename)
		{
			string path = Path.Combine("File:///", GetAssetsPath(), "textures", filename);

			byte[] data = File.ReadAllBytes(path);
			Texture2D tex = new Texture2D(2, 2);
			tex.LoadImage(data);
			tex.filterMode = FilterMode.Point;

			return tex;
		}
		public static Sprite LoadSprite(string filename, float pixelsPerUnit = 1f)
		{
			string path = Path.Combine("File:///", GetAssetsPath(), "textures", filename);
			Texture2D tex = LoadTexture(filename);
			return Sprite.Create(
					tex,
					new Rect(0, 0, tex.width, tex.height),
					new Vector2(0.5f, 0.5f),
					pixelsPerUnit
			);
		}
	}
}
