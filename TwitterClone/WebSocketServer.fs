
module WebSharper.AspNetCore.Tests.WebSocketServer

open WebSharper
open WebSharper.AspNetCore.WebSocket.Server
open System.Collections.Generic;
open System
open System.Threading
open System.Threading.Tasks
open System.Net.Sockets
open System.IO
open System.Linq;
open Newtonsoft.Json

type [<JavaScript; NamedUnionCases>]
    C2SMessage =
    | Register of reg: string
    | Login of randomInt:int
    | Logout of randomInt2: int
    | HashTag of hashTag:string
    | Mention of mention:string
    | Tweet of tweet:string
    | Subscribe of subs:string
    | Unsubscribe of unsub:string

and [<JavaScript; NamedUnionCases "type">]
    S2CMessage =
    | [<Name "string">] CommonResponse of value: string
    | [<Name "string">] RegisterResponse of value: string
    | [<Name "string">] LoginResponse of value: string
    | [<Name "string">] LogoutResponse of value: string
    | [<Name "string">] SubscribeResponse of value: string
    | [<Name "string">] UnsubscribeResponse of value: string
    | [<Name "string">] TweetResponse of value: string
    | [<Name "string">] HashTagResponse of value: string
    | [<Name "string">] MentionResponse of value: string
    | [<Name "string">] ErrorResponse of value:string
[<System.SerializableAttribute>]
type Tweet() =  
    [<DefaultValue>]
    val mutable By : string 
    [<DefaultValue>]
    val mutable Tweet : string
    [<DefaultValue>]
    val mutable Time : string

type User() = 
    [<DefaultValue>]
    val mutable Name : string
    [<DefaultValue>]
    val mutable Status : bool
    [<DefaultValue>]
    val mutable Feeds : List<Tweet>
    [<DefaultValue>]
    val mutable SubscribersList : HashSet<string>


