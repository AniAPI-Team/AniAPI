import falcon
import falcon.asgi
from resources.animeworld import AnimeworldResource

from resources.dreamsub import DreamsubResource
from resources.gogoanime import GogoanimeResource
from resources.animepisode import AnimepisodeResource

from resources.aniplaylist import AniplaylistResource

from resources.desuonline import DesuonlineResource

app = falcon.asgi.App()

DreamsubResource(app)
AnimeworldResource(app)
AnimepisodeResource(app)

GogoanimeResource(app)

AniplaylistResource(app)

DesuonlineResource(app)