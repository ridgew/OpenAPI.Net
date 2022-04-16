﻿using Samples.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAPI.Net.Auth;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ASP.NET.Sample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiCredentials _apiCredentials;

        public IndexModel(ApiCredentials apiCredentials)
        {
            _apiCredentials = apiCredentials;
        }

        public App App => new(_apiCredentials.ClientId, _apiCredentials.Secret, $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}{Request.Path}");

        public ActionResult OnGetAddAccount()
        {
            return Redirect(App.GetAuthUri().ToString());
        }

        public async Task<ActionResult> OnGetAsync([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            try
            {
                var token = await TokenFactory.GetToken(code, App);

                TempData["Token"] = JsonSerializer.Serialize(token);

                return RedirectToPage("ClientArea");
            }
            catch (Exception ex)
            {
                TempData["Exception"] = JsonSerializer.Serialize(ex);

                return RedirectToPage("Error");
            }
        }
    }
}