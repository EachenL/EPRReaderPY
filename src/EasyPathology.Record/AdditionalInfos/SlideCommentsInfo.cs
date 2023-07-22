using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.AdditionalInfos;

/// <summary>
/// 使用树形结构存储SlideComment的所有情况
/// </summary>
internal class SlideCommentsInfo : IAdditionalInfo {
	public uint GetLength(int version) => 
		SelectedTree.Length + ExpandedTree.Length + CommentTree.Length;

	public AdditionalInfoType Type => AdditionalInfoType.SlideComments;

	/// <summary>
	/// 医生选择项的压缩树
	/// </summary>
	public CompressedTree<UInt32BinaryReadWriteWithHeader> SelectedTree { get; } = new();

	/// <summary>
	/// 医生展开项的压缩树
	/// </summary>
	public CompressedTree<UInt32BinaryReadWriteWithHeader> ExpandedTree { get; } = new();

	/// <summary>
	/// 医生注释的压缩树
	/// </summary>
	public CompressedTree<CharBinaryReadWriteWithHeader> CommentTree { get; } = new();

	public void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		SelectedTree.Read(br, version);
		ExpandedTree.Read(br, version);
		CommentTree.Read(br, version);
		this.EndRead(br);
	}

	public void Write(BinaryWriter bw, int version) {
		this.WriteAdditionalInfoHeader(bw, version);
		SelectedTree.Write(bw, version);
		ExpandedTree.Write(bw, version);
		CommentTree.Write(bw, version);
	}
}