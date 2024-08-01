using AspNetCoreRateLimit;
using Microsoft.Extensions.Options;

namespace JsonPlaceholderAPI.Controllers
{
    public class RateLimitPolicies
    {
        private readonly RateLimitOptions _options;

        public RateLimitPolicies(IOptions<RateLimitOptions> options)
        {
            _options = options.Value;
        }

        public void SetRateLimitPolicies()
        {
            _options.GeneralRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*",
                    Period = "20s",
                    Limit = 5
                }
            };
        }
    }
}
