using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using KioskBrains.Clients.AllegroPl;
using Microsoft.Extensions.Options;
using KioskBrains.Clients.YandexTranslate;
using System.Threading;
using KioskBrains.Clients.AllegroPl.ServiceInterfaces;

namespace WebApplication.Controllers
{
    public class SearchController : Controller
    {
        private ILogger<AllegroPlClient> _logger;
        private IOptions<AllegroPlClientSettings> _settings;
        private IOptions<YandexTranslateClientSettings> _yandexSettings;
        private AllegroPlClient _client;
        private CancellationTokenSource _tokenSource;

        public SearchController(ILogger<AllegroPlClient> logger,
            IOptions<AllegroPlClientSettings> settings,
            IOptions<YandexTranslateClientSettings> yandexApiClientSettings,
            //ITokenService tokenService,
            ITranslateService translateService)
        {
            _logger = logger;
            _settings = settings;
            _yandexSettings = yandexApiClientSettings;
            _client = new AllegroPlClient(
                settings,
                new YandexTranslateClient(yandexApiClientSettings),
                logger,
                //tokenService,
                translateService);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
