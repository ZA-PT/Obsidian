using System.Threading.Tasks;
using Obsidian.Application.ProcessManagement;
using Obsidian.Domain.Repositories;

namespace Obsidian.Application.OAuth20.ImplicitGrant
{
    public class ImplicitGrantSaga : InteractionGrantSaga,
                                     IStartsWith<ImplicitGrantCommand, OAuth20Result>,
                                     IHandlerOf<OAuth20SignInMessage, OAuth20Result>,
                                     IHandlerOf<PermissionGrantMessage, OAuth20Result>
    {
        public ImplicitGrantSaga(IClientRepository clientRepo,
                                IUserRepository userRepo,
                                IPermissionScopeRepository scopeRepo,
                                OAuth20Service oauth20Service)
                                : base(clientRepo, userRepo, scopeRepo, oauth20Service)
        {
        }

        public async Task<OAuth20Result> StartAsync(ImplicitGrantCommand command)
            => await StartSagaAsync(command);



        public async override Task<OAuth20Result> HandleAsync(PermissionGrantMessage message)
        {
            //check granted scopes
            if (!TypLoadScopeFromNames(message.GrantedScopeNames, out _grantedScopes))
            {
                GoToState(OAuth20State.UserDenied);
                return CurrentStateResult();
            }

            //next step
            return await GrantPermissionAsync();
        }


        protected override async Task<OAuth20Result> GrantPermissionAsync()
        {
            _user.GrantClient(_client, _grantedScopes);
            await SaveUserAsync();
            GoToState(OAuth20State.Finished);
            var result= AccessTokenResult();
            result.RedirectUri = _redirectUri;
            return result;
        }

    }
}