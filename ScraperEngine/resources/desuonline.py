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

from cda_downloader import CDA

class VideoQuality(Enum):
    p480 = 480
    p720 = 720
    p1080 = 1080

class DesuonlineResource(ScraperResource):

    def __init__(self, app: falcon.App) -> None:
        super().__init__(app, "desuonline")
    
    async def get_mp4_link(self, cda_link, quality, https) -> String:
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
        
        cda_link = cda_link + f"?wersja={quality.value}p"
        
        return CDA(use_api=False).get_video_urls(urls=cda_link, only_urls=True, quality=quality.value)[0]

    async def get_possible_matchings(self, res: falcon.Response, title: str) -> List[Matching]:
        matchings = []

        url = f"{self.base_url}?s={uri.encode(title)}"

        try:
            has_ended = False
            page_number = 4

            while not has_ended:
                page = await execute_proxied_request(self, url)

                try:
                    show_elements = page.find("div", class_="listupd").find_all("article", class_="bs")

                    if len(show_elements) == 0:
                        raise Exception

                    for show_element in show_elements:
                        path = str(show_element.find_next("a")["href"]).replace(f"{self.base_url}/anime", "")[:-1]

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

        url = f"{self.base_url}{path}-odcinek-{number}"
        print(url)

        try:
            # This here works, but theres a faster method
            # But in case there are any bugs with current approach u you can use this

            # url = f"{self.base_url}{path}"
            # page = await execute_proxied_request(self, url)
            # epList = page.find("div", class_="eplister").find_next("ul").find_all("li")
            # episodeList = []

            # for i in reversed(range(len(epList))):
            #     episodeList.append(epList[i])

            # episodeLink = str(episodeList[number - 1].find("a")["href"])
            # episodePage = await execute_proxied_request(self, episodeLink)

            episodePage = await execute_proxied_request(self, url)

            sourcesList = episodePage.find("select", class_="mirror").find_all("option")

            for option in sourcesList:
                decodedString = base64.b64decode(str(option["value"])).decode('ascii')

                if "https://drive.google.com/" in decodedString:
                    bs = BeautifulSoup(decodedString, 'html.parser')
                    embedLink = bs.find('iframe')["src"]

                    videoID = embedLink.split('/')[-2]
                    
                    dlLink = f"https://drive.google.com/u/0/uc?id={videoID}&export=download&confirm=t"
                    episodes.append(Episode(f"Odcinek {number}", url, dlLink, None, "mp4"))

                if "https://ebd.cda.pl/" in decodedString:
                    bs = BeautifulSoup(decodedString, 'html.parser')
                    embedLink = bs.find('iframe')["src"]
                    
                    if embedLink == '':
                        raise Exception("Failed to get CDA Embed Link!")
                    
                    videoID = embedLink.split('/')[-1]
                    cdaVidLink = f"https://cda.pl/video/{videoID}"

                    if cdaVidLink == '':
                        raise Exception("Failed to get CDA Link!")

                    for quality in VideoQuality:
                        dlLink = await self.get_mp4_link(cdaVidLink, quality, False)
                        if dlLink != None:
                            episodes.append(Episode(f"Odcinek {number}", url, dlLink, quality.value, "mp4"))

        except Exception as e:
            print(str(e))
            raise

        return episodes
