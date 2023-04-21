using CoreWCF.Channels;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System;
using CoreWCF.Security;

namespace FaultExceptionIssueServer
{
    internal class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceModelServices().AddServiceModelMetadata();
            services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>(); 

            services.AddSingleton(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseServiceModel(builder =>
            {
                var host = Configuration.GetValue<string>("WCF:Host");
                var port = int.Parse(Configuration.GetValue<string>("WCF:Port"));
                var scheme = Uri.UriSchemeHttps;

                var httpsUri = new UriBuilder(scheme, host, port, $"{nameof(EmailService)}.svc").Uri;

                var binding = Configuration.GetSection("WCF:Services").GetValue<string>(nameof(EmailService));

                var encoding = Configuration.GetSection($"WCF:Bindings:{binding}:Encoding").Get<TextMessageEncodingBindingElement>();
                var httpsTransport = Configuration.GetSection($"WCF:Bindings:{binding}:HttpsTransport")
                    .Get<HttpsTransportBindingElement>();
                var security = SecurityBindingElement.CreateUserNameOverTransportBindingElement();
                security.AllowInsecureTransport = true;

                var customBinding = new CustomBinding();
                customBinding.Elements.Add(security);
                customBinding.Elements.Add(encoding);
                customBinding.Elements.Add(httpsTransport);

                builder.AddService<EmailService>();
                builder.AddServiceEndpoint<EmailService, IEmailService>(customBinding, httpsUri);
                 

                builder.ConfigureServiceHostBase<EmailService>(serviceHost =>
                {
                    var debugBehavior = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>() ?? new ServiceDebugBehavior();
                    debugBehavior.IncludeExceptionDetailInFaults = true;

                    if (!serviceHost.Description.Behaviors.Contains(typeof(ServiceDebugBehavior)))
                        serviceHost.Description.Behaviors.Add(debugBehavior);
                });

                builder.ConfigureServiceHostBase<EmailService>(serviceHost =>
                {
                    var srvCredentials = new ServiceCredentials
                    {
                        UserNameAuthentication =
                        {
                            UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom,
                            CustomUserNamePasswordValidator = new CustomUserNameValidator()
                        }
                    };

                    srvCredentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
                    serviceHost.Description.Behaviors.Add(srvCredentials);
                }); 

                var serviceMetadataBehavior = app.ApplicationServices.GetRequiredService<ServiceMetadataBehavior>();
                serviceMetadataBehavior.HttpGetEnabled = true;
                serviceMetadataBehavior.HttpsGetEnabled = true;
            });

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
        }
    }
}
