namespace Server.Models;

/// <summary>
/// 指定读取本地epr文件的参数
/// </summary>
/// <param name="FilePath">本地epr文件的路径</param>
/// <param name="ReadHeaderOnly">是否只读取epr文件头，如果是，则epr的帧数据不会被读取</param>
public record ReadEprArgument(string FilePath, bool ReadHeaderOnly);