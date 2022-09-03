from operator import is_
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
        series_name = uri.encode(path.split("/")[-1].replace(" ", "-").replace(",", ""))
        video_url = f"{self.base_url}/{series_name}-episode-{number}"
        try:
            page = await execute_proxied_request(self, video_url)
            links = page.find("ul", id="videos").find_next("li").find_all("a")
            for video in links:
                embed_id = video["data-id"]

                is_dub = video["data-version"] == "dubbed"
                vid_type = 'subbed'
                if is_dub:
                    vid_type = "dubbed"
    
                quality_text = video.select_one("span.btn-hd").text
                quality = 1080 if quality_text == "HD" else 480
                embed_url = f"{self.base_url}/embed/{embed_id}"
                page = await execute_proxied_request(self, embed_url)
                video_path = page.select_one("meta[property='og:video']")["content"]
                video_url = f"{self.base_url}{video_path}"
                episodes.append(Episode(f"Episode {number}", f"{embed_url}#{vid_type}", video_url, quality, "mp4"))
            
        except Exception as e:
            print(str(e))
            raise
        
        return episodes