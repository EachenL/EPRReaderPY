namespace EasyPathology.Record.Interfaces;

public enum FrameStateType : byte {
	Unknown,

	/// <summary>
	/// 医生的下诊断窗口
	/// </summary>
	SlideCommentWindow,

	/// <summary>
	/// 切片工具选择窗口
	/// </summary>
	SlideToolsWindow,

	/// <summary>
	/// 尺子工具
	/// </summary>
	SlideRulerToolWindow,

	/// <summary>
	/// 标记笔工具
	/// </summary>
	SlideMarkPenToolWindow,

	/// <summary>
	/// 细胞核探测工具
	/// </summary>
	CellNucleiDetectionTool,

	Next = 0xFF,
}

/// <summary>
/// 每一帧的附加状态，例如是否处于测量模式、窗口的位置等
/// </summary>
public interface IFrameState : IBinaryReadWriteWithHeader {
	public static int HeaderLength => sizeof(FrameStateType) + sizeof(uint);

	FrameStateType Type { get; }

	/// <summary>
	/// 两个状态是否相同，播放时如果与前一个状态相同就不播放
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	bool Equals(IFrameState? other);
}

