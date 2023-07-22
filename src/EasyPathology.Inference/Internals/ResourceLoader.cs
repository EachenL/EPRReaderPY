using EasyPathology.Core.Extensions;

namespace EasyPathology.Inference.Internals; 

internal static class ResourceLoader {
	public static string BasePath { get; } =
		Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(ResourceLoader).Assembly.Location).NotNull(), "Resources"));

	/// <summary>
	/// 通过相对路径获取绝对路径
	/// </summary>
	/// <remarks>
	/// 给定<b>segment_anything/decoder.onnx</b>，返回绝对路径
	/// </remarks>
	/// <param name="relativePath"></param>
	/// <returns></returns>
	public static string GetResourcePath(string relativePath) {
		return Path.GetFullPath(Path.Combine(BasePath, relativePath));
	}
}