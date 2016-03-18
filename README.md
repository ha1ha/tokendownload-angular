# tokendownload-angular
This an angular module to download files when working with tokens which contains of both client and server code (in c#).

## Solved Problem
When downloading files in an angular (or basically and other SPA-Framework) und using token-based authentication, you will need some additional efforts to let the user download files while ensuring the authentication and authorization of the user at the same time.
This because you cannot intercapt every link (and the HTTP-Request) in your application to add additional headers. This is how a traditional REST-Call is intercepted, by a HttpRequestInterceptor (in AngularJs)
There are different options while keeping the token-based approach intact

### InApp-Download and Base64-Url
You download the file using one or multiple XHR Requests in the SPA and transform it to a Base64-Data-Url to let the user download the file from the browser to the disk

### Use a session cookie
In addition to a token-based authentication, you will use a cookie-based approach. Cookies are sent to the server with every request (even) when the user clicks on the link

### Pass the token in the URL als Query-Parameter
Could be an option yes. But leads to security issues because, because the user will somewhen send that exact dowload link to his colleague. And voila, the colleague not only gets a working download link, she/he also gets a valid token for the whole application

### Create a limited download token
This is the idea of this code. The application server issues a token with limited lifetime, matching to that single file or ressource. Obviously, the token is signed.
