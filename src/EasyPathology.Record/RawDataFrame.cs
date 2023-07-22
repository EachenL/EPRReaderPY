using EasyPathology.Definitions.DataTypes;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record;

/// <summary>
/// 用于存储到epr中的原始帧数据
/// </summary>
/// <param name="GazeX">眼睛的注视x坐标，相对于屏幕左上角</param>
/// <param name="GazeY">眼睛的注视y坐标，相对于屏幕左上角</param>
/// <param name="VirtualLevel">缩放level，需要注意这个Level并不是OpenSlide里的真实Level</param>
/// <param name="ScreenX">屏幕左上角相对于切片左上角的x坐标</param>
/// <param name="ScreenY">屏幕左上角相对于切片左上角的y坐标</param>
/// <param name="LeftEyePosition">左眼和眼动仪的相对坐标，单位是cm</param>
/// <param name="RightEyePosition">右眼和眼动仪的相对坐标，单位是cm</param>
/// <param name="IsEyeTrackerDataValid">眼动数据有效性，24版本+</param>
/// <param name="TimeStamp">单位是秒</param>
/// <param name="CursorX">鼠标在屏幕上的X坐标（像素）</param>
/// <param name="CursorY">鼠标在屏幕上的Y坐标（像素）</param>
/// <param name="FrameStates">当前帧所有“状态”的列表</param>
public record RawDataFrame(
	int GazeX, 
	int GazeY, 
	int VirtualLevel, 
	int ScreenX, 
	int ScreenY, 
	Point3F LeftEyePosition, 
	Point3F RightEyePosition, 
	bool IsEyeTrackerDataValid, 
	double TimeStamp, 
	int CursorX, 
	int CursorY,
	IFrameState[] FrameStates) {

	/// <summary>
	/// 头部距离屏幕的高度，单位是cm
	/// </summary>
	public double EyeDistanceToScreen => (LeftEyePosition.Z + RightEyePosition.Z) / 20;

	/// <summary>
	/// 时间戳，Version>=23时，单位为秒。
	/// </summary>
	/// <remarks>
	/// 要注意的是，第一帧的<see cref="TimeStamp"/>不一定是0，如果要获得绝对时间，请使用<see cref="DataFrame.BeginTime"/>
	/// </remarks>
	public double TimeStamp { get; } = TimeStamp;

	public Point2D GazePosition => new(GazeX, GazeY);

	public override string ToString() => $"{nameof(GazeX)}: {GazeX} {nameof(GazeY)}: {GazeY}";
}