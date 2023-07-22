namespace EasyPathology.Inference.Models;

/// <param name="Rois"></param>
/// <param name="MinTimeSpan">一个Window最低要持续多久</param>
/// <param name="MinRoiCont">一个Window最少包含多少Roi</param>
public record RoiWindowAnalyzerParameter(IReadOnlyList<Roi> Rois, TimeSpan MinTimeSpan, int MinRoiCont = 1);