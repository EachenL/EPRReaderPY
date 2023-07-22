using EasyPathology.Definitions.DataTypes;
using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.AdditionalInfos; 

/// <summary>
/// 经过AI分割的Roi区域，由若干点组成
/// </summary>
public class SegmentedRoi : IAdditionalInfo, ITimeSeekable {
	public AdditionalInfoType Type => AdditionalInfoType.SegmentedRoi;

	public uint GetLength(int version) => 
		2 * sizeof(double) + sizeof(int) + (uint)Regions.Sum(r => sizeof(int) + r.Points.Length * sizeof(double) * 2);

	public TimeSpan BeginTime { get; set; }

	public TimeSpan EndTime { get; set; }

	/// <summary>
	/// 区域列表，每个点都是相对于最大缩放的
	/// </summary>
	public Region2D[] Regions { get; set; }

	public SegmentedRoi(BinaryReader br, int version) {
		Regions = Array.Empty<Region2D>();
		Read(br, version);
	}

	public SegmentedRoi(Region2D[] regions, TimeSpan beginTime, TimeSpan endTime) {
		Regions = regions ?? throw new ArgumentNullException(nameof(regions));
		BeginTime = beginTime;
		EndTime = endTime;
	}

	public void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		BeginTime = TimeSpan.FromMilliseconds(br.ReadDouble());
		EndTime = TimeSpan.FromMilliseconds(br.ReadDouble());

		Regions = new Region2D[br.ReadInt32()];
		for (var i = 0; i < Regions.Length; i++) {
			var points = new Point2D[br.ReadInt32()];
			for (var j = 0; j < points.Length; j++) {
				points[j] = new Point2D(br.ReadDouble(), br.ReadDouble());
			}

			Regions[i] = new Region2D(points);
		}
		this.EndRead(br);
	}

	public void Write(BinaryWriter bw, int version) {
		this.WriteAdditionalInfoHeader(bw, version);
		bw.Write(BeginTime.TotalMilliseconds);
		bw.Write(EndTime.TotalMilliseconds);

		bw.Write(Regions.Length);
		foreach (var region in Regions) {
			bw.Write(region.Points.Length);
			foreach (var point in region.Points) {
				bw.Write(point.X);
				bw.Write(point.Y);
			}
		}
	}
}