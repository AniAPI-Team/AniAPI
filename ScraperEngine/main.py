import falcon
import falcon.asgi
from resources.animeworld import AnimeworldResource

from resources.dreamsub import DreamsubResource
from resources.gogoanime import GogoanimeResource

from resources.aniplaylist import AniplaylistResource

from resources.desuonline import DesuonlineResource

app = falcon.asgi.App()

DreamsubResource(app)
AnimeworldResource(app)
GogoanimeResource(app)
DesuonlineResource(app)

AniplaylistResource(app)