using System.Diagnostics;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.FrameStates;

public static class FrameState {
	private static readonly Dictionary<FrameStateType, Func<IFrameState>> RegisteredFrameStates = new() {
		{ FrameStateType.SlideCommentWindow, static () => new SlideCommentWindowFrameState() },
		{ FrameStateType.SlideToolsWindow, static () => new SlideToolsWindowFrameState() },
		{ FrameStateType.SlideRulerToolWindow, static () => new SlideRulerToolWindowFrameState() },
		{ FrameStateType.SlideMarkPenToolWindow, static () => new SlideMarkPenToolWindowFrameState() },
	};

	public static FrameStateType ReadFrameStateType(this BinaryReader br) {
		try {
			return (FrameStateType)br.ReadByte();
		} catch (InvalidCastException) {
			return FrameStateType.Unknown;
		}
	}

	public static IFrameState ReadFrameState(this BinaryReader br, int version) {
		if (RegisteredFrameStates.TryGetValue(br.ReadFrameStateType(), out var frameStateInitializer)) {
			var frameState = frameStateInitializer();
			frameState.Read(br, version);
			return frameState;
		}

		throw new FrameStateTypeUnknownException();
	}

	public static void WriteFrameStateHeader(this IFrameState frameState, BinaryWriter bw, int version) {
		Debug.Assert(frameState != null);
		bw.Write((byte)frameState.Type);
		bw.Write(frameState.GetLength(version));
	}
}