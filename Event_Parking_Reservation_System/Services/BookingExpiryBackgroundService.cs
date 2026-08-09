using Microsoft.Extensions.Options;
using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Configuration;

namespace Event_Parking_Reservation_System.Services
{
    public class BookingExpiryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingExpiryBackgroundService> _logger;
        private readonly BookingSettings _settings;

        public BookingExpiryBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<BookingExpiryBackgroundService> logger,
            IOptions<BookingSettings> settings)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromSeconds(Math.Max(_settings.ExpiryScanIntervalSeconds, 5));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    var expiredCount = await bookingService.ExpireOverdueHoldsAsync();
                    if (expiredCount > 0)
                    {
                        _logger.LogInformation("Booking expiry job released {Count} overdue holds.", expiredCount);
                    }
                }
                catch (Exception ex)
                {
                    // Never let a single failed scan kill the background worker.
                    _logger.LogError(ex, "Booking expiry scan failed; will retry next interval.");
                }

                try
                {
                    await Task.Delay(interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected on shutdown — loop condition will exit next check.
                }
            }
        }
    }
}
