//Definition vars
var video;
var videoControls;
var playButton;
var playbackIcons;
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
var fullscreenButton;
var videoContainer;
var fullscreenIcons;
var pipButton;
var videoWorks;


var nextEpisodeWrapper;
var nextEpisodeDuration;
var nextEpisodeTimer;
var nextEpisodeTimeLeft;
var nextEpisodeStarted;
var secs;



// Init Variabili
function initVideoVariables() {
    video = document.querySelectorAll('.video')[0];
    videoControls = document.querySelectorAll('.video-controls')[0];
    playButton = document.querySelectorAll('.play')[0];
    playbackIcons = document.querySelectorAll('.playback-icons i');
    timeElapsed = document.getElementById('time-elapsed');
    duration = document.getElementById('duration');
    progressBar = document.querySelectorAll('.progress-bar')[0];
    seek = document.querySelectorAll('.seek')[0];
    seekTooltip = document.querySelectorAll('.seek-tooltip')[0];
    volumeButton = document.getElementById('volume-button');
    volumeIcons = document.querySelectorAll('.volume-button i');
    volumeMute = document.querySelectorAll('.fa-volume-mute')[0];
    volumeLow = document.querySelectorAll('.fa-volume-down')[0];
    volumeMiddle = document.querySelectorAll('.fa-volume')[0];
    volumeHigh = document.querySelectorAll('.fa-volume-up')[0];
    volume = document.querySelectorAll('.volume')[0];
    playbackAnimation = document.querySelectorAll('.playback-animation')[0];
    fullscreenButton = document.querySelectorAll('.fullscreen-button')[0];
    videoContainer = document.querySelectorAll('.video-container')[0];
    fullscreenIcons = document.querySelectorAll('.video-container i');
    pipButton = document.getElementById('pip-button');
    videoWorks = !!document.createElement('video').canPlayType;


    nextEpisodeWrapper = document.getElementById('nextEpisodeCountDown');
    nextEpisodeDuration = 5;
    nextEpisodeTimeLeft = document.getElementById("nextEpisodeTimeLeft");
    nextEpisodeStarted = false;
    secs = nextEpisodeDuration;



    // Add eventlisteners
    playButton.addEventListener('click', togglePlay);
    video.addEventListener('play', updatePlayButton);
    video.addEventListener('pause', updatePlayButton);
    video.addEventListener('timeupdate', updateTimeElapsed);
    video.addEventListener('timeupdate', updateProgress);
    video.addEventListener('volumechange', updateVolumeIcon);
    video.addEventListener('click', togglePlay);
    video.addEventListener('click', animatePlayback);
    video.addEventListener('mouseenter', showControls);
    video.addEventListener('mouseleave', hideControls);
    videoControls.addEventListener('mouseenter', showControls);
    videoControls.addEventListener('mouseleave', hideControls);
    seek.addEventListener('mousemove', updateSeekTooltip);
    //seek.addEventListener('input', skipAhead);
    seek.addEventListener('click', skipAhead);
    volume.addEventListener('input', updateVolume);
    volumeButton.addEventListener('click', toggleMute);
    fullscreenButton.addEventListener('click', toggleFullScreen);
    pipButton.addEventListener('click', togglePip);
    document.addEventListener('keyup', keyboardShortcuts);

    //Initialize Video Player
    video.addEventListener('loadedmetadata', initializeVideo);

    if (videoWorks) {
        video.controls = false;
        videoControls.classList.remove('hidden');
    }

    if (!('pictureInPictureEnabled' in document)) {
        pipButton.classList.add('hidden');
    }
}

// initializeVideo sets the video duration, and maximum value of the
// progressBar
function initializeVideo() {
    const videoDuration = Math.round(video.duration);

    if (isNaN(videoDuration))
        return;

    seek.setAttribute('max', videoDuration);
    progressBar.setAttribute('max', videoDuration);
    const time = formatTime(videoDuration);
    duration.innerText = `${time.minutes}:${time.seconds}`;
    duration.setAttribute('datetime', `${time.minutes}m ${time.seconds}s`);
}

// togglePlay toggles the playback state of the video.
// If the video playback is paused or ended, the video is played
// otherwise, the video is paused
function togglePlay() {
    if (video.paused || video.ended) {
        video.play();
    } else {
        video.pause();
    }
}

// updatePlayButton updates the playback icon and tooltip
// depending on the playback state
function updatePlayButton() {
    playbackIcons.forEach((icon) => icon.classList.toggle('hidden'));

    if (video.paused) {
        playButton.setAttribute('data-title', 'Play (spacebar)');
    } else {
        playButton.setAttribute('data-title', 'Pause (spacebar)');
    }
}

// formatTime takes a time length in seconds and returns the time in
// minutes and seconds
function formatTime(timeInSeconds) {
    if (isNaN(timeInSeconds))
        return;

    const result = new Date(timeInSeconds * 1000).toISOString().substr(11, 8);

    return {
        minutes: result.substr(3, 2),
        seconds: result.substr(6, 2),
    };
}

// updateTimeElapsed indicates how far through the video
// the current playback is by updating the timeElapsed element
function updateTimeElapsed() {
    const time = formatTime(Math.round(video.currentTime));
    timeElapsed.innerText = `${time.minutes}:${time.seconds}`;
    timeElapsed.setAttribute('datetime', `${time.minutes}m ${time.seconds}s`);
}

// updateProgress indicates how far through the video
// the current playback is by updating the progress bar
function updateProgress() {
    seek.value = Math.round(video.currentTime);
    progressBar.value = Math.round(video.currentTime);

    if ((video.duration - video.currentTime) <= 6) {
        nextEpisodeStart();
    }
}

