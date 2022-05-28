using System;
using System.Collections.Generic;

namespace ANovel
{
	public interface IEnvValue { }

	public interface IEnvDataHolder
	{
		IEnvDataHolder GetParent();
		bool Has<TValue>(string key) where TValue : struct, IEnvValue;
		TValue Get<TValue>(string key) where TValue : struct, IEnvValue;
		bool TryGet<TValue>(string key, out TValue value) where TValue : struct, IEnvValue;
		IEnumerable<KeyValuePair<string, TValue>> GetAll<TValue>() where TValue : struct, IEnvValue;
		IEnumerable<KeyValuePair<string, TInterface>> GetAllByInterface<TInterface>() where TInterface : class;
	}

	public interface IEnvData : IEnvDataHolder
	{
		new IEnvData GetParent();
		void Set<TValue>(string key, TValue value) where TValue : struct, IEnvValue;
		void Delete<TValue>(string key) where TValue : struct, IEnvValue;
		void DeleteAll<TValue>(Func<string, TValue, bool> func) where TValue : struct, IEnvValue;
		void DeleteAllByInterface<TInterface>(Func<string, TInterface, bool> func) where TInterface : class;
	}

}