using EasyPathology.Inference.Models;

namespace EasyPathology.Inference.Utils; 

internal static class ClusterScan {
	public static List<List<ClusterPoint>>? GetClusters(List<ClusterPoint> points, double eps, int minPts) {
		if (points.Count == 0) {
			return null;
		}
		eps *= eps;
		var clusterId = 1;
		foreach (var _ in points.Where(static p => p.ClusterId == ClusterPoint.Unclassified).Where(p => ExpandCluster(points, p, clusterId, eps, minPts))) {
			clusterId++;
		}
		// sort out points into their clusters, if any
		var maxClusterId = points.OrderBy(static p => p.ClusterId).Last().ClusterId;
		if (maxClusterId < 1) {
			return null; // no clusters, so list is empty
		}
		var clusters = new List<List<ClusterPoint>>(maxClusterId);
		for (var i = 0; i < maxClusterId; i++) {
			clusters.Add(new List<ClusterPoint>());
		}
		foreach (var p in points.Where(static p => p.ClusterId > 0)) {
			clusters[p.ClusterId - 1].Add(p);
		}
		return clusters;
	}

	private static List<ClusterPoint> GetRegion(IEnumerable<ClusterPoint> points, ClusterPoint p, double eps) {
		return (from t in points let distSquared = ClusterPoint.DistanceSquared(p, t) where distSquared <= eps select t).ToList();
	}

	private static bool ExpandCluster(IReadOnlyCollection<ClusterPoint> points, ClusterPoint p, int clusterId, double eps, int minPts) {
		var seeds = GetRegion(points, p, eps);
		if (seeds.Count < minPts) {  // no core point
			p.ClusterId = ClusterPoint.Noise;
			return false;
		} // all points in seeds are density reachable from point 'p'
		foreach (var seed in seeds) {
			seed.ClusterId = clusterId;
		}
		seeds.Remove(p);
		while (seeds.Count > 0) {
			var currentP = seeds[0];
			var result = GetRegion(points, currentP, eps);
			if (result.Count >= minPts) {
				foreach (var resultP in result.Where(static resultP => resultP.ClusterId is ClusterPoint.Unclassified or ClusterPoint.Noise)) {
					if (resultP.ClusterId == ClusterPoint.Unclassified) {
						seeds.Add(resultP);
					}
					resultP.ClusterId = clusterId;
				}
			}
			seeds.Remove(currentP);
		}
		return true;
	}
}