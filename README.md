# TWITTER CLONE using Web sockets and WebSharper in F#

### Team members: Harish Reddy Bollavar (only me)

### Basic requirements: Finished

### Bonus requirements: Not Finished

#### Video Link:

https://youtu.be/U0Eoc7ymqNo

#### Steps to run the code:

    * You need to have Paket and dotnet installed.
    * Extract the contents of the zip file.
    * *cd TwitterClone*
    * Now run the command : *dotnet build*.
    * If you have trouble building, use .NET 3.1, else run the command : *dotnet run*.
    * The webpage will be served on http://localhost:8081.

#### Result:

    * Successfully implemented a JSON endpoint to communicate from client to server.
    * Used Websockets instead of the old HTTP rest end points.
    * Because of Websockets, the Tweets will automatically appear in the users feed without any request from the user.
    * Used WebSharper to create the webinterface(Client).
