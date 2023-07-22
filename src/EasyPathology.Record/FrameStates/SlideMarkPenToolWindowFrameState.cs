using EasyPathology.Definitions.DataTypes;
using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.FrameStates; 

public class SlideMarkPenToolWindowFrameState : RecordableControlFrameState {
	public override FrameStateType Type => FrameStateType.SlideMarkPenToolWindow;

	public override uint GetLength(int version) {
		return base.GetLength(version) + sizeof(long) + sizeof(double) + sizeof(bool) + 2 * sizeof(uint);
	}

	public Color4B SelectedColor { get; set; }
	public double Thickness { get; set; }
	public bool IsEraser { get; set; }
	public uint ListId { get; set; }
	public uint ListLength { get; set; }

	public override void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		base.Read(br, version);
		SelectedColor = new Color4B(br.ReadInt64());
		Thickness = br.ReadDouble();
		IsEraser = br.ReadBoolean();
		ListId = br.ReadUInt32();
		ListLength = br.ReadUInt32();
		this.EndRead(br);
	}

	public override void Write(BinaryWriter bw, int version) {
		this.WriteFrameStateHeader(bw, version);
		base.Write(bw, version);
		bw.Write(SelectedColor.Value);
		bw.Write(Thickness);
		bw.Write(IsEraser);
		bw.Write(ListId);
		bw.Write(ListLength);
	}
}