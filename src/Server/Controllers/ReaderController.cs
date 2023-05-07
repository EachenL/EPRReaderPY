using EasyPathology.Definitions.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using EasyPathology.Record;
using Server.Models;

namespace Server.Controllers;

/// <inheritdoc />
[ApiController]
[Route("[controller]/[action]")]
public class ReaderController : ControllerBase {
	/// <summary>
	/// 读取一个本地的epr文件
	/// </summary>
	/// <param name="argument"><see cref="ReadEprArgument"/></param>
	/// <returns></returns>
	/// <remarks>
	///		readHeaderOnly为true时，epr的帧数据不会被读取，只会读取医生信息、快速哈希、切片信息等，大量节省了读取帧数据的时间
	/// </remarks>
	[HttpPost]
	public HttpResponse<EasyPathologyRecord> ReadEpr([Required] ReadEprArgument? argument) {
		if (argument == null) {
			return new ArgumentException("Invalid argument");
		}

		return EasyPathologyRecord.LoadFromFile(argument.FilePath, argument.ReadHeaderOnly);
	}
}