let Start() : StatefulAgent<S2CMessage, C2SMessage, int> =
    /// print to debug output and stdout
    let dprintfn x =
        Printf.ksprintf (fun s ->
            System.Diagnostics.Debug.WriteLine s
            stdout.WriteLine s
        ) x
    let mutable users = new Dictionary<string, User>()
    let mutable userGuIdMapper = new Dictionary<string, string>()
    let mutable guIdUserMapper =  new Dictionary<string, string>()
    let mutable hashTags  =  new Dictionary<string, List<Tweet>>()
    let mutable mentions = new Dictionary<string, List<Tweet>>()
    let mutable closedConnectionList =  new List<string>()
    let mutable totalUsers  = 0
    let mutable totalOnlineUsers = 0
    let mutable totalOfflineUsers = 0
    let mutable totalTweets = 0
    
    let mutable clientTaskList : (string  * WebSocketClient<S2CMessage,C2SMessage>) list = []
    
    let addClientTask cl = clientTaskList <- cl :: clientTaskList
    
    
    // Used to communicate with the client.
    let writeToClient (cl:WebSocketClient<S2CMessage,C2SMessage>, message ) =
        async{
            do! cl.PostAsync(message)
        }
    // Used to send a message to subscribed users of the users.
    let forwadMessageToSubscribedClients msg =
        clientTaskList |> List.map(fun (_, stream) -> writeToClient(stream,msg))
    
    // Used to send message to the client based on the cleint id.
    let sendMessageToClient (clientId, msg ) = 
        let client = clientTaskList |> List.find (fun (id, _) -> id = clientId) 
        let _, clientStream =  client
        writeToClient(clientStream,msg) |> Async.Ignore
    
    // Used to send message to the used using the userId.
    let sendMessageToUser userId msg = 
        let clientId  =  userGuIdMapper.[userId]
        let client = clientTaskList |> List.find (fun (id, _) -> id = clientId) 
        let _, clientStream =  client
        writeToClient(clientStream,msg) |> Async.Ignore

    
    // Used to remove a  connection from the clients list.
    let removeFirst pred list = 
        let rec removeFirstTailRec p l acc =
            match l with
            | [] -> acc |> List.rev
            | h::t when p h -> (acc |> List.rev) @ t
            | h::t -> removeFirstTailRec p t (h::acc)
        removeFirstTailRec pred list []

    // Used to create new user object.
    let createUser(name: string) = 
        let user = User()
        user.Name <- name
        user.Status <-  true
        user.Feeds <- new List<Tweet>()
        user.SubscribersList <- new HashSet<string>()
        user

    // Used to create new tweet object.
    let createTweet(tweetContent: string,user:string) = 
        let tweet =  Tweet()
        tweet.By <- user
        tweet.Time  <- DateTime.Now.ToString()
        tweet.Tweet <- tweetContent 
        tweet

    // Creates a socket connection with the client.
    fun client -> async {
        let clientIp = client.Connection.Context.Connection.RemoteIpAddress.ToString()
        let clientId =  System.Guid.NewGuid().ToString()
        addClientTask( clientId, client)
        return 0, fun state msg -> async {
            dprintfn "Received message #%i from %s" state clientId
            // This is used perform the required actions on the inmemory database(we are using lists) to communicate with the client.
            match msg with
            | Message data -> 
                match data with
                | Register userName ->
                    // If user is already present then donot add to db else add and acknowledge.
                    if(users.ContainsKey(userName)) then
                        do! sendMessageToClient(clientId, ErrorResponse "Error: Error while creating user. User already present") |> Async.Ignore
                        dprintfn "Ignoring request as user with id is already present"
                    else if (guIdUserMapper.ContainsKey(clientId)) then
                        do! sendMessageToClient(clientId, ErrorResponse "Error: You can only create a single user with single socket connection. User already present with this client. Open new tab for creating new user.") |> Async.Ignore
                        dprintfn "Ignoring request as user with id is already present"
                    else
                        let user = createUser userName
                        users.Add(userName, user) |> ignore
                        userGuIdMapper.Add(userName,clientId) |> ignore
                        guIdUserMapper.Add(clientId,userName) |> ignore
                        totalUsers <- totalUsers+1
                        totalOnlineUsers <- totalOnlineUsers+1
                        do! writeToClient(client, RegisterResponse "Successfully registered.")
                | Login ints ->
                    // If not logged in send the previous tweets else if user is already logged in then do nothing.
                    let user =  users.[guIdUserMapper.[clientId]]
                    if(not user.Status) then
                        user.Status <- true
                        totalOnlineUsers <- totalOnlineUsers + 1
                        totalOfflineUsers <- totalOfflineUsers - 1
                        do! writeToClient(client, ErrorResponse  "You are now logged in.")
                        if (user.Feeds.Count > 0) then
                            for i in user.Feeds do
                               do! sendMessageToClient(clientId, (TweetResponse(JsonConvert.SerializeObject(i)))) |> Async.Ignore
                        user.Feeds <-  new List<Tweet>()
                    else
                        do! writeToClient(client, ErrorResponse  "You are already logged in.")
                | Logout  ints->
                    // If logged in log him/her out else do nothing.
                    let user = users.[guIdUserMapper.[clientId]]
                    if user.Status then
                        user.Status <- false
                        totalOfflineUsers <- totalOfflineUsers + 1
                        totalOnlineUsers <- totalOnlineUsers - 1
                        do! writeToClient(client, CommonResponse  "You are logged out.")
                    else 
                        do! writeToClient(client, CommonResponse  "You are already logged out.")
                | HashTag hashtag ->
                    // If registered and logged in send the tweets with hashtags.
                    if (not (guIdUserMapper.ContainsKey(clientId)))  then 
                        do! sendMessageToClient(clientId, ErrorResponse "Error: Not registered yet") |> Async.Ignore
                    else
                        let userName =  guIdUserMapper.[clientId]
                        let user =  users.[userName]
                        if not user.Status then
                            do! sendMessageToClient(clientId, ErrorResponse "Error: Not Logged in")   |> Async.Ignore
                        else
                            if hashTags.ContainsKey(hashtag) &&  hashTags.[hashtag].Count >0 then
                                for i in hashTags.[hashtag] do 
                                    do! sendMessageToClient(clientId, (TweetResponse(JsonConvert.SerializeObject(i)))) |> Async.Ignore 
                                do! writeToClient(client, CommonResponse  "Tweets returned for the hashtag(Find them below)")
                            else 
                                do! writeToClient(client, CommonResponse  "No tweets found with given hashtag")
                | Mention mention->
                    // If registered and logged in send the tweets with mentions.
                    if (not (guIdUserMapper.ContainsKey(clientId)))  then 
                        do! sendMessageToClient(clientId, ErrorResponse "Error: Not registered yet") |> Async.Ignore
                    else
                        let userName =  guIdUserMapper.[clientId]
                        let user =  users.[userName]
                        if not user.Status then
                            do! sendMessageToClient(clientId, ErrorResponse "Error: Not Logged in")   |> Async.Ignore
                        else
                            if mentions.ContainsKey(mention) &&  mentions.[mention].Count >0 then
                                for i in mentions.[mention] do 
                                    do! sendMessageToClient(clientId, (TweetResponse(JsonConvert.SerializeObject(i)))) |> Async.Ignore
                                do! writeToClient(client, CommonResponse  "Tweets returned for the mention(Find them below)")
                            else 
                                do! writeToClient(client, ErrorResponse "No tweets found with given mention")
                | Tweet tweetMsg->
                    // If not registered or logged in then do nothing.
                    if (not (guIdUserMapper.ContainsKey(clientId)))  then 
                        do! sendMessageToClient(clientId, ErrorResponse "Error: Not registered yet") |> Async.Ignore
                    else
                        let userName =  guIdUserMapper.[clientId]
                        let user =  users.[userName]
                        if not user.Status then
                            do! sendMessageToClient(clientId, ErrorResponse "Error: Not Logged in")   |> Async.Ignore
                        else
                            // If registered and logged in, check for hashtags and mentions and add the tweet to storage.
                            let tweet =  createTweet(tweetMsg,userName)
                            totalTweets <- totalTweets + 1 
                            let notifiedUsers = new List<string>();
                            for i in tweet.Tweet.Split(' ') do
                                if (i.StartsWith "#") && i.Length>1 then
                                    if(not (hashTags.ContainsKey(i))) then 
                                        let list = new List<Tweet>()
                                        hashTags.Add(i,list) |> ignore
                                    hashTags.[i].Add(tweet)
                                else if i.StartsWith("@") && i.Length >1 then
                                    if users.ContainsKey(i.Substring(1)) then 
                                        if(not (mentions.ContainsKey(i))) then 
                                            let list = new List<Tweet>()
                                            mentions.TryAdd(i,list) |> ignore
                                        mentions.[i].Add(tweet)
                                        let receivingUser =  users.[i.Substring(1)] 
                                        if(not receivingUser.Status) then
                                            receivingUser.Feeds.Add(tweet)
                                        else
                                            let clientId  =  userGuIdMapper.[i.Substring(1)]
                                            if(not (closedConnectionList.Contains (clientId))) then
                                                do! sendMessageToUser(i.Substring(1)) (TweetResponse(JsonConvert.SerializeObject(tweet))) |> Async.Ignore
                                            else 
                                               dprintfn "Client is alreadry closed so ignoring." 
                                        notifiedUsers.Add(i.Substring(1))
                            if(user.SubscribersList.Count >0 ) then
                                let subscribersLeft = user.SubscribersList.Where(fun x -> not (notifiedUsers.Contains(x)))
                                for i in subscribersLeft do 
                                    let notifyUser  =  users.[i]
                                    if(not notifyUser.Status) then
                                        notifyUser.Feeds.Add(tweet)
                                    else
                                        let clientId  =  userGuIdMapper.[i]
                                        if(not (closedConnectionList.Contains (clientId))) then
                                            do! sendMessageToUser(i) (TweetResponse(JsonConvert.SerializeObject(tweet))) |> Async.Ignore
                                        else 
                                           dprintfn "Client is already closed so ignoring." 
                            do! sendMessageToClient(clientId, TweetResponse "Created tweet successfully")   |> Async.Ignore
                | Subscribe subscribeToUserName ->
                    if (not (guIdUserMapper.ContainsKey(clientId)))  then 
                        do! sendMessageToClient(clientId, ErrorResponse "Error: Not registered yet") |> Async.Ignore
                    else
                        let userName =  guIdUserMapper.[clientId]
                        let user =  users.[userName]
                        if not user.Status then
                            do! sendMessageToClient(clientId, ErrorResponse "Error: Not Logged in") |> Async.Ignore
                        else
                            // Add to subsscribers list of teh user.
                            if(users.ContainsKey(subscribeToUserName)) then
                                let userName =  guIdUserMapper.[clientId]
                                let subscribeToUser  =  users.[subscribeToUserName]
                                subscribeToUser.SubscribersList.Add(userName) |> ignore
                                do! sendMessageToClient(clientId, SubscribeResponse "Subscribed successfully.") |> Async.Ignore
                            else
                                do! sendMessageToClient(clientId, ErrorResponse "Error: Entered userId not present") |> Async.Ignore
                | Unsubscribe unSubscribeToUserName->
                    if (not (guIdUserMapper.ContainsKey(clientId)))  then 
                        do! sendMessageToClient(clientId, ErrorResponse "Error: Not registered yet") |> Async.Ignore
                    else
                        let userName =  guIdUserMapper.[clientId]
                        let user =  users.[userName]
                        if not user.Status then
                            do! sendMessageToClient(clientId, ErrorResponse "Error: Not Logged in") |> Async.Ignore
                        else
                            // Remove from subscribers list.
                            if(users.ContainsKey(unSubscribeToUserName) && users.[unSubscribeToUserName].SubscribersList.Contains(userName)) then
                                let userName =  guIdUserMapper.[clientId]
                                let unSubscribeToUser  =  users.[unSubscribeToUserName]
                                unSubscribeToUser.SubscribersList.RemoveWhere(fun x -> x= userName) |> ignore;
                            else 
                                do! sendMessageToClient(clientId, ErrorResponse "Error: UserId is not present or you are not subscribed to user") |> Async.Ignore
                return state + 1
            | Error exn -> 
                eprintfn "Error in WebSocket server connected to %s: %s" clientIp exn.Message
                do! client.PostAsync (CommonResponse ("Error: " + exn.Message))
                return state
            | Close ->
                closedConnectionList.Add(clientId)
                dprintfn "Closed connection to %s" clientIp
                return state
        }
    }
