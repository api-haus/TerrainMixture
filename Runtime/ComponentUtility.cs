using System;
using System.Reflection;
using UnityEngine;

namespace TerrainMixture.Runtime
{
	public static class ComponentUtility
	{
		public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
		{
			return go.AddComponent<T>().GetCopyOf(toAdd);
		}

		public static T GetCopyOf<T>(this T comp, T other) where T : Component
		{
			var type = comp.GetType();
			var othersType = other.GetType();
			if (type != othersType)
			{
				Debug.LogError(
					$"The type \"{type.AssemblyQualifiedName}\" of \"{comp}\" does not match the type \"{othersType.AssemblyQualifiedName}\" of \"{other}\"!");
				return null;
			}

			var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
			var pinfos = type.GetProperties(flags);

			foreach (var pinfo in pinfos)
			{
				if (pinfo.CanWrite)
				{
					try
					{
						pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
					}
					catch
					{
						/*
						 * In case of NotImplementedException being thrown.
						 * For some reason specifying that exception didn't seem to catch it,
						 * so I didn't catch anything specific.
						 */
					}
				}
			}

			var finfos = type.GetFields(flags);

			foreach (var finfo in finfos)
			{
				finfo.SetValue(comp, finfo.GetValue(other));
			}

			return comp;
		}
	}
}
