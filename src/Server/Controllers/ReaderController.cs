using EasyPathology.Definitions.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using EasyPathology.Record;
using Server.Models;
using EasyPathology.Inference.Analyzers;
using EasyPathology.Inference.Models;
using EasyPathology.Record.AdditionalInfos;
using EasyPathology.Definitions.Interfaces;

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
		var epr = EasyPathologyRecord.LoadFromFile(argument.FilePath, argument.ReadHeaderOnly);
		if (!argument.ReadHeaderOnly) {
            var roiAnalyzer = new RoiAnalyzer();
            var roiList = roiAnalyzer.Analysis(epr, new RoiAnalyzerParameter
            {
                VelocityThreshold = epr.AdditionalInfoSet.Threshold,
                MinPts = 10
            });
            epr.AdditionalInfoSet.RoiList.AddRange(roiList);
        }
		
        return epr;
	}
}