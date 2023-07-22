using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using EasyPathology.Definitions.DataTypes;
using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.AdditionalInfos;

/// <summary>
/// 医生的重点关注区
/// </summary>
public sealed class Roi : IAdditionalInfo, INotifyPropertyChanged {
	/// <summary>
	/// 相对于当前Level
	/// </summary>
	public float X { get; set; }

	/// <summary>
	/// 相对于当前Level
	/// </summary>
	public float Y { get; set; }

	/// <summary>
	/// 将X, Y打包获取
	/// </summary>
	[JsonIgnore]
	public Point2F Position => new(X, Y);

	/// <summary>
	/// 获取包围Box
	/// </summary>
	[JsonIgnore]
	public Rect2F Box => new(X - Radius, Y - Radius, 2 * Radius, 2 * Radius);

	public float Radius { get; set; }

	public int Index { get; set; }

	public int PointCount { get; set; }

	public int Level { get; set; }

	public int BeginFrameIndex { get; set; }

	public int EndFrameIndex { get; set; }

	/// <summary>
	/// 只记录RGB
	/// </summary>
	public Color4B Color { get; set; }

	public string? Comment { get; set; }

	[JsonIgnore]
	public bool IsSelected {
		get => isSelected;
		set {
			if (isSelected != value) {
				isSelected = value;
				OnPropertyChanged();
			}
		}
	}

	private bool isSelected;

	public Roi(BinaryReader br, int version) {
		Read(br, version);
	}

	public Roi(float x, float y, float radius, int level, int pointCount, int beginFrameIndex, int endFrameIndex) {
		X = x;
		Y = y;
		Radius = radius;
		Level = level;
		PointCount = pointCount;
		BeginFrameIndex = beginFrameIndex;
		EndFrameIndex = endFrameIndex;
	}

	public override string ToString() => $"#{Index}\n {(string.IsNullOrWhiteSpace(Comment) ? "No Comment" : Comment)}";

	public uint GetLength(int version) =>
		3U * sizeof(float) + 5U * sizeof(int) + 3U * sizeof(byte) + Comment.GetStringSaveLength();

	[JsonIgnore]
	public AdditionalInfoType Type => AdditionalInfoType.Roi;

	public void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		X = br.ReadSingle();
		Y = br.ReadSingle();
		Radius = br.ReadSingle();
		Index = br.ReadInt32();
		PointCount = br.ReadInt32();
		Level = br.ReadInt32();
		Color = new Color4B(br.ReadByte(), br.ReadByte(), br.ReadByte());
		BeginFrameIndex = br.ReadInt32();
		EndFrameIndex = br.ReadInt32();
		Comment = br.ReadString();
		this.EndRead(br);
	}

	public void Write(BinaryWriter bw, int version) {
		this.WriteAdditionalInfoHeader(bw, version);
		bw.Write(X);
		bw.Write(Y);
		bw.Write(Radius);
		bw.Write(Index);
		bw.Write(PointCount);
		bw.Write(Level);
		bw.Write(Color.R);
		bw.Write(Color.G);
		bw.Write(Color.B);
		bw.Write(BeginFrameIndex);
		bw.Write(EndFrameIndex);
		bw.Write(Comment ?? string.Empty);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}