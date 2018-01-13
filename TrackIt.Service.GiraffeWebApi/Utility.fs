module TrackIt.Service.GiraffeWebApi.Utility

open System
open Microsoft.AspNetCore.Http
open Giraffe.HttpHandlers
open Giraffe.HttpContextExtensions
open Newtonsoft.Json
open Newtonsoft.Json.Serialization

// Get the specified query string argument as a DateTime. Missing or invalid returns minimum.
let getQueryValueAsDate (ctx : HttpContext) (key : string ) : DateTime = 
    match ctx.TryGetQueryStringValue(key) with
        | Some value -> match DateTime.TryParse(value) with
                            | (sucess, date) ->
                                if (sucess) then date
                                else DateTime.MinValue
        | None -> DateTime.MinValue


// Camel case serialization for Newtonsoft .net -> json. Default for ASP.NET Core.
let serializeJsonCamelCase entity = 
    JsonConvert.SerializeObject(
        entity,
        JsonSerializerSettings(ContractResolver = DefaultContractResolver(NamingStrategy = CamelCaseNamingStrategy())))

let jsonCamelCase (entity : obj) : HttpHandler =
    setHttpHeader "Content-Type" "application/json" >=> 
        setBodyAsString (serializeJsonCamelCase entity)
