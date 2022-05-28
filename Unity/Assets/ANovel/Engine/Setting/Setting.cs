using ANovel.Core;
using System.Collections.Generic;

namespace ANovel.Engine
{
	public class Setting
	{
		public IScenarioLoader ScenarioLoader;
		public IResourceLoader ResourceLoader;
		public IEngineTime Time = new EngineTime();
		public bool UseDefaultTextEnvDataProcessor = true;
		public List<IEnvDataCustomProcessor> EnvDataCustomProcessor = new List<IEnvDataCustomProcessor>();

		public Conductor CreateConductor(EngineConfig config)
		{
			var reader = new BlockReader(ScenarioLoader, config.Symbols);
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