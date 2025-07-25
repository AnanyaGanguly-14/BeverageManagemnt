﻿using Common;
using DalLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
namespace BeverageManagemnt
{
    public class AdminOnlyFilter : Attribute,IAsyncAuthorizationFilter
    {
        IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        public AdminOnlyFilter(IHttpContextAccessor httpContextAccessor, AppDbContext context) { 
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext authorizationFilterContext)
        {
            ExceptionDetails exceptionDetails;
            var user = _httpContextAccessor.HttpContext?.User;
            if (user != null)
            {
                //var userId = user.FindFirst(ClaimTypes.Name)?.Value;
                var userId = ApiConstants.USERID;
                var userRole = await (from userDetails in _context.UserDetails
                                      where userDetails.USER_ACCESS_ID == userId
                                      select userDetails.USER_ROLE
                                      ).FirstOrDefaultAsync();
                if (userRole != null)
                {
                    if (!userRole.Contains(ApiConstants.USERROLE))
                    {
                        exceptionDetails = new ExceptionDetails()
                        {
                            Code = ApiConstants.AUTHORIZATIONCODE,
                            Message = ApiConstants.AUTHORIZATIONMESSAGE
                        };
                        authorizationFilterContext.Result = new JsonResult(exceptionDetails){ };
                    }
                }
                
            }
        }
    }
}
