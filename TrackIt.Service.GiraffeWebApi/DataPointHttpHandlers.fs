module TrackIt.Service.GiraffeWebApi.DataPointHttpHandlers

open System.Collections.Generic
open Microsoft.AspNetCore.Http
open Giraffe.HttpHandlers
open Giraffe.HttpContextExtensions
open Giraffe.Tasks
open TrackIt.Domain.Model.Dto
open TrackIt.Domain.Model.Models
open TrackIt.Service.GiraffeWebApi.Config
open TrackIt.Service.GiraffeWebApi.Services
open TrackIt.Service.GiraffeWebApi.Utility

let deleteDataPoint id =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            use manager = getDataPointManager configuration
            let! result = manager.DeleteAsync(Criteria<IEnumerable<string>>(Value = [|id|]))
            if (result.Success) then return! jsonCamelCase true next ctx
            else return! (setStatusCode 400 >=> jsonCamelCase result) next ctx
        }

let upsertDataPoints =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! dataPoints = ctx.BindJson<IEnumerable<DataPoint>>()
            use manager = getDataPointManager configuration
            let! result = manager.UpsertAsync(Criteria<IEnumerable<DataPoint>>(Value = dataPoints))
            if (result.Success) then return! jsonCamelCase result.Value next ctx
            else return! (setStatusCode 400 >=> jsonCamelCase result) next ctx
        }
