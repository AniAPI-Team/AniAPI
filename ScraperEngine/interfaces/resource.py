import falcon
import time

from typing import List
from models.episode import Episode
from models.matching import Matching
from utils.session import get_proxied_response_header
from utils.mongo import get_database

class ScraperResource:

    db_data = None

    def __init__(self, app: falcon.App, name: str) -> None:
        self.name = name
        self.size = 0
        app.add_route(f"/{name}/matchings", self)
        app.add_route(f"/{name}/episode", self)

    async def check_base_url(self):
        if self.db_data == None:
            self.db_data = get_database()["website"].find_one({"name":self.name})

        self.base_url = self.db_data["site_url"]

        response = await get_proxied_response_header(self.name, self.base_url)

        try:
            url = str(response.url)

            if url[-1] == "/":
                url = url[0:-1]

            if self.base_url != url:
                get_database()["website"].update_one(
                    {"name":self.name},
                    {"$set":{"site_url":url}})

                self.base_url = response.url

                print(f"{self.name} URL CHANGED!")
        except Exception as e:
            print(str(e))

    async def get_possible_matchings(self, res: falcon.Response, title: str) -> List[Matching]:
        pass

    async def get_episode(self, res: falcon.Response, path: str, number: int) -> List[Episode]:
        pass

    async def on_get(self, req : falcon.Request, res : falcon.Response):
        self.size = 0
        request_start = time.time()

        await self.check_base_url()

        if(not self.db_data["active"]):
            res.text = "Website not activated"
            res.status = falcon.HTTP_503
            return

        try:
            if req.path == f"/{self.name}/matchings":
                title = req.params.get("title")

                if not title:
                    res.text = "Please provide an anime title"
                    res.status = falcon.HTTP_400
                    return

                matchings = await self.get_possible_matchings(res, title)

                response_time = (time.time() - request_start) * 1000
                print(f"{req.path}?{req.query_string} completed in {response_time:.0f}ms")

                res.media = {
                    "size": self.size,
                    "data": [m.dump() for m in matchings]
                }
                res.status = falcon.HTTP_200
                return
                
            elif req.path == f"/{self.name}/episode":
                path = req.params.get("path")
                number = req.params.get("number")

                if not path:
                    res.text = "Please provide a valid path to scrape"
                    res.status = falcon.HTTP_400
                    return

                if not str(number).isnumeric():
                    res.text = "Please provide a valid episode number"
                    res.status = falcon.HTTP_400
                    return

                episodes = await self.get_episode(res, path, int(number))

                response_time = (time.time() - request_start) * 1000
                print(f"{req.path}?{req.query_string} completed in {response_time:.0f}ms")

                res.media = {
                    "size": self.size,
                    "data": [e.dump() for e in episodes]
                }
                res.status = falcon.HTTP_200
                return
        except Exception as e:
            res.status = falcon.HTTP_500
            res.text = str(e)
            return

        res.status = falcon.HTTP_404