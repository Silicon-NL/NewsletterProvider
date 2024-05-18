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
    public class Subscribe(ILogger<Subscribe> logger, DataContext dataContext)
    {
        private readonly ILogger<Subscribe> _logger = logger;
        private readonly DataContext _dataContext = dataContext;

        [Function("Subscribe")]

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
                        _dataContext.Entry(existingSubscriber).CurrentValues.SetValues(subscribeEntity);
                        await _dataContext.SaveChangesAsync();
                        return new OkObjectResult(new { Status = 200, Message = "Subscriber was updated." });
                    }

                    _dataContext.AspNetSubscribers.Add(subscribeEntity);
                    await _dataContext.SaveChangesAsync();
                    return new OkObjectResult(new { Status = 201, Message = "Subscriber is now subscribed." });
                }
            }
            return new BadRequestObjectResult(new { Status = 400, Message = "Unable to subscribe" });
        }
    }
}
