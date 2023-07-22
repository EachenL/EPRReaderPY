using EasyPathology.Definitions.DataTypes;
using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyPathology.Record.FrameStates;

public class SlideRulerToolWindowFrameState : RecordableControlFrameState {
	public override FrameStateType Type => FrameStateType.SlideRulerToolWindow;

	public override uint GetLength(int version) {
		var size = (IsVisible ? 7U * sizeof(int) : 0U) + base.GetLength(version);
		if (IsVisible) {
			if (version >= 24) {
				size += 4 * (sizeof(double) - sizeof(int)) - sizeof(int) + sizeof(bool);
			}
		}

		return size;
	}

	public Point2D Point1 { get; set; } = Point2D.NaN;
	public Point2D Point2{ get; set; } = Point2D.NaN;
	public uint ListId { get; set; }
	public uint ListLength{ get; set; }
	public bool IsEraser { get; set; }

	public override void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		base.Read(br, version);
		if (IsVisible) {
			if (version >= 24) {
				Point1 = new Point2D(br.ReadDouble(), br.ReadDouble());
				Point2 = new Point2D(br.ReadDouble(), br.ReadDouble());
			} else {
				Point1 = new Point2D(br.ReadInt32(), br.ReadInt32());
				Point2 = new Point2D(br.ReadInt32(), br.ReadInt32());
			}
			ListId = br.ReadUInt32();
			ListLength = br.ReadUInt32();
			switch (version) {
			case < 24:
				br.BaseStream.Seek(sizeof(int), SeekOrigin.Current);  // SelectedIndex
				break;
			case >= 24:
				IsEraser = br.ReadBoolean();
				break;
			}
		}
		this.EndRead(br);
	}

	public override void Write(BinaryWriter bw, int version) {
		this.WriteFrameStateHeader(bw, version);
		base.Write(bw, version);
		if (IsVisible) {
			if (version >= 24) {
				bw.Write(Point1.X);
				bw.Write(Point1.Y);
				bw.Write(Point2.X);
				bw.Write(Point2.Y);
			} else {
				bw.Write((int)Point1.X);
				bw.Write((int)Point1.Y);
				bw.Write((int)Point2.X);
				bw.Write((int)Point2.Y);
			}
			bw.Write(ListId);
			bw.Write(ListLength);
			switch (version) {
			case < 24:
				bw.Write(-1);  // SelectedIndex
				break;
			case >= 24:
				bw.Write(IsEraser);
				break;
			}
		}
	}

	public override bool Equals(IFrameState? other) =>
		// ReSharper disable once IdentifierTypo
		base.Equals(other) && (other is SlideRulerToolWindowFrameState slms) &&
		(Point1 == slms.Point1) && (Point2 == slms.Point2) &&
		(ListId == slms.ListId) && (ListLength == slms.ListLength);

	public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = false, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals });
}