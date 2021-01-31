//Definition vars
var video;
var timeElapsed;
var duration;
var progressBar;
var seek;
var seekTooltip;
var volumeButton;
var volumeIcons;
var volumeMute;
var volumeLow;
var volumeMiddle;
var volumeHigh;
var volume;
var playbackAnimation;
var videoContainer;
var pipButton;
var fullScreenButton;
var videoControls;
var playbackControls;
var fullscreenIcons;


//var nextEpisodeWrapper;
//var nextEpisodeDuration;
//var nextEpisodeTimer;
//var nextEpisodeTimeLeft;
//var nextEpisodeStarted;
//var secs;

window.VideoPlayer = {
    InitVideoVariables: function () {
        if (video)
            return;

        //Container
        videoContainer = document.querySelectorAll('.video-container')[0];

        //Video
        video = document.querySelectorAll('.video')[0];
        duration = document.getElementById('duration');

        //Controls
        videoControls = document.querySelectorAll('.video-controls')[0];
        playbackControls = document.querySelectorAll('.playback-animation')[0];
        fullscreenIcons = document.querySelectorAll('.fullscreen-button i');
        playIcons = document.querySelectorAll('.play i');

        //Progress
        timeElapsed = document.getElementById('time-elapsed');
        progressBar = document.querySelectorAll('.progress-bar')[0];
        seek = document.querySelectorAll('.seek')[0];
        seekTooltip = document.querySelectorAll('.seek-tooltip')[0];

        //Volume
        volumeButton = document.getElementById('volume-button');
        volumeIcons = document.querySelectorAll('.volume-button i');
        volumeMute = document.querySelectorAll('.fa-volume-mute')[0];
        volumeLow = document.querySelectorAll('.fa-volume-down')[0];
        volumeMiddle = document.querySelectorAll('.fa-volume')[0];
        volumeHigh = document.querySelectorAll('.fa-volume-up')[0];
        volume = document.querySelectorAll('.volume')[0];

        //Video Size
        pipButton = document.querySelectorAll('.pip-button')[0];
        fullScreenButton = document.querySelectorAll('.fullscreen-button')[0];

        // Add eventlisteners
        video.addEventListener('timeupdate', VideoPlayer.UpdateProgress);
        seek.addEventListener('mousemove', VideoPlayer.updateSeekTooltip);

        volume.addEventListener('input', VideoPlayer.UpdateVolume);
        volumeButton.addEventListener('click', VideoPlayer.ToggleMute);

        pipButton.addEventListener('click', VideoPlayer.TogglePip);

        seek.addEventListener('click', VideoPlayer.SkipAhead);

        video.addEventListener('loadedmetadata', VideoPlayer.VideoLoaded);
    },
    VideoLoaded: function () {
        const videoDuration = Math.round(video.duration);

        if (isNaN(videoDuration))
            return;

        seek.setAttribute('max', videoDuration);
        progressBar.setAttribute('max', videoDuration);

        const result = new Date(videoDuration * 1000).toISOString().substr(11, 8);
        const time = {
            minutes: result.substr(3, 2),
            seconds: result.substr(6, 2)
        };
        duration.innerText = `${time.minutes}:${time.seconds}`;
        duration.setAttribute('datetime', `${time.minutes}m ${time.seconds}s`);

        document.querySelectorAll('.play .fa-play').forEach((icon) => icon.classList.remove('hidden'));
        document.querySelectorAll('.play .fa-pause').forEach((icon) => icon.classList.add('hidden'));
    },
    ChangeVideoUrl: async function (newUrl) {
        //Render senza modifica url
        if (video.url == newUrl)
            return;

        video.url = newUrl;
        await video.load();

        VideoPlayer.VideoLoaded();
    },
    ShowVideoControls: function () {
        videoControls.classList.remove('hide');
        playbackControls.classList.remove('hide');
    },
    HideVideoControls: function () {
        videoControls.classList.add('hide');
        playbackControls.classList.add('hide');
    },
    TogglePlay: function (isPlay) {
        if (!video.paused) {
            video.pause();
            VideoPlayer.ShowVideoControls();
        }
        else {
            video.play();
            setTimeout(VideoPlayer.HideVideoControls, 1000);
        }

        playIcons.forEach((icon) => icon.classList.toggle('hidden'));
        playbackControls.animate([{ opacity: 1, transform: 'scale(1)', }, { opacity: 0, transform: 'scale(1.3)' }], { duration: 1000, });
    },
    UpdateProgress: function () {
        //Update time
        const result = new Date(Math.round(video.currentTime) * 1000).toISOString().substr(11, 8);
        const time = {
            minutes: result.substr(3, 2),
            seconds: result.substr(6, 2)
        };

        //const time = formatTime(Math.round(video.currentTime));
        timeElapsed.innerText = `${time.minutes}:${time.seconds}`;
        timeElapsed.setAttribute('datetime', `${time.minutes}m ${time.seconds}s`);

        //Update progressBar
        seek.value = Math.round(video.currentTime);
        progressBar.value = Math.round(video.currentTime);

        if ((video.duration - video.currentTime) <= 6) {
            nextEpisodeStart();
        }
    },
    updateSeekTooltip: function (event) {
        const skipTo = Math.round(
            (event.offsetX / event.target.clientWidth) *
            parseInt(event.target.getAttribute('max'), 10)
        );

        if (isNaN(skipTo))
            return;

        seek.setAttribute('data-seek', skipTo);


        const result = new Date(skipTo * 1000).toISOString().substr(11, 8);
        const t = {
            minutes: result.substr(3, 2),
            seconds: result.substr(6, 2)
        };
        seekTooltip.textContent = `${t.minutes}:${t.seconds}`;
        const rect = video.getBoundingClientRect();
        seekTooltip.style.left = `${event.pageX - rect.left}px`;
    },
    UpdateVolume: function () {
        video.volume = volume.value;

        volumeIcons.forEach((icon) => {
            icon.classList.add('hidden');
        });

        volumeButton.setAttribute('data-title', 'Mute (M)');

        if (video.muted || video.volume === 0) {
            volumeMute.classList.remove('hidden');
            volumeButton.setAttribute('data-title', 'Unmute (M)');
        } else if (video.volume > 0 && video.volume <= 0.3) {
            volumeLow.classList.remove('hidden');
        } else if (video.volume > 0.3 && video.volume <= 0.6) {
            volumeMiddle.classList.remove('hidden');
        } else {
            volumeHigh.classList.remove('hidden');
        }
    },
    ToggleMute: function () {
        video.muted = !video.muted;

        if (video.muted) {
            volume.setAttribute('data-volume', volume.value);
            volume.dataset.volume = volume.value;
            volume.value = 0;
        } else {
            volume.value = volume.dataset.volume;
        }
        VideoPlayer.UpdateVolume();
        volume.blur();
    },
    TogglePip: async function () {
        try {
            if (video !== document.pictureInPictureElement) {
                pipButton.disabled = true;
                await video.requestPictureInPicture();
            } else {
                await document.exitPictureInPicture();
            }
        } catch (error) {
            console.error(error);
        } finally {
            pipButton.disabled = false;
        }
    },
    SkipAhead: function (event) {

        const skipTo = event.target.dataset.seek
            ? event.target.dataset.seek
            : event.target.value;
        video.currentTime = skipTo;
        progressBar.value = skipTo;
        seek.value = skipTo;
        seek.blur();
    },
    ToggleFullScreen: function () {
        if (document.fullscreenElement) {
            document.exitFullscreen();
        } else if (document.webkitFullscreenElement) {
            // Need this to support Safari
            document.webkitExitFullscreen();
        } else if (videoContainer.webkitRequestFullscreen) {
            // Need this to support Safari
            videoContainer.webkitRequestFullscreen();
        } else {
            videoContainer.requestFullscreen();
        }

        fullscreenIcons.forEach((icon) => icon.classList.toggle('hidden'));

        if (document.fullscreenElement || document.webkitFullscreenElement) {
            fullScreenButton.setAttribute('data-title', 'Exit full screen (f)');
            video.classList.remove("h-auto");
        } else {
            fullScreenButton.setAttribute('data-title', 'Full screen (f)');
            video.classList.add("h-auto");
        }
    },
    ScroolToVideo: function () {
        //video && video.scrollIntoView({ behavior: "smooth", block: 'center' });
        video && video.scrollIntoView();
    }
};
