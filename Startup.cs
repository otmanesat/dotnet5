using System;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using HelloDotNet5.Configuration;
using HelloDotNet5.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Polly;

namespace HelloDotNet5
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeSerializer(BsonType.String));
            var MongoSettings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
            services.AddSingleton<IMongoClient>(serviceProvider => {
                return new MongoClient(MongoSettings.ConnectionString);
            });
            // services.AddSingleton<IItemsRepository, InMemItemsRepository>();
            services.AddSingleton<IItemsRepository, MongoDbItemsRepository>();
            services.AddSingleton<IWeatherClient,WeatherClient>();
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));
            services.AddControllers(
                options => {
                    options.SuppressAsyncSuffixInActionNames = false;
                }
            );
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HelloDotNet5", Version = "v1" });
            });
            services.AddHttpClient<WeatherClient>()
                 .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt))))
                 .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(3,TimeSpan.FromSeconds(10)));
            services.AddHealthChecks()
            .AddCheck<ExtenalEndPointHealthCheck>("OpenWeather")
            .AddMongoDb(
                MongoSettings.ConnectionString,
                name:"Mongo Health Check",
                timeout: TimeSpan.FromSeconds(5),
                tags:new[]{"ready"}

            );

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HelloDotNet5 v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("health/ready", new HealthCheckOptions{
                    Predicate = (check) => check.Tags.Contains("ready"),
                    ResponseWriter =  async (context,report) => {
                        var result = JsonSerializer.Serialize(
                            new {
                                status = report.Status.ToString(),
                                checks = report.Entries.Select( entry => new {
                                    name = entry.Key,
                                    status = entry.Value.Status.ToString(),
                                    exception = entry.Value.Exception != null ? entry.Value.Exception.Message : "none",
                                    duration = entry.Value.Duration.ToString()
                                })
                            }
                        );
                        context.Response.ContentType = MediaTypeNames.Application.Json;
                        await context.Response.WriteAsync(result);
                    }
                });
                endpoints.MapHealthChecks("health/live", new HealthCheckOptions{
                    Predicate = (_) => false
                });
            });
        }
    }
}
