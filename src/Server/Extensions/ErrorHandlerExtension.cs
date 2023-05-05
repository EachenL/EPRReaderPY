using System.Net;
using EasyPathology.Definitions.Interfaces;
using Microsoft.AspNetCore.Diagnostics;

namespace Server.Extensions;

/// <summary>
/// 处理错误并写入日志
/// </summary>
public static class ErrorHandlerExtension {
	/// <summary>
	/// 用于ILoggerService的泛型参数
	/// </summary>
	[Serializable]
	public class ErrorHandlerMiddleWare { }

	/// <summary>
	/// 添加异常处理的拓展方法
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder builder) {
		return builder.UseExceptionHandler(handler => {
			var logger = handler.ApplicationServices.GetRequiredService<ILoggerService<ErrorHandlerMiddleWare>>();

			handler.Run(c => {
				var e = c.Features.Get<IExceptionHandlerFeature>();
				if (e == null) {
					return Task.CompletedTask;
				}

				c.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				c.Response.ContentType = "application/json";
				logger.Exception(e.Error);
				return c.Response.Body.WriteAsync(
					System.Text.Encoding.UTF8.GetBytes(
						System.Text.Json.JsonSerializer.Serialize(e.Error)
					)
				).AsTask();
			});
		});
	}
}