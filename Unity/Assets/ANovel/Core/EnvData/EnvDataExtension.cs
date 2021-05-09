using System;
using System.Collections.Generic;

namespace ANovel.Core
{
	public delegate void UpdateEnvDataDelegate<TValue>(ref TValue arg);

	public static class EnvDataExtension
	{
		public static IEnumerable<string> GetKeys<TValue>(this IEnvData self, Func<TValue, bool> func = null) where TValue : struct
		{
			foreach (var kvp in self.GetAll<TValue>())
			{
				if (func == null || func(kvp.Value))
				{
					yield return kvp.Key;
				}
			}
		}

		public static void DeleteAll<TValue>(this IEnvData self) where TValue : struct
		{
			self.DeleteAll<TValue>(null);
		}

		public static void DeleteAll<TValue>(this IEnvData self, Func<TValue, bool> func) where TValue : struct
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

		public static void Update<TValue>(this IEnvData self, string key, Func<TValue, TValue> update) where TValue : struct
		{
			self.TryGet<TValue>(key, out var value);
			self.Set(key, update(value));
		}

		public static void Update<TValue>(this IEnvData self, string key, UpdateEnvDataDelegate<TValue> update) where TValue : struct
		{
			self.TryGet<TValue>(key, out var value);
			update(ref value);
			self.Set(key, value);
		}

		public static void Update<TValue, TArg>(this IEnvData self, string key, TArg data) where TValue : struct, IEnvDataUpdate<TArg>
		{
			self.TryGet<TValue>(key, out var value);
			value.Update(data);
			self.Set(key, value);
		}

		public static IEnvData GetRoot(this IEnvData self)
		{
			var cur = self;
			while (cur.GetParent() != null)
			{
				cur = cur.GetParent();
			}
			return cur;
		}

		public static IEnvDataHolder GetRoot(this IEnvDataHolder self)
		{
			var cur = self;
			while (cur.GetParent() != null)
			{
				cur = cur.GetParent();
			}
			return cur;
		}

		public static bool TryGetSingle<TValue>(this IEnvDataHolder self, out TValue value) where TValue : struct
		{
			self = self.GetRoot();
			var key = EvnDataTypePrefix<TValue>.TypeName;
			return self.TryGet(key, out value);
		}

		public static TValue GetSingle<TValue>(this IEnvDataHolder self) where TValue : struct
		{
			self = self.GetRoot();
			var key = EvnDataTypePrefix<TValue>.TypeName;
			return self.Get<TValue>(key);
		}

		public static TValue GetSingleOrCreate<TValue>(this IEnvDataHolder self) where TValue : struct
		{
			self = self.GetRoot();
			var key = EvnDataTypePrefix<TValue>.TypeName;
			self.TryGet(key, out TValue value);
			return value;
		}

		public static void DeleteSingle<TValue>(this IEnvData self) where TValue : struct
		{
			self = self.GetRoot();
			var key = EvnDataTypePrefix<TValue>.TypeName;
			self.Delete<TValue>(key);
		}

		public static void SetSingle<TValue>(this IEnvData self, TValue value) where TValue : struct
		{
			self = self.GetRoot();
			var key = EvnDataTypePrefix<TValue>.TypeName;
			self.Set(key, value);
		}


		public static void UpdateSingle<TValue>(this IEnvData self, Func<TValue, TValue> update) where TValue : struct
		{
			self = self.GetRoot();
			var key = EvnDataTypePrefix<TValue>.TypeName;
			self.Update(key, update);
		}

		public static void UpdateSingle<TValue>(this IEnvData self, UpdateEnvDataDelegate<TValue> update) where TValue : struct
		{
			self = self.GetRoot();
			var key = EvnDataTypePrefix<TValue>.TypeName;
			self.Update(key, update);
		}

		public static void UpdateSingle<TValue, TArg>(this IEnvData self, TArg data) where TValue : struct, IEnvDataUpdate<TArg>
		{
			self = self.GetRoot();
			var key = EvnDataTypePrefix<TValue>.TypeName;
			self.Update<TValue, TArg>(key, data);
		}

	}

}