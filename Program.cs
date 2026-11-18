using PropertyManagementSystem.Configuration;
using PropertyManagementSystem.Data;
using PropertyManagementSystem.Repository;
using PropertyManagementSystem.Services;

namespace PropertyManagementSystem
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var contextFactory = new RuntimeDbContextFactory(
                AppConfiguration.GetConnectionString());
            var userRepository = new UserRepository(contextFactory);
            var propertyRepository = new PropertyRepository(contextFactory);
            var authenticationService = new AuthenticationService(userRepository);
            var userAccountService = new UserAccountService(userRepository);
            var propertyManagementService = new PropertyManagementService(propertyRepository);
            var currentUserSession = new CurrentUserSession();

            Application.Run(new Form1(
                authenticationService,
                currentUserSession,
                userAccountService,
                propertyManagementService));
        }
    }
}
