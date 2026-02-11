using Microsoft.Extensions.Caching.Distributed;
using Org.BouncyCastle.Crypto;
using System.Data;
using System.Text;
using System.Text.Json;

namespace eShop.Api.Customer
{
    //public class CustomerService
    //{
    //    private readonly IMapper _mapper;
    //    private readonly IDistributedCache _cache;
    //    private readonly IHttpClientFactory _httpFactory;

    //    private readonly string? address = Environment.GetEnvironmentVariable("TokenURL");
    //    private readonly string? clientId = Environment.GetEnvironmentVariable("ClientId");
    //    private readonly string? clientSecret = Environment.GetEnvironmentVariable("ClientSecret");

    //    public CustomerService(IHttpClientFactory httpFactory, IMapper mapper, IDistributedCache cache)
    //    {
    //        _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
    //        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    //        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    //    }

    //    private static string CacheKeyFor(string environmentUrl, string clientId, string clientSecret) => $"TokenLookup:{environmentUrl:N},{clientId:N},{clientSecret:N}";

    //    public async Task<IEnumerable<Customer>> GetCustomerAsync(Guid customerId, LegalEntityLookup legalEntityLookup)
    //    {
    //        var baseUrl = legalEntityLookup.EnvironmentURL;

    //        //TODO: enable dynamic client credentials per legal entity
    //        //var clientId = Environment.GetEnvironmentVariable($"{legalEntityLookup.EnvironmentName}_ClientId");
    //        //var clientSecret = Environment.GetEnvironmentVariable($"{legalEntityLookup.EnvironmentName}_ClientSecret");

    //        var http = _httpFactory.CreateClient();
    //        var tokenResponse = await http.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
    //        {
    //            Address = address,
    //            ClientId = clientId,
    //            ClientSecret = clientSecret,
    //            Scope = $"{legalEntityLookup.EnvironmentURL}/.default"
    //        });

    //        http.Dispose();

    //        //await _cache.SetStringAsync(cacheKey, json, options, ct).ConfigureAwait(false);

    //        http = _httpFactory.CreateClient();
    //        http.SetBearerToken(tokenResponse.AccessToken);

    //        http.BaseAddress = new Uri(baseUrl);

    //        var refitSettings = new RefitSettings
    //        {
    //            ContentSerializer = new SystemTextJsonContentSerializer(new System.Text.Json.JsonSerializerOptions
    //            {
    //                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    //            })
    //        };

    //        var customerClient = RestService.For<ICustomerClient>(http, refitSettings);

    //        var odata = new ODataParameters
    //        {
    //            Filter = $"{D365FOConfiguration.QingIdField} eq '{customerId}' and {D365FOConfiguration.CompanyId} eq '{legalEntityLookup.LegalEntity}'",
    //            Top = 1,
    //            Select = D365FOConfiguration.CustomerSelectFields,
    //            CrossCompany = true
    //        };

    //        var customerResponse = await customerClient.GetCustomersAsync(odata);

    //        return _mapper.Map<IEnumerable<Customer>>(customerResponse.Value);
    //    }
    //}
}




//public class LegalEntityLookupService : ILegalEntityLookupService
//{
//    private readonly IDistributedCache _cache;
//    private readonly TimeSpan _cacheTtl;
//    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
//    private readonly ApplicationDbContext db;
//    private readonly ILogger logger;

//    public LegalEntityLookupService(ApplicationDbContext db, IDistributedCache cache, ILogger<HttpLoggingHandler> logger)
//    {
//        this.db = db;
//        this.logger = logger;
//        _cache = cache;

//        var ttlSeconds = 3600 * 24;
//        _cacheTtl = TimeSpan.FromSeconds(ttlSeconds);
//    }

//    private static string CacheKeyFor(Guid sisOrganizationId) => $"OrganizationLookup:{sisOrganizationId:N}";

//    public async Task<LegalEntityLookup?> FindByKeyGuidAsync(Guid sisOrganizationID, CancellationToken ct)
//    {
//        var message = new StringBuilder();

//        string cacheKey = CacheKeyFor(sisOrganizationID);
//        LegalEntityLookup result = new LegalEntityLookup();

//        try
//        {
//            message.AppendLine("Initiating cache read for:");
//            message.AppendLine(cacheKey);

//            logger.Log(
//                LogLevel.Information,
//                new EventId(100, "RequestStart"),
//                message,
//                null,
//                (state, ex) => state.ToString());

//            message.Clear();

