namespace ANovel.Serialization
{
	public interface ICustomMapSerialization
	{
		int Length { get; }
		void Write(Writer writer);
		void Read(int length, Reader reader);
	}

	public interface ICustomArraySerialization
	{
		int Length { get; }
		void Write(Writer writer);
		void Read(int length, Reader reader);
	}


}