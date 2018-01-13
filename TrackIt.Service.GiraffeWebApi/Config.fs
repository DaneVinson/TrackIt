
module TrackIt.Service.GiraffeWebApi.Config

open System.IO
open Microsoft.Extensions.Configuration

let configuration =
    let builder = ConfigurationBuilder()
    builder.SetBasePath(Directory.GetCurrentDirectory()) |> ignore
    builder.AddJsonFile("config.json") |> ignore
    builder.Build()
