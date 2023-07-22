namespace EasyPathology.Inference.Interfaces;

/// <summary>
/// 结合Slide图像进行分析
/// <remarks>在Preprocess的时候传入当前视图的slide图像进行预处理</remarks>
/// </summary>
/// <typeparam name="TOutput"></typeparam>
/// <typeparam name="TParameter"></typeparam>
public interface ISlideAnalyzer<out TOutput, in TParameter> {
	void Preprocess(Mat slideMat);

	TOutput Analysis(TParameter parameter);
}