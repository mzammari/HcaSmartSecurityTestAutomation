using System;
using SmartSecurityTestAutomation.Models;
using SmartSecurityTestAutomation.Page.Objects.Login;
using Xunit;
using Xunit.Abstractions;

namespace SmartSecurityTestAutomation.Tests;

/// <summary>
/// A focused login demo. Inherits TestMaster so it uses the same runtime
/// setup, browser wiring, and default security user as the rest of the suite,
/// then does one thing on screen: log in to SMART Security and prove it worked.
///
/// Good for a live demo because the assertions are simple and the failure
/// points are obvious.
/// </summary>
public class LoginDemoTests : TestMaster
{
    public LoginDemoTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact(DisplayName = "SmartSecurity_Login_Demo")]
    [Trait("Category", "UI")]
    [Trait("Category", "Demo")]
    public void LoginDemo()
    {
        AdoTestCaseName = "SmartSecurity_Login_Demo";
        TimeSaved       = 60;

        // DefaultUser is the "Full / Security - Administrator" account loaded
        // from SecurityUsers/securityUsers.qa.json during TestMaster setup.
        SecurityUser defaultUser = GlobalTestVariables.DefaultUser;

        // Optional override from the .runsettings file. The <EnvironmentVariables>
        // block sets process env vars, so if TestUserName / TestPassword are
        // present they win; otherwise fall back to the default security user.
        string userName = Environment.GetEnvironmentVariable("TestUserName");
        string password = Environment.GetEnvironmentVariable("TestPassword");

        if (string.IsNullOrWhiteSpace(userName)) userName = defaultUser.UserName;
        if (string.IsNullOrWhiteSpace(password)) password = defaultUser.Creds;

        var loginPage = new SmartSecurityLoginPage(Browser);
        loginPage.Login(Url, userName, password);

        // Primary assertion: a Ping access_token in session storage. This is the
        // strongest, most reliable proof the SSO login (network intercept +
        // form login) actually completed.
        var token = loginPage.GetSessionToken();
        Assert.False(string.IsNullOrEmpty(token),
            "Expected a Ping access_token in session storage after login.");

        // Secondary, visual confirmation of the landing page. Left as a soft
        // check (logged, not asserted) until the landing element id is confirmed
        // on your environment — promote it to an Assert.True once verified.
        var landingVisible = loginPage.IsAdminLandingDisplayed();
        Steps.AddTestStep(
            $"Confirm admin landing is displayed (visible: {landingVisible}).",
            "SMART Security admin landing page should be shown.");
    }
}
