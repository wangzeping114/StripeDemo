using Stripe;

var builder = WebApplication.CreateBuilder(args);
var stripeKey = builder.Configuration.GetValue<string>("Stripe:SecretKey");
await UisingKeys(stripeKey);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
var app = builder.Build();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.MapControllers();
app.Run();

static async Task UisingKeys(string stripeKey)
{
    if (string.IsNullOrEmpty(stripeKey))
    {
        throw new ArgumentNullException(nameof(stripeKey), "Stripe secret key is not set.");
    }
    StripeConfiguration.ApiKey = stripeKey;
    // ��������������� Stripe ��ʼ������
    var customerService = new CustomerService();
    var createOptions = new CustomerCreateOptions
    {
        Email = "demo@demo.com",
        Name = "Demo User",
    };
    await customerService.CreateAsync(createOptions);
}

static async Task UsingKeysWithRequestOptions(string stripeKey)
{
    var client = new SystemNetHttpClient();
    var stripeClient = new StripeClient(stripeKey, httpClient: client);
    var customerService = new CustomerService(stripeClient);
    var requestOptions = new RequestOptions
    {
        ApiKey = stripeKey,
    };
    var createOptions = new CustomerCreateOptions
    {
        Email = "demo@demo.com",
        Name = "Demo User",
    };
    await customerService.CreateAsync(createOptions, requestOptions);
}