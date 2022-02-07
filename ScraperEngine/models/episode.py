class Episode:

    def __init__(self, title: str, path: str, url: str, quality: int, format: str) -> None:
        self.title = title
        self.path = path
        self.url = url
        self.quality = quality
        self.format = format
        pass

    def dump(self):
        return {
            "title": self.title,
            "path": self.path,
            "url": self.url,
            "quality": self.quality,
            "format": self.format
        }