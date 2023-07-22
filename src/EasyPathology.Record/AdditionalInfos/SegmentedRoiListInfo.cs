using EasyPathology.Core.Collections;
using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.AdditionalInfos; 

public class SegmentedRoiListInfo : IAdditionalInfo {
	public uint GetLength(int version) => 
		sizeof(int) + (uint)SegmentedRoiList.Sum(segmentedRoi => segmentedRoi.GetLength(version));

	public AdditionalInfoType Type => AdditionalInfoType.SegmentedRoiList;

	public ConcurrentObservableCollection<SegmentedRoi> SegmentedRoiList { get; } = new();

	public void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		SegmentedRoiList.Clear();
		var count = br.ReadUInt32();
		for (var i = 0; i < count; i++) {
			AdditionalInfoUtils.CheckType(br, AdditionalInfoType.SegmentedRoi);
			SegmentedRoiList.Add(new SegmentedRoi(br, version));
		}
		this.EndRead(br);
	}

	public void Write(BinaryWriter bw, int version) {
		this.WriteAdditionalInfoHeader(bw, version);
		bw.Write(SegmentedRoiList.Count);
		foreach (var roi in SegmentedRoiList) {
			roi.Write(bw, version);
		}
	}
}