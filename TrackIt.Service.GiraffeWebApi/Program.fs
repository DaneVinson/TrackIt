module TrackIt.Service.GiraffeWebApi.Main

open System
open System.IO
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe.HttpHandlers
open Giraffe.Middleware
open Giraffe.Razor.Middleware
open TrackIt.Service.GiraffeWebApi.Config
open TrackIt.Service.GiraffeWebApi.DataPointHttpHandlers
open TrackIt.Service.GiraffeWebApi.CategoryHttpHandlers
    
let authorize = requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

// Web app
let webApp =
    choose [
        DELETE >=>
            choose [
                routef "/api/categories/%s" (fun (id) -> authorize >=> deleteCategory(id))
                routef "/api/datapoints/%s" (fun (id) -> authorize >=> deleteDataPoint(id))
            ]
        GET >=>
            choose [
                route "/api/categories" >=> authorize >=> getCategories
                routef "/api/categories/%s" (fun (id) -> authorize >=> getCategory(id))
                route "/api/handshake" >=> text "true"
                route "/api/handshake/auth" >=> authorize >=> text "true" 
            ]
        POST >=>
            choose [
                route "/api/categories" >=> authorize >=> upsertCategory
                route "/api/datapoints" >=> authorize >=> upsertDataPoints
            ]
        PUT >=>
            choose [
                route "/api/categories" >=> authorize >=> upsertCategory
                route "/api/datapoints" >=> authorize >=> upsertDataPoints
             ]
        setStatusCode 404 >=> text "Not Found" ]

// Error handler
let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader().AllowCredentials() |> ignore

// Config
let configureApp (app : IApplicationBuilder) =
    app.UseCors configureCors |> ignore
    app.UseStaticFiles() |> ignore
    app.UseAuthentication() |> ignore
    app.UseGiraffeErrorHandler errorHandler
    app.UseGiraffe webApp

// Services
let configureServices (services : IServiceCollection) =
    // TODO: Griaffe caused? 
    //      Exception thrown without a call to AddRazorEngine
    //      Changing to service.AddMvc does not work
    services.AddRazorEngine "{views_path}" |> ignore

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun (options) -> 
                                options.Audience <- configuration.["aad:clientId"]
                                options.MetadataAddress <- 
                                            sprintf "https://login.microsoftonline.com/%s/v2.0/.well-known/openid-configuration?p=%s" 
                                                configuration.["aad:tenant"] 
                                                configuration.["aad:policyId"])
            |> ignore

// Logging
let configureLogging (builder : ILoggingBuilder) =
    let filter (logLevel : LogLevel) = logLevel.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main argv =
    let contentRoot = Directory.GetCurrentDirectory()
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(Path.Combine(contentRoot, "WebRoot"))
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0