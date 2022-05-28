using ANovel.Serialization;
using System;

namespace ANovel.Core
{
	public class DiffData<TValue> : ICustomMapSerialization
	{
		public static readonly DiffData<TValue> Empty = new DiffData<TValue>();

		public Type Type => typeof(TValue);

		public bool HasData => New.Length > 0 || Update.Length > 0 || Delete.Length > 0;

		public NewDiffData<TValue>[] New = Array.Empty<NewDiffData<TValue>>();

		public UpdateDiffData<TValue>[] Update = Array.Empty<UpdateDiffData<TValue>>();

		public DeleteDiffData<TValue>[] Delete = Array.Empty<DeleteDiffData<TValue>>();

		int ICustomMapSerialization.Length
		{
			get
			{
				int size = 0;
				if (New.Length > 0) size++;
				if (Update.Length > 0) size++;
				if (Delete.Length > 0) size++;
				return size;
			}
		}

		void ICustomMapSerialization.Write(Writer writer)
		{
			if (New.Length > 0)
			{
				writer.Write(nameof(New));
				Packer.Pack(writer, New);
			}
			if (Update.Length > 0)
			{
				writer.Write(nameof(Update));
				Packer.Pack(writer, Update);
			}
			if (Delete.Length > 0)
			{
				writer.Write(nameof(Delete));
				Packer.Pack(writer, Delete);
			}
		}

		void ICustomMapSerialization.Read(int length, Reader reader)
		{
			for (int i = 0; i < length; i++)
			{
				switch (reader.ReadString())
				{
					case nameof(New):
						New = Packer.Unpack<NewDiffData<TValue>[]>(reader);
						break;
					case nameof(Update):
						Update = Packer.Unpack<UpdateDiffData<TValue>[]>(reader);
						break;
					case nameof(Delete):
						Delete = Packer.Unpack<DeleteDiffData<TValue>[]>(reader);
						break;
					default:
						reader.ReadSkip();
						break;
				}
			}
		}

	}

	public readonly struct NewDiffData<TValue>
	{
		public readonly string Key;
		public readonly TValue Value;

		public NewDiffData(string key, TValue value)
		{
			Key = key;
			Value = value;
		}
	}

	public readonly struct DeleteDiffData<TValue>
	{
		public readonly string Key;

		public readonly TValue Value;

		public DeleteDiffData(string key, TValue value)
		{
			Key = key;
			Value = value;
		}
	}

	public readonly struct UpdateDiffData<TValue>
	{
		public readonly string Key;

		public readonly TValue Prev;

		public readonly TValue Current;

		public UpdateDiffData(string key, TValue prev, TValue current)
		{
			Key = key;
			Prev = prev;
			Current = current;
		}
	}

}