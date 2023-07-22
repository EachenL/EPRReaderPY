using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.FrameStates;

public class SlideToolsWindowFrameState : RecordableControlFrameState {
	public override FrameStateType Type => FrameStateType.SlideToolsWindow;

	public override void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		base.Read(br, version);
		this.EndRead(br);
	}

	public override void Write(BinaryWriter bw, int version) {
		this.WriteFrameStateHeader(bw, version);
		base.Write(bw, version);
	}
}