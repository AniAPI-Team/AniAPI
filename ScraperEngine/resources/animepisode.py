import falcon
import aiohttp
from utils.session import execute_proxied_request
import re
from falcon import uri
from typing import List
from bs4 import BeautifulSoup
from interfaces.resource import ScraperResource
from models.episode import Episode
from models.matching import Matching

class AnimepisodeResource(ScraperResource):

    def __init__(self, app: falcon.App) -> None:
		# On this line, use the name you used inside MongoDB's websites collection
        super().__init__(app, "animepisode")

    def fix_title(self, title: str) -> str:
        # set the variable "is_dubbed" to true if the title contains "Dubbed" in it, otherwise set it to false, do this using regexp
        is_dubbed = title in "Dubbed"
        title = title.replace("Dubbed", "").replace("Subbed", "").replace("English", "").strip()
        title = title.replace("Episode","").strip()
        # Use Regexp to find the last number found in the title, pop it from the title and return it to the variable "episode_number"
        episode_number = re.findall(r'\d+', title)[-1]
        title = title.replace(episode_number, "").strip()

        

        return {
            "title": title,
            "is_dubbed": is_dubbed,
            "episode_number": episode_number
        }

        



        



        
        
    async def get_possible_matchings(self, res: falcon.Response, title: str) -> List[Matching]:
        matchings = []
        url = f"{self.base_url}/search?q={title}"
        try:
            page = await execute_proxied_request(self, url)
            articles = page.select_one("#main").find_all("article")
            for article in articles:
                content = article.find({ "class": "blog-entry-inner" }).find({"class": "blog-entry-content"})
                link = content.find("header").find().find("a")
                title = link.text
                url = link.get("href")
                matchings.append(Matching(title, url))
        
        except Exception as e:
            print(str(e))
            raise
        
        return matchings

    async def get_episode(self, res: falcon.Response, path: str, number: int) -> List[Episode]:
        episodes = []
        url = f"{self.base_url}{path}"
        try:
			# Btw im going on a trip, so ill be in car with shitty internet in like 20mins, so you should copy your code
            # Yeah, i can commit
            # could you commit your code to your github? and add me as a collaborator?
            page = await execute_proxied_request(self, url)
            article  = page.select_one("#main").find("article")
            

        except Exception as e:
            print(str(e))
            raise
        
        return episodes