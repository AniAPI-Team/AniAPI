import falcon
import aiohttp
from utils.session import execute_proxied_request

from falcon import uri
from typing import List
from bs4 import BeautifulSoup
from interfaces.resource import ScraperResource
from models.episode import Episode
from models.matching import Matching

class DreamsubResource(ScraperResource):

    def __init__(self, app: falcon.App) -> None:
        super().__init__(app, "dreamsub")

    async def get_possible_matchings(self, res: falcon.Response, title: str) -> List[Matching]:
        matchings = []

        url = f"{self.base_url}/search/?q={uri.encode(title)}"

        try:
            page = await execute_proxied_request(self, url)

            show_elements = page.find(id="main-content").find_all(class_="tvBlock")
            
            for show_element in show_elements:
                title_element = show_element.find(class_="tvTitle").find(class_="title")
                path_element = show_element.find(class_="showStreaming").find("a")

                matchings.append(Matching(title_element.text.strip(), path_element["href"]))
        except Exception as e:
            print(str(e))
            raise
        
        return matchings

    async def get_episode(self, res: falcon.Response, path: str, number: int) -> List[Episode]:
        episodes = []

        url = f"{self.base_url}{path}/{str(number)}"

        try:
            page = await execute_proxied_request(self, url)

            title_element = page.find(id="main-content",class_="video").find(id="current_episode_name")
            source_elements = page.find(id="main-content",class_="onlyDesktop").find(class_="goblock-content").find_all(class_="dwButton")

            for source_element in source_elements:
                q = int(source_element.text[0:-1])
                url = source_element["href"]

                if not any(x.quality == q for x in episodes) and url:
                    episodes.append(Episode(title_element.text.strip(), url, url, q, "mp4"))

        except Exception as e:
            print(str(e))
            raise
        
        return episodes