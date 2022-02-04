class Episode:

    def __init__(self, title: str, url: str, quality: int, format: str) -> None:
        self.title = title
        self.url = url
        self.quality = quality
        self.format = format
        pass

    def dump(self):
        return {
            "title": self.title,
            "url": self.url,
            "quality": self.quality,
            "format": self.format
        }