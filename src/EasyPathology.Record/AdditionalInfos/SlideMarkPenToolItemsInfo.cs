using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.AdditionalInfos;

/// <summary>
/// 使用树形结构存储SlideMarkPenToolItems的所有情况
/// </summary>
internal class SlideMarkPenToolItemsInfo : IAdditionalInfo {
	public uint GetLength(int version) => ItemsTree.Length + PointsTree.Length;

	public AdditionalInfoType Type => AdditionalInfoType.SlideMarkPenTool;

	public CompressedTree<SlideMarkPenToolItem> ItemsTree { get; } = new();

	public CompressedTree<Point2IBinaryReadWriteWithHeader> PointsTree { get; } = new();

	public void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		ItemsTree.Read(br, version);
		PointsTree.Read(br, version);
		this.EndRead(br);
	}

	public void Write(BinaryWriter bw, int version) {
		this.WriteAdditionalInfoHeader(bw, version);
		ItemsTree.Write(bw, version);
		PointsTree.Write(bw, version);
	}
}