using Infrastructure.Contexts;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NewsletterProvider.Functions
{
    public class UnSubscribe(ILogger<UnSubscribe> logger, DataContext dataContext)
    {
        private readonly ILogger<UnSubscribe> _logger = logger;
        private readonly DataContext _dataContext = dataContext;

        [Function("UnSubscribe")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(body))
            {
                var subscribeEntity = JsonConvert.DeserializeObject<SubscribeEntity>(body);
                if (subscribeEntity != null)
                {
                    var existingSubscriber = await _dataContext.AspNetSubscribers.FirstOrDefaultAsync(x => x.Email == subscribeEntity.Email);
                    if (existingSubscriber != null)
                    {
                        _dataContext.Remove(existingSubscriber);
                        await _dataContext.SaveChangesAsync();
                        return new OkObjectResult(new { Status = 200, Message = "Subscription was removed." });
                    }

                }
            }
            return new BadRequestObjectResult(new { Status = 400, Message = "Unable to unsubscribe" });
        }
    }
}
