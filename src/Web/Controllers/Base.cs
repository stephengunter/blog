using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Helpers;
using Microsoft.AspNetCore.Authorization;
using ApplicationCore.Views;
using ApplicationCore.Settings;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.AspNetCore.Cors;
using ApplicationCore.Consts;

namespace Web.Controllers
{
	[EnableCors("Global")]
	[Route("[controller]")]
	public abstract class BaseController : ControllerBase
	{
		protected string RemoteIpAddress => Request.HttpContext.Connection.RemoteIpAddress?.ToString();

		protected string CurrentUserName => User.Claims.IsNullOrEmpty() ? "" : User.Claims.Where(c => c.Type == "sub").FirstOrDefault().Value;

		protected string CurrentUserId => User.Claims.IsNullOrEmpty() ? "" : User.Claims.Where(c => c.Type == "id").FirstOrDefault().Value;

		protected IEnumerable<string> CurrentUseRoles
		{
			get
			{
				var entity = User.Claims.Where(c => c.Type == "roles").FirstOrDefault();
				if (entity == null) return null;
				return entity.Value.Split(',');
			}
			
		}

		protected IActionResult RequestError(string key, string msg)
		{
			ModelState.AddModelError(key, msg);
			return BadRequest(ModelState);
		}

		
	}

	[EnableCors("Api")]
	[Route("api/[controller]")]
	public abstract class BaseApiController : BaseController
	{
		
	}

	//[EnableCors("Admin")]
	[Route("admin/[controller]")]
	[Authorize(Policy = "Admin")]
	public class BaseAdminController : BaseController
	{

		protected void ValidateRequest(AdminRequest model, AdminSettings adminSettings)
		{
			if (model.Key != adminSettings.Key) ModelState.AddModelError("key", "認證錯誤");

		}
	}
}
