using EasyPathology.Record;

namespace Server.Models;

/// <summary>
/// 指定保存本地epr文件的参数
/// </summary>
/// <param name="FilePath">本地epr文件的路径</param>
/// <param name="EasyPathologyRecord">要转换成epr的json</param>
public record SaveEprArgument(string FilePath, EasyPathologyRecord EasyPathologyRecord);