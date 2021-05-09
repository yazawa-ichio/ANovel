using System;

namespace ANovel.Serialization
{
	public interface IDataFormatter
	{
		void Write(Writer writer, object obj);
		object Read(Reader reader, Type type);
	}

	public abstract partial class DataFormatter : IDataFormatter
	{

		void IDataFormatter.Write(Writer writer, object obj)
		{
			Write(writer, obj);
		}

		object IDataFormatter.Read(Reader reader, Type type)
		{
			return Read(reader, type);
		}

		protected abstract void Write(Writer writer, object obj);

		protected abstract object Read(Reader reader, Type type);
	}


	public abstract class DataFormatter<T> : IDataFormatter
	{
		void IDataFormatter.Write(Writer writer, object obj)
		{
			Write(writer, (T)obj);
		}

		object IDataFormatter.Read(Reader reader, Type type)
		{
			return Read(reader);
		}

		public abstract void Write(Writer writer, T obj);

		public abstract T Read(Reader reader);

	}

}