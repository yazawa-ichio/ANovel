using System;
using System.Collections.Generic;

namespace ANovel
{
	public interface IEnvDataHolder
	{
		IEnvDataHolder GetParent();
		bool Has<TValue>(string key) where TValue : struct;
		TValue Get<TValue>(string key) where TValue : struct;
		bool TryGet<TValue>(string key, out TValue value) where TValue : struct;
		IEnumerable<KeyValuePair<string, TValue>> GetAll<TValue>() where TValue : struct;
	}

	public interface IEnvData : IEnvDataHolder
	{
		new IEnvData GetParent();
		void Set<TValue>(string key, TValue value) where TValue : struct;
		void Delete<TValue>(string key) where TValue : struct;
		void DeleteAll<TValue>(Func<string, TValue, bool> func) where TValue : struct;
		void DeleteAllByInterface<TInterface>(Func<string, TInterface, bool> func) where TInterface : class;
	}

}