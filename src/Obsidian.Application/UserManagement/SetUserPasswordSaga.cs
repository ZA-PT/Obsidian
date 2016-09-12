﻿using Obsidian.Application.ProcessManagement;
using Obsidian.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Obsidian.Application.UserManagement
{
    public class SetUserPasswordSaga : Saga, IStartsWith<SetUserPasswordCommand, UserPasswordSettingResult>
    {
        private bool _isCompleted;

        private readonly IUserRepository _repo;

        public SetUserPasswordSaga(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<UserPasswordSettingResult> StartAsync(SetUserPasswordCommand command)
        {
            _isCompleted = true;
            //check username
            var user = await _repo.FindByIdAsync(command.UserId);
            if (user == null)
            {
                return new UserPasswordSettingResult
                {
                    Succeed = false,
                    Message = $"User of user id {command.UserId} doesn't exists."
                };
            }
            user.SetPassword(command.Password);
            await _repo.SaveAsync(user);
            return new UserPasswordSettingResult
            {
                Succeed = true,
                Message = $"Password of User {command.UserId} changed."
            };

        }

        protected override bool IsProcessCompleted()
        {
            return _isCompleted;
        }
    }
}
