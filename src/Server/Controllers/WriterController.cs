using Microsoft.AspNetCore.Mvc;
using Server.Models;
using System.ComponentModel.DataAnnotations;
using HttpResponse = EasyPathology.Definitions.Models.HttpResponse;

namespace Server.Controllers;

/// <inheritdoc />
[ApiController]
[Route("[controller]/[action]")]
public class WriterController : ControllerBase {
	/// <summary>
	/// 保存epr到本地
	/// </summary>
	/// <param name="argument"><see cref="SaveEprArgument"/></param>
	/// <returns></returns>
	[HttpPost]
	public HttpResponse SaveEpr([Required] SaveEprArgument? argument) {
		if (argument == null) {
			return new ArgumentException("Invalid argument");
		}

		using var fs = System.IO.File.OpenWrite(argument.FilePath);
		argument.EasyPathologyRecord.Save(fs);
		return new HttpResponse();
	}
}