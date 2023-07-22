using EasyPathology.Definitions.Interfaces;

namespace EasyPathology.Inference.Models;

/// <summary>
/// 代表一个时间窗口，这个时间窗口内医生没有移动切片，其中的注视点有一定的意义
/// </summary>
public record RoiWindow(IReadOnlyList<DataFrame> DataFrames, IReadOnlyList<Roi> Rois) : ITimeSeekable {
	public TimeSpan BeginTime => DataFrames[0].BeginTime;

	public TimeSpan EndTime => DataFrames[^1].EndTime;
}