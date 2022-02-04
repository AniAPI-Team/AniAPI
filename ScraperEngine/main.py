import falcon
import falcon.asgi

from resources.dreamsub import DreamsubResource
from resources.gogoanime import GogoanimeResource

from resources.aniplaylist import AniplaylistResource

app = falcon.asgi.App()

DreamsubResource(app)
GogoanimeResource(app)

AniplaylistResource(app)