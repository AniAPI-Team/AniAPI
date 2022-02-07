# Adding a new website

In case you want to add your favourite website to AniAPI's core, you can find here some useful infos.

[Be sure to setup your environment correctly](https://github.com/AniAPI-Team/AniAPI/tree/main/GETTING_STARTED.md)
[How to add a website to our scraper engine](https://github.com/AniAPI-Team/AniAPI/blob/main/ScraperEngine)

## IWebsiteScraper

Create a class with the following naming convention:

`{WebsiteName}Website`

Let's say for example `GogoanimeWebsite`.

Now, declare it `public` and let it inherits `IWebsite`, like below:

```csharp
public class GogoanimeWebsite : IWebsite {

  public GogoanimeWebsite(Website website) : base(website)
  {
  }

  public override bool AnalyzeMatching(Anime anime, AnimeMatching matching, string sourceTitle)
  {
    // Insert here your custom logic
    // in order to customize anime matchings

    // Be sure to call super method!
    return base.AnalyzeMatching(anime, matching, sourceTitle);
  }

  public override string BuildAPIProxyURL(AppSettings settings, AnimeMatching matching, string url, Dictionary<string, string> values = null)
  {
    // Insert here your custom logic
    // in order to customize the values
    // used while building the proxy url

    // Be sure to call super method!
    return base.BuildAPIProxyURL(settings, matching, url, values);
  }
}
```

### WebsiteID

This is the internal reference used by **MongoDB** to fetch the website's informations, must be unique.
Give it a progressive numeric value (check already implemented websites to know which value you have to give).

### WebsiteType

Just give it your website's class type.

### AnalyzeMatching

This is the method called by the engine everytime an **Anime** is analyzed across the website.

### BuildAPIProxyURL

This method is used to build an internal url, in order to use AniAPI's proxy.

If you need to understand better how thing works, you can watch the websites already implemented, like [this one](https://github.com/AniAPI-Team/AniAPI/blob/main/SyncService/Models/Websites/GogoanimeWebsite.cs).

## Pull request

When your implementation is done, you need to make a pull request to let us review it and accept it (if all is good 🧐).

**Inside the PR comment, please give us the following informations:**

```
site_url: (this is the base url of the website, for example "https://gogoanime.lol" for gogoanime)
localization: (the i18n code representing the website locale)
```

Nice, you have implemented your favourite website 😄

Thank you 😙
