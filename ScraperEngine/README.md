# Adding a new website to scraper engine

## ScraperResource

As first thing, please copy the code below and paste it inside a `your_website_name.py` file under `resources` folder.
```python
import falcon
import aiohttp
from utils.session import execute_proxied_request

from falcon import uri
from typing import List
from bs4 import BeautifulSoup
from interfaces.resource import ScraperResource
from models.episode import Episode
from models.matching import Matching

class YourWebsiteNameHere(ScraperResource):

    def __init__(self, app: falcon.App) -> None:
		# On this line, use the name you used inside MongoDB's websites collection
        super().__init__(app, "yourwebsitenamehere")

    async def get_possible_matchings(self, res: falcon.Response, title: str) -> List[Matching]:
        matchings = []

        try:
			# Your website logic here
			
        except Exception as e:
            print(str(e))
            raise
        
        return matchings

    async def get_episode(self, res: falcon.Response, path: str, number: int) -> List[Episode]:
        episodes = []

        try:
			# Your website logic here
            
        except Exception as e:
            print(str(e))
            raise
        
        return episodes
```
After, edit `main.py` file and add (at the end of the file) this line of code:
```python
YourWebsiteNameHere(app)
```
Alright! Now you can write your custom logic to add your favourite website to AniAPI.

If you need more help going throught this, you can refer to [this example](https://github.com/AniAPI-Team/AniAPI/blob/main/ScraperEngine/resources/gogoanime.py).