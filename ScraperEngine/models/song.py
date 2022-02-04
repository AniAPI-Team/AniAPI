class Song:

    def __init__(self, id: str, type: str) -> None:
        self.id = id
        self.type = type

    def dump(self):
        return {
            "id": self.id,
            "type": self.type
        }