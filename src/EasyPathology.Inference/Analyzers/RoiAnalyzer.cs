using System.Numerics;

namespace EasyPathology.Inference.Analyzers;

/// <summary>
/// 分析生成Roi顺序图
/// </summary>
public class RoiAnalyzer : IEasyPathologyRecordAnalyzer<List<Roi>, RoiAnalyzerParameter> {
	/// <summary>
	/// 默认的渐变色
	/// </summary>
	private static readonly GradientColor DefaultGradientColor = new(Color4B.Green, Color4B.OrangeRed);

	public List<Roi> Analysis(EasyPathologyRecord epr, RoiAnalyzerParameter parameter) {
		var result = new List<Roi>();
		var gradientColor = parameter.GradientColor ?? DefaultGradientColor;
		var max = 0;

		var zoom = epr.LevelScales;
		var levelCount = epr.MaxLevel - epr.MinLevel + 1;
		var levelWidths = new int[levelCount];
		var levelHeights = new int[levelCount];
		for (var i = 0; i < levelCount; i++) {
			levelWidths[i] = (int)(epr.MaxLevelWidth * zoom[epr.MaxLevel - i] / zoom[epr.MinLevel]);
			levelHeights[i] = (int)(epr.MaxLevelHeight * zoom[epr.MaxLevel - i] / zoom[epr.MinLevel]);
		}

		var rawClusterPoints = new List<ClusterPoint>();  // 没有聚类的
		var clusteredPoints = new List<ClusterPoint>();  // 已经聚好类的，按时间排序

		var sumR = 0d;
		var dataFrames = epr.DataFrames.Where(d => d.IsEyeTrackerDataValid && d.Velocity <= parameter.VelocityThreshold).ToList();
		if (dataFrames.Count == 0) {
			return result;
		}

		var prevLevel = dataFrames[0].VirtualLevel;
		for (var i = 0; i < dataFrames.Count; i++) {
			var dataFrame = dataFrames[i];
			if (dataFrame.VirtualLevel != prevLevel || i == dataFrames.Count - 1) {
				var eps = sumR / rawClusterPoints.Count;
				sumR = 0d;
				var clusterPointsArray = ClusterScan.GetClusters(rawClusterPoints, eps, parameter.MinPts);
				rawClusterPoints.Clear();
				if (clusterPointsArray != null) {
					clusteredPoints.AddRange(clusterPointsArray.SelectMany(static cps => cps).OrderBy(static cp => cp.FrameIndex));
				}

				prevLevel = dataFrame.VirtualLevel;
			}

			var eyeX = dataFrame.GazeX - dataFrame.ScreenX;
			var eyeY = dataFrame.GazeY - dataFrame.ScreenY;
			if (eyeX > levelWidths[epr.MaxLevel - dataFrame.VirtualLevel] || eyeY > levelHeights[epr.MaxLevel - dataFrame.VirtualLevel] || eyeX < 0 || eyeY < 0) {
				continue;
			}

			sumR += dataFrame.Radius;
			rawClusterPoints.Add(new ClusterPoint(eyeX, eyeY, dataFrame.FrameIndex, dataFrame.VirtualLevel, dataFrame.Radius));
		}

		if (clusteredPoints.Count == 0) {
			return result;
		}

		var clusteredPointsArray = clusteredPoints.ToArray();
		var prevClusterIdIndex = 0;
		var prevClusterId = clusteredPointsArray[0].ClusterId;
		var prevClusterLevel = clusteredPointsArray[0].Level;
		for (var i = 1; i < clusteredPointsArray.Length; i++) {
			var clusterPoint = clusteredPointsArray[i];
			if (clusterPoint.ClusterId == prevClusterId && clusterPoint.Level == prevClusterLevel && i != clusteredPointsArray.Length - 1) {  // 按照时间顺序，遇到了不同类别的点
				continue;
			}

			var clusterPoints = clusteredPointsArray[prevClusterIdIndex..i];

			var roi = CalculateRoi(clusterPoints, prevClusterLevel, (float)clusterPoints.Average(static cp => cp.Radius));
			if (float.IsNaN(roi.X) || float.IsNaN(roi.Y) || float.IsNaN(roi.Radius)) {
				// Debugger.Break();
				continue;
			}
			
			result.Add(roi);
			if (max < roi.PointCount) {
				max = roi.PointCount;
			}

			prevClusterIdIndex = i;
			prevClusterId = clusterPoint.ClusterId;
			prevClusterLevel = clusterPoint.Level;
		}

		for (var i = 0; i < result.Count; i++) {
			var roi = result[i];
			roi.Color = gradientColor.Eval((float)roi.PointCount / max);
			roi.Index = i + 1;
		}

		return result;
	}

	// 求外接圆圆心,根据三边相等
	private static Vector2 CalculateCircleCenter(Vector2 a, Vector2 b, Vector2 c) {
		float a1 = b.X - a.X, b1 = b.Y - a.Y, c1 = (a1 * a1 + b1 * b1) / 2;
		float a2 = c.X - a.X, b2 = c.Y - a.Y, c2 = (a2 * a2 + b2 * b2) / 2;
		var d = a1 * b2 - a2 * b1;
		return new Vector2(a.X + (c1 * b2 - c2 * b1) / d, a.Y + (a1 * c2 - a2 * c1) / d);
	}

	/// <summary>
	/// 计算Roi的辅助类
	/// </summary>
	/// <param name="p">按照FrameIndex排列好的聚类点</param>
	/// <param name="roiLevel"></param>
	/// <param name="averageRadius"></param>
	/// <returns></returns>
	private static Roi CalculateRoi(ClusterPoint[] p, int roiLevel, float averageRadius) {
		var set = new HashSet<Point2I>();  // 去重
		var distinctArray = new ClusterPoint[p.Length];
		var n = 0;
		foreach (var clusterPoint in p) {
			if (set.Contains(clusterPoint)) {
				continue;
			}

			set.Add(clusterPoint);
			distinctArray[n++] = clusterPoint;
		}
		
		p = distinctArray;
		var central = new Vector2(p[0].X, p[0].Y);
		float r = 0;
		for (var i = 1; i < n; i++) {
			if (Vector2.Distance(central, p[i]) + float.Epsilon > r) {
				central = p[i];
				r = 0;
				for (var j = 0; j < i; j++) {
					if (Vector2.Distance(central, p[j]) + float.Epsilon > r) {
						central = new Vector2((p[i].X + p[j].X) / 2f, (p[i].Y + p[j].Y) / 2f);
						r = Vector2.Distance(central, p[j]);
						for (var k = 0; k < j; k++) {
							if (Vector2.Distance(central, p[k]) + float.Epsilon > r) {
								central = CalculateCircleCenter(p[i], p[j], p[k]);
								r = Vector2.Distance(central, p[k]);
							}
						}
					}
				}
			}
		}

		return new Roi(central.X, central.Y, r + averageRadius, roiLevel, p.Length, p[0].FrameIndex, p[n - 1].FrameIndex);
	}
}