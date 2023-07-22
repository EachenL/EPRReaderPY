namespace EasyPathology.Inference.Interfaces;

/// <summary>
/// 结合Epr进行分析，无参
/// </summary>
/// <typeparam name="TOutput"></typeparam>
public interface IEasyPathologyRecordAnalyzer<out TOutput> {
	TOutput Analysis(EasyPathologyRecord epr);
}

public interface IEasyPathologyRecordAnalyzer<out TOutput, in TParameter> {
	TOutput Analysis(EasyPathologyRecord epr, TParameter parameter);
}