﻿using System.Threading.Tasks;

namespace Obsidian.Domain.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> FindByUserNameAsync(string userName);
    }
}
