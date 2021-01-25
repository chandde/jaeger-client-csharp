using Jaeger.Samplers;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;

namespace Jaeger.Example.WebApi
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        private ILoggerFactory loggerFactory;

        public IConfiguration Configuration { get; }

        public Startup(ILogger<Startup> logger, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            this.loggerFactory = loggerFactory;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // Use "OpenTracing.Contrib.NetCore" to automatically generate spans for ASP.NET Core, Entity Framework Core, ...
            // See https://github.com/opentracing-contrib/csharp-netcore for details.
            services.AddOpenTracing();

            // Adds the Jaeger Tracer.
            services.AddSingleton<ITracer>(serviceProvider =>
            {
                // string serviceName = serviceProvider.GetRequiredService<IWebHostEnvironment>().ApplicationName;

                // This will log to a default localhost installation of Jaeger.
                //var tracer = new Tracer.Builder(serviceName)
                //    .WithSampler(new ConstSampler(true))
                //    .Build();

                // var loggerFactory = new LoggerFactory(); // get Microsoft.Extensions.Logging ILoggerFactory
                // var serviceName = "testService";

                //Jaeger.Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
                //    .RegisterSenderFactory<ThriftSenderFactory>();
                ////Configuration config = new Configuration(serviceName, loggerFactory)
                ////    // .WithSampler(...)   // optional, defaults to RemoteControlledSampler with HttpSamplingManager on localhost:5778
                ////    .WithReporter(Jaeger.Configuration.ReporterConfiguration.FromEnv(loggerFactory)); // optional, defaults to RemoteReporter with UdpSender on localhost:6831 when ThriftSenderFactory is registered
                //Configuration config = Jaeger.Configuration.FromEnv(loggerFactory);

                //ITracer tracer = config.GetTracer().wi;

                //// Allows code that can't use DI to also access the tracer.
                //GlobalTracer.Register(tracer);

                var senderResolver = new SenderResolver(loggerFactory).RegisterSenderFactory<ThriftSenderFactory>();

                var senderConfiguration = new Configuration.SenderConfiguration(loggerFactory)
                    .WithSenderResolver(senderResolver).WithEndpoint("http://192.168.1.34:14268/api/traces"); // optional, defaults to Configuration.SenderConfiguration.DefaultSenderResolver


                var reporterConfiguration = new Configuration.ReporterConfiguration(loggerFactory)
                    .WithSender(senderConfiguration) // optional, defaults to UdpSender at localhost:6831 when ThriftSenderFactory is registered
                    .WithLogSpans(true);             // optional, defaults to no LoggingReporter

                var tracer = new Configuration("campaignUI", loggerFactory)
                    .WithReporter(reporterConfiguration) // optional, defaults to RemoteReporter with UdpSender at localhost:6831 when ThriftSenderFactory is registered
                    .GetTracer();

                return tracer;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}