//            var cached = await _cache.GetStringAsync(cacheKey, ct).ConfigureAwait(false);

//            if (!string.IsNullOrEmpty(cached))
//            {
//                message.AppendLine("Cache response:");
//                message.AppendLine(cached);

//                logger.Log(
//                    LogLevel.Information,
//                    new EventId(100, "RequestStart"),
//                    message,
//                    null,
//                    (state, ex) => state.ToString());

//                message.Clear();

//                result = JsonSerializer.Deserialize<LegalEntityLookup>(cached, _jsonOptions);
//                return result;
//            }
//        }
//        catch
//        {
//            message.AppendLine("Cache read error for key:");
//            message.AppendLine(cacheKey);

//            logger.Log(
//                LogLevel.Error,
//                new EventId(100, "RequestStart"),
//                message,
//                null,
//                (state, ex) => state.ToString());

//            message.Clear();

//            await _cache.RemoveAsync(cacheKey, ct).ConfigureAwait(false);
//        }

//        try
//        {
//            message.AppendLine("Initiate SQL db query for SIS Organization ID:");
//            message.AppendLine($"{sisOrganizationID}");

//            logger.Log(
//                LogLevel.Information,
//                new EventId(100, "RequestStart"),
//                message,
//                null,
//                (state, ex) => state.ToString());

//            message.Clear();

//            result = await OrganizationMappingDatabaseQueryAsync(sisOrganizationID, ct).ConfigureAwait(false);

//            message.AppendLine("Returned SQL db query for SIS Organization ID:");
//            message.AppendLine($"{sisOrganizationID}");

//            logger.Log(
//                LogLevel.Information,
//                new EventId(100, "RequestStart"),
//                message,
//                null,
//                (state, ex) => state.ToString());

//            message.Clear();

//            // TODO: create as helper method?
//            if (result != null)
//            {
//                var json = JsonSerializer.Serialize(result, _jsonOptions);

//                message.AppendLine("SQL DB query response:");
//                message.AppendLine(json);

//                logger.Log(
//                LogLevel.Information,
//                new EventId(100, "RequestStart"),
//                message,
//                null,
//                (state, ex) => state.ToString());

//                message.Clear();

//                var options = new DistributedCacheEntryOptions
//                {
//                    AbsoluteExpirationRelativeToNow = _cacheTtl
//                };
//                await _cache.SetStringAsync(cacheKey, json, options, ct).ConfigureAwait(false);
//            }

//        }
//        catch (Exception ex)
//        {
//            message.AppendLine($"SQL DB query error for {sisOrganizationID}:");
//            message.AppendLine(ex.Message);

//            logger.Log(
//            LogLevel.Information,
//            new EventId(100, "RequestStart"),
//            message,
//            null,
//            (state, ex) => state.ToString());

//            message.Clear();
//        }


//        return result;
//    }

//    private async Task<LegalEntityLookup?> OrganizationMappingDatabaseQueryAsync(Guid sisOrganizationID, CancellationToken ct)
//    {
//        var message = new StringBuilder();

//        var envType = Environment.GetEnvironmentVariable("EnvironmentType");

//        message.AppendLine($"SQL DB query finished for {sisOrganizationID} and {envType} environment");

//        logger.Log(
//        LogLevel.Information,
//        new EventId(100, "RequestStart"),
//        message,
//        null,
//        (state, ex) => state.ToString());

//        message.Clear();

//        var result = await (
//            from m in db.MappingSISIDToD365LE.AsNoTracking()
//            join l in db.D365FOLegalEntities.AsNoTracking() on m.CountryCode equals l.CountryCode
//            join e in db.D365FOEnvironments.AsNoTracking() on l.EnvironmentID equals e.ID
//            where m.SISOrganizationID == sisOrganizationID && e.EnvironmentType == envType
//            select new LegalEntityLookup
//            {
//                SISID = m.SISOrganizationID,
//                CountryCode = m.CountryCode,
//                LegalEntity = l.LegalEntity,
//                EnvironmentName = e.EnvironmentName,
//                EnvironmentURL = e.EnvironmentURL,
//                EnvironmentType = e.EnvironmentType
//            })
//        .FirstOrDefaultAsync(ct);

//        message.AppendLine($"SQL DB query finished for {sisOrganizationID}:");

//        logger.Log(
//        LogLevel.Information,
//        new EventId(100, "RequestStart"),
//        message,
//        null,
//        (state, ex) => state.ToString());

//        message.Clear();

//        if (result == null) return null;
//        return result;
//    }
//}
