using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ApplicationCore.Helpers;
using ApplicationCore.Consts;

namespace ApplicationCore.Authorization
{
	public static class IdendityHelpers
	{
		public static string CurrentUserId(this AuthorizationHandlerContext context)
		{
			var entity = context.User.Claims.Where(c => c.Type == "id").FirstOrDefault();
			if (entity == null) return "";

			return entity.Value;
		}

		public static IEnumerable<string> CurrentUseRoles(this AuthorizationHandlerContext context)
		{
			var entity = context.User.Claims.Where(c => c.Type == "roles").FirstOrDefault();
			if (entity == null) return null;


			return entity.Value.Split(',');
		}


		public static string CurrentUserName(this AuthorizationHandlerContext context)
		{
			var entity = context.User.Claims.Where(c => c.Type == "sub").FirstOrDefault();
			if (entity == null) return "";

			return entity.Value;


		}
		public static bool CurrentUserIsDev(this AuthorizationHandlerContext context)
		{
			var roles = CurrentUseRoles(context);
			if (roles.IsNullOrEmpty()) return false;

			var match = roles.Where(r => r.EqualTo(RoleTypes.Dev.ToString())).FirstOrDefault();

			return match != null;
		}

		public static bool CurrentUserIsBoss(this AuthorizationHandlerContext context)
		{
			var roles = CurrentUseRoles(context);
			if (roles.IsNullOrEmpty()) return false;

			var match = roles.Where(r => r.EqualTo(RoleTypes.Boss.ToString())).FirstOrDefault();

			return match != null;
		}


	}
}
