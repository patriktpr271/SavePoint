using Microsoft.AspNetCore.Identity;

namespace SavePoint.BusinessLogic.Tests.Helpers
{
    /// <summary>
    /// Helper for creating a Mock&lt;UserManager&lt;TUser&gt;&gt; with the minimum
    /// constructor wiring required by ASP.NET Core Identity.
    /// All collaborators except IUserStore are passed as null which matches
    /// common community guidance for unit-testing UserManager.
    /// </summary>
    public static class UserManagerMockFactory
    {
        public static Mock<UserManager<TUser>> Create<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }
    }
}
