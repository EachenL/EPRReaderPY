using EasyPathology.Core.Utils;

namespace EasyPathology.Inference.Models;

/// <summary>
/// <see cref="Analyzers.RoiAnalyzer"/>所使用的参数
/// </summary>
public class RoiAnalyzerParameter {
	public double VelocityThreshold { get; set; }

	public int MinPts { get; set; }

	/// <summary>
	/// 生成时所用的颜色渐变
	/// </summary>
	public GradientColor? GradientColor { get; set; }
}