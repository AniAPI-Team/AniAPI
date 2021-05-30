window.App = (() => {
    return {
        ChangeURL: (url) => {
            history.pushState(null, '', url);
            console.log('Pushed new url to history', url);
        }
    };
})();