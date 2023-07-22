using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasyPathology.Core.Strings;
using EasyPathology.Definitions.DataTypes;
using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.AdditionalInfos;

/// <summary>
/// 测量Slide长度的一项，可以根据OpenSlider里记录的micronsPerPixel计算长度
/// </summary>
public sealed class SlideRulerToolItem : IBinaryReadWriteWithHeader, INotifyPropertyChanged {
	public uint Id { get; private set; }

	/// <summary>
	/// 起点，相对于切片最大缩放
	/// </summary>
	public Point2D P1 { get; set; } = Point2D.NaN;

	/// <summary>
	/// 终点，相对于切片最大缩放
	/// </summary>
	public Point2D P2 { get; set; } = Point2D.NaN;

	/// <summary>
	/// 可以获取到真实的长度，调用<see cref="Calculate"/>来计算
	/// </summary>
	public double ActualLength { get; private set; }

	/// <summary>
	/// 用于绑定到前端
	/// </summary>
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

	public uint GetLength(int version) {
		if (version >= 24) {
			return 1U * sizeof(uint) + 4U * sizeof(double) + sizeof(double);
		}

		return 1U * sizeof(uint) + 4U * sizeof(int) + sizeof(double);
	}

	public void Read(BinaryReader br, int version) {
		Id = br.ReadUInt32();
		if (version >= 24) {
			P1 = new Point2D(br.ReadDouble(), br.ReadDouble());
			P2 = new Point2D(br.ReadDouble(), br.ReadDouble());
		} else {
			P1 = new Point2D(br.ReadInt32(), br.ReadInt32());
			P2 = new Point2D(br.ReadInt32(), br.ReadInt32());
		}
		ActualLength = br.ReadDouble();
	}

	public void Write(BinaryWriter bw, int version) {
		bw.Write(Id);
		if (version >= 24) {
			bw.Write(P1.X);
			bw.Write(P1.Y);
			bw.Write(P2.X);
			bw.Write(P2.Y);
		} else {
			bw.Write((int)P1.X);
			bw.Write((int)P1.Y);
			bw.Write((int)P2.X);
			bw.Write((int)P2.Y);
		}

		bw.Write(ActualLength);
	}

	public void Reset() {
		P1 = P2 = Point2D.NaN;
		ActualLength = 0d;
	}

	/// <summary>
	/// 根据P1和P2计算<see cref="ActualLength"/>的值
	/// </summary>
	/// <param name="micronsPerPixelX">X轴每像素多少英尺，不能小于等于0</param>
	/// <param name="micronsPerPixelY">Y轴每像素多少英尺，不能小于等于0</param>
	public void Calculate(double micronsPerPixelX, double micronsPerPixelY) {
		if (micronsPerPixelX <= 0) {
			throw new ArgumentOutOfRangeException(nameof(micronsPerPixelX), Resources.SlideLengthMeasure_Calculate_MicronsPerPixelXCannotBeZeroOrNegative);
		}
		if (micronsPerPixelY <= 0) {
			throw new ArgumentOutOfRangeException(nameof(micronsPerPixelY), Resources.SlideLengthMeasure_Calculate_MicronsPerPixelYCannotBeZeroOrNegative);
		}
		var xLength = (P2.X - P1.X) * micronsPerPixelX;
		var yLength = (P2.Y - P1.Y) * micronsPerPixelY;
		ActualLength = Math.Sqrt(xLength * xLength + yLength * yLength);
	}

	/// <summary>
	/// 设置终点，相对于切片最大缩放
	/// </summary>
	/// <param name="point"></param>
	/// <param name="micronsPerPixelX"></param>
	/// <param name="micronsPerPixelY"></param>
	public void SetEndPoint(Point2D point, double micronsPerPixelX, double micronsPerPixelY) {
		P2 = point;
		Calculate(micronsPerPixelX, micronsPerPixelY);
	}

	/// <summary>
	/// 将NowMeasure复制一份，仅仅克隆值，Id会自增
	/// </summary>
	/// <returns></returns>
	public SlideRulerToolItem Clone() {
		return new SlideRulerToolItem {
			Id = ++Id,
			P1 = P1,
			P2 = P2,
			ActualLength = ActualLength,
		};
	}

	//public static implicit operator Rect(SlideRulerTool sm) 
	//	=> new(sm.P1.x, sm.P1.y, sm.P2.x - sm.P1.x, sm.P2.y - sm.P1.y);

	public override bool Equals(object? other) => other is SlideRulerToolItem slm && Equals(slm);

	public bool Equals(SlideRulerToolItem other) => Id == other.Id;

	// ReSharper disable once NonReadonlyMemberInGetHashCode
	public override int GetHashCode() => (int)Id;

	public override string ToString() => $"{Id} ({P1.X}, {P1.Y})→({P2.X}, {P2.Y}): {ActualLength:0.0} μm";

	public event PropertyChangedEventHandler? PropertyChanged;

	private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}