# Adding a new website

In case you want to add your favourite website to AniAPI's core, you can find here some useful infos.

## IWebsiteScraper

Create a class with the following naming convention:

`{WebsiteName}Scraper`

Let's say for example `GogoanimeScraper`.

Now, declare it `public` and let it inherits `IWebsiteScraper`, like below:

```csharp
public class GogoanimeScraper : IWebsiteScraper {

  public GogoanimeScraper(WebsiteScraperService service) : base(service)
  {
  }

  protected override long WebsiteID => 3;

  protected override Type WebsiteType => typeof(GogoanimeScraper);
  
  protected override async Task<AnimeMatching> GetMatching(Page webPage, string animeTitle)
  {
  
  }
  
  protected override async Task<EpisodeMatching> GetEpisode(Page webPage, AnimeMatching matching, int number)
  {
  
  }

}
```

### WebsiteID

This is the internal reference used by **MongoDB** to fetch the website's informations.
We will discuss this later.
For now, give it a progressive numeric value (check already implemented websites to know which value you have to give).

### WebsiteType

Just give it your website's class type.

### GetMatching

This is the method called by the engine everytime an **Anime** is analyzed across the website.
It should scrape the website's anime index page, in order to find a matching title and start analyzing it.
It returns an `AnimeMatching` object:

* `Title`, the website's anime title
* `Description`, the website's anime description
* `Path`, the website's anime relative path

### GetEpisode

This method is used to fetch all the episodes when a full (`100%`) match has been found on an **Anime**.
It is called foreach episode, so if the anime has 25 episodes, it will be called 25 times.
It should scrape the website's anime detail page, in order to find all the episodes and save them.
It returns an `EpisodeMatching` object:

* `Title`, the website's anime's episode title
* `Number`, the website's anime's episode progressive number
* `Path`, the website's anime's episode relative path
* `Source`, the website's anime's episode video url

If you need to understand better how thing works, you can watch the websites already implemented, like [this one](https://github.com/AniAPI-Team/AniAPI/blob/main/SyncService/Models/WebsiteScrapers/DreamsubScraper.cs).

## Pull request

When your implementation is done, you need to make a pull request to let us review it and accept it (if all is good 🧐).

**Inside the PR comment, please give us the following informations:**

```
site_url: (this is the base url of the website, for example "https://gogoanime.pe/" for gogoanime)
can_block_requests: (this means that the website can block useless requests like css/adblockers/ads/etc without going in error)
localization: (the i18n code representing the website locale)
```

Nice, you have implemented your favourite website 😄

Thank you 😙
