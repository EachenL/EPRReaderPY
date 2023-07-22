using System.Text;
using EasyPathology.Definitions.DataTypes;

namespace EasyPathology.Record.Interfaces;

public interface IBinaryReadWriteWithHeader {
	/// <summary>
	/// 这段信息的长度，该长度不包含头部信息（Type和Length）
	/// </summary>
	uint GetLength(int version);

	/// <summary>
	/// 从BinaryReader读取数据
	/// <para>读取应包含以下操作</para>
	/// <para>首先，调用this.BeginRead(br)，这会读取Length，记录下当前的位置</para>
	/// <para>Type不需要被读取，因为它在前面被读取，从而决定了要实例化哪个对象</para>
	/// <para>之后如果有基类，调用base.Read(br)</para>
	/// <para>然后读取自己的数据</para>
	/// <para>最后调用this.EndRead(br)，这会检查读取是否正确</para>
	/// <para>如果读取出现错误，会自动使用length跳过当前数据，并抛出异常</para>
	/// </summary>
	/// <remarks>
	/// this.BeginRead(br)目的是写出Length，this.EndRead(br)目的是校验Length是否符合预期。
	/// 如果数据非常简单，可以不调用这个函数。
	/// </remarks>
	/// <param name="br"></param>
	/// <param name="version"></param>
	void Read(BinaryReader br, int version);

	/// <summary>
	/// 写出内容到BinaryWriter
	/// <para>写出应包含以下操作</para>
	/// <para>首先使用this.WriteFrameStateHeader()写出头部数据</para>
	/// <para>之后如果有基类，调用base.Write(bw)</para>
	/// <para>最后写出自己的数据</para>
	/// </summary>
	/// <param name="bw"></param>
	/// <param name="version"></param>
	void Write(BinaryWriter bw, int version);
}

public struct UInt32BinaryReadWriteWithHeader : IBinaryReadWriteWithHeader {
	public UInt32BinaryReadWriteWithHeader() { }

	public UInt32BinaryReadWriteWithHeader(uint value) {
		Value = value;
	}

	public uint Value { get; set; }

	public uint GetLength(int version) => sizeof(uint);

	public void Read(BinaryReader br, int version) => Value = br.ReadUInt32();

	public void Write(BinaryWriter bw, int version) => bw.Write(Value);

	public override bool Equals(object? obj) => obj is UInt32BinaryReadWriteWithHeader wrapper && Value == wrapper.Value;

	// ReSharper disable once NonReadonlyMemberInGetHashCode
	public override int GetHashCode() => Value.GetHashCode();

	public override string ToString() => Value.ToString();

	public static implicit operator UInt32BinaryReadWriteWithHeader(uint value) => new(value);
}

public struct CharBinaryReadWriteWithHeader : IBinaryReadWriteWithHeader {
	public CharBinaryReadWriteWithHeader() { }

	public CharBinaryReadWriteWithHeader(char value) {
		Value = value;
	}

	public char Value { get; set; }

	public unsafe uint GetLength(int version) {
		var value = Value;
		return (uint)Encoding.UTF8.GetByteCount(&value, 1);
	}

	public void Read(BinaryReader br, int version) => Value = br.ReadChar();

	public void Write(BinaryWriter bw, int version) => bw.Write(Value);

	public override bool Equals(object? obj) => obj is CharBinaryReadWriteWithHeader wrapper && Value == wrapper.Value;

	// ReSharper disable once NonReadonlyMemberInGetHashCode
	public override int GetHashCode() => Value.GetHashCode();

	public override string ToString() => Value.ToString();

	public static implicit operator CharBinaryReadWriteWithHeader(char value) => new(value);
}

public struct Point2IBinaryReadWriteWithHeader : IBinaryReadWriteWithHeader {
	public Point2IBinaryReadWriteWithHeader() { }

	public Point2IBinaryReadWriteWithHeader(Point2I value) {
		Value = value;
	}

	public Point2I Value { get; set; }

	public uint GetLength(int version) => 2 * sizeof(int);

	public void Read(BinaryReader br, int version) => Value = new Point2I(br.ReadInt32(), br.ReadInt32());

	public void Write(BinaryWriter bw, int version) {
		bw.Write(Value.X);
		bw.Write(Value.Y);
	}

	public override bool Equals(object? obj) => obj is Point2IBinaryReadWriteWithHeader wrapper && Value == wrapper.Value;

	// ReSharper disable once NonReadonlyMemberInGetHashCode
	public override int GetHashCode() => Value.GetHashCode();

	public override string ToString() => Value.ToString();

	public static implicit operator Point2IBinaryReadWriteWithHeader(Point2I value) => new(value);
}


// ReSharper disable once InconsistentNaming
public static class IBinaryReadWriteUtils {
	private static long startPos;
	private static uint length;

	// ReSharper disable once UnusedParameter.Global
	public static void BeginRead(this IBinaryReadWriteWithHeader _, BinaryReader br) {
		length = br.ReadUInt32();
		startPos = br.BaseStream.Position;
	}

	// ReSharper disable once UnusedParameter.Global
	public static void EndRead(this IBinaryReadWriteWithHeader _, BinaryReader br) {
		var readLength = (uint)(br.BaseStream.Position - startPos);
		if (length != readLength) {
			br.BaseStream.Seek(startPos + length, SeekOrigin.Begin);
			throw new BinaryReadLengthMismatchException(length, readLength);
		}
	}

	/// <summary>
	/// 获取string存储时所占的长度，7bit encoded
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static uint GetStringSaveLength(this string? str) {
		if (str == null) {
			return 1;
		}
		var num = (uint)Encoding.UTF8.GetByteCount(str);
		var length = 1U + num;
		while (num >= 0x80) {
			length++;
			num >>= 7;
		}
		return length;
	}
}