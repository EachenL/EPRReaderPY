using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.AdditionalInfos;

/// <summary>
/// 使用树形结构存储SlideRulerToolItems的所有情况
/// </summary>
internal class SlideRulerToolItemsInfo : IAdditionalInfo {
	public uint GetLength(int version) => Tree.Length;

	public AdditionalInfoType Type => AdditionalInfoType.SlideRulerTool;

	public CompressedTree<SlideRulerToolItem> Tree { get; } = new();

	public void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		Tree.Read(br, version);
		this.EndRead(br);
	}

	public void Write(BinaryWriter bw, int version) {
		this.WriteAdditionalInfoHeader(bw, version);
		Tree.Write(bw, version);
	}
}