// updateSeekTooltip uses the position of the mouse on the progress bar to
// roughly work out what point in the video the user will skip to if
// the progress bar is clicked at that point
function updateSeekTooltip(event) {
    const skipTo = Math.round(
        (event.offsetX / event.target.clientWidth) *
        parseInt(event.target.getAttribute('max'), 10)
    );

    if (isNaN(skipTo))
        return;

    seek.setAttribute('data-seek', skipTo);
    const t = formatTime(skipTo);
    seekTooltip.textContent = `${t.minutes}:${t.seconds}`;
    const rect = video.getBoundingClientRect();
    seekTooltip.style.left = `${event.pageX - rect.left}px`;
}

// skipAhead jumps to a different point in the video when the progress bar
// is clicked
function skipAhead(event) {
    const skipTo = event.target.dataset.seek
        ? event.target.dataset.seek
        : event.target.value;
    video.currentTime = skipTo;
    progressBar.value = skipTo;
    seek.value = skipTo;
    seek.blur();
}

// updateVolume updates the video's volume
// and disables the muted state if active
function updateVolume() {
    if (video.muted) {
        video.muted = false;
    }

    video.volume = volume.value;
}

// updateVolumeIcon updates the volume icon so that it correctly reflects
// the volume of the video
function updateVolumeIcon() {
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
}

// toggleMute mutes or unmutes the video when executed
// When the video is unmuted, the volume is returned to the value
// it was set to before the video was muted
function toggleMute() {
    video.muted = !video.muted;

    if (video.muted) {
        volume.setAttribute('data-volume', volume.value);
        volume.value = 0;
    } else {
        volume.value = volume.dataset.volume;
    }
    volume.blur();
}

// animatePlayback displays an animation when
// the video is played or paused
function animatePlayback() {
    playbackAnimation.animate(
        [
            {
                opacity: 1,
                transform: 'scale(1)',
            },
            {
                opacity: 0,
                transform: 'scale(1.3)',
            },
        ],
        {
            duration: 500,
        }
    );
}

// toggleFullScreen toggles the full screen state of the video
// If the browser is currently in fullscreen mode,
// then it should exit and vice versa.
function toggleFullScreen() {
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

    updateFullscreenButton();
}

// updateFullscreenButton changes the icon of the full screen button
// and tooltip to reflect the current full screen state of the video
function updateFullscreenButton() {
    fullscreenIcons.forEach((icon) => icon.classList.toggle('hidden'));

    if (document.fullscreenElement) {
        fullscreenButton.setAttribute('data-title', 'Exit full screen (F)');
    } else {
        fullscreenButton.setAttribute('data-title', 'Full screen (F)');
    }
}

// togglePip toggles Picture-in-Picture mode on the video
async function togglePip() {
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
}

// hideControls hides the video controls when not in use
// if the video is paused, the controls must remain visible
function hideControls() {
    if (video.paused) {
        return;
    }

    videoControls.classList.add('hide');
}

// showControls displays the video controls
function showControls() {
    videoControls.classList.remove('hide');
}

// keyboardShortcuts executes the relevant functions for
// each supported shortcut key
function keyboardShortcuts(event) {
    const { key } = event;
    switch (key) {
        case ' ': //Spacebar
            togglePlay();
            animatePlayback();
            if (video.paused) {
                showControls();
            } else {
                setTimeout(() => {
                    hideControls();
                }, 2000);
            }
            break;
        case 'm':
            toggleMute();
            break;
        case 'f':
            toggleFullScreen();
            break;
        case 'p':
            togglePip();
            break;
        case 'ArrowRight':
            var NextStop = parseInt(progressBar.value) + 10;
            if (video.duration <= NextStop)
                seek.value = video.duration;

            video.currentTime = NextStop;
            progressBar.value = NextStop;
            seek.value = NextStop;
            break;
        case 'ArrowLeft':
            var NextStop = parseInt(progressBar.value) - 10;
            if (NextStop <= 0)
                seek.value = 0;

            video.currentTime = NextStop;
            progressBar.value = NextStop;
            seek.value = NextStop;
            break;
        case 'ArrowUp':
            if ((parseFloat(volume.value) + 0.2) >= 1) {
                volume.value = 1;
            }
            volume.value = parseFloat(volume.value) + 0.2;
            updateVolume();
            break;
        case 'ArrowDown':
            if ((parseFloat(volume.value) - 0.2) <= 0) {
                volume.value = 0;
            }
            volume.value = parseFloat(volume.value) - 0.2;
            updateVolume();
            break;
    }
}

//// Gestione SLIDE Prossimo Episodio
//const nextEpisodeWrapper = document.getElementById('nextEpisodeCountDown');
//const nextEpisodeDuration = 5;
//var nextEpisodeTimer;
//var nextEpisodeTimeLeft = document.getElementById("nextEpisodeTimeLeft");
//var nextEpisodeStarted = false;
//var secs = nextEpisodeDuration;


function nextEpisodeStart() {
    if (!nextEpisodeStarted) {
        secs = nextEpisodeDuration;
        nextEpisodeTimer = setInterval(nextEpisodeUpdate, 1000);
        nextEpisodeStarted = true;
    }

    nextEpisodeWrapper.classList.remove("hide");
    nextEpisodeWrapper.querySelectorAll('.slide')[0].classList.add("slideAnimation");
}
function nextEpisodeUpdate() {
  
  if (secs < 1)
  {
    nextEpisodeTimeLeft.innerHTML = "";
    clearInterval(nextEpisodeTimer);
	nextEpisodeStarted = false;
	//AVVIA PROSSIMO EPISODIO!!!
	return;
  }
  
  secs--;
  nextEpisodeTimeLeft.innerHTML = ' in ' + secs + ' sec.';        
  
 }