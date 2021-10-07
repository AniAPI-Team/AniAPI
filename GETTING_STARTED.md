# Getting started

Follow this step-by-step guide to setup your environment and start contributing to AniAPI.

## Source code

As first thing, you need to download AniAPI's source code. You can do that using a GUI client (like [Github Desktop](https://desktop.github.com/)) or directly from command line:

```bash
git clone https://github.com/AniAPI-Team/AniAPI.git
```

## IDE

After, you need an IDE which supports `C#` (**.NET Core**). I suggest you to use [Visual Studio](https://visualstudio.microsoft.com/it/vs/), but another similar can be also good.

## MongoDB

Firstly, download and install [MongoDB Community Server](https://www.mongodb.com/try/download/community), leaving all settings as default.
After that create a new database (using shell or any GUI client) called `aniapi_dotnet`.

AniAPI uses a specific collection as configurations container, named `app_settings` (you have to manually create it).
You also need to setup some fields in order to configure your environment.

At the end of this step, your `app_settings` collection should have a single document like this one below:

```json
{
  "_id": 0,
  "jwt_secret": "...",
  "recaptcha_secret": "...",
  "proxy_host": "...",
  "proxy_port": "...",
  "proxy_username": "...",
  "proxy_password": "...",
  "proxy_count": ...,
  "resources_version": "...",
  "api_endpoint":"...",
  "smtp": {
    "host": "...",
    "port": ...,
    "username": "...",
    "password": "...",
    "address":"..."
  },
  "mal": {
    "client_id": "...",
    "client_secret": "..."
  }
}
```

**All fields should exists, leave blank ones you do not need!**

### Secret

`jwt_secret` field is used by **WebAPI** project as secret to sign JWTs.
You can go to [this website](https://www.grc.com/passwords.htm) and take the third random string as secret, for example:

![Secret example](/static/secret_image.png)

### Recaptcha

`recaptcha_secret` field is used by **WebAPI** project to avoid bots on `registration` and `login` pages. It involves [reCAPTCHA](https://www.google.com/recaptcha/about/) service by *Google*.

As this field is used only in production environment, you can leave it blank.

### Proxies

As AniAPI **SyncService** project needs proxies to avoid blocked site requests, you need to setup this part.
**If you do not plan to use this project, you can skip this step**.

In this step you will setup a *Webshare* free-tier account.

#### Create a Webshare account

Simply go to [signup page](https://proxy.webshare.io/register/) and create your account.

#### Get your proxies configuration

Once you login with your new account, go to [proxy's list page](https://proxy.webshare.io/proxy/list?).
Refer to the first table's line and fill these fields:

* `proxy_host` with your **Proxy Address** value
* `proxy_port` with your **Port** value
* `proxy_username` with your **Username** value
* `proxy_password` with your **Password** value
* `proxy_count` with the number of rows you see (**free-tier plan should have 10 free proxies**)

### SMTP

`smtp` object is used by **WebAPI** project inside the user creation process.
This field is not necessary for the moment, fill it only if you have an easy access to a SMTP server.

**If you did not fill it, please do the following thing in order to have a working source code**:

1. Open your `UserController` file and comment from line `248` to line `264`
2. Everytime you create a new user, you will need to confirm his email manually going at: `https://api.aniapi.com/v1/auth/%USER_ID%`

### MyAnimeList

`mal` object is used by **SyncService** project while syncing the `UserStory` of users.
**If you do not plan to use this project, you can skip this step**.

In this step you will setup a *MyAnimeList* API client.

### Create a MAL API client

Once you login with your account, go to [API page](https://myanimelist.net/apiconfig) and click on **Create ID**.

![Create ID location](/static/mal_createid_image.png)

Inside the next page, fill all necessary fields and submit the form.
**Leave `App Type` as `web`**

### Get your client configuration

Now go to your client detail page and fill these fields:

![Detail location](/static/mal_detail_image.png)

* `client_id` with your **Client ID** value
* `client_secret` with your **Client Secret** value

### Others

* Fill `resources_version` with `1.0`
* Fill `api_endpoint` with your **WebAPI** project's localhost url

## Websites

Finally, create a new collection named `website` and add these documents:

```json
{
  "_id": 1,
  "name": "dreamsub",
  "active": true,
  "official": true,
  "site_url": "https://dreamsub.cc",
  "can_block_requests": true,
  "localization": "it",
  "creation_date": {
    "$date": "2021-10-07T00:00:00.000Z"
  }
}
```
```json
{
  "_id": 2,
  "name": "animeworld",
  "active": true,
  "official": false,
  "site_url": "https://www.animeworld.tv/",
  "can_block_requests": true,
  "localization": "it",
  "creation_date": {
    "$date": "2021-10-07T00:00:00.000Z"
  }
}
```
```json
{
  "_id": 2,
  "name": "gogoanime",
  "active": true,
  "official": true,
  "site_url": "https://gogoanime.pe/",
  "can_block_requests": true,
  "localization": "en",
  "creation_date": {
    "$date": "2021-10-07T00:00:00.000Z"
  }
}
```

**Now you are ready to use AniAPI on your environment ;)**