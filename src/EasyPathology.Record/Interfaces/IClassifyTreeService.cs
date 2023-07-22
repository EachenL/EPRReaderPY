using EasyPathology.Record.AdditionalInfos;

namespace EasyPathology.Record.Interfaces;

public interface IClassifyTreeService {
	ClassifyTree Load(Stream stream, ClassifyTreeLoadParameter parameter);

	void Save(ClassifyTree classifyTree, Stream stream);
}

public record ClassifyTreeLoadParameter {
	/// <summary>
	/// 只有叶子节点可以选择
	/// </summary>
	public bool OnlyLeafNodeCanCheck { get; set; }
}