// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open FSharp.Data
open System.Text.RegularExpressions
open System.Text
open System.Web
open System.Diagnostics

[<EntryPoint>]
let main argv =
    // Get URL info
    let zoom = argv.[0]
    let uri = new Uri (argv.[1])

    let html = Http.RequestString uri.OriginalString

    // Get confNo
    let confnoRegex = new Regex (sprintf "https://%s/j/(\\d+)" uri.Host)
    let confNo = (confnoRegex.Match html).Groups.[1].Value

    let dataRegex = new Regex "window\.launchBase64 = \"([^\"]+)"
    let base64Data = (dataRegex.Match html).Groups.[1].Value
    let rawData = Convert.FromBase64String base64Data
                  |> Encoding.UTF8.GetString
    let identificationRegex = new Regex "�\u0001\0�\u0001y([^�]+)�\u0001 ([^�]+)�\u0001%(.+)$"
    let [uss; tid; utid] = (identificationRegex.Match rawData).Groups
                           |> List.ofSeq   
                           |> List.skip 1
                           |> List.map (fun g -> g.Value)

    let rawConfId = sprintf "utid=%s&uss=%s&tid=%s" utid uss tid
    let confId = Encoding.UTF8.GetBytes rawConfId
                 |> Convert.ToBase64String
                 |> HttpUtility.UrlEncode

    // Build the command line arguments
    let arguments = sprintf "-url=\"zoommtg://%s/join?action=join&confno=%s&confid=%s&browser=chrome\""
                            uri.Host confNo confId

    // Start Zoom
    let startInfo = new ProcessStartInfo(zoom, arguments)
    let p = new Process()
    p.StartInfo <- startInfo
    p.Start() |> ignore
            
    0 // return an integer exit code