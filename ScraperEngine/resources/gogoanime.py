import falcon
import aiohttp
import re

from falcon import uri
from typing import List
from bs4 import BeautifulSoup
from interfaces.resource import ScraperResource
from models.episode import Episode
from models.matching import Matching
from utils.session import execute_proxied_request

class GogoanimeResource(ScraperResource):

    def __init__(self, app: falcon.App) -> None:
        super().__init__(app, "gogoanime")

    async def get_possible_matchings(self, res: falcon.Response, title: str) -> List[Matching]:
        matchings = []

        url = f"{self.base_url}/search.html?keyword={uri.encode(title)}"

        try:
            has_ended = False
            page_number = 2

            while(has_ended == False):
                page = await execute_proxied_request(self.name, url)

                try:
                    show_elements = page.find("ul", class_="items").find_all("li", class_="video-block")

                    if len(show_elements) == 0:
                        raise Exception
                
                    for show_element in show_elements:
                        title_element = show_element.find(class_="img").find(class_="picture").find("img")
                        path_element = show_element.find("a")

                        path_parts = path_element["href"].split("-")

                        matchings.append(Matching(title_element["alt"], "-".join(path_parts[:-1])))

                    url = f"{self.base_url}/search.html?keyword={uri.encode(title)}&page={str(page_number)}"
                    page_number = page_number + 1
                except:
                    has_ended = True
                
        except Exception as e:
            print(str(e))
            raise falcon.HTTPInternalServerError()
        finally:
            return matchings

    async def get_episode(self, res: falcon.Response, path: str, number: int) -> List[Episode]:
        episodes = []

        url = f"{self.base_url}{path}-{str(number)}"

        try:
            page = await execute_proxied_request(self.name, url)

            title_element = page.find(class_="video-info-left").find("h1")

            frame_element = page.find(class_="play-video").find("iframe")
            match = re.search("id=(.*?)&t", frame_element["src"])
            
            if match:
                url = f"https://gogoplay.link/api.php?id={match.group(1)}"

                page = await execute_proxied_request(self.name, url)

                match = re.search("m3u8.:.(.*?).}", page.text)
                show_url = match.group(1).replace("\\", "")

                if show_url:
                    episodes.append(Episode(title_element.text.strip(), show_url, 720, "m3u8"))
                    
        except Exception as e:
            print(str(e))
            raise falcon.HTTPInternalServerError()
        finally:
            return episodes