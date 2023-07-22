using EasyPathology.Core.Utils;
using Microsoft.ML.OnnxRuntime;

namespace EasyPathology.Inference.Utils;

/// <summary>
/// 自动管理onnx InferenceSession的生命周期
/// </summary>
public class InferenceSessionPool : ObjectPool<object, InferenceSession> {
	public static InferenceSessionPool Shared { get; }

	static InferenceSessionPool() {
		Shared = new InferenceSessionPool();
	}

	public InferenceSessionPool() : base(Generator, Processor) { }

	/// <summary>
	/// 将路径标准化处理
	/// </summary>
	/// <param name="onnxPathOrBytes"></param>
	/// <returns></returns>
	private static object Processor(object onnxPathOrBytes) {
		if (onnxPathOrBytes is string onnxPath) {
			return Path.GetFullPath(Environment.ExpandEnvironmentVariables(onnxPath));
		}

		return onnxPathOrBytes;
	}

	private static InferenceSession Generator(object onnxPathOrBytes) {
		return onnxPathOrBytes switch {
			string onnxPath => new InferenceSession(onnxPath),
			byte[] onnxBytes => new InferenceSession(onnxBytes),
			_ => throw new ArgumentException(@"Invalid argument type", nameof(onnxPathOrBytes))
		};
	}
}