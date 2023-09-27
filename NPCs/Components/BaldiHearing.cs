using Additions.Characters;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace TeachersAdditions.Components
{
	public class BaldiHearing : MonoBehaviour
	{
		public Teacher teacher;

		private readonly Vector3[] soundLocations = new Vector3[128];
		private int currentSoundVal = 0;
		public bool noIndicator = false;
		public bool targetingSound = false;

		public delegate void SoundTargetChangedHandler(BaldiHearing sender, Vector3 Position);
		public event SoundTargetChangedHandler SoundTargetChanged;

		public void ClearSoundLocations()
		{
			for (int i = 0; i < soundLocations.Length; i++)
				soundLocations[i] = Vector3.zero;
			currentSoundVal = 0;
		}

		public bool HasSoundLocation()
		{
			for (int i = 0; i < soundLocations.Length; i++)
				if (soundLocations[i] != Vector3.zero)
					return true;
			currentSoundVal = 0;
			return false;
		}

		public void Hear(Vector3 position, int value)
		{
			soundLocations[value] = position;
			targetingSound = true;

			if (value >= currentSoundVal)
			{
				if (!noIndicator)
					for (int i = 0; i < Singleton<CoreGameManager>.Instance.setPlayers; i++)
						Singleton<CoreGameManager>.Instance.GetHud(i).ActivateBaldicator(true);
				UpdateSoundTarget();
				return;
			}

			if (!noIndicator)
				for (int j = 0; j < Singleton<CoreGameManager>.Instance.setPlayers; j++)
					Singleton<CoreGameManager>.Instance.GetHud(j).ActivateBaldicator(false);
		}

		public virtual Vector3 UpdateSoundTarget()
		{
			for (int i = soundLocations.Length - 1; i >= 0; i--)
			{
				if (soundLocations[i] != Vector3.zero)
				{
					var pos = soundLocations[i];
					currentSoundVal = i;
					soundLocations[i] = Vector3.zero;
					SoundTargetChanged.Invoke(this, pos);
					return pos;
				}
			}
			targetingSound = false;
			currentSoundVal = 0;
			return Vector3.zero;
		}

		public BaldiHearing Initialize(Teacher teacher)
		{
			this.teacher = teacher;
			return this;
		}

		public static BaldiHearing Create(GameObject gameObject, Teacher teacher)
		{
			return gameObject.AddComponent<BaldiHearing>().Initialize(teacher);
		}


	}
}
