module WebSharper.AspNetCore.Tests.WebSocketClient

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.AspNetCore.WebSocket
open WebSharper.AspNetCore.WebSocket.Client
open WebSocketServer
open WebSharper.UI
open Newtonsoft.Json.Linq
open System


module Server = WebSocketServer

// This us the client code that is viewed on the browser side.
[<JavaScript>]
let WebSocketTest (endpoint: WebSocketEndpoint<Server.S2CMessage, Server.C2SMessage>) =
    let mutable currentServer: WebSocketServer<WebSocketServer.S2CMessage, C2SMessage> option = None
    // This is used to hold the messages received from the server.
    let serverMessagesContainer = Elt.pre [] []

    let messagesHeader =
        Elt.div [] [
            Elt.h3 [] [
                text "Messages from server"
            ]
        ]
    // This is used to hold the tweets returned from the server.
    let tweetContainer = Elt.div [] []

    // This method is used to write to server .
    let writen fmt =
        Printf.ksprintf
            (fun s ->
                JS.Document.CreateTextNode(s + "\n")
                |> serverMessagesContainer.Dom.AppendChild
                |> ignore)
            fmt

    // Retweet handler.
    let retweet msg =
        currentServer.Value.Post(Server.Tweet msg)

    // This is used to populate the retweets on the UI side.
    let addTweetInUi (by, tweet, time) =
        let retweetButton =
            Elt.button [ attr.``class`` "retweet-handler btn btn-info"
                         on.click (fun _ _ -> retweet (tweet)) ] [
                text "Retweet!"
            ]

        let tweetContainerDiv = JS.Document.CreateElement("div")
        let tweetContentDiv = JS.Document.CreateElement("div")
        let retweetButtonDiv = JS.Document.CreateElement("div")
        let reteetButtonDom = retweetButton.Dom

        let retweetOption =
            retweetButtonDiv.AppendChild(reteetButtonDom)

        tweetContentDiv.InnerHTML <-
            String.Format(
                "<div style=\"display:flex;justify-content:center\"><h4>{0}</h4><h4 style=\"margin-left:10px\">{1}</h4><h4 style=\"margin-left:10px\">{2}</h4><div>",
                tweet,
                time,
                by
            )

        let tweetContainerContent =
            tweetContainerDiv.AppendChild(tweetContentDiv)

        tweetContainerContent.AppendChild(retweetOption)
        |> ignore

        tweetContainer.Dom.AppendChild(tweetContainerContent)
        |> ignore

    // This is used to connect with server using the websocket and .
    let server =
        async {
            return!
                ConnectStateful endpoint
                <| fun server ->
                    async {
                        return
                            0,
                            fun state msg ->
                                async {
                                    match msg with
                                    // Got some data from server.
                                    | Message data ->
                                        match data with
                                        | CommonResponse x ->
                                            // If type is tweet then add to tweets else add to server messages div.
                                            if (x.Contains("By")
                                                && x.Contains("Time")
                                                && x.Contains("Tweet")) then
                                                let tweetDetails = x.JS.Split(",")
                                                let by = tweetDetails.[0].JS.Split("\"").[3]
                                                let tweet = tweetDetails.[1].JS.Split("\"").[3]
                                                let time = tweetDetails.[2].JS.Split("\"").[3]
                                                addTweetInUi (by, tweet, time)
                                            else
                                                writen "%s" x
                                        | _ -> ignore ()

                                        return (state + 1)
                                    | Close ->
                                        writen "WebSocket connection closed."
                                        return state
                                    | Open ->
                                        writen "WebSocket connection open."
                                        return state
                                    | Error ->
                                        writen "WebSocket connection error!"
                                        return state
                                }
                    }
        }
    // Initiate server.
    server
        .AsPromise()
        .Then(fun x -> currentServer <- Some(x))
    |> ignore
    // Some variables to populate the input fields.
    let registerUserName = Var.Create ""
    let subscribeUserName = Var.Create ""
    let unSubscribeUserName = Var.Create ""
    let tweetContent = Var.Create ""
    let hashTagQuery = Var.Create ""
    let mentionQuery = Var.Create ""

    // Based on the action performed by user send message to server.
    let enableOnClick action _ _ =
        async {
            if currentServer = None then
                JS.Alert("Retrying connection with the server. Please try again.")

                server
                    .AsPromise()
                    .Then(fun x -> currentServer <- Some(x))
                |> ignore

            match action with
            | "Register" ->
                if (registerUserName.Value.Length < 6) then
                    JS.Alert("UserName must be atleast 6 characters.")
                else
                    currentServer.Value.Post(Server.Register registerUserName.Value)
            | "Subscribe" ->
                if (subscribeUserName.Value.Length < 6) then
                    JS.Alert("UserName must be atleast 6 characters.")
                else
                    currentServer.Value.Post(Server.Subscribe subscribeUserName.Value)
            | "Login" -> currentServer.Value.Post(Server.Login 1)
            | "Logout" -> currentServer.Value.Post(Server.Logout 1)
            | "Unsubscribe" ->
                if (unSubscribeUserName.Value.Length < 6) then
                    JS.Alert("UserName must be atleast 6 characters.")
                else
                    currentServer.Value.Post(Server.Unsubscribe unSubscribeUserName.Value)
            | "Tweet" ->
                if (tweetContent.Value.Length < 10) then
                    JS.Alert("Tweet must be atleast 10 characters.")
                else
                    currentServer.Value.Post(Server.Tweet tweetContent.Value)
            | "HashTag" ->
                if (hashTagQuery.Value.Length < 1) then
                    JS.Alert("Please enter atleast 1 characters.")
                else
                    currentServer.Value.Post(Server.HashTag("#" + hashTagQuery.Value))
            | "Mention" ->
                if (mentionQuery.Value.Length < 1) then
                    JS.Alert("Please enter atleast 1 characters.")
                else
                    currentServer.Value.Post(Server.Mention("@" + mentionQuery.Value))
            | _ -> ignore ()

            ignore ()
        }
        |> Async.Start

    // Register new user form.
    let registerForm =
        div [] [
            Doc.Input [ attr.``class`` "form-control" ] registerUserName
            button [ attr.``class`` "btn btn-primary"
                     on.click (enableOnClick "Register") ] [
                text "Register"
            ]
        ]
    // Subscribe user form.
    let subscribeForm =
        div [] [
            Doc.Input [ attr.``class`` "form-control" ] subscribeUserName
            button [ attr.``class`` "btn btn-primary"
                     on.click (enableOnClick "Subscribe") ] [
                text "Subscribe"
            ]
        ]
    // Unsubscribe user form.
    let unsubscribeForm =
        div [] [
            Doc.Input [ attr.``class`` "form-control" ] unSubscribeUserName
            button [ attr.``class`` "btn btn-primary"
                     on.click (enableOnClick "Unsubscribe") ] [
                text "Unsubscribe"
            ]
        ]
    // create Tweet  form.
    let createTweetForm =
        div [] [
            Doc.Input [ attr.``class`` "form-control" ] tweetContent

            button [ attr.``class`` "btn btn-primary"
                     on.click (enableOnClick "Tweet") ] [
                text "Tweet"
            ]
        ]
    // Search for hashtag form.
    let hashTagForm =
        div [] [
            Doc.Input [ attr.``class`` "form-control" ] hashTagQuery

            button [ attr.``class`` "btn btn-primary"
                     on.click (enableOnClick "HashTag") ] [
                text "HashTag search"
            ]
        ]
    // Mention form.
    let mentionForm =
        div [] [
            Doc.Input [ attr.``class`` "form-control" ] mentionQuery

            button [ attr.``class`` "btn btn-primary"
                     on.click (enableOnClick "Mention") ] [
                text "Mention search"
            ]
        ]
    // Login &  Logout buttons.
    let loginAndLogout =
        div [ attr.``class`` "user-login-logout" ] [
            button [ attr.``class`` "btn btn-success"
                     on.click (enableOnClick "Login") ] [
                text "Login"
            ]
            button [ attr.``class`` "btn btn-danger"
                     on.click (enableOnClick "Logout") ] [
                text "Logout"
            ]
        ]
    // Final page view.
    div [] [
        loginAndLogout
        div [] [
            registerForm
            subscribeForm
            unsubscribeForm
            createTweetForm
            hashTagForm
            mentionForm
            messagesHeader
            serverMessagesContainer
            tweetContainer
        ]

    ]

open WebSharper.AspNetCore.WebSocket

let MyEndPoint (url: string) : WebSharper.AspNetCore.WebSocket.WebSocketEndpoint<Server.S2CMessage, Server.C2SMessage> =
    WebSocketEndpoint.Create(url, "/ws", JsonEncoding.Readable)
