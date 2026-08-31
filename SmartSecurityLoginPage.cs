using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SmartSecurityTestAutomation.Models;
using SmartSecurityTestAutomation.Page.Objects.Ping;
using WebUiAutomation;

namespace SmartSecurityTestAutomation.Page.Objects.Login
{
    /// <summary>
    /// Page object that encapsulates signing in to the SMART Security admin app
    /// through the Ping SSO flow — including the network-intercept credential
    /// handling in AuthorizationPage.SignInThruPing — and confirming the
    /// post-login landing.
    ///
    /// This mirrors the framework's own proven UI-login path
    /// (NavigateToUrl(url, SecurityUser) in the UI tests) but wraps it as a
    /// reusable page object with explicit login-success checks, which is what
    /// you want to point at during the demo.
    /// </summary>
    public class SmartSecurityLoginPage : AdminPageMaster
    {
        private readonly Browser _browser;
        private readonly WebDriverWait _wait;

        public SmartSecurityLoginPage(Browser browser) : base(browser)
        {
            _browser = browser;
            _wait = new WebDriverWait(browser.Driver, TimeSpan.FromSeconds(60));
        }

        #region Page Objects

        // Safe (non-throwing) landing marker. Uses the wait+By HpgElement
        // constructor so .Exists() can be polled without throwing on timeout
        // (same pattern as AdminPageMaster.SiteThrobber).
        //
        // NOTE: By.Id("Admin") is the AdminTile the base page exposes. Confirm
        // this element is present on YOUR post-login landing page; if the search
        // landing uses a different anchor, swap the locator here.
        private HpgElement AdminLanding => new(_wait, By.Id("Admin"), "Admin Landing Tile");

        #endregion Page Objects

        #region Actions

        /// <summary>
        /// Opens the app URL and signs in as the supplied security user via Ping SSO.
        /// </summary>
        public void Login(string appUrl, SecurityUser user)
        {
            Login(appUrl, user.UserName, user.Creds);
        }

        /// <summary>
        /// Opens the app URL and signs in with the supplied credentials via Ping SSO.
        /// </summary>
        public void Login(string appUrl, string userName, string password)
        {
            Steps.AddTestStep($"Navigate to {appUrl}.",
                "SMART Security Ping sign-in page should load.");
            _browser.Navigate(appUrl);
            Thread.Sleep(1000); // matches the framework's page-load settling delay

            // The actual sign-in. SignInThruPing registers the Selenium 4
            // NetworkAuthenticationHandler (network intercept) to satisfy the
            // native medcity.net Basic-auth challenge, then completes the
            // ExternalLoginPage form (UserId / Password / submit).
            Steps.AddTestStep($"Sign in through Ping as {userName}.",
                "User should be authenticated and returned to SMART Security.");
            var pingAuth = new AuthorizationPage(_browser);
            pingAuth.SignInThruPing(userName, password);

            WaitForThrobber();
        }

        #endregion Actions

        #region Validations

        /// <summary>
        /// The definitive proof of a successful Ping login: the access_token the
        /// SSO flow drops into session storage (same value the framework's API
        /// setup reads). Returns null/empty if login did not complete.
        /// </summary>
        public string GetSessionToken()
        {
            var js = (IJavaScriptExecutor)_browser.Driver;
            return (string)js.ExecuteScript("return sessionStorage.getItem('access_token')");
        }

        /// <summary>
        /// True when the SMART Security admin landing element is visible after login.
        /// Non-throwing — safe to use directly in an assertion.
        /// </summary>
        public bool IsAdminLandingDisplayed()
        {
            try
            {
                return AdminLanding.Exists();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Convenience check: login is considered successful when a session
        /// token is present. Token presence is the most reliable signal and does
        /// not depend on any particular landing-page element.
        /// </summary>
        public bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(GetSessionToken());
        }

        #endregion Validations
    }
}
