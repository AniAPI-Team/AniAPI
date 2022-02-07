import falcon
from models.song import Song

from utils.session import get_proxied_response_json_post

class AniplaylistResource:

    def __init__(self, app: falcon.App) -> None:
        app.add_route("/song/{title}", self)

    async def on_get(self, req : falcon.Request, res : falcon.Response, title: str):
        if not title:
            res.text = "Please provide an anime title"
            res.status = falcon.HTTP_400

        songs = []

        url = "https://p4b7ht5p18-dsn.algolia.net/1/indexes/*/queries?x-algolia-agent=Algolia%20for%20JavaScript%20(3.35.1)%3B%20Browser%20(lite)&x-algolia-application-id=P4B7HT5P18&x-algolia-api-key=cd90c9c918df8b42327310ade1f599bd"
        headers = {
            "Referer": "https://aniplaylist.com/"
        }
        payload = {
            "requests": [
                {
                    "indexName": "songs_prod",
                    "params": f"query={title}&maxValuesPerFacet=100&hitsPerPage=100&highlightPreTag=__ais-highlight__&highlightPostTag=__%2Fais-highlight__&page=0&tagFilters=&facetFilters=%5B%5B%22song_type%3AEnding%22%2C%22song_type%3AOpening%22%5D%5D"
                }
            ]
        }

        try:
            json = await get_proxied_response_json_post("aniplaylist", url, headers, payload)
            json_songs = json["results"][0]["hits"]

            for song in json_songs:
                has_match = False

                for t in song["anime_titles"]:
                    if title.lower() == t.lower():
                        has_match = True

                if has_match == False:
                    continue

                id = str(song["spotify_url"]).split("/")[-1]
                type = song["song_type"]

                songs.append(Song(id, type))
        except Exception as e:
            print(str(e))
        finally:
            res.media = [s.dump() for s in songs]
            res.status = falcon.HTTP_200