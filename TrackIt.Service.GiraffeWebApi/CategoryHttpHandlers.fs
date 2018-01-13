module TrackIt.Service.GiraffeWebApi.CategoryHttpHandlers

open System.Collections.Generic
open Microsoft.AspNetCore.Http
open Giraffe.HttpHandlers
open Giraffe.HttpContextExtensions
open Giraffe.Tasks
open TrackIt.Domain.Model.Base
open TrackIt.Domain.Model.Dto
open TrackIt.Domain.Model.Models
open TrackIt.Service.GiraffeWebApi.Config
open TrackIt.Service.GiraffeWebApi.Services
open TrackIt.Service.GiraffeWebApi.Utility

let getUserId (ctx:HttpContext) =
    let claim = ctx.User.FindFirst "http://schemas.microsoft.com/identity/claims/objectidentifier"
    claim.Value

let deleteCategory id =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            use manager = getCategoryManager configuration
            let! result = manager.DeleteAsync(Criteria<IEnumerable<string>>(
                                                    PrimaryId = getUserId(ctx), 
                                                    Value = [|id|]))
            if (result.Success) then return! jsonCamelCase true next ctx
            else return! (setStatusCode 400 >=> jsonCamelCase result) next ctx
        }
 
let getCategories =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            use manager = getCategoryManager configuration
            let! result = manager.GetAllAsync(Criteria<EmptyModel>(PrimaryId = getUserId(ctx)))
            if (result.Success) then return! jsonCamelCase result.Value next ctx
            else return! (setStatusCode 400 >=> jsonCamelCase result) next ctx
        }

let getCategory id =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            use manager = getCategoryManager configuration
            let! result = manager.GetAsync(Criteria<DateRange>(
                                                PrimaryId = id, 
                                                Value = DateRange(
                                                            From = getQueryValueAsDate ctx "from", 
                                                            To = getQueryValueAsDate ctx "to")))
            if (result.Success) then return! jsonCamelCase result.Value next ctx
            else return! (setStatusCode 400 >=> jsonCamelCase result) next ctx
        }

let upsertCategory =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! category = ctx.BindJson<Category>()
            use manager = getCategoryManager configuration
            let! result = manager.UpsertAsync(Criteria<IEnumerable<Category>>(
                                                PrimaryId = getUserId(ctx), 
                                                Value = [|category|]))
            if (result.Success) then return! jsonCamelCase result.Value next ctx
            else return! (setStatusCode 400 >=> jsonCamelCase result) next ctx
        }
