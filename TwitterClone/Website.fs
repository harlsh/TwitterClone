module WebSharper.AspNetCore.Tests.Website

open Microsoft.Extensions.Logging
open WebSharper
open WebSharper.AspNetCore
open WebSharper.JavaScript
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Templating

type IndexTemplate = Template<"Main.html", clientLoad=ClientLoad.FromDocument>

[<AbstractClass>]
type RpcUserSession() =
    [<Rpc>]
    abstract GetLogin : unit -> Async<option<string>>

    [<Rpc>]
    abstract Login : name: string -> Async<unit>

    [<Rpc>]
    abstract Logout : unit -> Async<unit>

type EndPoint = | [<EndPoint "/">] Home


[<JavaScript>]
type SomeRecord = { Name: string }

[<Rpc>]
let DoSomething () = async { return { Name = "Yo." } }

[<JavaScript>]
[<Require(typeof<Resources.BaseResource>, "//maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css")>]
module Client =
    open WebSharper.UI.Client

    let Main wsep =
        IndexTemplate
            .Body()
            .WebSocketTest(WebSocketClient.WebSocketTest wsep)
            .Doc()


open WebSharper.UI.Server

type MyWebsite(logger: ILogger<MyWebsite>) =
    inherit SiteletService<EndPoint>()

    override this.Sitelet =
        Application.MultiPage (fun (ctx: Context<_>) (ep: EndPoint) ->
            match ep with
            | Home ->
                let wsep =
                    WebSocketClient.MyEndPoint(ctx.RequestUri.ToString())

                IndexTemplate()
                    .Main(client <@ Client.Main wsep @>)
                    .Doc()
                |> Content.Page)
