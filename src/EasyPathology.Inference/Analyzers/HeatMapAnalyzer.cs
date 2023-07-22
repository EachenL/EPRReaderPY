namespace EasyPathology.Inference.Analyzers;

/// <summary>
/// 分析并生成热图
/// </summary>
public class HeatMapAnalyzer : IEasyPathologyRecordAnalyzer<List<HeatMapPoint>> {
	public List<HeatMapPoint> Analysis(EasyPathologyRecord epr) {
		int width = (int)epr.InitialWidth, height = (int)epr.InitialHeight;  // 按照刚开始进入的倍率来
		var zoom = epr.LevelScales;
		var list = new List<HeatMapPoint>();
		DataFrame? prevData = null;
		foreach (var data in epr.DataFrames) {
			var x = (float)(data.GazeX - data.ScreenX) / zoom[data.VirtualLevel];  // 全部缩放到开始进入的倍率
			var y = (float)(data.GazeY - data.ScreenY) / zoom[data.VirtualLevel];
			if (data.GazeX > width || data.GazeY > height || data.GazeX < 0 || data.GazeY < 0 || prevData != null && (prevData.ScreenX != data.ScreenX || prevData.ScreenY != data.ScreenY || prevData.VirtualLevel != data.VirtualLevel)) {
				prevData = data;
				continue;
			}
			var radius = (float)Math.Round(data.Radius / zoom[data.VirtualLevel]);
			list.Add(new HeatMapPoint(x, y, radius, data.VirtualLevel));
			//if (d.Velocity < StaticInfos.THRESHOLD) {
			//	d.Radius = d.Radius * zoom[d.Level] / ratio;
			//	list.Add(d);
			//}
			prevData = data;
		}

		return list;
	}
}