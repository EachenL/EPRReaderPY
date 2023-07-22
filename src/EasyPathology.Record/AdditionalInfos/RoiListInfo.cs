using EasyPathology.Core.Collections;
using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.AdditionalInfos;

/// <summary>
/// 记录了一组Roi数据
/// </summary>
internal class RoiListInfo : IAdditionalInfo {
	public uint GetLength(int version) => 
		sizeof(int) + (uint)RoiList.Sum(roi => roi.GetLength(version));

	public AdditionalInfoType Type => AdditionalInfoType.RoiList;
		
	public ConcurrentObservableCollection<Roi> RoiList { get; } = new();

	public void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		RoiList.Clear();
		var count = br.ReadUInt32();
		for (var i = 0; i < count; i++) {
			AdditionalInfoUtils.CheckType(br, AdditionalInfoType.Roi);
			RoiList.Add(new Roi(br, version));
		}
		this.EndRead(br);
	}

	public void Write(BinaryWriter bw, int version) {
		this.WriteAdditionalInfoHeader(bw, version);
		bw.Write(RoiList.Count);
		foreach (var roi in RoiList) {
			roi.Write(bw, version);
		}
	}
}