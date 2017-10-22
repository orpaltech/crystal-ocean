using System;
using CrystalOcean.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace CrystalOcean.Web.Extensions
{
    public static class UrlHelperExtensions
    {
        public static String EmailConfirmationLink(this IUrlHelper urlHelper, String email, String code, String scheme)
        {
            return urlHelper.Action(
                action: nameof(AccountController.ConfirmEmail),
                controller: "Account",
                values: new { email, code },
                protocol: scheme);
        }

        public static String ResetPasswordCallbackLink(this IUrlHelper urlHelper, String email, String code, String scheme)
        {
            return urlHelper.Action(
                action: nameof(AccountController.ResetPassword),
                controller: "Account",
                values: new { email, code },
                protocol: scheme);
        }
    }
}