using System.Runtime.InteropServices;
using EasyPathology.Definitions.Interfaces;

namespace EasyPathology.Inference.Models; 

/// <summary>
/// 热力图的一个点
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct HeatMapPoint : IBvhPoint {
	/// <summary>
	/// 相对于刚开始进入的倍率
	/// </summary>
	public float X { get; }

	public float Y { get; }

	/// <summary>
	/// 相对于刚开始进入的倍率
	/// </summary>
	public float Radius { get; }

	/// <summary>
	/// EP的Level，不是slide中的level
	/// </summary>
	public int Level { get; }

	/// <summary>
	/// 考虑半径
	/// </summary>
	public BoundsF Bounds => new(X - Radius, Y - Radius, X + Radius, Y + Radius);

	/// <summary>
	/// 中点
	/// </summary>
	public BoundsF CentroidBounds => new(X, Y, X, Y);

	public HeatMapPoint(float x, float y, float radius, int level) {
		X = x;
		Y = y;
		Radius = radius;
		Level = level;
	}
}