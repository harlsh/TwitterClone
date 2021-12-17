(function(Global)
{
 "use strict";
 var WebSharper,AspNetCore,Tests,WebSocketClient,Website,SomeRecord,Client,TwitterClone_Templates,TwitterClone_JsonEncoder,TwitterClone_JsonDecoder,UI,Doc,AttrProxy,AttrModule,Arrays,Concurrency,Unchecked,JavaScript,Promise,WebSocket,Client$1,WithEncoding,JSON,Utils,Var$1,IntelliFactory,Runtime,Templating,Runtime$1,Server,ProviderBuilder,Handler,TemplateInstance,Client$2,Templates,ClientSideJson,Provider;
 WebSharper=Global.WebSharper=Global.WebSharper||{};
 AspNetCore=WebSharper.AspNetCore=WebSharper.AspNetCore||{};
 Tests=AspNetCore.Tests=AspNetCore.Tests||{};
 WebSocketClient=Tests.WebSocketClient=Tests.WebSocketClient||{};
 Website=Tests.Website=Tests.Website||{};
 SomeRecord=Website.SomeRecord=Website.SomeRecord||{};
 Client=Website.Client=Website.Client||{};
 TwitterClone_Templates=Global.TwitterClone_Templates=Global.TwitterClone_Templates||{};
 TwitterClone_JsonEncoder=Global.TwitterClone_JsonEncoder=Global.TwitterClone_JsonEncoder||{};
 TwitterClone_JsonDecoder=Global.TwitterClone_JsonDecoder=Global.TwitterClone_JsonDecoder||{};
 UI=WebSharper&&WebSharper.UI;
 Doc=UI&&UI.Doc;
 AttrProxy=UI&&UI.AttrProxy;
 AttrModule=UI&&UI.AttrModule;
 Arrays=WebSharper&&WebSharper.Arrays;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 Unchecked=WebSharper&&WebSharper.Unchecked;
 JavaScript=WebSharper&&WebSharper.JavaScript;
 Promise=JavaScript&&JavaScript.Promise;
 WebSocket=AspNetCore&&AspNetCore.WebSocket;
 Client$1=WebSocket&&WebSocket.Client;
 WithEncoding=Client$1&&Client$1.WithEncoding;
 JSON=Global.JSON;
 Utils=WebSharper&&WebSharper.Utils;
 Var$1=UI&&UI.Var$1;
 IntelliFactory=Global.IntelliFactory;
 Runtime=IntelliFactory&&IntelliFactory.Runtime;
 Templating=UI&&UI.Templating;
 Runtime$1=Templating&&Templating.Runtime;
 Server=Runtime$1&&Runtime$1.Server;
 ProviderBuilder=Server&&Server.ProviderBuilder;
 Handler=Server&&Server.Handler;
 TemplateInstance=Server&&Server.TemplateInstance;
 Client$2=UI&&UI.Client;
 Templates=Client$2&&Client$2.Templates;
 ClientSideJson=WebSharper&&WebSharper.ClientSideJson;
 Provider=ClientSideJson&&ClientSideJson.Provider;
 WebSocketClient.WebSocketTest=function(endpoint)
 {
  var currentServer,serverMessagesContainer,messagesHeader,tweetContainer,server,b,registerUserName,subscribeUserName,unSubscribeUserName,tweetContent,hashTagQuery,mentionQuery,registerForm,subscribeForm,unsubscribeForm,createTweetForm,hashTagForm,mentionForm;
  function writen(fmt)
  {
   return fmt(function(s)
   {
    var x;
    x=self.document.createTextNode(s+"\n");
    serverMessagesContainer.elt.appendChild(x);
   });
  }
  function retweet(msg)
  {
   currentServer.$0.Post({
    $:5,
    $0:msg
   });
  }
  function addTweetInUi(by,tweet,time)
  {
   var retweetButton,tweetContainerDiv,tweetContentDiv,retweetOption,$1,tweetContainerContent;
   retweetButton=Doc.Element("button",[AttrProxy.Create("class","retweet-handler btn btn-info"),AttrModule.Handler("click",function()
   {
    return function()
    {
     return retweet(tweet);
    };
   })],[Doc.TextNode("Retweet!")]);
   tweetContainerDiv=self.document.createElement("div");
   tweetContentDiv=self.document.createElement("div");
   retweetOption=self.document.createElement("div").appendChild(retweetButton.elt);
   tweetContentDiv.innerHTML=($1=[tweet,time,by],"<div style=\"display:flex;justify-content:center\"><h4>"+(Arrays.get($1,0)==null?"":Global.String(Arrays.get($1,0)))+("</h4><h4 style=\"margin-left:10px\">"+(Arrays.get($1,1)==null?"":Global.String(Arrays.get($1,1))))+("</h4><h4 style=\"margin-left:10px\">"+(Arrays.get($1,2)==null?"":Global.String(Arrays.get($1,2))))+"</h4><div>");
   tweetContainerContent=tweetContainerDiv.appendChild(tweetContentDiv);
   tweetContainerContent.appendChild(retweetOption);
   tweetContainer.elt.appendChild(tweetContainerContent);
  }
  function enableOnClick(action,a,a$1)
  {
   var b$1;
   return Concurrency.Start((b$1=null,Concurrency.Delay(function()
   {
    return Concurrency.Combine(Unchecked.Equals(currentServer,null)?(Global.alert("Retrying connection with the server. Please try again."),Promise.OfAsync(server).then(function(x)
    {
     currentServer={
      $:1,
      $0:x
     };
    }),Concurrency.Zero()):Concurrency.Zero(),Concurrency.Delay(function()
    {
     return Concurrency.Combine(action==="Register"?registerUserName.Get().length<6?(Global.alert("UserName must be atleast 6 characters."),Concurrency.Zero()):(currentServer.$0.Post({
      $:0,
      $0:registerUserName.Get()
     }),Concurrency.Zero()):action==="Subscribe"?subscribeUserName.Get().length<6?(Global.alert("UserName must be atleast 6 characters."),Concurrency.Zero()):(currentServer.$0.Post({
      $:6,
      $0:subscribeUserName.Get()
     }),Concurrency.Zero()):action==="Login"?(currentServer.$0.Post({
      $:1,
      $0:1
     }),Concurrency.Zero()):action==="Logout"?(currentServer.$0.Post({
      $:2,
      $0:1
     }),Concurrency.Zero()):action==="Unsubscribe"?unSubscribeUserName.Get().length<6?(Global.alert("UserName must be atleast 6 characters."),Concurrency.Zero()):(currentServer.$0.Post({
      $:7,
      $0:unSubscribeUserName.Get()
     }),Concurrency.Zero()):action==="Tweet"?tweetContent.Get().length<10?(Global.alert("Tweet must be atleast 10 characters."),Concurrency.Zero()):(currentServer.$0.Post({
      $:5,
      $0:tweetContent.Get()
     }),Concurrency.Zero()):action==="HashTag"?hashTagQuery.Get().length<1?(Global.alert("Please enter atleast 1 characters."),Concurrency.Zero()):(currentServer.$0.Post({
      $:3,
      $0:"#"+hashTagQuery.Get()
     }),Concurrency.Zero()):action==="Mention"?mentionQuery.Get().length<1?(Global.alert("Please enter atleast 1 characters."),Concurrency.Zero()):(currentServer.$0.Post({
      $:4,
      $0:"@"+mentionQuery.Get()
     }),Concurrency.Zero()):Concurrency.Zero(),Concurrency.Delay(function()
     {
      return Concurrency.Zero();
     }));
    }));
   })),null);
  }
  currentServer=null;
  serverMessagesContainer=Doc.Element("pre",[],[]);
  messagesHeader=Doc.Element("div",[],[Doc.Element("h3",[],[Doc.TextNode("Messages from server")])]);
  tweetContainer=Doc.Element("div",[],[]);
  server=(b=null,Concurrency.Delay(function()
  {
   return WithEncoding.ConnectStateful(function(a)
   {
    return JSON.stringify((TwitterClone_JsonEncoder.j())(a));
   },function(a)
   {
    return(TwitterClone_JsonDecoder.j())(JSON.parse(a));
   },endpoint,function()
   {
    var b$1;
    b$1=null;
    return Concurrency.Delay(function()
    {
     return Concurrency.Return([0,function(state)
     {
      return function(msg)
      {
       var b$2;
       b$2=null;
       return Concurrency.Delay(function()
       {
        var data,x,tweetDetails;
        return msg.$==3?(writen(function($1)
        {
         return $1("WebSocket connection closed.");
        }),Concurrency.Return(state)):msg.$==2?(writen(function($1)
        {
         return $1("WebSocket connection open.");
        }),Concurrency.Return(state)):msg.$==1?(writen(function($1)
        {
         return $1("WebSocket connection error!");
        }),Concurrency.Return(state)):(data=msg.$0,Concurrency.Combine(data.$==0?(x=data.$0,x.indexOf("By")!=-1&&x.indexOf("Time")!=-1&&x.indexOf("Tweet")!=-1?(tweetDetails=x.split(","),(addTweetInUi(Arrays.get(Arrays.get(tweetDetails,0).split("\""),3),Arrays.get(Arrays.get(tweetDetails,1).split("\""),3),Arrays.get(Arrays.get(tweetDetails,2).split("\""),3)),Concurrency.Zero())):((writen(function($1)
        {
         return function($2)
         {
          return $1(Utils.toSafe($2));
         };
        }))(x),Concurrency.Zero())):Concurrency.Zero(),Concurrency.Delay(function()
        {
         return Concurrency.Return(state+1);
        })));
       });
      };
     }]);
    });
   });
  }));
  Promise.OfAsync(server).then(function(x)
  {
   currentServer={
    $:1,
    $0:x
   };
  });
  registerUserName=Var$1.Create$1("");
  subscribeUserName=Var$1.Create$1("");
  unSubscribeUserName=Var$1.Create$1("");
  tweetContent=Var$1.Create$1("");
  hashTagQuery=Var$1.Create$1("");
  mentionQuery=Var$1.Create$1("");
  registerForm=Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control")],registerUserName),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrModule.Handler("click",(Runtime.Curried3(enableOnClick))("Register"))],[Doc.TextNode("Register")])]);
  subscribeForm=Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control")],subscribeUserName),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrModule.Handler("click",(Runtime.Curried3(enableOnClick))("Subscribe"))],[Doc.TextNode("Subscribe")])]);
  unsubscribeForm=Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control")],unSubscribeUserName),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrModule.Handler("click",(Runtime.Curried3(enableOnClick))("Unsubscribe"))],[Doc.TextNode("Unsubscribe")])]);
  createTweetForm=Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control")],tweetContent),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrModule.Handler("click",(Runtime.Curried3(enableOnClick))("Tweet"))],[Doc.TextNode("Tweet")])]);
  hashTagForm=Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control")],hashTagQuery),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrModule.Handler("click",(Runtime.Curried3(enableOnClick))("HashTag"))],[Doc.TextNode("HashTag search")])]);
  mentionForm=Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control")],mentionQuery),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrModule.Handler("click",(Runtime.Curried3(enableOnClick))("Mention"))],[Doc.TextNode("Mention search")])]);
  return Doc.Element("div",[],[Doc.Element("div",[AttrProxy.Create("class","user-login-logout")],[Doc.Element("button",[AttrProxy.Create("class","btn btn-success"),AttrModule.Handler("click",(Runtime.Curried3(enableOnClick))("Login"))],[Doc.TextNode("Login")]),Doc.Element("button",[AttrProxy.Create("class","btn btn-danger"),AttrModule.Handler("click",(Runtime.Curried3(enableOnClick))("Logout"))],[Doc.TextNode("Logout")])]),Doc.Element("div",[],[registerForm,subscribeForm,unsubscribeForm,createTweetForm,hashTagForm,mentionForm,messagesHeader,serverMessagesContainer,tweetContainer])]);
 };
 SomeRecord.New=function(Name)
 {
  return{
   Name:Name
  };
 };
 Client.Main=function(wsep)
 {
  var b,W,_this,p,i;
  return(b=(W=WebSocketClient.WebSocketTest(wsep),(_this=new ProviderBuilder.New$1(),(_this.h.push({
   $:0,
   $0:"websockettest",
   $1:W
  }),_this))),(p=Handler.CompleteHoles(b.k,b.h,[]),(i=new TemplateInstance.New(p[1],TwitterClone_Templates.body(p[0])),b.i=i,i))).get_Doc();
 };
 TwitterClone_Templates.body=function(h)
 {
  Templates.LoadLocalTemplates("main");
  return h?Templates.NamedTemplate("main",{
   $:1,
   $0:"body"
  },h):void 0;
 };
 TwitterClone_JsonEncoder.j=function()
 {
  return TwitterClone_JsonEncoder._v?TwitterClone_JsonEncoder._v:TwitterClone_JsonEncoder._v=(Provider.EncodeUnion(void 0,{
   unsub:7,
   subs:6,
   tweet:5,
   mention:4,
   hashTag:3,
   randomInt2:2,
   randomInt:1,
   reg:0
  },[["Register",[["$0","reg",Provider.Id(),0]]],["Login",[["$0","randomInt",Provider.Id(),0]]],["Logout",[["$0","randomInt2",Provider.Id(),0]]],["HashTag",[["$0","hashTag",Provider.Id(),0]]],["Mention",[["$0","mention",Provider.Id(),0]]],["Tweet",[["$0","tweet",Provider.Id(),0]]],["Subscribe",[["$0","subs",Provider.Id(),0]]],["Unsubscribe",[["$0","unsub",Provider.Id(),0]]]]))();
 };
 TwitterClone_JsonDecoder.j=function()
 {
  return TwitterClone_JsonDecoder._v?TwitterClone_JsonDecoder._v:TwitterClone_JsonDecoder._v=(Provider.DecodeUnion(void 0,"type",[["string",[["$0","value",Provider.Id(),0]]],["string",[["$0","value",Provider.Id(),0]]],["string",[["$0","value",Provider.Id(),0]]],["string",[["$0","value",Provider.Id(),0]]],["string",[["$0","value",Provider.Id(),0]]],["string",[["$0","value",Provider.Id(),0]]],["string",[["$0","value",Provider.Id(),0]]],["string",[["$0","value",Provider.Id(),0]]],["string",[["$0","value",Provider.Id(),0]]],["string",[["$0","value",Provider.Id(),0]]]]))();
 };
}(self));
