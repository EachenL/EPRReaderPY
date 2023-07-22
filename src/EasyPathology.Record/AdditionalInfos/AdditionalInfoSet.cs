using System.Text.Json.Serialization;
using EasyPathology.Core.Collections;
using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.AdditionalInfos;

/// <summary>
/// 存储在epr文件中的附加信息
/// </summary>
public class AdditionalInfoSet {
	public double Threshold {
		get => GetAdditionalInfo<ThresholdInfo>(AdditionalInfoType.Threshold).Threshold;
		set => GetAdditionalInfo<ThresholdInfo>(AdditionalInfoType.Threshold).Threshold = value;
	}

	public ConcurrentObservableCollection<Roi> RoiList {
		get => GetAdditionalInfo<RoiListInfo>(AdditionalInfoType.RoiList).RoiList;
		set {
			// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			if (value == null) {
				GetAdditionalInfo<RoiListInfo>(AdditionalInfoType.RoiList).RoiList.Clear();
			} else {
				GetAdditionalInfo<RoiListInfo>(AdditionalInfoType.RoiList).RoiList.Reset(value);
			}
		}
	}

	public ClassifyTree DiagnosisClassifyTree {
		get => GetAdditionalInfo<ClassifyTree>(AdditionalInfoType.DiagnosisClassifyTree);
		set {
			// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			if (value != null) {
				additionalInfos[AdditionalInfoType.DiagnosisClassifyTree] = value;
			}
		}
	}

	[JsonIgnore]
	public CompressedTree<SlideRulerToolItem> SlideRulerToolItemsTree => GetAdditionalInfo<SlideRulerToolItemsInfo>(AdditionalInfoType.SlideRulerTool).Tree;

	[JsonIgnore]
	public CompressedTree<UInt32BinaryReadWriteWithHeader> SlideCommentSelectedTree => GetAdditionalInfo<SlideCommentsInfo>(AdditionalInfoType.SlideComments).SelectedTree;

	[JsonIgnore]
	public CompressedTree<UInt32BinaryReadWriteWithHeader> SlideCommentExpandedTree => GetAdditionalInfo<SlideCommentsInfo>(AdditionalInfoType.SlideComments).ExpandedTree;

	[JsonIgnore]
	public CompressedTree<CharBinaryReadWriteWithHeader> SlideCommentCommentTree => GetAdditionalInfo<SlideCommentsInfo>(AdditionalInfoType.SlideComments).CommentTree;

	[JsonIgnore] // EPR里有这个数据
	public DoctorDetailInfo DoctorDetailInfo {
		get => GetAdditionalInfo<DoctorDetailInfo>(AdditionalInfoType.DoctorDetailInfo);
		set {
			// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			if (value != null) {
				additionalInfos[AdditionalInfoType.DoctorDetailInfo] = value;
			}
		}
	}

	public ConcurrentObservableCollection<SegmentedRoi> SegmentedRoiList {
		get => GetAdditionalInfo<SegmentedRoiListInfo>(AdditionalInfoType.SegmentedRoiList).SegmentedRoiList;
		set {
			// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			if (value == null) {
				GetAdditionalInfo<SegmentedRoiListInfo>(AdditionalInfoType.SegmentedRoiList).SegmentedRoiList.Clear();
			} else {
				GetAdditionalInfo<SegmentedRoiListInfo>(AdditionalInfoType.SegmentedRoiList).SegmentedRoiList.Reset(value);
			}
		}
	}

	[JsonIgnore]
	public CompressedTree<SlideMarkPenToolItem> SlideMarkPenToolItemsTree => GetAdditionalInfo<SlideMarkPenToolItemsInfo>(AdditionalInfoType.SlideMarkPenTool).ItemsTree;

	[JsonIgnore]
	public CompressedTree<Point2IBinaryReadWriteWithHeader> SlideMarkPenToolPointsTree => GetAdditionalInfo<SlideMarkPenToolItemsInfo>(AdditionalInfoType.SlideMarkPenTool).PointsTree;

	private readonly Dictionary<AdditionalInfoType, IAdditionalInfo> additionalInfos = new();

	/// <summary>
	/// 根据Type获取，如果没有则new并返回
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	private T GetAdditionalInfo<T>(AdditionalInfoType type) where T : IAdditionalInfo, new() {
		if (!additionalInfos.TryGetValue(type, out var info)) {
			info = new T();
			additionalInfos.Add(type, info);
		}

		return (T)info;
	}

	private static IAdditionalInfo ParseType(AdditionalInfoType type) {
		return type switch {
			AdditionalInfoType.Threshold => new ThresholdInfo(),
			AdditionalInfoType.RoiList => new RoiListInfo(),
			AdditionalInfoType.SlideRulerTool => new SlideRulerToolItemsInfo(),
			AdditionalInfoType.SlideComments => new SlideCommentsInfo(),
			AdditionalInfoType.DiagnosisClassifyTree => new ClassifyTree(),
			AdditionalInfoType.DoctorDetailInfo => new DoctorDetailInfo(),
			AdditionalInfoType.SlideMarkPenTool => new SlideMarkPenToolItemsInfo(),
			AdditionalInfoType.SegmentedRoiList => new SegmentedRoiListInfo(),
			_ => throw new AdditionalInfoTypeUnknownException(type)
		};
	}

	public void Read(BinaryReader br, int version) {
		var infoCount = br.ReadUInt16();
		for (var i = 0; i < infoCount; i++) {
			var type = br.ReadAdditionInfoType();
			var info = ParseType(type);
			info.Read(br, version);
			additionalInfos.Add(type, info);
		}
	}

	public void Write(BinaryWriter bw, int version) {
		bw.Write((ushort)additionalInfos.Count);
		foreach (var info in additionalInfos.Values) {
			info.Write(bw, version);
		}
	}
}
