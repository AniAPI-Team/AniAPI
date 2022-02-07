import falcon
import aiohttp
from utils.session import execute_proxied_request

from falcon import uri
from typing import List
from bs4 import BeautifulSoup
from interfaces.resource import ScraperResource
from models.episode import Episode
from models.matching import Matching

class AnimeworldResource(ScraperResource):

    def __init__(self, app: falcon.App) -> None:
        super().__init__(app, "animeworld")

    async def get_possible_matchings(self, res: falcon.Response, title: str) -> List[Matching]:
        matchings = []

        url = f"{self.base_url}/search?keyword={uri.encode(title)}"

        try:
            page = await execute_proxied_request(self, url)

            show_elements = page.find(class_="film-list").find_all(class_="item")
            
            for show_element in show_elements:
                title_element = show_element.find(class_="inner").find(class_="name")
                path_element = show_element.find(class_="inner").find(class_="name")

                matchings.append(Matching(title_element.text.strip(), path_element["href"]))
        except Exception as e:
            print(str(e))
            raise
        
        return matchings

    async def get_episode(self, res: falcon.Response, path: str, number: int) -> List[Episode]:
        episodes = []

        url = f"{self.base_url}{path}"

        try:
            page = await execute_proxied_request(self, url)

            server = page.select(".server.active")[0]
            url = self.base_url + server.find_all("li", class_="episode")[number - 1].find("a")["href"]

            page = await execute_proxied_request(self, url)

            source = page.find(id="download").find(id="alternativeDownloadLink")["href"]

            episodes.append(Episode(f"Episodio {number}", url, source, None, "mp4"))

        except Exception as e:
            print(str(e))
            raise
        
        return episodes