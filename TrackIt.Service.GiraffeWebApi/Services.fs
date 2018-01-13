module TrackIt.Service.GiraffeWebApi.Services

open Microsoft.Extensions.Configuration
open TrackIt.Domain.Model.Interfaces
open TrackIt.Domain.Model.Models
open TrackIt.Domain.Logic.Managers
open TrackIt.Data.Dapper
open TrackIt.Data.DocumentDB
open TrackIt.Data.EFCore

let getRepository<'T when 'T :> IModel and 'T : not struct and 'T : (new : unit -> 'T)> (config : IConfigurationRoot) = 
    match config.["appSettings:repository"] with
        | "dapper" ->
            let dataConfig = DapperConfiguration(config.["ConnectionStrings:TrackItBravo"])
            new TrackIt.Data.Dapper.Repository<'T>(dataConfig) :> IRepository<'T>
        | "documentdb" -> 
            let dataConfig = DocumentDBConfiguration(
                                config.["documentDB:AccountKey"], 
                                config.["documentDB:AccountUri"], 
                                config.["documentDB:DatabaseName"])
            new TrackIt.Data.DocumentDB.Repository<'T>(dataConfig) :> IRepository<'T>
        | "efcore" ->
            let dataConfig = DbConfiguration(config.["ConnectionStrings:TrackItAlpha"])
            new TrackIt.Data.EFCore.Repository<'T>(dataConfig) :> IRepository<'T>
        | _ -> new TrackIt.Data.DocumentDB.Repository<'T>(DocumentDBConfiguration("", "", "")) :> IRepository<'T>

let getDataPointManager config = new DataPointManager((getRepository<DataPoint> config), (getRepository<Category> config))

let getCategoryManager config = new CategoryManager(
                                        (getRepository<Category> config), 
                                        (getRepository<DataPoint> config))
