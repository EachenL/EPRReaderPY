using EasyPathology.Definitions.DataTypes;
using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record;

/// <summary>
///     用于储存每一帧的数据，包含处理后数据
/// </summary>
public record DataFrame :
	RawDataFrame,
	ITimeSeekable {
	/// <summary>
	///     用于储存每一帧的数据，包含处理后数据
	/// </summary>
	/// <param name="rawDataFrame"></param>
	/// <param name="beginTime">这一帧的起始时间。由于第一帧的时间戳往往大于0，这个是减去第一帧后运算出来的</param>
	/// <param name="duration"></param>
	/// <param name="gazePosition"></param>
	/// <param name="rightEyePosition"></param>
	/// <param name="radius">视野焦点半径（像素，相对于最大缩放)</param>
	/// <param name="blinkValue">一个0-1的值，表示眨眼，1完全闭眼，0完全睁开</param>
	/// <param name="velocity">视线的角速度，角度制</param>
	/// <param name="frameIndex">当前帧的下标</param>
	/// <param name="frameStates"></param>
	/// <param name="isFrameStateEqualsToPrevFrame">这个记录当前帧的某个State与上一帧的是否相同</param>
	/// <param name="leftEyePosition"></param>
	public DataFrame(RawDataFrame rawDataFrame,
		TimeSpan beginTime,
		TimeSpan duration,
		Point2D gazePosition,
		Point3F leftEyePosition,
		Point3F rightEyePosition,
		double radius,
		double blinkValue,
		double velocity,
		int frameIndex,
		IFrameState[] frameStates,
		bool[] isFrameStateEqualsToPrevFrame) :
			base((int)gazePosition.X,
			(int)gazePosition.Y,
			rawDataFrame.VirtualLevel,
			rawDataFrame.ScreenX,
			rawDataFrame.ScreenY,
			leftEyePosition,
			rightEyePosition,
			rawDataFrame.IsEyeTrackerDataValid,
			rawDataFrame.TimeStamp,
			rawDataFrame.CursorX,
			rawDataFrame.CursorY,
			frameStates) {
		BeginTime = beginTime;
		EndTime = beginTime + duration;
		Radius = radius;
		BlinkValue = blinkValue;
		Velocity = velocity;
		FrameIndex = frameIndex;
		IsFrameStateEqualsToPrevFrame = isFrameStateEqualsToPrevFrame;
	}

	/// <summary>这一帧的起始时间。由于第一帧的时间戳往往大于0，这个是减去第一帧后运算出来的</summary>
	public TimeSpan BeginTime { get; }

	public TimeSpan EndTime { get; }

	/// <summary>视野焦点半径（像素，相对于最大缩放)</summary>
	public double Radius { get; }

	/// <summary>一个0-1的值，表示眨眼，1完全闭眼，0完全睁开</summary>
	public double BlinkValue { get; }

	/// <summary>视线的角速度，角度制</summary>
	public double Velocity { get; }

	/// <summary>当前帧的下标</summary>
	public int FrameIndex { get; }

	public bool[] IsFrameStateEqualsToPrevFrame { get; }
}