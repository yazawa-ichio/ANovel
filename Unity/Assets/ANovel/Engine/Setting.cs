using ANovel.Core;
using ANovel.Service;
using System.Collections.Generic;

namespace ANovel
{
	public class Setting
	{
		public IScenarioLoader ScenarioLoader;
		public IResourceLoader ResourceLoader;
		public IEngineTime Time;
		public bool UseDefaultTextEnvDataProcessor = true;
		public List<IEnvDataCustomProcessor> EnvDataCustomProcessor;

		public Conductor CreateConductor()
		{
			var reader = new BlockReader(ScenarioLoader);
			var conductor = new Conductor(reader, ResourceLoader);
			conductor.Container.Set(Time);
			if (UseDefaultTextEnvDataProcessor)
			{
				conductor.EnvDataHook.Add(new EngineEnvDataProcessor());
			}
			if (EnvDataCustomProcessor != null)
			{
				foreach (var processor in EnvDataCustomProcessor)
				{
					conductor.EnvDataHook.Add(processor);
				}
			}
			return conductor;
		}

	}
}