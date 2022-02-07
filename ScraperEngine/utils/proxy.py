from utils.mongo import get_database

proxies_capacitor = {}
proxy_parameters = None

def get_lowest_load_proxy(name):
    global proxies_capacitor
    global proxy_parameters

    if proxy_parameters == None:
        settings = get_database()["app_settings"].find_one({"_id":0})
        proxy_parameters = {
            "host": f"http://{str(settings['proxy_host'])}:{str(settings['proxy_port'])}",
            "user": settings["proxy_username"],
            "password": settings["proxy_password"]
        }

    if not name in proxies_capacitor:
        proxies_capacitor[name] = 0

    proxies_capacitor[name] += 1

    if proxies_capacitor[name] > 100:
        proxies_capacitor[name] = 1

    return {
        "user": f"{str(proxy_parameters['user'])}{str(proxies_capacitor[name])}",
        "password": proxy_parameters["password"],
        "host": proxy_parameters["host"]
    }