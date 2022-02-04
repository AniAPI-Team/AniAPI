import aiohttp
import ujson

from bs4 import BeautifulSoup
from utils.proxy import get_lowest_load_proxy

session = aiohttp.ClientSession(json_serialize=ujson.dumps)

async def execute_proxied_request(name, url) -> BeautifulSoup:
    global session
    
    try:
        session_proxy = get_lowest_load_proxy(name)
        auth = aiohttp.BasicAuth(session_proxy["user"], session_proxy["password"])

        async with session.get(url,
                               proxy=session_proxy["host"],
                               proxy_auth=auth) as page:
            return BeautifulSoup(await page.text(), "html.parser")

    except Exception as e:
        print(str(e))
        raise

async def get_proxied_response_json(name, url, headers, body):
    global session

    try:
        session_proxy = get_lowest_load_proxy(name)
        auth = aiohttp.BasicAuth(session_proxy["user"], session_proxy["password"])

        async with session.post(url,
                                proxy=session_proxy["host"],
                                proxy_auth=auth,
                                json=body,
                                headers=headers) as response:
            return await response.json()
    except Exception as e:
        print(str(e))
        raise

async def get_proxied_response_header(name, url) -> int:
    global session

    try:
        session_proxy = get_lowest_load_proxy(name)
        auth = aiohttp.BasicAuth(session_proxy["user"], session_proxy["password"])

        async with session.get(url,
                                proxy=session_proxy["host"],
                                proxy_auth=auth) as response:
            return response
    except Exception as e:
        print(str(e))
        return 500