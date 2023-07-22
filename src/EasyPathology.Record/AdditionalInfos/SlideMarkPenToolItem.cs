using System.Collections.Specialized;
using EasyPathology.Definitions.DataTypes;
using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.AdditionalInfos; 

/// <summary>
/// 标记笔的墨迹
/// </summary>
public class SlideMarkPenToolItem : IBinaryReadWriteWithHeader {
	public Color4B Color { get; set; }
	/// <summary>
	/// 相对于最大倍率的
	/// </summary>
	public double Thickness { get; set; }

	public uint ListId { get; set; }
	public uint ListLength { get; set; }
	public CompressedTree<Point2IBinaryReadWriteWithHeader>.RecordableList Points { get; } = new();

	/// <summary>
	/// AABB包围盒
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public Rect2I AABB { get; private set; } = Rect2I.Empty;

	public SlideMarkPenToolItem() {
		Points.CollectionChanged += Points_OnCollectionChanged;
	}

	public SlideMarkPenToolItem(Color4B color, double thickness = 4d) : this() {
		Color = color;
		Thickness = thickness;
	}

	public uint GetLength(int version) {
		return sizeof(long) + sizeof(double) + 2 * sizeof(uint);
	}

	public void Read(BinaryReader br, int version) {
		Color = new Color4B(br.ReadInt64());
		Thickness = br.ReadDouble();
		ListId = br.ReadUInt32();
		ListLength = br.ReadUInt32();
	}

	public void Write(BinaryWriter bw, int version) {
		bw.Write(Color.Value);
		bw.Write(Thickness);
		bw.Write(ListId);
		bw.Write(ListLength);
	}

	private void Points_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		if (e.Action == NotifyCollectionChangedAction.Add) {
			// 只是添加新项，就直接Union计算新的AABB
			foreach (Point2IBinaryReadWriteWithHeader newItem in e.NewItems!) {
				AABB = AABB.Union(newItem.Value);
			}
		} else {
			// 其余情况要直接重新计算所有AABB，这种情况很少
			AABB = Points.Aggregate(Rect2I.Empty, (current, point) => current.Union(point.Value));
		}
	}
}