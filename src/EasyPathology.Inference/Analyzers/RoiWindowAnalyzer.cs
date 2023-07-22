namespace EasyPathology.Inference.Analyzers; 

public class RoiWindowAnalyzer : IEasyPathologyRecordAnalyzer<List<RoiWindow>, RoiWindowAnalyzerParameter> {
	public List<RoiWindow> Analysis(EasyPathologyRecord epr, RoiWindowAnalyzerParameter parameter) {
		var result = new List<RoiWindow>();

		// RoiWindow里的第一个
		var firstDataFrame = epr.DataFrames[0];
		var firstDataFrameIndex = 0;
		
		var orderedRois = parameter.Rois.OrderBy(r => r.BeginFrameIndex).ToList();
		var prevAddedRoiStartIndex = 0;
		void CheckThenAdd(DataFrame currentDataFrame, int index) {
			if (currentDataFrame.BeginTime - firstDataFrame.BeginTime >= parameter.MinTimeSpan) {
				// 如果收集到的时间片大于阈值，就设为一个新的RoiWindow
				var rois = new List<Roi>();
				for (; prevAddedRoiStartIndex < orderedRois.Count; prevAddedRoiStartIndex++) {
					var roi = orderedRois[prevAddedRoiStartIndex];
					var roiStartTime = epr.DataFrames[roi.BeginFrameIndex].BeginTime;
					var roiEndTime = epr.DataFrames[roi.BeginFrameIndex].EndTime;
					if (roiEndTime >= currentDataFrame.BeginTime || roiStartTime >= currentDataFrame.BeginTime) {
						break;
					}

					if (roiStartTime >= firstDataFrame.BeginTime) {
						// TODO 这里只使用了StartTime作为判定
						rois.Add(roi);
					}
				}

				if (rois.Count >= parameter.MinRoiCont) {
					result.Add(new RoiWindow(epr.DataFrames[firstDataFrameIndex..index], rois));
				}
			}
		}

		for (var i = 1; i < epr.DataFrames.Length; i++) {
			var dataFrame = epr.DataFrames[i];
			if (dataFrame.ScreenX != firstDataFrame.ScreenX ||
			    dataFrame.ScreenY != firstDataFrame.ScreenY ||
			    dataFrame.VirtualLevel != firstDataFrame.VirtualLevel) {
				// 切片发生了移动
				CheckThenAdd(dataFrame, i);

				firstDataFrame = dataFrame;
				firstDataFrameIndex = i;
			}
		}

		CheckThenAdd(epr.DataFrames[^1], epr.DataFrames.Length);
		return result;
	}
}