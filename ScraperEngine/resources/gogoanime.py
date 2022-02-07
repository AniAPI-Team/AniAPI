import falcon
import aiohttp
import re

from falcon import uri
from typing import List
from bs4 import BeautifulSoup
from interfaces.resource import ScraperResource
from models.episode import Episode
from models.matching import Matching
from utils.session import execute_proxied_request, get_proxied_response_json_get

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
                page = await execute_proxied_request(self, url)

                try:
                    show_elements = page.find(class_="last_episodes").find("ul", class_="items").find_all("li")

                    if len(show_elements) == 0:
                        raise Exception
                
                    for show_element in show_elements:
                        element = show_element.find(class_="name").find("a")
                        path = str(element["href"]).replace(self.base_url, "")

                        matchings.append(Matching(element["title"], path))

                    url = f"{self.base_url}/search.html?keyword={uri.encode(title)}&page={str(page_number)}"
                    page_number = page_number + 1
                except:
                    has_ended = True
                
        except Exception as e:
            print(str(e))
            raise

        return matchings

    async def get_episode(self, res: falcon.Response, path: str, number: int) -> List[Episode]:
        episodes = []

        url = f"{self.base_url}{path}"

        try:
            page = await execute_proxied_request(self, url)

            movie_id = page.find(id="movie_id").get("value")
            slug = path.split("/")[-1]

            url = f"{self.base_url}/ajax/load_list_episode?ep_start={number}&ep_end={number}&id={movie_id}&slug={slug}"
            page = await execute_proxied_request(self, url)

            url = page.find("a")["href"]
            page = await execute_proxied_request(self, url)

            embed_url = str(page.find_all("iframe")[0]["src"])

            page = await execute_proxied_request(self, embed_url, {
                "Referer": url
            })

            video_id = embed_url.split("/")[-1].split("?")[0]
            s_key = re.search("window.skey = '(.*?)';", str(page))
            host = self.base_url.replace("https://", "")

            if s_key:
                s_key = s_key.group(1)

                vidstream_url = f"https://vidstream.pro/info/{video_id}?domain={host}&skey={s_key}"
                json = await get_proxied_response_json_get(self, vidstream_url, {
                    "Referer": embed_url
                })

                video_url = str(json["media"]["sources"][0]["file"]).replace("#.mp4", "")

                episodes.append(Episode(f"Episode {number}", vidstream_url, video_url, None, "m3u8"))
                    
        except Exception as e:
            print(str(e))
            raise
        
        return episodes