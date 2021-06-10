namespace H3tech.Function

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Newtonsoft.Json
open Microsoft.Extensions.Logging
open FSharp.Data
open System.Text.Json

module gethtmlmeta =

    [<AllowNullLiteral>]
    type UrlContainer() =
        member val Url = "" with get, set

    [<Literal>]
    let Url = "url"

    [<FunctionName("gethtmlmeta")>]
    let run ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)>]req: HttpRequest) (log: ILogger) =
        async {
            log.LogInformation("F# HTTP trigger function processed a request.")
            let urlOpt = 
                if req.Query.ContainsKey(Url) then
                    Some(req.Query.[Url].[0])
                else
                    None
            use stream = new StreamReader(req.Body)
            let! reqBody = stream.ReadToEndAsync() |> Async.AwaitTask
            let data = JsonConvert.DeserializeObject<UrlContainer>(reqBody)
            let url =
                match urlOpt with
                | Some n -> n
                | None ->
                   match data with
                   | null -> ""
                   | nc -> nc.Url          
            let results = HtmlDocument.Load(url)
            let ogProperties = 
                results.Descendants ["meta"]
                |> Seq.choose (fun x -> 
                       x.TryGetAttribute("property")
                       |> Option.map (fun a -> a.Value(), x.AttributeValue("content"))
                )
                |> Seq.toList
            let ogPropsJson = JsonSerializer.Serialize(Map ogProperties)
            let responseMessage =             
                if (String.IsNullOrWhiteSpace(url)) then
                    "This HTTP triggered function executed successfully. Pass a url in the query string or in the request body for a JSON response."
                else
                    ogPropsJson
            return OkObjectResult(responseMessage) :> IActionResult
        } |> Async.StartAsTask