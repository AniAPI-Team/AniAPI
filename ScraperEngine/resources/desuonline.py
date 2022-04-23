import base64
from enum import Enum
import http
from os import link
from tkinter import N
from tokenize import String
from urllib import request
import falcon
import aiohttp
import re
import requests

from falcon import uri
from typing import List
from bs4 import BeautifulSoup
from interfaces.resource import ScraperResource
from models.episode import Episode
from models.matching import Matching
from utils.session import execute_proxied_request, get_proxied_response_json_get

remove_keys = { "_XDDD", "_CDA", "_ADC", "_CXD", "_QWE", "_Q5", "_IKSDE" }

regex_file = re.compile('"""file"":""(.*?)(?:"")""')

class VideoQuality(Enum):
    auto = 0
    p360 = 360
    p480 = 480
    p720 = 720
    p1080 = 1080

class DesuonlineResource(ScraperResource):

    def __init__(self, app: falcon.App) -> None:
        print("DesuOnline initialized!")
        super().__init__(app, "desuonline")
    
    async def get_mp4_link(self, cda_link, quality: VideoQuality.auto, https) -> String:
        if cda_link.endswith("/vfilm"):
            cda_link = cda_link[:len(cda_link)-5]
        
        if cda_link.endswith("/"):
            cda_link = cda_link[:len(cda_link)-1]

        if cda_link.startswith("http://"):
            cutLink = ""
            
            for x in range(len(cda_link)):
                if x >= 7:
                    continue
                else:
                    cutLink += cda_link[x]
            
            cda_link = "https://www." + cutLink
        
        if quality != VideoQuality.auto:
            cda_link = cda_link + f"?wersja={quality.value}p"

        print("Getting page: " + cda_link)
        
        headers = {
            'Referer': 'https://www.cda.pl',
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:74.0) Gecko/20100101 Firefox/74.0',
            'Accept-Encoding': 'identity',
        }
        
        cdaPage = requests.get(cda_link, headers=headers).text

        with open("Output.txt", "w") as text_file:
            text_file.write(cdaPage)

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
        else:
            raise Exception("No regex matches")

    async def get_possible_matchings(self, res: falcon.Response, title: str) -> List[Matching]:
        matchings = []

        url = f"{self.base_url}?s={uri.encode(title)}"
        print(url)

        try:
            has_ended = False
            page_number = 4

            while not has_ended:
                page = await execute_proxied_request(self, url)

                try:
                    show_elements = page.find("div", class_="listupd").find_all("article", class_="bs")
                    print(len(show_elements))

                    if len(show_elements) == 0:
                        raise Exception

                    for show_element in show_elements:
                        path = str(show_element.find_next("a")["href"]).replace(self.base_url, "")
                        
                        print("Adding to matchings")
                        match = show_element.find("div", class_="tt").find_next("h2").string
                        matchings.append(Matching(match, path))

                    if len(show_elements == 10):
                        url = f"{self.base_url}page/{page_number}/?s={uri.encode(title)}"
                        page_number = page_number + 1
                    else:
                        has_ended = True
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
            epList = page.find("div", class_="eplister").find_next("ul").find_all("li")

            episodeLink = str(epList[number].find("a")["href"])
            print(episodeLink)

            episodePage = await execute_proxied_request(self, episodeLink)

            sourcesList = episodePage.find("select", class_="mirror").find_all("option")

            cdaVidLink = ''

            for option in sourcesList:
                decodedString = base64.b64decode(str(option["value"])).decode('ascii')
                cdaEmbedLink = ''
                if "https://ebd.cda.pl/" in decodedString:
                    for x in range(13, len(decodedString)):
                        if decodedString[x] == '"':
                            break
                        else:
                            cdaEmbedLink += decodedString[x]
                    
                    if cdaEmbedLink == '':
                        raise Exception("Failed to get CDA Embed Link!")
                    
                    videoID = cdaEmbedLink.split('/')[-1]
                    cdaVidLink = f"https://cda.pl/video/{videoID}"

            if cdaVidLink == '':
                raise Exception("Failed to get CDA Link!")

            print(cdaVidLink)

            for quality in VideoQuality:
                if quality == VideoQuality.auto:
                    continue
                dlLink = await self.get_mp4_link(cdaVidLink, quality, False)
                print(dlLink)
                if dlLink != None:
                    episodes.append(Episode(f"Odcinek {number}", url, dlLink, quality.value, "mp4"))

        except Exception as e:
            print(str(e))
            raise

        return episodes
