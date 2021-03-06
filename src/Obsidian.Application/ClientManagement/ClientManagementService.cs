﻿using Microsoft.Extensions.DependencyInjection;
using Obsidian.Domain;
using Obsidian.Domain.Repositories;
using Obsidian.Foundation;
using Obsidian.Foundation.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Obsidian.Application.ClientManagement
{
    [Service(ServiceLifetime.Scoped)]
    public class ClientManagementService
    {
        private readonly IClientRepository _repo;
        public ClientManagementService(IClientRepository repo)
        {
            _repo = repo;
        }

        public async Task<Client> CreateClient(string displayName,string redirectUri)
        {
            var client = Client.Create(Guid.NewGuid(), redirectUri);
            client.DisplayName = displayName;
            client.UpdateSecret();
            await _repo.AddAsync(client);
            return client;
        }

        public async Task<Client> UpdateClientSecret(Guid clientId)
        {
            var client = await _repo.FindByIdAsync(clientId);
            if (client == null) throw new EntityNotFoundException($"Can not find client with id {clientId}");
            client.UpdateSecret();
            await _repo.SaveAsync(client);
            return client;
        }

        public async Task<Client> UpdateClient(Guid clientId,string displayName,string redirectUri)
        {
            var client = await _repo.FindByIdAsync(clientId);
            if (client == null) throw new EntityNotFoundException($"Can not find client with id {clientId}");
            client.DisplayName = displayName;
            client.RedirectUri = new Uri(redirectUri);
            await _repo.SaveAsync(client);
            return client;
        }
}
}
