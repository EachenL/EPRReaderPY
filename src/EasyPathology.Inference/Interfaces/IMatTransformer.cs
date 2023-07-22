using OpenCvSharp;

namespace EasyPathology.Inference.Interfaces; 

/// <summary>
/// 将输入转换，获得输出
/// </summary>
public interface IMatTransformer {
	Mat Transform(Mat input);
}