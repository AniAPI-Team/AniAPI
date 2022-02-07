class Matching:

    def __init__(self, title: str, path: str) -> None:
        self.title = title
        self.path = path

    def dump(self):
        return {
            "title": self.title,
            "path": self.path
        }