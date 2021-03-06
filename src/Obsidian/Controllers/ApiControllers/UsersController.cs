﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Obsidian.Application;
using Obsidian.Application.Dto;
using Obsidian.Application.UserManagement;
using Obsidian.Authorization;
using Obsidian.Domain;
using Obsidian.Domain.Repositories;
using Obsidian.Foundation;
using Obsidian.Misc;
using System;
using System.Threading.Tasks;

namespace Obsidian.Controllers.ApiControllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    { 
        private readonly IUserRepository _userRepository;
        private readonly UserManagementService _userManagementService;

        public UsersController(IUserRepository userRepo, UserManagementService userManagementService)
        {
            _userRepository = userRepo;
            _userManagementService = userManagementService;
        }

        [HttpGet]
        [RequireClaim(ManagementAPIClaimsType.IsUserAcquirer, "Yes")]
        public async Task<IActionResult> Get()
        {
            var query = await _userRepository.QueryAllAsync();
            return Ok(query.ProjectTo<QueryModel.User>());
        }

        [HttpGet("{id:guid}")]
        [RequireClaim(ManagementAPIClaimsType.IsUserAcquirer, "Yes")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userRepository.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(Mapper.Map<QueryModel.User>(user));
        }

        [HttpPost]
        [ValidateModel]
        [RequireClaim(ManagementAPIClaimsType.IsUserCreator, "Yes")]
        public async Task<IActionResult> Post([FromBody]UserCreationDto dto)
        {
            try
            {
                var newUser = await _userManagementService.CreateAsync(dto);
                return Created(Url.Action(nameof(GetById), new { id = newUser.Id }), null);
            }
            catch (ArgumentException ex)
            {
                // HTTP 409 Conflict
                return StatusCode(409, ex.Message);
            }
        }

        [HttpPut("{id:guid}/Claims")]
        [ValidateModel]
        [RequireClaim(ManagementAPIClaimsType.IsUserClaimsEditor, "Yes")]
        public async Task<IActionResult> UpdateClaims([FromBody]UpdateUserClaimsDto dto, Guid id)
        {
            try
            {
                await _userManagementService.UpdateUserClaimsAsync(id, dto);
                return Ok();
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}/Profile")]
        [RequireClaim(ManagementAPIClaimsType.IsUserProfileEditor, "Yes")]
        [ValidateModel]
        public async Task<IActionResult> UpdateProfile([FromBody]UserProfile profile, Guid id)
        {
            try
            {
                await _userManagementService.UpdateUserProfileAsync(id, profile);
                return Ok();
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}/Password")]
        [ValidateModel]
        [RequireClaim(ManagementAPIClaimsType.IsUserPasswordEditor, "Yes")]
        public async Task<IActionResult> SetPassword([FromBody]UpdateUserPasswordDto dto, Guid id)
        {
            try
            {
                await _userManagementService.SetPasswordAsync(id, dto.Password);
                return Ok();
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}/UserName")]
        [ValidateModel]
        [RequireClaim(ManagementAPIClaimsType.IsUserNameEditor, "Yes")]
        public async Task<IActionResult> SetUserName([FromBody]UpdateUserNameDto dto, Guid id)
        {
            try
            {
                await _userManagementService.SetUserNameAsync(id, dto.UserName);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return StatusCode(409, ex.Message);
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }
    }
}