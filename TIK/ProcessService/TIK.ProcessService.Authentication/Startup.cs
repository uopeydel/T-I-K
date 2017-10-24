﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using TIK.Applications.Authentication.Queries;
using TIK.Domain.Member;
using TIK.Persistance.ElasticSearch.Mocks;
using TIK.Domain.UserAccounts;

namespace TIK.ProcessService.Authentication
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
            services.JwtBearerAuthentication(Configuration);

            services.AddTransient<IUserAccountQuery, UserAccountQuery>();
            services.AddTransient<IUserAccountRepository, MockUserAccountRepository>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");

                // Register a simple error handler to catch token expiries and change them to a 401, 
                // and return all other errors as a 500. This should almost certainly be improved for
                // a real application.
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Use(async (context, next) =>
                    {
                        var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;
                        // This should be much more intelligent - at the moment only expired 
                        // security tokens are caught - might be worth checking other possible 
                        // exceptions such as an invalid signature.
                        if (error != null && error.Error is SecurityTokenExpiredException)
                        {
                            context.Response.StatusCode = 401;
                            // What you choose to return here is up to you, in this case a simple 
                            // bit of JSON to say you're no longer authenticated.
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(
                                JsonConvert.SerializeObject(
                                    new { authenticated = false, tokenExpired = true }));
                        }
                        else if (error != null && error.Error != null)
                        {
                            context.Response.StatusCode = 500;
                            context.Response.ContentType = "application/json";
                            // TODO: Shouldn't pass the exception message straight out, change this.
                            await context.Response.WriteAsync(
                                JsonConvert.SerializeObject
                                (new { success = false, error = error.Error.Message }));
                        }
                        // We're not trying to handle anything else so just let the default 
                        // handler handle.
                        else await next();
                    });
                });
            }

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
