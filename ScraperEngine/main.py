import falcon
import falcon.asgi
from resources.animegg import AnimeggResource
from resources.animeworld import AnimeworldResource

from resources.dreamsub import DreamsubResource
from resources.gogoanime import GogoanimeResource

from resources.aniplaylist import AniplaylistResource

from resources.desuonline import DesuonlineResource

app = falcon.asgi.App()

DreamsubResource(app)
AnimeworldResource(app)
<<<<<<< HEAD
AnimeggResource(app)
GogoanimeResource(app)
AniplaylistResource(app)
DesuonlineResource(app)
=======
GogoanimeResource(app)
DesuonlineResource(app)

AniplaylistResource(app)
>>>>>>> d7199668263eb2b75d9a3687805f2f1593ab84ff
