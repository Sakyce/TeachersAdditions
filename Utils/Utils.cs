using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Utility
{
	public static class Utility
	{
		public static T Choice<T>(this T[] array)
		{
			return array[UnityEngine.Random.Range(0, array.Length)];
		}
		public static void PrintArray<T>(List<T> list)
		{
			Console.Write("{");
			foreach (T obj in list)
			{
				if (obj == null) { continue; }
				Console.Write(obj.ToString());
				Console.Write(" , ");
			}
			Console.WriteLine("}");
		}
		public static T GetCopyOf<T>(this Component comp, T other) where T : Component
		{
			Type type = comp.GetType();
			Debug.Assert(type == other.GetType());
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
			PropertyInfo[] pinfos = type.GetProperties(flags);
			foreach (var pinfo in pinfos)
			{
				if (pinfo.CanWrite)
				{
					try
					{
						pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
					}
					catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
				}
			}
			FieldInfo[] finfos = type.GetFields(flags);
			foreach (var finfo in finfos)
			{
				finfo.SetValue(comp, finfo.GetValue(other));
			}
			return (T)comp;
		}

	}
	class SimpleEnumerator : IEnumerable
	{
		public IEnumerator enumerator;
		public Action prefixAction = () => { };
		public Action postfixAction = () => { };
		public Action<object> preItemAction = (a) => { };
		public Action<object> postItemAction = (a) => { };
		public Func<object, object> itemAction = (a) => { return a; };
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public IEnumerator GetEnumerator()
		{
			prefixAction();
			while (enumerator.MoveNext())
			{
				var item = enumerator.Current;
				preItemAction(item);
				yield return itemAction(item);
				postItemAction(item);
			}
			postfixAction();
		}
	}
}
