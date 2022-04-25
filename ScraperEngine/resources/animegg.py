import falcon
import aiohttp
from utils.session import execute_proxied_request

from falcon import uri
from typing import List
from bs4 import BeautifulSoup
from interfaces.resource import ScraperResource
from models.episode import Episode
from models.matching import Matching

class AnimeggResource(ScraperResource):

    def __init__(self, app: falcon.App) -> None:
		# On this line, use the name you used inside MongoDB's websites collection
        super().__init__(app, "animegg")

    async def get_possible_matchings(self, res: falcon.Response, title: str) -> List[Matching]:
        matchings = []
        url = f"{self.base_url}/search/?q={uri.encode(title)}"
        try:
            page = await execute_proxied_request(self, url)
            # Search results class is "mse"
            results = page.find_all(class_="mse")
            for result in results:
                url = result.get("href")
                title = result.select_one(".searchre > .media-body > .first > h2").text
                matchings.append(Matching(title, url))
        except Exception as e:
            print(str(e))
            raise
        
        return matchings

    async def get_episode(self, res: falcon.Response, path: str, number: int) -> List[Episode]:
        episodes = []
        series_name = path.split("/")[-1]

        # Thats just some weird shit anime.gg does
        if number == 1:
            number = 0 
        
        url = f"{self.base_url}/{series_name}-episode-{number}"
        print(url)
        try:
            page = await execute_proxied_request(self, url)
            title = page.select_one(".e4tit").text
            print(title)
            iframe = page.find("iframe", class_="video")
            print(iframe)
            print(f"{self.base_url}{iframe.get('src')}")
            embedPage = await execute_proxied_request(self, f"{self.base_url}{iframe.get('src')}")
            print(embedPage)
            video = embedPage.find_next("video")
            print(video)
            dlUrl = video["src"]
            print(dlUrl)
            episodes.append(Episode(title, url, dlUrl, format="mp4"))
             
        except Exception as e:
            print(str(e))
            raise
        
        return episodes