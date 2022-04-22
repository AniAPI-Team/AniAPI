import base64
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


class DesuonlineResource(ScraperResource):

    def __init__(self, app: falcon.App) -> None:
        print("DesuOnline initialized!")
        super().__init__(app, "desuonline")

    async def get_possible_matchings(self, res: falcon.Response, title: str) -> List[Matching]:
        matchings = []

        url = f"{self.base_url}/?s={uri.encode(title)}"
        print(url)

        try:
            has_ended = False
            page_number = 4

            while not has_ended:
                page = await execute_proxied_request(self, url)

                try:
                    show_elements = page.find(class_="bixbox").find("div", class_="listupd").find_all("article")

                    if len(show_elements) == 0:
                        raise Exception

                    for show_element in show_elements:
                        element = show_element.find(class_="bsx").find("a")
                        path = str(element["href"]).replace(self.base_url, "")

                        matchings.append(Matching(element["oldtitle"], path))

                    url = f"{self.base_url}/page/{page_number}/?s={uri.encode(title)}"
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
        print(url)

        try:
            page = await execute_proxied_request(self, url)
            epList = page.find("div", class_="eplister").find("ul").find_all("li").reverse()

            episodeLink = str(epList[number].find("a")["href"])

            episodePage = await execute_proxied_request(self, episodeLink)

            sourcesList = episodePage.find("select", class_="mirror").find_all("option")

            cdaEmbedLink = ''

            for option in sourcesList:
                decodedString = base64.b64decode(str(option["value"]))
                if "https://ebd.cda.pl/" in decodedString:
                    for x in range(13, decodedString.__len__()):
                        if decodedString[x] == '"':
                            break
                        else:
                            cdaEmbedLink += decodedString[x]

            if cdaEmbedLink == '':
                raise Exception("Failed to get CDA Link!")
            
            embedPage = await execute_proxied_request(self, cdaEmbedLink)
            cdaVidLink = str(embedPage.find("h1", class_="title").find("a")["href"])

            # TODO: GET VIDEO LINK DATA FROM CDA LINK


        except Exception as e:
            print(str(e))
            raise

        return episodes
