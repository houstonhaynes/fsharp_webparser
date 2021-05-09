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
    // Define a nullable container to deserialize into.
    [<AllowNullLiteral>]
    type UrlContainer() =
        member val Url = "" with get, set

    // For convenience, it's better to have a central place for the literal.
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

            let links = 
                results.Descendants ["meta"]
                |> Seq.choose (fun x -> 
                       x.TryGetAttribute("name")
                       |> Option.map (fun a -> x.InnerText(), a.Value())
                )
                |> Seq.toList

            let linksJson = JsonSerializer.Serialize links

            let responseMessage =             
                if (String.IsNullOrWhiteSpace(url)) then
                    "This HTTP triggered function executed successfully. Pass a url in the query string or in the request body for a JSON response."
                else
                    linksJson

            return OkObjectResult(responseMessage) :> IActionResult
        } |> Async.StartAsTask