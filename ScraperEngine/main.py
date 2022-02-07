import falcon
import falcon.asgi
from resources.animeworld import AnimeworldResource

from resources.dreamsub import DreamsubResource
from resources.gogoanime import GogoanimeResource

from resources.aniplaylist import AniplaylistResource

app = falcon.asgi.App()

DreamsubResource(app)
AnimeworldResource(app)

GogoanimeResource(app)

AniplaylistResource(app)