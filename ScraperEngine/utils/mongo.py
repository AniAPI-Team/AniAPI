from glob import glob
from pydoc import cli
from sqlite3 import connect
import os

from pymongo import MongoClient

client = None

def get_database():
    global client

    if client == None:
        client = MongoClient(os.getenv("MONGODB_URI"))

    return client[os.getenv("MONGODB_NAME")]