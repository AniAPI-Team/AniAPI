import base64
from enum import Enum
from os import link
from tokenize import String
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

remove_keys = { "_XDDD", "_CDA", "_ADC", "_CXD", "_QWE", "_Q5", "_IKSDE" }

regex_link = re.compile("https:\/\/www.cda.pl\/video\/([^\/\s]+)")
regex_file = re.compile("""file"":""(.*?)(?:"")""")

class VideoQuality(Enum):
    auto = 0,
    p360 = 360,
    p480 = 480,
    p720 = 720,
    p1080 = 1080

class DesuonlineResource(ScraperResource):

    def __init__(self, app: falcon.App) -> None:
        print("DesuOnline initialized!")
        super().__init__(app, "desuonline")
    
    async def get_mp4_link(cda_link, quality: VideoQuality.auto, https: False) -> String:
        if cda_link.endswith("/vfilm"):
            cda_link = cda_link[:len(cda_link)-5]
        
        if cda_link.endswith("/"):
            cda_link = cda_link[:len(cda_link)-1]

        if cda_link.startsWith("http://"):
            cutLink = ""
            
            for x in range(len(cda_link)):
                if x >= 7:
                    continue
                else:
                    cutLink += cda_link[x]
            
            cda_link = "https://" + cutLink
        
        if not re.match(regex_link, cda_link):
            return None
        
        cdaPage = await execute_proxied_request(cda_link, {
            "Referer": "https://www.cda.pl",
            "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:74.0) Gecko/20100101 Firefox/74.0",
            "Accept-Encoding": "identity"
        })

        match = regex_file.match(cdaPage)

        if match and match.groups().count() >= 2:
            key = match.groups()[0]
            decryptedString = ""

            for vkey in remove_keys:
                key = key.replace(vkey, "")
            
            for c in key:
                if (c >= 33 and c <= 126):
                    decryptedString += (33 + ((c + 14) % 94))
                else:
                    decryptedString += c

            decryptedString = decryptedString.replace(".cda.mp4", "")
            decryptedString = decryptedString.replace(".2cda.pl", ".cda.pl")
            decryptedString = decryptedString.replace(".3cda.pl", ".cda.pl")

            if https:
                return "https://" + decryptedString + ".mp4"
            else:
                return "http://" + decryptedString + ".mp4"

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

            dlLink = await get_mp4_link(cdaVidLink, VideoQuality.p1080)

            episodes.append(Episode(f"Odcinek {number}", url, dlLink, 1080, "mp4"))

        except Exception as e:
            print(str(e))
            raise

        return episodes
