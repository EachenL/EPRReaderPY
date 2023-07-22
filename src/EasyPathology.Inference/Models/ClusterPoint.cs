using System.Numerics;
using EasyPathology.Definitions.DataTypes;

namespace EasyPathology.Inference.Models;

/// <summary>
/// 代表聚类后的一个点
/// </summary>
public record ClusterPoint(int X, int Y, int FrameIndex, int Level, double Radius) {
	public const int Noise = -1;
	public const int Unclassified = 0;

	/// <summary>
	/// X坐标，相对于切片最大缩放
	/// </summary>
	public int X { get; } = X;

	/// <summary>
	/// Y坐标，相对于切片最大缩放
	/// </summary>
	public int Y { get; } = Y;

	/// <summary>
	/// 聚类编号，-1为噪声，0为未分类
	/// </summary>
	public int ClusterId { get; set; }

	/// <summary>
	/// 处于录像中的第几帧
	/// </summary>
	public int FrameIndex { get; } = FrameIndex;

	/// <summary>
	/// 第几个缩放Level，注意，不是Epr真实level
	/// </summary>
	public int Level { get; } = Level;
	
	public double Radius { get; set; } = Radius;

	public static implicit operator Vector2(ClusterPoint p) => new(p.X, p.Y);

	public static implicit operator Point2I(ClusterPoint p) => new(p.X, p.Y);

	/// <summary>
	/// 返回两个聚类点的距离平方
	/// </summary>
	/// <param name="p1"></param>
	/// <param name="p2"></param>
	/// <returns></returns>
	public static int DistanceSquared(ClusterPoint p1, ClusterPoint p2) {
		var diffX = p2.X - p1.X;
		var diffY = p2.Y - p1.Y;
		return diffX * diffX + diffY * diffY;
	}

	public override string ToString() => $"({X}, {Y}, {ClusterId}, {FrameIndex}, {Level})";
}