using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;

namespace TestAppVeyorUrlRewrites.IntegrationTests
{
    internal static class Helper
    {
        /// <summary>
        /// Backing field for the <see cref="EnvironmentIsAppveyor"/> property.
        /// </summary>
        static bool? _environmentIsAppveyor;

        /// <summary>
        /// Backing field for the <see cref="Siteroot"/> property.
        /// </summary>
        static Uri _siteroot;

        /// <summary>
        /// Gets the value indicating whether the current environment is the AppVeyor build server.
        /// </summary>
        public static bool EnvironmentIsAppveyor
        {
            get
            {
                if (!_environmentIsAppveyor.HasValue)
                    _environmentIsAppveyor = "True".Equals(
                        Environment.GetEnvironmentVariable("APPVEYOR"),
                        StringComparison.OrdinalIgnoreCase);

                return _environmentIsAppveyor.Value;
            }
        }

        /// <summary>
        /// Gets the root of the authentication site.
        /// </summary>
        public static Uri Siteroot
        {
            get
            {
                return _siteroot
                       ?? (_siteroot = new Uri(GetSetting("siteroot")));
            }
        }

        /// <summary>
        /// Gets the setting my the <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <returns>The setting my the <paramref name="name"/>.</returns>
        public static string GetSetting(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            return (EnvironmentIsAppveyor
                ? Environment.GetEnvironmentVariable(name)
                : ConfigurationManager.AppSettings[name]) ?? string.Empty;
        }
    }

    [Subject("Static files")]
    public class When_browsing_to_the_home_page
    {
        static IWebDriver _driver;
        static Uri _start;
        static Uri _end;

        Establish context = () =>
        {
            var driverService = PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            _driver = new PhantomJSDriver(driverService);
            var builder = new UriBuilder(Helper.Siteroot);

            _end = builder.Uri;
            //if (_end.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            //{
            //    builder.Scheme = Uri.UriSchemeHttp;
            //    builder.Port = 80;
            //}
            _start = builder.Uri;
        };

        Cleanup after = () =>
        {
            using (_driver)
            {
                _driver.Quit();
            }
        };

        Because of = () => _driver.Navigate().GoToUrl(_start);

        It should_redirect_to_HTTPS_file = () => _driver.Url.ShouldEqual(_end.AbsoluteUri);

        It should_include_the_text = () => _driver.PageSource.ShouldContain("This should redirect to HTTPS");
    }
}
