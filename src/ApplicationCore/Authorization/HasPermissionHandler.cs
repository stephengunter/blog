using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore.Helpers;

namespace ApplicationCore.Authorization
{
	public class HasPermissionHandler : AuthorizationHandler<HasPermissionRequirement>
	{

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasPermissionRequirement requirement)
		{
			Permissions permissione = requirement.Permission;

			
			if(permissione == Permissions.Admin)
			{
				if(context.CurrentUserIsBoss() || context.CurrentUserIsDev())
				{
					context.Succeed(requirement);
					return Task.CompletedTask;
				}
				
			}

			context.Fail();
			return Task.CompletedTask;
		}
	}
}
