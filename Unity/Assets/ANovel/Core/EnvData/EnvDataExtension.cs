using System;
using System.Collections.Generic;

namespace ANovel.Core
{
	public delegate void UpdateEnvDataDelegate<TValue>(ref TValue arg);

	public static class EnvDataExtension
	{
		public static IEnumerable<string> GetKeys<TValue>(this IEnvData self, Func<TValue, bool> func = null) where TValue : struct, IEnvDataValue<TValue>
		{
			foreach (var kvp in self.GetAll<TValue>())
			{
				if (func == null || func(kvp.Value))
				{
					yield return kvp.Key;
				}
			}
		}

		public static void DeleteAll<TValue>(this IEnvData self) where TValue : struct, IEnvDataValue<TValue>
		{
			self.DeleteAll<TValue>(null);
		}

		public static void DeleteAll<TValue>(this IEnvData self, Func<TValue, bool> func) where TValue : struct, IEnvDataValue<TValue>
		{
			self.DeleteAll<TValue>((_, v) =>
			{
				return func(v);
			});
		}

		public static void DeleteAllByInterface<TInterface>(this IEnvData self) where TInterface : class
		{
			self.DeleteAllByInterface<TInterface>(null);
		}

		public static void DeleteAllByInterface<TInterface>(this IEnvData self, Func<TInterface, bool> func) where TInterface : class
		{
			self.DeleteAllByInterface<TInterface>((_, v) =>
			{
				return func(v);
			});
		}

		public static void Update<TValue>(this IEnvData self, string key, Func<TValue, TValue> update) where TValue : struct, IEnvDataValue<TValue>
		{
			self.TryGet<TValue>(key, out var value);
			self.Set(key, update(value));
		}

		public static void Update<TValue>(this IEnvData self, string key, UpdateEnvDataDelegate<TValue> update) where TValue : struct, IEnvDataValue<TValue>
		{
			self.TryGet<TValue>(key, out var value);
			update(ref value);
			self.Set(key, value);
		}

		public static void Update<TValue, TArg>(this IEnvData self, string key, TArg data) where TValue : struct, IEnvDataValue<TValue>, IEnvDataUpdate<TArg>
		{
			self.TryGet<TValue>(key, out var value);
			value.Update(data);
			self.Set(key, value);
		}

	}

}