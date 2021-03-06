﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using WebApi.Services.Interop;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ProfilesController : Controller
    {
        private readonly IProfilesService service_;

        public ProfilesController(IProfilesService service)
        {
            service_ = service;
        }

        // GET profiles
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<ActionResult> GetUserProfiles()
        {
            var profiles = await service_
                    .GetProfiles()
                ;

            return Ok(profiles);
        }

        // GET profiles/my
        [HttpGet("my")]
        public async Task<ActionResult> GetUserProfile()
        {
            if (!GetAuthenticatedUserId(out var identifier))
                return BadRequest();

            var profile = await service_
                    .GetProfile(identifier)
                ;

            return Ok(profile);
        }

        [HttpPatch("my")]
        public async Task<ActionResult> UpdateUserProfile([FromBody] UpdateProfileRequest profile)
        {
            if (!GetAuthenticatedUserId(out var identifier))
                return BadRequest();

            var updated = await service_
                .UpdateProfile(identifier, profile.DisplayName, profile.ForwardingAddress);
                ;

            return Ok(updated);
        }

        // POST profiles/my/addresses
        [HttpPost("my/addresses/")]
        public async Task<ActionResult> CreateMaskedEmail([FromBody] MaskedEmailRequest request)
        {
            if (!GetAuthenticatedUserId(out var identifier))
                return BadRequest();

            var address = await service_
                    .CreateMaskedEmail(identifier, request.Name, request.PasswordHash, request.Description, request.EnableForwarding)
                ;

            // TODO: 201
            return Ok(address);
        }

        // GET profiles/my/adresses
        [HttpGet("my/addresses")]
        public async Task<ActionResult> GetMaskedEmails()
        {
            if (!GetAuthenticatedUserId(out var identifier))
                return BadRequest();

            var addresses = await service_
                    .GetMaskedEmails(identifier)
                ;

            return Ok(addresses);
        }

        // GET profiles/my/adresses/{email}
        [HttpGet("my/addresses/{email}")]
        public async Task<ActionResult> GetMaskedEmails(string email)
        {
            if (!GetAuthenticatedUserId(out var identifier))
                return BadRequest();

            var address = await service_
                    .GetMaskedEmail(identifier, email)
                ;

            return Ok(address);
        }

        // PATCH profiles/my/addresses/{email}
        [HttpPatch("my/addresses/{email}/enableForwarding")]
        public async Task<ActionResult> ToggleMaskedEmailForwarding(string email)
        {
            if (!GetAuthenticatedUserId(out var identifier))
                return BadRequest();

            var address = await service_
                    .ToggleMaskedEmailForwarding(identifier, email)
                ;

            return Ok(address);
        }

        // PATCH profiles/my/addresses/{email}
        [HttpPatch("my/addresses/{email}")]
        public async Task<ActionResult> UpdateMaskedEmail(string email, [FromBody] UpdateMaskedEmailRequest request)
        {
            if (!GetAuthenticatedUserId(out var identifier))
                return BadRequest();

            await service_
                    .UpdateMaskedEmail(identifier, email, request.Name, request.Description)
                ;

            return Ok();
        }

        // DELETE profiles/my/adresses/{email}
        [HttpDelete("my/addresses/{email}")]
        public async Task<ActionResult> DeleteMaskedEmails(string email)
        {
            if (!GetAuthenticatedUserId(out var identifier))
                return BadRequest();

            await service_.DeleteMaskedEmail(identifier, email)
                ;

            return NoContent();
        }

        private bool GetAuthenticatedUserId(out string identifier)
        {
            const string ns = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
            var sub = User.Claims.FirstOrDefault(c => c.Type == ns)?.Value;
            identifier = sub;

            return true;
        }
    }
}