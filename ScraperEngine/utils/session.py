import zlib
import brotli
import aiohttp
import ujson

from bs4 import BeautifulSoup
from utils.proxy import get_lowest_load_proxy

session = aiohttp.ClientSession(json_serialize=ujson.dumps,auto_decompress=False)

async def execute_proxied_request(resource, url: str, headers = {}):
    global session
    
    try:
        session_proxy = get_lowest_load_proxy(resource.name)
        auth = aiohttp.BasicAuth(session_proxy["user"], session_proxy["password"])

        async with session.get(url,
                               proxy=session_proxy["host"],
                               proxy_auth=auth,
                               headers=headers) as res:
            resource.size += len(await res.read())
            res = decompress(res)

            return BeautifulSoup(await res.text(), "html.parser")

    except Exception as e:
        print(str(e))
        raise

async def get_proxied_response_json_get(resource, url: str, headers = {}):
    global session

    try:
        session_proxy = get_lowest_load_proxy(resource.name)
        auth = aiohttp.BasicAuth(session_proxy["user"], session_proxy["password"])

        async with session.get(url,
                                proxy=session_proxy["host"],
                                proxy_auth=auth,
                                headers=headers) as res:
            resource.size += len(await res.read())
            res = decompress(res)

            return await res.json()

    except Exception as e:
        print(str(e))
        raise

async def get_proxied_response_json_post(name: str, url: str, headers = {}, body = {}):
    global session

    try:
        session_proxy = get_lowest_load_proxy(name)
        auth = aiohttp.BasicAuth(session_proxy["user"], session_proxy["password"])

        async with session.post(url,
                                proxy=session_proxy["host"],
                                proxy_auth=auth,
                                json=body,
                                headers=headers) as res:
            await res.read()
            res = decompress(res)
            
            return await res.json()

    except Exception as e:
        print(str(e))
        raise

async def get_proxied_response_header(name: str, url: str) -> aiohttp.ClientResponse:
    global session

    try:
        session_proxy = get_lowest_load_proxy(name)
        auth = aiohttp.BasicAuth(session_proxy["user"], session_proxy["password"])

        async with session.head(url,
                                proxy=session_proxy["host"],
                                proxy_auth=auth) as res:
            return res

    except Exception as e:
        print(str(e))
        return 500

def decompress(res: aiohttp.ClientResponse) -> aiohttp.ClientResponse:
    encoding = res.headers["Content-Encoding"]

    if encoding == "gzip":
        res._body = zlib.decompress(res._body, 16 + zlib.MAX_WBITS)
    elif encoding == "deflate":
        res._body = zlib.decompress(res._body, -zlib.MAX_WBITS)
    elif encoding == "br":
        res._body = brotli.decompress(res._body)

    return res