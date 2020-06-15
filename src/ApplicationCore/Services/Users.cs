using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationCore.DataAccess;
using ApplicationCore.Models;
using Microsoft.AspNetCore.Identity;
using ApplicationCore.Helpers;
using ApplicationCore.Exceptions;
using ApplicationCore.Consts;

namespace ApplicationCore.Services
{
    public interface IUsersService
    {
        Task<User> FindUserByEmailAsync(string email);
        Task<User> CreateUserAsync(string email, bool emailConfirmed);
        Task<IList<string>> GetRolesAsync(User user);

        Task<User> FindUserByIdAsync(string id);
        Task<IEnumerable<User>> FetchUsersAsync(string role = "", string keyword = "");
        IEnumerable<IdentityRole> FetchRoles();

        IEnumerable<IdentityRole> GetRolesByUserId(string userId);
        Task<bool> IsAdminAsync(User user);
    }

    public class UsersService : IUsersService
    {
        DefaultContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersService(DefaultContext context, UserManager<User> userManager, RoleManager<IdentityRole>  roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        
        
        public async Task<User> FindUserByEmailAsync(string email) => await _userManager.FindByEmailAsync(email);

        public async Task<User> CreateUserAsync(string email, bool emailConfirmed)
        {
            var user = new User
            {
                Email = email,
                UserName = email,
                EmailConfirmed = emailConfirmed
            };
            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded) return user;

            var error = result.Errors.FirstOrDefault();
            throw new CreateUserException($"{error.Code} : {error.Description}");
        }

        public async Task<IList<string>> GetRolesAsync(User user) => await _userManager.GetRolesAsync(user);

        public async Task<User> FindUserByIdAsync(string id) => await _userManager.FindByIdAsync(id);

        public async Task<IEnumerable<User>> FetchUsersAsync(string role = "", string keyword = "")
        {
            var users = _userManager.Users;

            if (!String.IsNullOrEmpty(role))
            {
                var selectedRole = await _roleManager.FindByNameAsync(role);
                if (selectedRole != null)
                {
                    var userIdsInRole = _context.UserRoles.Where(x => x.RoleId == selectedRole.Id).Select(b => b.UserId).Distinct().ToList();
                    users = users.Where(user => userIdsInRole.Contains(user.Id));
                }
            }
            

            if (String.IsNullOrEmpty(keyword)) return users;
            if (users.IsNullOrEmpty()) return users;

            return users.Where(u => u.UserName.CaseInsensitiveContains(keyword));
        }

        public IEnumerable<IdentityRole> FetchRoles() => _roleManager.Roles.ToList();

        public IEnumerable<IdentityRole> GetRolesByUserId(string userId)
        {
            var userRoles = _context.UserRoles.Where(x => x.UserId == userId);
            var roleIds = userRoles.Select(ur => ur.RoleId);

            return _roleManager.Roles.Where(r => roleIds.Contains(r.Id));
        }

        public async Task<bool> IsAdminAsync(User user)
        {
            var roles = await GetRolesAsync(user);
            if (roles.IsNullOrEmpty()) return false;
            var match = roles.Where(r => r.Equals(RoleTypes.Dev.ToString()) || r.Equals(RoleTypes.Boss.ToString())).FirstOrDefault();

            return match != null;
        }
    }
}
